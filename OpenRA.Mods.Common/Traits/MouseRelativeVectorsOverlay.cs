using System;
using OpenRA.Graphics;
using OpenRA.Traits;
using OpenRA.Mods.Common.Commands;
using OpenRA.Mods.Common.Graphics;

namespace OpenRA.Mods.Common.Traits
{
	public class MouseRelativeVectorsOverlayInfo : TraitInfo<MouseRelativeVectorsOverlay> { }

	public class MouseRelativeVectorsOverlay : IPostRender, IWorldLoaded, IChatCommand
	{
		const string CommandName = "relative-vec-overlay";
		const string CommandDesc = "Toggles the mouse-relative geometry overlay";

		public bool Enabled;

		SpriteFont font;

		void IWorldLoaded.WorldLoaded(World w, WorldRenderer wr)
		{
			var console = w.WorldActor.TraitOrDefault<ChatCommands>();
			var help = w.WorldActor.TraitOrDefault<HelpCommand>();

			if (console == null || help == null)
				return;

			console.RegisterCommand(CommandName, this);
			help.RegisterHelp(CommandName, CommandDesc);

			font = Game.Renderer.Fonts["TinyBold"];
		}

		void IChatCommand.InvokeCommand(string name, string arg)
		{
			if (name == CommandName)
				Enabled ^= true;
		}

		void IPostRender.RenderAfterWorld(WorldRenderer wr, Actor self)
		{
			if (!Enabled)
				return;

			if (font == null)
			{
				font = Game.Renderer.Fonts["TinyBold"];
				return;
			}

			var map = wr.World.Map;
			var mouseCell = wr.Viewport.ViewToWorld(Viewport.LastMousePos);

			var vecs = new[] {
				new CVec(-1, 0),
				new CVec(0, -1),
				new CVec(1, 0),
				new CVec(0, 1),
				new CVec(-1, -1),
				new CVec(1, -1),
				new CVec(1, 1),
				new CVec(-1, 1),
			};

			foreach (var vec in vecs)
			{
				var cell = mouseCell - vec * 2;
				var text = new TextRenderable(font, map.CenterOfCell(cell), 0, System.Drawing.Color.Red, vec.ToString());
				text.Render(wr);
			}
		}
	}
}
