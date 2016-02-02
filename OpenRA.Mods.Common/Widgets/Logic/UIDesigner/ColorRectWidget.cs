using System;
using System.Drawing;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets.UIDesigner
{
	public class ColorRectWidget : Widget
	{
		public Func<Color> GetColor;
		public Func<float> GetWidth;

		public ColorRectWidget()
		{
			GetColor = () => Color.Red;
			GetWidth = () => 1f;
		}

		protected ColorRectWidget(ColorRectWidget widget)
			: base(widget)
		{
			GetColor = widget.GetColor;
			GetWidth = widget.GetWidth;
		}

		public override Widget Clone()
		{
			return new ColorRectWidget(this);
		}

		public override void Draw()
		{
			var rb = RenderBounds.InflateBy(0, 0, -1, -1);
			WidgetUtils.DrawRect(rb, GetColor(), GetWidth());
		}
	}
}