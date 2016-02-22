using System;

namespace OpenRA.Mods.Common.UtilityCommands
{
	public class EchoMiniYaml : IUtilityCommand
	{
		string IUtilityCommand.Name { get { return "--echo"; } }

		void IUtilityCommand.Run(ModData modData, string[] args)
		{
			var filename = args[1];
			var my = MiniYaml.FromFile(filename);
			foreach (var line in my.ToLines(false))
				Console.WriteLine(line);
		}

		bool IUtilityCommand.ValidateArguments(string[] args)
		{
			return args.Length == 2;
		}
	}
}
