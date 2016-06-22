#region Copyright & License Information
/*
 * Copyright 2007-2016 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	public class TerritoryOwnershipManagerInfo : ITraitInfo
	{
		public readonly string InitialOwnerInternalName = "Neutral";

		public object Create(ActorInitializer init) { return new TerritoryOwnershipManager(init, this); }
	}

	public class TerritoryOwnershipManager : INotifyCreated, IRadarSignature
	{
		readonly Dictionary<Player, CellLayer<int>> ownershipValues;
		readonly TerritoryOwnershipManagerInfo info;
		readonly World world;

		readonly Dictionary<Player, IEnumerable<CPos>> cachedOwnedCells;

		public TerritoryOwnershipManager(ActorInitializer init, TerritoryOwnershipManagerInfo info)
		{
			ownershipValues = new Dictionary<Player, CellLayer<int>>();
			cachedOwnedCells = new Dictionary<Player, IEnumerable<CPos>>();

			this.info = info;
			world = init.World;
		}

		public void UpdateValue(Player ofPlayer, CPos location, int delta)
		{
			CellLayer<int> values;
			if (!ownershipValues.TryGetValue(ofPlayer, out values))
			{
				values = new CellLayer<int>(ofPlayer.World.Map);
				ownershipValues.Add(ofPlayer, values);
			}

			values[location] += delta;
			values[location] = values[location].Clamp(0, int.MaxValue);
			cachedOwnedCells[ofPlayer] = Enumerable.Empty<CPos>();
		}

		public void UpdateValue(Player ofPlayer, IEnumerable<CPos> locations, int delta)
		{
			CellLayer<int> values;
			if (!ownershipValues.TryGetValue(ofPlayer, out values))
			{
				values = new CellLayer<int>(ofPlayer.World.Map);
				ownershipValues.Add(ofPlayer, values);
			}

			foreach (var location in locations)
			{
				values[location] += delta;
				values[location] = values[location].Clamp(0, int.MaxValue);
			}

			cachedOwnedCells[ofPlayer] = Enumerable.Empty<CPos>();
		}

		public int GetValue(Player ofPlayer, CPos location)
		{
			CellLayer<int> values;
			if (!ownershipValues.TryGetValue(ofPlayer, out values))
				return -1;

			return values[location];
		}

		public Player GetOwningPlayer(CPos ofLocation)
		{
			var lastValue = 0;
			Player ret = null;

			foreach (var kv in ownershipValues)
			{
				var value = kv.Value[ofLocation];
				if (value > lastValue)
				{
					lastValue = value;
					ret = kv.Key;
				}
			}

			return ret;
		}

		IEnumerable<CPos> GetOwnedCells(Player owner)
		{
			if (!ownershipValues.ContainsKey(owner))
				return Enumerable.Empty<CPos>();

			IEnumerable<CPos> cached = new CPos[0];
			if (cachedOwnedCells.TryGetValue(owner, out cached) && cached.Any())
				return cached;

			var ret = new HashSet<CPos>();
			foreach (var cell in world.Map.AllCells)
				if (GetOwningPlayer(cell) == owner)
					ret.Add(cell);

			cachedOwnedCells[owner] = ret;
			return ret;
		}

		IEnumerable<CPos> GetOwnedEdgeCells(Player owner)
		{
			var ownedCells = GetOwnedCells(owner);
			if (!ownedCells.Any())
				return Enumerable.Empty<CPos>();

			var markedForRemoval = new HashSet<CPos>();
			foreach (var cell in ownedCells)
			{
				var numContained = 0;

				foreach (var neighbor in Util.Neighbours(cell, false, false))
					if (ownedCells.Contains(neighbor))
						numContained++;

				if (numContained == 4)
					markedForRemoval.Add(cell);
			}

			return ownedCells.Except(markedForRemoval);
		}

		void INotifyCreated.Created(Actor self)
		{
			self.World.AddFrameEndTask(world => {
				var initialOwner = world.Players.FirstOrDefault(p => p.InternalName == info.InitialOwnerInternalName);
				if (initialOwner == null)
					throw new InvalidDataException("No player with InternalName '{0}' could be found.".F(info.InitialOwnerInternalName));

				var defaultValues = new CellLayer<int>(world.Map);

				// The default player must be the initial result of `GetOwningPlayer` for all cells.
				// As soon as another player stakes a claim they will be the owner.
				defaultValues.Clear(1);

				ownershipValues.Add(initialOwner, defaultValues);
			});
		}

		IEnumerable<Pair<CPos, Color>> IRadarSignature.RadarSignatureCells(Actor self)
		{
			foreach (var player in ownershipValues.Keys)
				if (world.RenderPlayer == null || player.Stances[world.RenderPlayer] == Stance.Ally)
					foreach (var cell in GetOwnedEdgeCells(player))
						yield return Pair.New(cell, player.Color.RGB.WithBrightness(0.9f));
		}
	}

	static class ColorExts
	{
		public static Color WithBrightness(this Color c, float brightness)
		{
			//brightness = brightness.Clamp(0, 1);
			return Color.FromArgb(c.A,
				(int)(c.R * brightness).Clamp(0, 255),
				(int)(c.G * brightness).Clamp(0, 255),
				(int)(c.B * brightness).Clamp(0, 255));
		}
	}
}