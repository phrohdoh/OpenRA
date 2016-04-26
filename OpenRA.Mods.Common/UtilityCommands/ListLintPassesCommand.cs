using System;
using System.Linq;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.UtilityCommands
{
	public class ListLintPassesCommand : IUtilityCommand
	{
		string IUtilityCommand.Name { get { return "--list-lint-passes"; } }

		void IUtilityCommand.Run(ModData modData, string[] args)
		{
			var lintPassTypes = modData.ObjectCreator.GetTypesImplementing<ILintPass>().ToArray();
			var lintRulePassTypes = modData.ObjectCreator.GetTypesImplementing<ILintRulesPass>().ToArray();
			var lintMapPassTypes = modData.ObjectCreator.GetTypesImplementing<ILintMapPass>().ToArray();
			var allLintTypes = lintPassTypes.Append(lintRulePassTypes).Append(lintMapPassTypes);

			foreach (var type in allLintTypes)
				Console.WriteLine(type.Name);
		}

		bool IUtilityCommand.ValidateArguments(string[] args) { return true; }
	}
}