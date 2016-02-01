using System;
using System.Collections.Generic;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets.Logic
{
	static class WidgetExts
	{
		public static T GetOrThrow<T>(this Widget parent, string childId) where T : Widget
		{
			var ret = parent.GetOrNull<T>(childId);
			if (ret == null)
				throw new Exception("Could not find widget '{0}' as a child of '{1}'!".F(childId, parent.Id));

			return ret;
		}
	}

	public class UIDesignerLogic : ChromeLogic
	{
		ScrollPanelWidget scrollPanel;
		ScrollItemWidget scrollItemTemplate;

		[ObjectCreator.UseCtor]
		public UIDesignerLogic(Widget widget, Action onExit, World world, Dictionary<string, MiniYaml> logicArgs)
		{
			SetupTools(widget.GetOrThrow<BackgroundWidget>("TOOLS_BACKGROUND"));
			SetupDesignSpace(widget.GetOrThrow<ContainerWidget>("UI_DESIGN_SPACE"));
		}

		void SetupDesignSpace(ContainerWidget designRoot)
		{
			scrollPanel = designRoot.GetOrThrow<ScrollPanelWidget>("BUTTON_CONTAINER");
			scrollPanel.Layout = new GridLayout(scrollPanel);
			scrollItemTemplate = scrollPanel.GetOrThrow<ScrollItemWidget>("TEMPLATE");
		}

		void SetupTools(BackgroundWidget toolsRoot)
		{
			var hueBG = toolsRoot.GetOrThrow<BackgroundWidget>("HUE_BG");
			var hueSlider = hueBG.GetOrThrow<HueSliderWidget>("HUE_SLIDER");

			var scrollItemCreatorButton = toolsRoot.GetOrThrow<ButtonWidget>("BUTTON_CREATOR");
			scrollItemCreatorButton.OnClick = () =>
			{
				var scrollItem = ScrollItemWidget.Setup(scrollItemTemplate, () => false, () => { });
				scrollItem.GetOrThrow<LabelWidget>("TITLE").GetText = () => scrollItem.Id;
				scrollPanel.AddChild(scrollItem);
			};

			if (hueSlider != null)
				hueSlider.OnChange += (float newValue) =>
				{
				foreach (var item in scrollPanel.Children)
					{
						foreach (var item2 in item.Children)
						{
							var lbl = item2 as LabelWidget;
							if (lbl == null)
								continue;

							lbl.GetText = () => newValue.ToString();
						}
					}
				};
		}
	}
}