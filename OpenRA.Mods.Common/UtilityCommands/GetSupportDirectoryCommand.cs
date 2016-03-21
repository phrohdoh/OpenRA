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

namespace OpenRA.Mods.Common.UtilityCommands
{
	class GetSuportDirectoryCommand : IUtilityCommand
	{
		string IUtilityCommand.Name { get { return "--support-dir"; } }

		bool IUtilityCommand.ValidateArguments(string[] args) { return true; }

		[Desc("Outputs the absolute path to your OpenRA support directory.")]
		void IUtilityCommand.Run(ModData modData, string[] args)
		{
			Console.WriteLine(Platform.SupportDir);
		}
	}
}
