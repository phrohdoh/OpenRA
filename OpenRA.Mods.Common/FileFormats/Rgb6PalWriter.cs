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

using System.IO;
using System.Drawing;

namespace OpenRA.Mods.Common
{
	public static class Rgb6PalWriter
	{
		public static void ToFile(uint[] colors, string filename)
		{
			using (var file = File.Create(filename))
			{
				foreach (var color in colors)
				{
					var c = Color.FromArgb((int)color);
					file.WriteByte((byte)(c.R >> 2));
					file.WriteByte((byte)(c.G >> 2));
					file.WriteByte((byte)(c.B >> 2));
				}
			}
		}

		public static void ToTextWriter(uint[] colors, TextWriter writer)
		{
			foreach (var color in colors)
			{
				var c = Color.FromArgb((int)color);
				writer.WriteLine("{0} {1} {2}".F(c.R, c.G, c.B));
			}
		}
	}
}