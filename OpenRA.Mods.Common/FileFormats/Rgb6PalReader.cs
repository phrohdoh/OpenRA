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

namespace OpenRA.Mods.Common.FileFormats
{
	public static class Rgb6PalReader
	{
		static void Throw(string message)
		{
			throw new InvalidDataException("{0}: {1}".F(nameof(Rgb6PalReader), message));
		}

		public static uint[] FromFile(string filename)
		{
			using (var file = File.OpenRead(filename))
				return FromStream(file);
		}

		public static uint[] FromStream(Stream s)
		{
			var colors = new uint[s.Length / 3];
			using (var reader = new BinaryReader(s))
				for (var i = 0; i < colors.Length; i++)
				{
					var r = (byte)(reader.ReadByte() << 2);
					var g = (byte)(reader.ReadByte() << 2);
					var b = (byte)(reader.ReadByte() << 2);
					colors[i] = (uint)((255 << 24) | (r << 16) | (g << 8) | b);
				}

			return colors;
		}
	}
}