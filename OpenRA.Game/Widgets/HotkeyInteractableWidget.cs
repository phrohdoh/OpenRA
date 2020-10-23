using System;
using OpenRA.Graphics;
using OpenRA.Primitives;

namespace OpenRA.Widgets
{
	public class HotkeyInteractableWidget : Widget
	{
		SpriteFont hotkeyFont;
		protected (
			Hotkey hotkey,
			Func<Color> fgColorFn,
			float2 absPosRelViewportTopLeft
		)[] hotkeyOverlays;

		public readonly string HotkeyOverlayFont = "TinyBold";

		public HotkeyInteractableWidget() {}
		public HotkeyInteractableWidget(HotkeyInteractableWidget other) : base(other)
		{
			hotkeyFont = other.hotkeyFont;
			hotkeyOverlays = other.hotkeyOverlays;
		}

        public override void Initialize(WidgetArgs args)
        {
            base.Initialize(args);
			hotkeyOverlays = new (Hotkey hotkey, Func<Color> fgColorFn, float2 absPosRelViewportTopLeft)[]{};
			hotkeyFont = Game.Renderer.Fonts[HotkeyOverlayFont];
        }

        public override void DrawOuter()
        {
			base.DrawOuter();

			if (IsVisible() && Game.Settings.Game.DisplayHotkeyOverlays)
				foreach ((var hotkey, var fgColorFn, var absPosRelViewportTopLeft) in hotkeyOverlays)
					if (hotkey.IsValid())
						hotkeyFont.DrawTextWithContrast(
							text: hotkey.DisplayString(),
							location: absPosRelViewportTopLeft,
							fg: fgColorFn(),
							bg: Color.Black,
							offset: 2
						);
        }
	}
}
