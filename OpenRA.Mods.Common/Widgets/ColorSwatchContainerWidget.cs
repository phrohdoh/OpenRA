using System.Collections.Generic;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets
{
	public class ColorSwatchContainerWidget : ContainerWidget
	{
		public List<ColorSwatchWidget> Swatches = new List<ColorSwatchWidget>();

		[ObjectCreator.UseCtor]
		public ColorSwatchContainerWidget() { }

		protected ColorSwatchContainerWidget(ColorSwatchContainerWidget other)
			: base(other)
		{

		}

		public override Widget Clone() { return new ColorSwatchContainerWidget(this); }
	}
}