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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using OpenRA.Primitives;
using OpenRA.Traits;
using System.Linq;

namespace OpenRA.Mods.Common.Traits
{
	public class TerritoryOwnershipInfluenceInfo : ITraitInfo
	{
		[FieldLoader.Require]
		public readonly int Range;

		public object Create(ActorInitializer init) { return new TerritoryOwnershipInfluence(init, this); }
	}

	public class TerritoryOwnershipInfluence : INotifyCreated, INotifyKilled, IRadarSignature
	{
		readonly TerritoryOwnershipManager manager;

		Actor self;
		TerritoryOwnershipInfluenceInfo info;
		Player owner { get { return self.Owner; } }

		public TerritoryOwnershipInfluence(ActorInitializer init, TerritoryOwnershipInfluenceInfo info)
		{
			self = init.Self;
			this.info = info;

			manager = init.World.WorldActor.TraitOrDefault<TerritoryOwnershipManager>();
			if (manager == null)
				throw new InvalidDataException("The World actor must have the TerritoryOwnershipManager trait to use TerritoryOwnershipInfluence.");
		}

		IEnumerable<CPos> GetInfluenceCells()
		{
			return self.World.Map.FindTilesInCircle(self.Location, info.Range, false);
		}

		void INotifyKilled.Killed(Actor self, AttackInfo e)
		{
			manager.ClearValue(owner, self.Location);
		}

		void INotifyCreated.Created(Actor self)
		{
			self.World.AddFrameEndTask(world => {
				manager.UpdateValue(owner, GetInfluenceCells(), 1);
			});
		}

		IEnumerable<Pair<CPos, Color>> IRadarSignature.RadarSignatureCells(Actor self)
		{
			foreach (var cell in GetInfluenceCells())
				yield return Pair.New(cell, self.Owner.Color.RGB);
		}
	}
}