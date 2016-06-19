using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenRA.FileSystem;

namespace OpenRA.Mods.Common
{
	public class ListTraitUsesCommand : IUtilityCommand
	{
		const string GitHubFormatString = "--github-format";
		const string AppendTraitKey = "--append-trait-key";

		struct Arguments
		{
			internal readonly IEnumerable<string> TraitNames;
			internal readonly IEnumerable<string> Flags;
			internal readonly string FormatString;
			internal readonly bool AppendTraitNameToFormatString;

			internal Arguments(IEnumerable<string> traitNames, IEnumerable<string> flags, string formatString, bool appendTraitNameToFormatString)
			{
				TraitNames = traitNames;
				Flags = flags;
				FormatString = formatString;
				AppendTraitNameToFormatString = appendTraitNameToFormatString;
			}
		}

		Arguments argObject;

		string IUtilityCommand.Name => "--list-trait-uses";
		bool IUtilityCommand.ValidateArguments(string[] args)
		{
			if (args.Length < 2)
				return false;

			var traitNames = args.Where(str => !str.StartsWith("--"));
			if (!traitNames.Any())
				return false;

			var flags = args.Except(traitNames);
			var formatString = flags.Contains(GitHubFormatString) ? "{0}#L{1}" : "{0}:{1}";

			argObject = new Arguments(traitNames, flags, formatString, flags.Contains(AppendTraitKey));
			return true;
		}

		[Desc("[flag1 [flag2 [flagN]]] trait1 [trait2 [traitN]]",
			"List usages of all given traits (filenames and line numbers).")]
		void IUtilityCommand.Run(ModData modData, string[] args)
		{
			Game.ModData = modData;

			var ruleFiles = modData.Manifest.Rules;
			var pathStrStart = Path.Combine("mods", modData.Manifest.Mod.Id);

			foreach (var ruleFile in ruleFiles)
			{
				string name;
				IReadOnlyPackage package;

				if (!modData.ModFiles.TryGetPackageContaining(ruleFile, out package, out name))
					continue;

				var topLevelNodes = MiniYaml.FromStream(package.GetStream(name), name);

				foreach (var topLevelNode in topLevelNodes)
				{
					var matchingTraitNodes = topLevelNode.Value.Nodes.Where(n => argObject.TraitNames.Contains(n.Key.Split(new[] { '@' }, 2)[0]));

					foreach (var traitNode in matchingTraitNodes)
					{
						var output = argObject.FormatString.F(Path.Combine(pathStrStart, traitNode.Location.Filename), traitNode.Location.Line);

						if (argObject.AppendTraitNameToFormatString)
							output = string.Concat(output, " ", traitNode.Key);

						Console.WriteLine(output);
					}
				}
			}
		}
	}
}