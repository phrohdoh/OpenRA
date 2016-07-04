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

using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.FileFormats;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	public class PaletteFromJascInfo : ITraitInfo
	{
		[FieldLoader.Require, PaletteDefinition]
		[Desc("Internal palette name")]
		public readonly string Name = null;

		[FieldLoader.Require]
		[Desc("Filename to load")]
		public readonly string Filename = null;

		public readonly bool AllowModifiers = true;

		[Desc("If a palette with the same name has already been added can this palette overwrite it?")]
		public readonly bool OverwriteDuplicates = true;

		public object Create(ActorInitializer init) { return new PaletteFromJasc(this); }
	}

	public class PaletteFromJasc : ILoadsPalettes, IProvidesAssetBrowserPalettes
	{
		readonly PaletteFromJascInfo info;
		public PaletteFromJasc(PaletteFromJascInfo info) { this.info = info; }

		IEnumerable<string> IProvidesAssetBrowserPalettes.PaletteNames { get { yield return info.Name; } }

		void ILoadsPalettes.LoadPalettes(WorldRenderer wr)
		{
			string[] lines;
			using (var s = wr.World.Map.Open(info.Filename))
				lines = s.ReadAllLines().ToArray();

			var colors = JascPalReader.FromLines(lines);
			wr.AddPalette(info.Name, new ImmutablePalette(colors), info.AllowModifiers, info.OverwriteDuplicates);
		}
	}
}