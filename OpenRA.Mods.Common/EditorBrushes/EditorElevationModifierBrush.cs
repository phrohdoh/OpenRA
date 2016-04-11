using System;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Graphics;
using System.Collections.Generic;

namespace OpenRA.Mods.Common.Widgets
{
	public static class CPosExts
	{
		public static CPos[] Expand(this CPos cell, Map map, bool includeOffMapCells = false, bool includeOriginCell = true)
		{
			var tl = new MPos(cell.X - 1, cell.Y - 1).ToCPos(map.Grid.Type);
			var br = new MPos(cell.X + 1, cell.Y + 1).ToCPos(map.Grid.Type);

			var ret = new List<CPos>();

			for (var x = tl.X; x < br.X; x++)
			{
				for (var y = tl.Y; y < br.Y; y++)
				{
					var c = new CPos(x, y);
					if (!includeOffMapCells && !map.Contains(c))
						continue;

					if (!includeOriginCell && c == cell)
						continue;

					ret.Add(c);
				}
			}

			return ret.ToArray();
		}
	}

	public class EditorElevationModifierBrush : IEditorBrush
	{
		World world;
		WorldRenderer wr;
		Map map;
		CellLayer<byte> mapHeight;

		enum ElevationMod
		{
			Lower = -1,
			Raise = 1,
		}

		readonly Dictionary<MouseButton, ElevationMod> buttonToModType = new Dictionary<MouseButton, ElevationMod>
		{
			{ MouseButton.Left, ElevationMod.Raise },
			{ MouseButton.Right, ElevationMod.Lower }
		};

		public EditorElevationModifierBrush(EditorViewportControllerWidget editorWidget, WorldRenderer wr)
		{
			world = wr.World;
			map = world.Map;
			mapHeight = map.Height;
			this.wr = wr;
		}

		void IEditorBrush.Tick() { }
		void IDisposable.Dispose() { }

		bool IEditorBrush.HandleMouseInput(MouseInput mi)
		{
			if (mi.Event != MouseInputEvent.Down)
				return false;

			var cell = wr.Viewport.ViewToWorld(mi.Location);
			mapHeight[cell] = GetNewHeight(cell, mi);
			var surroundingCells = Util.Neighbours(cell, true, false);

			foreach (var c in surroundingCells)
				IncrementHeight(c);

			return true;
		}

		byte GetNewHeight(CPos cell, MouseInput mi)
		{
			if (mi.Button == MouseButton.Middle)
				return 0;

			var currHeight = mapHeight[cell];
			return (byte)((currHeight + (int)buttonToModType[mi.Button]).Clamp(0, map.Grid.MaximumTerrainHeight));
		}

		void IncrementHeight(CPos cell)
		{
			mapHeight[cell] = (byte)((mapHeight[cell] + 1).Clamp(0, map.Grid.MaximumTerrainHeight));
		}

		void DecrementHeight(CPos cell)
		{
			mapHeight[cell] = (byte)((mapHeight[cell] - 1).Clamp(0, map.Grid.MaximumTerrainHeight));
		}
	}
}