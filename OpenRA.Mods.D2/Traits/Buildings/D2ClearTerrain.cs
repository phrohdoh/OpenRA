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
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.D2.Traits
{
	public class D2ClearTerrainInfo : ITraitInfo, Requires<BuildingInfo>
	{
		public readonly CVec Size = new CVec(1,1);

		public object Create(ActorInitializer init) { return new D2ClearTerrain(init.Self, this); }
	}

	public class D2ClearTerrain : INotifyAddedToWorld
	{
		readonly D2ClearTerrainInfo info;

		public D2ClearTerrain(Actor self, D2ClearTerrainInfo info)
		{
			this.info = info;
		}

		public void AddedToWorld(Actor self)
		{
			var smudgeLayers = self.World.WorldActor.TraitsImplementing<SmudgeLayer>().ToDictionary(x => x.Info.Type);

			var origin = self.Location;
			for (var x = 0; x < info.Size.X; x++)
			{
				for (var y = 0; y < info.Size.Y; y++)
				{
					var c = origin + new CVec(x, y);
					// clear SmudgeLayer at this position
					foreach (var item in smudgeLayers)
					{
						var smudgeLayer = item.Value;
						smudgeLayer.RemoveSmudge(c);
					}
				}
			}
		}
	}
}
