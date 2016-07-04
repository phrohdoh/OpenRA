using System.IO;

namespace OpenRA.Mods.Common.FileFormats
{
	public static class GplPalWriter
	{
		public static void ToTextWriter(uint[] colors, TextWriter writer, string name = "<no name>")
		{
			writer.WriteLine("GIMP Palette");
			writer.WriteLine("Name: {0}".F(name));
			writer.WriteLine("Columns: {0}".F(colors.Length));
			writer.WriteLine("#");

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