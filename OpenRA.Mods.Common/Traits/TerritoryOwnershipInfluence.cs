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

	public class TerritoryOwnershipInfluence : INotifyCreated,
		//IRadarSignature,
		INotifyKilled,
		INotifySold
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

		//IEnumerable<CPos> GetOwnedCells()
		//{
		//	foreach (var cell in GetInfluenceCells())
		//		if (manager.GetOwningPlayer(cell) == owner)
		//			yield return cell;
		//}

		void ClearValue()
		{
			manager.ClearValue(owner, GetInfluenceCells());
		}

		void INotifyKilled.Killed(Actor self, AttackInfo e) { ClearValue(); }

		void INotifyCreated.Created(Actor self)
		{
			self.World.AddFrameEndTask(world => {
				manager.UpdateValue(owner, GetInfluenceCells(), 1);
			});
		}

		//IEnumerable<CPos> GetOwnedEdgeCells()
		//{
		//	var markedForRemoval = new HashSet<CPos>();
		//	var ownedCells = GetOwnedCells();

		//	foreach (var cell in ownedCells)
		//	{
		//		var numContained = 0;

		//		foreach (var neighbor in Util.Neighbours(cell, false, false))
		//			if (ownedCells.Contains(neighbor))
		//				numContained++;

		//		if (numContained == 4)
		//			markedForRemoval.Add(cell);
		//	}

		//	return ownedCells.Except(markedForRemoval);
		//}

		//IEnumerable<Pair<CPos, Color>> IRadarSignature.RadarSignatureCells(Actor self)
		//{
		//	//foreach (var cell in GetInfluenceCells())
		//	//	if (manager.GetOwningPlayer(cell) == owner)
		//	foreach (var cell in GetOwnedEdgeCells())
		//		yield return Pair.New(cell, self.Owner.Color.RGB);
		//}

		void INotifySold.Selling(Actor self) { }

		void INotifySold.Sold(Actor self) { ClearValue(); }
	}
}