using System;
using System.IO;
using System.Linq;

namespace OpenRA.Mods.Common.UtilityCommands
{
	class InstallMapOptions : IOptionObject
	{
		[Option("overwrite", "Overwrite an existing map in the event of a naming conflict.")]
		public bool ShouldOverwriteExisting { get; private set; }

		[Option("dry-run", "Dry run -- do not actually save to disk.")]
		public bool DryRun { get; private set; }

		[Option("verbose", "Give me more information than necessary!")]
		public bool Verbose { get; private set; }
	}

	public class InstallMapCommand : IUtilityCommand
	{
		string IUtilityCommand.Name => "--install-map";

		[Desc("id1,id2,id3,idN [options]", "Download a given range of map IDs")]
		void IUtilityCommand.Run(Utility utility, string[] args)
		{
			// HACK: The engine code assumes that Game.ModData is set.
			Game.ModData = utility.ModData;

			var options = new InstallMapOptions();
			if (!OptionsUtils.ParseOptions(args.Skip(1), options))
			{
				Console.WriteLine("Failed to parse arguments.");
				Environment.Exit(1);
			}

			var mapIdsArg = args[1];
			var mapIds = mapIdsArg.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			var json = MapApiUtils.GetJsonForMapIds(mapIds);
			var maps = MapApiUtils.GetMapObjectsFromJson(json);

			foreach (var map in maps)
			{
				var filename = MapApiUtils.GetValidFilenameIncludingHash(map) + ".oramap";
				var destinationDir = MapApiUtils.GetPathForModMaps(map.GameMod, map.Parser);
				var absolutePath = Path.Combine(destinationDir, filename);

				if (!options.ShouldOverwriteExisting && File.Exists(absolutePath))
				{
					Console.Write($"{filename} already exists. Overwrite? [y/N] ");
					var input = Console.ReadLine().ToLowerInvariant();
					if (input != "y")
						continue;
				}

				var absolutePathOutput = absolutePath.Contains(" ") ? string.Concat("\"", absolutePath, "\"") : absolutePath;

				if (options.DryRun)
					Console.WriteLine($"Would have saved '{map.Title}' to:{Environment.NewLine}{absolutePathOutput}");
				else
				{
					MapApiUtils.WriteMapToAbsolutePath(map, absolutePath);
					Console.WriteLine($"Saved '{map.Title}' to:{Environment.NewLine}{absolutePathOutput}");
				}
			}
		}

		bool IUtilityCommand.ValidateArguments(string[] args)
		{
			return args.Length > 1;
		}
	}
}
