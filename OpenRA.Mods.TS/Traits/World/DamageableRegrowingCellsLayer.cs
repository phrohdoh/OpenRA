using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.TS.Traits
{
	public class DamageableRegrowingCellsLayerInfo : ITraitInfo
	{
		[Desc("Maximum damage each cell can take before breaking.")]
		public readonly int MaxStrength = int.MaxValue;

		[Desc("Cells regrows once per this many ticks.")]
		public readonly int RegrowthDelay = 25;

		[Desc("Replaces cell type A with cell type B when the world is loaded."), FieldLoader.LoadUsing("LoadReplaceCellTypes")]
		public readonly Dictionary<string[], string> ReplaceCellTypes = null;

		public object Create(ActorInitializer init) { return new DamageableRegrowingCellsLayer(init.Self, this); }

		static object LoadReplaceCellTypes(MiniYaml y)
		{
			MiniYaml cellTypes;
			if (!y.ToDictionary ().TryGetValue ("ReplaceCellTypes", out cellTypes))
				return null;

			return cellTypes.Nodes.ToDictionary(
				kv => FieldLoader.GetValue<string[]>("(key)", kv.Key),
				kv => FieldLoader.GetValue<string>("(value)", kv.Value.Value));
		}
	}

	public class DamageableRegrowingCellsLayer : ITick, IWorldLoaded
	{
		readonly DamageableRegrowingCellsLayerInfo info;
//		readonly TileSet tileset;

		Dictionary<string, string> cellReplacements = new Dictionary<string, string>();

		CellLayer<int> strength;
		int ticksSinceLastRegrowth;

		public DamageableRegrowingCellsLayer(Actor self, DamageableRegrowingCellsLayerInfo info)
		{
			this.info = info;
			ticksSinceLastRegrowth = info.RegrowthDelay;

//			tileset = self.World.Map.Rules.TileSets[self.World.Map.Tileset];
		}
			
		void IWorldLoaded.WorldLoaded(World w, WorldRenderer wr)
		{
//			var cells = w.Map.AllCells;

			foreach (var kv in info.ReplaceCellTypes)
			{
				// TODO: cellReplacements should be <ushort, string>
				// by getting the ushort from the tileset definitions in this method when given a string type

				foreach (var toReplace in kv.Key)
				{
					if (!cellReplacements.ContainsKey(toReplace))
						cellReplacements.Add(toReplace, kv.Value);
					else
						cellReplacements[toReplace] = kv.Value;
				}
			}
			/*
			foreach (var cell in cells.Where(c => cellReplacements.ContainsKey(w.Map.MapTiles.Value[c].Type)))
			{
				
			}
			*/
		}

		public bool DamageCell(CPos cell, int damage)
		{
			if (!strength.Contains(cell))
				return false;

			if ((strength[cell] -= damage) <= 0)
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
