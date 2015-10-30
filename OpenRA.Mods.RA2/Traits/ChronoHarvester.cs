using System;
using OpenRA.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.RA2.Activities;

namespace OpenRA.Mods.RA2.Traits
{
	[Desc("Harvester that teleports to refineries.")]
	public class ChronoHarvesterInfo : HarvesterInfo
	{
		public override object Create(ActorInitializer init) { return new ChronoHarvester(init.Self, this); }
	}

	public class ChronoHarvester : Harvester
	{
		public ChronoHarvester(Actor self, ChronoHarvesterInfo info)
			: base (self, info)
		{
		}

		public override Activity DeliverResources(Actor self)
		{
			return new ChronoDeliverResources(self);
		}
	}
}

