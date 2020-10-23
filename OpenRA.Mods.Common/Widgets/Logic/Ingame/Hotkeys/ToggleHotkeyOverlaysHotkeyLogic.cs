#region Copyright & License Information
/*
 * Copyright 2020 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using OpenRA.Mods.Common.Lint;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets.Logic.Ingame
{
	[ChromeLogicArgsHotkeys("ToggleHotkeyOverlays")]
	public class ToggleHotkeyOverlaysHotkeyLogic : SingleHotkeyBaseLogic
	{
		[ObjectCreator.UseCtor]
		public ToggleHotkeyOverlaysHotkeyLogic(Widget widget, ModData modData, Dictionary<string, MiniYaml> logicArgs)
			: base(widget, modData, "ToggleHotkeyOverlaysKey", "WORLD_KEYHANDLER", logicArgs) { }

		protected override bool OnHotkeyActivated(KeyInput e)
		{
			Game.Settings.Game.DisplayHotkeyOverlays ^= true;
			return true;
		}
	}
}

