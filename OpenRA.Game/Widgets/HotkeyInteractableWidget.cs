using OpenRA.Graphics;
using OpenRA.Primitives;

namespace OpenRA.Widgets
{
	public class HotkeyInteractableWidget : Widget
	{
		SpriteFont hotkeyFont;
		protected (Hotkey hotkey, float2 offset)[] hotkeyOverlays;

        public override void Initialize(WidgetArgs args)
        {
            base.Initialize(args);
			hotkeyOverlays = new (Hotkey hotkey, float2 offset)[0];
			hotkeyFont = Game.Renderer.Fonts["hotkey"];
        }

        public override void DrawOuter()
        {
			base.DrawOuter();

			if (IsVisible() && Game.GetModifierKeys().HasModifier(Modifiers.Meta))
				foreach ((var hotkey, var offset) in hotkeyOverlays)
					hotkeyFont.DrawTextWithContrast(
						text: hotkey.DisplayString(),
						location: offset,
						fg: Color.Gold,
						bg: Color.Black,
						offset: 2
					);
        }
	}
}
