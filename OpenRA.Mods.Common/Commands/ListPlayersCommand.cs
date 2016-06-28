#region Copyright & License Information
/*
 * Copyright 2007-2016 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Commands
{
	public class ListPlayersCommandInfo : TraitInfo<ListPlayersCommand> { }

	public class ListPlayersCommand : IChatCommand, IWorldLoaded
	{
		World world;

		public void WorldLoaded(World w, WorldRenderer wr)
		{
			world = w;
			var console = world.WorldActor.Trait<ChatCommands>();
			var help = world.WorldActor.Trait<HelpCommand>();

			console.RegisterCommand("list-players", this);
			help.RegisterHelp("list-players", "list internal and player-friendly player names");
		}

		public void InvokeCommand(string name, string arg)
		{
			if (name != "list-players")
				return;

			foreach (var player in world.Players)
				Game.Debug($"{player.InternalName}: {player.PlayerName}");
		}
	}
}