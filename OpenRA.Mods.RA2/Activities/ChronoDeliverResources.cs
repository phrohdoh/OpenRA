using System;
using OpenRA.Activities;
using OpenRA.Mods.Common.Activities;
using OpenRA.Traits;

namespace OpenRA.Mods.RA2.Activities
{
	public class ChronoDeliverResources : DeliverResources
	{
		public ChronoDeliverResources(Actor self)
			: base (self)
		{ }

		protected override Activity MoveToRefinery(Actor self, CPos dest)
		{
			var pos = self.Trait<IPositionable>();

			if (pos.CanEnterCell(dest))
				return Util.SequenceActivities(new SimpleTeleport(dest), this);

			Util.RunActivity(self, Movement.MoveTo(dest, 0));
			return this;
		}
	}
}

