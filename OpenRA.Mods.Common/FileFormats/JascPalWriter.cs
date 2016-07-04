using System.IO;

namespace OpenRA.Mods.Common.FileFormats
{
	public static class JascPalWriter
	{
		public static void ToTextWriter(uint[] colors, TextWriter writer)
		{
			writer.WriteLine("JASC-PAL");
			writer.WriteLine("0100");
			writer.WriteLine(colors.Length);

			foreach (var color in colors)
			{
				var r = (color >> 16) & 0xff;
				var g = (color >>  8) & 0xff;
				var b = (color >>  0) & 0xff;

				writer.WriteLine("{0} {1} {2}".F(r, g, b));
			}
		}
	}
}