using System;
using OpenRA.Graphics;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets.Logic
{
	public class MapEditorViewportLogic : ChromeLogic
	{
		EditorViewportControllerWidget vc;

		[ObjectCreator.UseCtor]
		public MapEditorViewportLogic(Widget widget, World world, WorldRenderer worldRenderer)
		{
			vc = widget as EditorViewportControllerWidget;

			var btnDefaultBrush = widget.Get<ButtonWidget>("DEFAULT_BRUSH");
			btnDefaultBrush.OnClick += vc.ClearBrush;

			var btnElevationBrush = widget.GetOrNull<ButtonWidget>("ELEVATION_BRUSH");
			if (btnElevationBrush != null)
			{
				btnElevationBrush.OnClick += () => { vc.SetBrush(new EditorElevationModifierBrush(vc, worldRenderer)); };
			}
		}
	}
}