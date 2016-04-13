using System;
using OpenRA.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace OpenRA.Mods.Common.Widgets
{
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

		readonly Dictionary<CVec, byte> vecToRampType = new Dictionary<CVec, byte>
		{
			{ new CVec(1, 0),   1 },
			{ new CVec(0, 1),   2 },
			{ new CVec(-1, 0),  3 },
			{ new CVec(0, -1),  4 },
			{ new CVec(1, 1),   5 },
			{ new CVec(-1, 1),  6 },
			{ new CVec(-1, -1), 7 },
			{ new CVec(1, -1),  8 }
		};

		bool IEditorBrush.HandleMouseInput(MouseInput mi)
		{
			if (mi.Event != MouseInputEvent.Down)
				return false;

			var clickedCell = wr.Viewport.ViewToWorld(mi.Location);
			var newHeight = GetNewHeight(clickedCell, mi);
			mapHeight[clickedCell] = newHeight;

			var surroundingCells = Util.Neighbours(clickedCell, true, false)
				.ToDictionary(c => c, c => clickedCell - c);

			var rampTypesToBecome = surroundingCells.ToDictionary(kvp => kvp.Key, kvp => vecToRampType[kvp.Value]);
			foreach (var cell in rampTypesToBecome.Keys)
			{
				var rampToBecome = rampTypesToBecome[cell];
				var tile = world.Map.Tiles[cell];
				var tileInfo = world.Map.Rules.TileSet.GetTileInfo(tile);
				if (tileInfo != null)
				{
					// TODO: This cell is already a ramp.
					//       Need to create a transformation table.
					//       e.g. a flat ramp piece may need to become a corner.
					if (tileInfo.RampType != 0) { }

					ushort templateId;
					if (map.TryFindTemplateWithRampType(rampToBecome, out templateId))
						world.Map.Tiles[cell] = new TerrainTile(templateId, 0);
				}
			}

			/* TODO: Maybe roll this data (and the transformation table) into a struct.
			var heightDiffs = surroundingCells.Where(c => mapHeight[c.Key] != newHeight)
				.ToDictionary(c => c, c => newHeight - mapHeight[c.Key]);

			foreach (var key in heightDiffs.Keys)
			{
				var diff = heightDiffs[key];
				if (diff == 0) { }
			}
			*/

			return true;
		}

		byte GetNewHeight(CPos cell, MouseInput mi)
		{
			if (mi.Button == MouseButton.Middle)
				return 0;

			var currHeight = mapHeight[cell];
			return (byte)((currHeight + (int)buttonToModType[mi.Button]).Clamp(0, map.Grid.MaximumTerrainHeight));
		}
	}
}
