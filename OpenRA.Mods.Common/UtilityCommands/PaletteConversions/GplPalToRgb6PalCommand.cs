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
using System.IO;
using System.Linq;
using OpenRA.Mods.Common.FileFormats;

namespace OpenRA.Mods.Common.UtilifyCommands
{
	public class GplPalToRgb6PalCommand : IUtilityCommand
	{
		string IUtilityCommand.Name => "--gpl-to-rgb6";

		Arguments a;
		class Arguments
		{
			public bool ShouldSendToStdout;
			public string InputFilename;
			public string OutputFilename;
		}

		[Desc("in.gpl [out.pal OR --to-stdout]", "Converts a GIMP palette to a 6bit VGA palette.")]
		void IUtilityCommand.Run(ModData modData, string[] args)
		{
			var outputFilename = a.OutputFilename ?? Path.GetFileNameWithoutExtension(a.InputFilename);

			var colors = GplPalReader.FromFile(a.InputFilename);
			if (colors.Length == 0)
			{
				Console.WriteLine("Failed to read colors from {0}".F(a.InputFilename));
				Environment.Exit(1);
			}

			if (a.ShouldSendToStdout)
			{
				Rgb6PalWriter.ToTextWriter(colors, Console.Out);
				return;
			}

			Rgb6PalWriter.ToFile(colors, outputFilename);
			Console.WriteLine("Wrote to {0}".F(outputFilename));
		}

		bool IUtilityCommand.ValidateArguments(string[] args)
		{
			args = args.Skip(1).ToArray();
			if (args.Length == 0)
				return false;

			a = new Arguments();

			if (args.Contains("--to-stdout"))
			{
				a.ShouldSendToStdout = true;
				args = args.Where(arg => arg != "--to-stdout").ToArray();
			}

			if (args.Length == 0)
				return false;

			a.InputFilename = args[0];
			if (args.Length > 1)
				a.OutputFilename = args[1];

			return true;
		}
	}
}
