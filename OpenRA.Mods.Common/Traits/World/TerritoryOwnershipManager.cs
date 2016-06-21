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
using OpenRA.Traits;
using System.Linq;
using System.IO;
using OpenRA.Primitives;
using System;
using System.Drawing;

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

		public TerritoryOwnershipManager(ActorInitializer init, TerritoryOwnershipManagerInfo info)
		{
			ownershipValues = new Dictionary<Player, CellLayer<int>>();
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
		}

		public void ClearValue(Player ofPlayer, CPos location)
		{
			CellLayer<int> values;
			if (!ownershipValues.TryGetValue(ofPlayer, out values))
				return;

			values[location] = 0;
		}

		public void ClearValue(Player ofPlayer, IEnumerable<CPos> locations)
		{
			CellLayer<int> values;
			if (!ownershipValues.TryGetValue(ofPlayer, out values))
				return;

			foreach (var location in locations)
				values[location] = 0;
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
				yield break;

			foreach (var cell in world.Map.AllCells)
				if (GetOwningPlayer(cell) == owner)
					yield return cell;
		}

		IEnumerable<CPos> GetOwnedEdgeCells(Player owner)
		{
			var ownedCells = GetOwnedCells(owner).ToArray();
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
				if (player.Stances[world.RenderPlayer] == Stance.Ally)
					foreach (var cell in GetOwnedEdgeCells(player))
						yield return Pair.New(cell, player.Color.RGB);
		}
	}
}