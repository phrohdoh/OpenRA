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
		readonly World world;
		readonly WorldRenderer wr;
		readonly TileSet tileSet;

		Dictionary<string, string> cellReplacements = new Dictionary<string, string>();

		CellLayer<int> strength;
		int ticksSinceLastRegrowth;

		public DamageableRegrowingCellsLayer(Actor self, DamageableRegrowingCellsLayerInfo info)
		{
			world = self.World;
			this.info = info;
			ticksSinceLastRegrowth = info.RegrowthDelay;
		}
			
		void IWorldLoaded.WorldLoaded(World w, WorldRenderer wr)
		{
			this.wr = wr;
			tileSet = wr.World.TileSet;

			foreach (var kv in info.ReplaceCellTypes)
			{
				foreach (var sourceType in kv.Key)
					if (!cellReplacements.ContainsKey(sourceType))
						cellReplacements.Add(sourceType, kv.Value);
					else
						cellReplacements[sourceType] = kv.Value;
			}

			foreach (var cell in w.Map.AllCells.Where(IsDesiredCell))
				ReplaceCellWithOther(cell);
		}

		void ReplaceCellWithOther(CPos cell)
		{
			
		}

		// TODO: Generalize this and use this to get kv.key info
		bool IsDesiredCell(CPos cell)
		{
			var tile = tileSet.GetTileInfo(world.Map.MapTiles.Value[cell]);
			if (tile == null)
				return false;

			var index = world.Map.GetTerrainIndex(cell);
			if (index == byte.MaxValue)
				return false;

			return tileSet[index].Type == tile.TerrainType;
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
				// todo: regrow ice
			}
		}
	}
}
