using System;
using System.Collections.Generic;
using System.Drawing;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets.UIDesigner.Logic
{
	static class WidgetExts
	{
		public static T GetOrThrow<T>(this Widget parent, string childId) where T : Widget
		{
			if (parent == null)
				throw new NullReferenceException("'parent' widget is null. childId = " + childId);

			var ret = parent.GetOrNull<T>(childId);
			if (ret == null)
				throw new Exception("Could not find widget '{0}' as a child of '{1}'!".F(childId, parent.Id));

			return ret;
		}
	}

	public class UIDesignerLogic : ChromeLogic
	{
		BackgroundWidget initialBackground;

		//ContainerWidget designRoot;
		//BackgroundWidget toolsRoot;
		InvisibleBackgroundWidget mouseHandler;

		[ObjectCreator.UseCtor]
		public UIDesignerLogic(Widget widget, Action onExit, Dictionary<string, MiniYaml> logicArgs)
		{
			var deadSpaceBorder = widget.GetOrThrow<ColorBlockWidget>("DEAD_SPACE");
			deadSpaceBorder.GetColor = () => Color.Black;

			var deadSpaceStroke = widget.GetOrThrow<ColorRectWidget>("DEAD_SPACE_STROKE");
			if (deadSpaceStroke != null) { }

			SetupDesignSpace(deadSpaceBorder.GetOrThrow<ContainerWidget>("UI_DESIGN_SPACE"));
			SetupTools(widget.GetOrThrow<BackgroundWidget>("TOOLS_BACKGROUND"));
		}

		void SetupDesignSpace(ContainerWidget designRoot)
		{
			initialBackground = designRoot.GetOrThrow<BackgroundWidget>("BUTTON_CONTAINER");
			mouseHandler = designRoot.GetOrThrow<InvisibleBackgroundWidget>("MOUSE_HANDLER");
			mouseHandler.OnMouseInput += MouseInputOnDesignSpace;
		}

		void SetupTools(BackgroundWidget toolsRoot)
		{
			var lblTools = toolsRoot.GetOrThrow<LabelWidget>("TOOLS_LABEL");
			var lblToolsParent = lblTools.Parent;
			lblTools.Text = lblToolsParent == null ? "Parent is null" : lblToolsParent.Id;
		}

		void MouseInputOnDesignSpace(MouseInput mi)
		{
			if (mi.Event == MouseInputEvent.Up)
				Console.WriteLine("MouseUp({0})", mi.Location);
			else if (mi.Event == MouseInputEvent.Down)
				Console.WriteLine("MouseDown({0})", mi.Location);

			if (initialBackground == null) { }
		}
	}
}