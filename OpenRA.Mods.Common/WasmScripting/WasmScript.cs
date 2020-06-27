using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;
using OpenRA.WasmScripting;

namespace OpenRA.Mods.Common.WasmScripting
{
	public class WasmScriptInfo : TraitInfo, Requires<SpawnMapActorsInfo>
	{
		public override object Create(ActorInitializer init) { return new WasmScript(this); }
	}

	public class WasmScript : IWorldLoaded
	{
		readonly WasmScriptInfo info;
		WasmScriptContext context;

		public WasmScript(WasmScriptInfo info)
		{
			this.info = info;
		}

		void IWorldLoaded.WorldLoaded(World w, WorldRenderer _wr)
		{
			context = new WasmScriptContext(w);
			context.WorldDidLoad();
		}
	}
}
