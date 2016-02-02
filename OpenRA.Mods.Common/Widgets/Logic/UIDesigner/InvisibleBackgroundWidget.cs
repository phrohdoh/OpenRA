using System;
using OpenRA.Mods.Common.Widgets;

namespace OpenRA.Mods.Common
{
	public class InvisibleBackgroundWidget : BackgroundWidget
	{
		public Action<MouseInput> OnMouseInput = _ => { };

		public override void Draw()
		{
			/*base.Draw ();*/
		}

		public override void DrawOuter ()
		{
			/*base.DrawOuter ();*/
		}

		public override bool HandleMouseInput(MouseInput mi)
		{
			OnMouseInput(mi);
			return base.HandleMouseInput(mi);
		}
	}
}