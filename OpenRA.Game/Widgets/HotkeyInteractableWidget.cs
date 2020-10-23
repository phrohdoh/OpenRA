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
			Func<bool> isActivatableNowFn,
			float2 absPosRelViewportTopLeft
		)[] hotkeyOverlays;

		public HotkeyInteractableWidget() {}
		public HotkeyInteractableWidget(HotkeyInteractableWidget other) : base(other)
		{
			hotkeyFont = other.hotkeyFont;
			hotkeyOverlays = other.hotkeyOverlays;
		}

        public override void Initialize(WidgetArgs args)
        {
            base.Initialize(args);
			hotkeyOverlays = new (Hotkey hotkey, Func<bool> isActivatableNowFn, float2 absPosRelViewportTopLeft)[]{};
			hotkeyFont = Game.Renderer.Fonts["hotkey"];
        }

        public override void DrawOuter()
        {
			base.DrawOuter();

			if (IsVisible() && Game.GetModifierKeys().HasModifier(Modifiers.Meta))
				foreach ((var hotkey, var isActivatableNowFn, var absPosRelViewportTopLeft) in hotkeyOverlays)
					if (hotkey.IsValid())
						hotkeyFont.DrawTextWithContrast(
							text: hotkey.DisplayString(),
							location: absPosRelViewportTopLeft,
							fg: isActivatableNowFn() ? Color.Gold : Color.Gray,
							bg: Color.Black,
							offset: 2
						);
        }
	}
}
