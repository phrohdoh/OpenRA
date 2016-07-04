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

namespace OpenRA.Mods.Common.FileFormats
{
	public static class JascPalReader
	{
		const int HeaderLineLength = 3;

		static void Throw(string message)
		{
			throw new InvalidDataException("{0}: {1}".F(nameof(GplPalReader), message));
		}

		public static bool FromLines(string[] lines, out uint[] colors)
		{
			colors = new uint[0];

			if (lines.Length < HeaderLineLength + 1)
				Throw("Could not load palette from incomplete file.");

			var identifier = lines[0];
			if (!string.Equals(identifier, "JASC-PAL", StringComparison.InvariantCultureIgnoreCase))
				Throw("Expected identifier JASC-PAL but found '{0}'.".F(identifier));

			var version = lines[1];
			int ver;

			if (!int.TryParse(version, out ver) || ver != 100)
				Throw("Expected version 0100 but found '{0}'.".F(version));

			var lengthStr = lines[2];
			var length = uint.MaxValue;

			if (!uint.TryParse(lengthStr, out length) || length == 0)
				Throw(length == 0 ? "A zero-length palette is invalid." : "Could not parse palette length from '{0}'.".F(lengthStr));

			if (length > 256)
				Throw("Maximum supported entry count is 256. This file has {0}.".F(length));

			length = (uint)Math.Min(length, lines.Skip(3).Count());
			colors = new uint[length];

			for (int lineIndex = HeaderLineLength, colorIndex = 0; lineIndex < length + HeaderLineLength; lineIndex++)
			{
				byte r = 0;
				byte g = 0;
				byte b = 0;

				var entries = lines[lineIndex].Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Take(3).ToArray();
				if (entries.Length != 3)
					Throw("Line {0} is invalid (must contain exactly 3 space-delimited values).".F(lineIndex + 1));

				for (var ei = 0; ei < entries.Length; ei++)
				{
					var entry = entries[ei];

					if (ei == 0 && !byte.TryParse(entry, out r))
						Throw("Color {0}'s R value '{1}' could not be parsed into a byte.".F(colorIndex, entry));

					if (ei == 1 && !byte.TryParse(entry, out g))
						Throw("Color {0}'s G value '{1}' could not be parsed into a byte.".F(colorIndex, entry));

					if (ei == 2 && !byte.TryParse(entry, out b))
						Throw("Color {0}'s B value '{1}' could not be parsed into a byte.".F(colorIndex, entry));
				}

				colors[colorIndex++] = (uint)((255 << 24) | (r << 16) | (g << 8) | b);
			}

			return true;
		}

		public static bool FromFile(string filename, out uint[] colors)
		{
			var lines = File.ReadAllLines(filename);
			return FromLines(lines, out colors);
		}
	}
}
