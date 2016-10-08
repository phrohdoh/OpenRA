using System;
using System.Linq;

namespace OpenRA.Mods.Common.UtilityCommands
{
	public class ExtractContentFilesCommand : IUtilityCommand
	{
		string IUtilityCommand.Name => "--extract-content-files";
		bool IUtilityCommand.ValidateArguments(string[] args) => true;

		void IUtilityCommand.Run(Utility utility, string[] args)
		{
			if (!utility.ModData.Manifest.Contains<ModContent>())
			{
				Console.WriteLine($"The manifest for {utility.ModData.Manifest.Id} does not define ModContent.");
				Environment.Exit(1);
			}

			var modContent = utility.ModData.Manifest.Get<ModContent>();
			var firstDownload = modContent.Downloads.FirstOrDefault();
			if (firstDownload == null)
			{
				Console.WriteLine($"ModContent.Downloads for {utility.ModData.Manifest.Id} is empty.");
				Environment.Exit(2);
			}

			var downloadYaml = MiniYaml.Load(utility.ModData.DefaultFileSystem, modContent.Downloads, null);
			foreach (var d in downloadYaml)
				Console.WriteLine($"{d.Key}: {d.Value}");
		}
	}
}