using System.IO;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Commands;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Common
{
	public class TerritoryOwnershipDebugOverlayInfo : ITraitInfo, Requires<TerritoryOwnershipManagerInfo>
	{
		public object Create(ActorInitializer init) { return new TerritoryOwnershipDebugOverlay(init, this); }
	}

	public class TerritoryOwnershipDebugOverlay : IWorldLoaded, IPostRender, IChatCommand, INotifyCreated
	{
		const string CommandName = "territory-overlay";
		const string CommandDesc = "Toggles the territory-ownership overlay";

		readonly World world;

		Map map;
		TerritoryOwnershipManager manager;

		SpriteFont font;
		bool enabled;

		Player[] trackedPlayers;

		public TerritoryOwnershipDebugOverlay(ActorInitializer init, TerritoryOwnershipDebugOverlayInfo info)
		{
			world = init.World;
		}

		void IWorldLoaded.WorldLoaded(World w, WorldRenderer wr)
		{
			var console = w.WorldActor.TraitOrDefault<ChatCommands>();
			var help = w.WorldActor.TraitOrDefault<HelpCommand>();

			if (console == null || help == null)
				return;

			console.RegisterCommand(CommandName, this);
			help.RegisterHelp(CommandName, CommandDesc);
		}

		void IPostRender.RenderAfterWorld(WorldRenderer wr, Actor self)
		{
			if (!enabled || manager == null)
				return;

			if (font == null && !Game.Renderer.Fonts.TryGetValue("TinyBold", out font))
				throw new InvalidDataException("Could not load default font TinyBold.");

			foreach (var player in trackedPlayers)
			{
				//if (world.RenderPlayer == null || player.Stances[world.RenderPlayer] == Stance.Ally)
				//{
					foreach (var cell in manager.GetOwnedCells(player))
					{
						var text = new TextRenderable(font, map.CenterOfCell(cell), 0, player.Color.RGB, manager.GetValue(player, cell).ToString());
						text.Render(wr);
					}
				//}
			}
		}

		void IChatCommand.InvokeCommand(string command, string arg)
		{
			if (command == CommandName)
				enabled ^= true;
		}

		void INotifyCreated.Created(Actor self)
		{
			self.World.AddFrameEndTask(w =>
			{
				map = world.Map;
				manager = world.WorldActor.Trait<TerritoryOwnershipManager>();
				trackedPlayers = manager.GetTrackedPlayers();
			});
		}
	}
}