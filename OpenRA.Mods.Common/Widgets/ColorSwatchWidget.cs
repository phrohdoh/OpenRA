using OpenRA.Graphics;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets
{
	public class ColorSwatchWidget : ButtonWidget
	{
		public HSLColor Color;

		[ObjectCreator.UseCtor]
		public ColorSwatchWidget(ModData modData)
			: base(modData) {  }

		protected ColorSwatchWidget(ColorSwatchWidget other)
			: base(other)
		{
			Color = other.Color;
		}

		public override void Draw()
		{
			base.Draw();
			var rb = RenderBounds;
			rb.Inflate(-2, -2);
			WidgetUtils.FillRectWithColor(rb, Color.RGB);
		}

		public override Widget Clone() { return new ColorSwatchWidget(this); }
	}
}