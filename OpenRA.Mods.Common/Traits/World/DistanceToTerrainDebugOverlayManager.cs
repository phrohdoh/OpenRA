using OpenRA.Graphics;
using OpenRA.Mods.Common.Commands;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	public class DistanceToTerrainDebugOverlayManagerInfo : ITraitInfo
	{
		[Desc("The font used to render the world units above/below terrain.",
			"Should match the value as-is in the Fonts section of the mod manifest (do not convert to lowercase).")]
		public readonly string Font = "Small";

		object ITraitInfo.Create(ActorInitializer init) => new DistanceToTerrainDebugOverlayManager(init.Self, this);
	}

	public class DistanceToTerrainDebugOverlayManager : INotifyCreated, IChatCommand
	{
		const string CommandName = "dist-terrain-overlay";
		const string CommandHelp = "Displays distance above/below terrain.";

		public readonly SpriteFont Font;
		public bool Enabled;

		public DistanceToTerrainDebugOverlayManager(Actor self, DistanceToTerrainDebugOverlayManagerInfo info)
		{
			if (!Game.Renderer.Fonts.TryGetValue(info.Font, out Font))
				throw new YamlException("Could not find font '{0}'".F(info.Font));
		}

		void INotifyCreated.Created(Actor self)
		{
			// This lives on the world actor.
			var console = self.TraitOrDefault<ChatCommands>();
			var help = self.TraitOrDefault<HelpCommand>();
			
			if (console == null || help == null)
				return;
			
			console.RegisterCommand(CommandName, this);
			help.RegisterHelp(CommandName, CommandHelp);
		}

		void IChatCommand.InvokeCommand(string command, string arg)
		{
			if (command == CommandName)
				Enabled ^= true;
		}
	}
}