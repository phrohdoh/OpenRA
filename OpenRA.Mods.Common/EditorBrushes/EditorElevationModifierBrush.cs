using System;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Graphics;

namespace OpenRA.Mods.Common.Widgets
{
	public class EditorElevationModifierBrush : IEditorBrush
	{
		World world;
		WorldRenderer wr;

		public EditorElevationModifierBrush(EditorViewportControllerWidget editorWidget, WorldRenderer wr)
		{
			world = wr.World;
			this.wr = wr;
		}

		void IEditorBrush.Tick() { }
		void IDisposable.Dispose() { }

		bool IEditorBrush.HandleMouseInput(MouseInput mi)
		{
			if (mi.Event != MouseInputEvent.Down)
				return false;

			var map = world.Map;
			var cell = wr.Viewport.ViewToWorld(mi.Location);

			var mapHeight = map.Height;
			var currHeight = mapHeight[cell];

			var whichClick = mi.Button == MouseButton.Left ? 1 : -1;

			mapHeight[cell] = (byte)((currHeight + whichClick).Clamp(0, map.Grid.MaximumTerrainHeight));
			return true;
		}
	}
}