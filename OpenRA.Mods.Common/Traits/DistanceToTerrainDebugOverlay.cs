using OpenRA.Graphics;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	public class DistanceToTerrainDebugOverlayInfo : ITraitInfo, Requires<IOccupySpaceInfo>
	{
		object ITraitInfo.Create(ActorInitializer init) => new DistanceToTerrainDebugOverlay(init.Self, this);
	}

	public class DistanceToTerrainDebugOverlay : IPostRender
	{
		readonly DistanceToTerrainDebugOverlayManager manager;
		readonly RgbaColorRenderer renderer;
		readonly Map map;

		public DistanceToTerrainDebugOverlay(Actor self, DistanceToTerrainDebugOverlayInfo info)
		{
			renderer = Game.Renderer.WorldRgbaColorRenderer;
			map = self.World.Map;

			manager = self.World.WorldActor.TraitOrDefault<DistanceToTerrainDebugOverlayManager>();}

		void IPostRender.RenderAfterWorld(WorldRenderer wr, Actor self)
		{
			if (manager == null || !manager.Enabled)
				return;

			var pos = self.CenterPosition;
			var dat = map.DistanceAboveTerrain(pos);

			var dist = dat.Length;
			if (dist == 0)
				return;

			var vec = new WVec(0, 0, -dist);
			var ground = pos + vec;

			renderer.DrawLine(wr.ScreenPosition(pos), wr.ScreenPosition(ground), 1f, self.Owner.Color.RGB);
			new TextRenderable(manager.Font, ground, 1, self.Owner.Color.RGB, dist.ToString()).Render(wr);
		}
	}
}