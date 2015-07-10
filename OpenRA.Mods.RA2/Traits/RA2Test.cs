#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using OpenRA.Traits;

namespace OpenRA.Mods.RA2.Traits
{
	public class RA2TestInfo : ITraitInfo
	{
		public object Create(ActorInitializer init) { return new RA2Test(init.Self, this); }
	}

	public class RA2Test
	{
		public RA2Test(Actor self, RA2TestInfo info)
		{
			Game.Debug("Created ra2test for {0}", self.Info.Name);
		}
	}
}