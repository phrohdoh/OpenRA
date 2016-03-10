using System;
using System.Linq;
using System.IO;
using OpenRA.Graphics;
using OpenRA.Mods.Common.SpriteLoaders;

namespace OpenRA.Mods.TS
{
	public class GetShpDataCommand : IUtilityCommand
	{
		string IUtilityCommand.Name { get { return "--shp-data"; } }

		void IUtilityCommand.Run(ModData modData, string[] args)
		{
			var filename = args[1];
			var frames = new ISpriteFrame[0];

			using (var s = File.OpenRead(filename))
			{
				var loader = modData.SpriteLoaders.FirstOrDefault(l => l.TryParseSprite(s, out frames));
				if (loader == null)
					Environment.Exit(1);
			}

			for (var i = 0; i < frames.Length; i++)
			{
				var frame = frames[i];
				Console.WriteLine("## frame: {0}", i);
				Console.WriteLine((frame.Data ?? new byte[frame.Size.Width * frame.Size.Height]).JoinWith(","));
			}
		}

		bool IUtilityCommand.ValidateArguments(string[] args)
		{
			return args.Length == 2;
		}
	}
}
