#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using OpenRA.FileSystem;
using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	class PaletteFromJascFileInfo : ITraitInfo
	{
		[FieldLoader.Require, PaletteDefinition]
		[Desc("Internal palette name.")]
		public readonly string Name = null;

		[Desc("If defined, load the palette only for this tileset.")]
		public readonly string Tileset = null;

		[FieldLoader.Require]
		[Desc("Name of the file to load.")]
		public readonly string Filename = null;

		[Desc("Map listed indices to shadow. Ignores previous color.")]
		public readonly int[] ShadowIndex = { };

		public readonly bool AllowModifiers = true;

		public object Create(ActorInitializer init) { return new PaletteFromJascFile(init.World, this); }
	}

	class PaletteFromJascFile : ILoadsPalettes, IProvidesAssetBrowserPalettes
	{
		readonly World world;
		readonly PaletteFromJascFileInfo info;
		public PaletteFromJascFile(World world, PaletteFromJascFileInfo info)
		{
			this.world = world;
			this.info = info;
		}

		public void LoadPalettes(WorldRenderer wr)
		{
			var colors = new uint[Palette.Size];
			using (var s = GlobalFileSystem.Open(info.Filename))
			{
				var lines = s.ReadAllLines().ToArray();
				if (lines[0] != "JASC-PAL")
					throw new InvalidDataException("File {0} is not a valid JASC platte!".F(info.Filename));
				
				for (var i = 0; i < Palette.Size; i++)
				{
					var split = lines[i + 3].Split(' ');
					colors[i] = (uint)Color.FromArgb(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2])).ToArgb();
					if (lines[i + 3] == "0 0 0")
						colors[i] = 0;
				}
			}

			wr.AddPalette(info.Name, new ImmutablePalette(colors), info.AllowModifiers);
		}

		public IEnumerable<string> PaletteNames
		{
			get
			{
				// Only expose the palette if it is available for the shellmap's tileset (which is a requirement for its use).
				if (info.Tileset == null || info.Tileset == world.TileSet.Id)
					yield return info.Name;
			}
		}
	}
}
