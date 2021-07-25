using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Scripting;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Scripting
{
	public class ClojureScriptInfo : TraitInfo, Requires<SpawnMapActorsInfo>
	{
		public readonly string ScriptFileName = null;
		public override object Create(ActorInitializer init) { return new ClojureScript(this); }
	}

	public class ClojureScript : ITick, IWorldLoaded, INotifyActorDisposing
	{
		readonly ClojureScriptInfo info;
		ClojureScriptContext context;
		bool disposed;

		public ClojureScript(ClojureScriptInfo info)
		{
			this.info = info;
		}

		void IWorldLoaded.WorldLoaded(World world, WorldRenderer worldRenderer)
		{
			context = new ClojureScriptContext(world, worldRenderer, this.info.ScriptFileName);
			context.WorldLoaded();
		}

		void ITick.Tick(Actor self)
		{
			context.Tick(self);
		}

		void INotifyActorDisposing.Disposing(Actor self)
		{
			if (disposed)
				return;

			context?.Dispose();

			disposed = true;
		}
	}
}
