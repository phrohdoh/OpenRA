using System;
using OpenRA.Traits;

namespace OpenRA.Mods.TS.Traits
{
	public class IceLayerInfo : ITraitInfo
	{
		public readonly int MaxStrength = int.MaxValue;

		public object Create(ActorInitializer init) { return new IceLayer(init.Self, this); }
	}

	public class IceLayer
	{
		CellLayer<int> strength;

		public IceLayer(Actor self, IceLayerInfo info)
		{
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
	}
}