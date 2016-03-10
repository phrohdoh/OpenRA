using System;
using OpenRA.Graphics;

namespace OpenRA.Mods.Common
{
	public class GetPalRgbCommand : IUtilityCommand
	{
		string IUtilityCommand.Name { get { return "--pal-rgb"; } }

		void IUtilityCommand.Run(ModData modData, string[] args)
		{
			var filename = args[1];
			var pal = new ImmutablePalette(filename, new int[0]);
			for (var i = 0; i < 255; i++)
			{
				var c = pal.GetColor(i);
				Console.WriteLine("{{r: {0}, g: {1}, b: {2}}}", c.R, c.G, c.B);
			}
		}

		bool IUtilityCommand.ValidateArguments(string[] args)
		{
			return args.Length == 2;
		}
	}
}