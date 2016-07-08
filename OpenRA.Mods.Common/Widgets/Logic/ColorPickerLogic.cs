#region Copyright & License Information
/*
 * Copyright 2007-2016 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Widgets;
using System.Linq;
using System.Drawing;

namespace OpenRA.Mods.Common.Widgets.Logic
{
	public class ColorPickerLogic : ChromeLogic
	{
		enum PanelType
		{
			Mixer,
			Swatches,
		}

		PanelType panel = PanelType.Mixer;

		[ObjectCreator.UseCtor]
		public ColorPickerLogic(Widget widget, ModData modData, World world, HSLColor initialColor, Action<HSLColor> onChange, WorldRenderer worldRenderer)
		{
			string actorType;
			if (!ChromeMetrics.TryGet("ColorPickerActorType", out actorType))
				actorType = "mcv";

			var preview = widget.GetOrNull<ActorPreviewWidget>("PREVIEW");
			var actor = world.Map.Rules.Actors[actorType];

			var td = new TypeDictionary();
			td.Add(new HideBibPreviewInit());
			td.Add(new OwnerInit(world.WorldActor.Owner));
			td.Add(new FactionInit(world.WorldActor.Owner.PlayerReference.Faction));

			if (preview != null)
				preview.SetPreview(actor, td);

			var hueSlider = widget.Get<SliderWidget>("HUE");
			var mixer = widget.Get<ColorMixerWidget>("MIXER");

			hueSlider.IsVisible = () => panel == PanelType.Mixer;
			mixer.IsVisible = () => panel == PanelType.Mixer;

			var mixerButton = widget.GetOrNull<ButtonWidget>("MIXER_TAB_BUTTON");
			if (mixerButton != null)
				mixerButton.OnClick = () => panel = PanelType.Mixer;

			var swatchesButton = widget.GetOrNull<ButtonWidget>("SWATCHES_TAB_BUTTON");
			if (swatchesButton != null)
				swatchesButton.OnClick = () => panel = PanelType.Swatches;

			var randomButton = widget.GetOrNull<ButtonWidget>("RANDOM_BUTTON");
			if (randomButton != null)
				randomButton.IsVisible = () => panel == PanelType.Mixer;

			hueSlider.OnChange += _ => mixer.Set(hueSlider.Value);
			mixer.OnChange += () => onChange(mixer.Color);

			hueSlider.Parent.IsVisible = () => panel == PanelType.Mixer;
			mixer.Parent.IsVisible = () => panel == PanelType.Mixer;

			if (randomButton != null)
				randomButton.OnClick = () =>
				{
					// Avoid colors with low sat or lum
					var hue = (byte)Game.CosmeticRandom.Next(255);
					var sat = (byte)Game.CosmeticRandom.Next(70, 255);
					var lum = (byte)Game.CosmeticRandom.Next(70, 255);

					mixer.Set(new HSLColor(hue, sat, lum));
					hueSlider.Value = hue / 255f;
				};

			var hexInput = widget.GetOrNull<TextFieldWidget>("HEX_VALUE");
			if (hexInput != null)
			{
				hexInput.IsVisible = () => panel == PanelType.Swatches;
				hexInput.OnTextEdited = () =>
				{
					var text = hexInput.Text;
					if (text.Length != 6)
						return;

					var color = System.Drawing.ColorTranslator.FromHtml("#" + text);
					var hsl = new HSLColor(color);
					hueSlider.Value = hsl.H / 255f;
					mixer.Set(hsl);
					hexInput.Text = mixer.Color.ToHexString();
				};

				hexInput.OnEscKey = hexInput.YieldKeyboardFocus;
				hexInput.OnEnterKey = hexInput.YieldKeyboardFocus;
				hexInput.OnEscKey = hexInput.YieldKeyboardFocus;

				mixer.OnChange += () =>
				{
					hexInput.YieldKeyboardFocus();
					hexInput.Text = mixer.Color.ToHexString();
				};
			}

			var validator = modData.Manifest.Get<ColorValidator>();

			var predeterminedColorSwatchContainer = widget.GetOrNull<ColorSwatchContainerWidget>("PREDETERMINED_COLORS");
			if (predeterminedColorSwatchContainer != null)
			{
				predeterminedColorSwatchContainer.IsVisible = () => panel == PanelType.Swatches;

				ColorSwatchWidget last = null;
				foreach (var color in validator.PredeterminedPlayerColors)
				{
					var swatch = new ColorSwatchWidget(modData);
					predeterminedColorSwatchContainer.Children.Add(swatch);

					swatch.Color = color;
					swatch.OnClick = () => mixer.Set(swatch.Color);

					var offsetRect = predeterminedColorSwatchContainer.RenderBounds;
					offsetRect.Offset(5, 5);

					if (last == null)
						swatch.Bounds = Rectangle.FromLTRB(predeterminedColorSwatchContainer.Bounds.X + 5,
						                                   predeterminedColorSwatchContainer.Bounds.Y + 5,
						                                   predeterminedColorSwatchContainer.Bounds.X + 25 + 5,
						                                   predeterminedColorSwatchContainer.Bounds.Y + 25 + 5);

					else if (last.Bounds != Rectangle.Empty)
						swatch.Bounds = Rectangle.FromLTRB(last.Bounds.X + 5, 5, last.Bounds.X + 25 + 5, 25 + 5);

					last = swatch;
				}
			}

			//var swatches = widget.Children.Where(w => w is ColorSwatchWidget).Cast<ColorSwatchWidget>().ToArray();
			//for (var i = 0; i < swatches.Length; i++)
			//{
			//	var swatch = swatches[i];
			//	var savedColor = Game.Settings.Player.SavedColors[i];
			//	swatch.Color = savedColor;
			//	swatch.IsVisible = () => panel == PanelType.Swatches;
			//	swatch.OnClick = () => mixer.Set(swatch.Color);
			//}

			// Set the initial state
			mixer.SetPaletteRange(validator.HsvSaturationRange[0], validator.HsvSaturationRange[1], validator.HsvValueRange[0], validator.HsvValueRange[1]);
			mixer.Set(initialColor);

			hueSlider.Value = initialColor.H / 255f;
			onChange(mixer.Color);
		}

		public static void ShowColorDropDown(DropDownButtonWidget color, ColorPreviewManagerWidget preview, World world)
		{
			Action onExit = () =>
			{
				Game.Settings.Player.Color = preview.Color;
				Game.Settings.Save();
			};

			color.RemovePanel();

			Action<HSLColor> onChange = c => preview.Color = c;

			var colorChooser = Game.LoadWidget(world, "COLOR_CHOOSER", null, new WidgetArgs()
			{
				{ "onChange", onChange },
				{ "initialColor", Game.Settings.Player.Color }
			});

			color.AttachPanel(colorChooser, onExit);
		}
	}
}