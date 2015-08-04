using System;
using OpenRA.Traits;

namespace OpenRA.Mods.TS.Traits
{
	public class IceLayerInfo : ITraitInfo
	{
		[Desc("Maximum damage each ice cell can take before breaking.")]
		public readonly int MaxStrength = int.MaxValue;

		[Desc("Ice regrows once per this many ticks.")]
		public readonly int RegrowthDelay = 25;

		public object Create(ActorInitializer init) { return new IceLayer(init.Self, this); }
	}

	public class IceLayer : ITick
	{
		readonly IceLayerInfo info;

		CellLayer<int> strength;
		int ticksSinceLastRegrowth;

		public IceLayer(Actor self, IceLayerInfo info)
		{
			this.info = info;
			ticksSinceLastRegrowth = info.RegrowthDelay;
		}

		public bool DamageCell(CPos cell, int damage)
		{
			if (!strength.Contains(cell))
				return false;

			if (strength[cell] -= damage <= 0)
				OnCellDeath(cell);

			return true;
		}

		void OnCellDeath(CPos cell)
		{

		}

		public void Tick(Actor self)
		{
			if (--ticksSinceLastRegrowth <= 0)
			{
				ticksSinceLastRegrowth = info.RegrowthDelay;
				// todo: regrowice
			}
		}
	}
}
