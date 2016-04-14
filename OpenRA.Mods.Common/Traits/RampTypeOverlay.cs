using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Commands;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.Common
{
	[Desc("Renders a debug overlay showing the ramp-type for all ramps. Attach this to the world actor.")]
	public class RampTypeOverlayInfo : TraitInfo<RampTypeOverlay> { }

	public class RampTypeOverlay : IPostRender, IWorldLoaded, IChatCommand
	{
		const string CommandName = "ramp-type-overlay";
		const string CommandDesc = "Toggles the ramp-type overlay";

		public bool Enabled;

		SpriteFont font;

		public void WorldLoaded(World w, WorldRenderer wr)
		{
			var console = w.WorldActor.TraitOrDefault<ChatCommands>();
			var help = w.WorldActor.TraitOrDefault<HelpCommand>();

			if (console == null || help == null)
				return;

			console.RegisterCommand(CommandName, this);
			help.RegisterHelp(CommandName, CommandDesc);
		}

		public void InvokeCommand(string name, string arg)
		{
			if (name == CommandName)
				Enabled ^= true;
		}

		public void RenderAfterWorld(WorldRenderer wr, Actor self)
		{
			if (!Enabled)
				return;

			if (font == null)
			{
				font = Game.Renderer.Fonts["TinyBold"];
				return;
			}

			var map = wr.World.Map;
			var tileSet = map.Rules.TileSet;

			foreach (var uv in wr.Viewport.AllVisibleCells.CandidateMapCoords)
			{
				if (!map.Height.Contains(uv))
					continue;

				var tile = map.Tiles[uv];
				var ti = tileSet.GetTileInfo(tile);
				var ramp = ti != null ? ti.RampType : 0;

				if (ramp != 0)
				{
					var cell = uv.ToCPos(wr.World.Map);
					var text = new TextRenderable(font, map.CenterOfCell(cell), 0, Color.Green, ramp.ToString());
					text.Render(wr);
				}
			}
		}
	}
}