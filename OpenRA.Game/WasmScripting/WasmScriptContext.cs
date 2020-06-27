using System;
// using System.Linq;
using System.Text;
using WasmerSharp;

namespace OpenRA.WasmScripting
{
	public sealed class WasmScriptContext
	{
		public World World { get; private set; }

		private Instance mapInstance { get; set; }

		public WasmScriptContext(World world)
		{
			World = world;

			Log.AddChannel("wasm", "wasm.log");

			var mem = Memory.Create(minPages: 5, maxPages: 5);
			var memImport = new Import("env", "mem", mem);

			var fnLogDebugMsgImport = new Import("env", "log",
				new ImportFunction((Action<InstanceContext, int, int>)(Wasm_LogDebugMessage))
			);

			var mapWasmBytes = world.Map.Open("map.wasm").ReadAllBytes();
			mapInstance = new Instance(mapWasmBytes, memImport, fnLogDebugMsgImport);
		}

		public void WorldDidLoad()
		{
			CallWasmFunc(funcName: "world_did_load"/*, required: true*/);
			CallWasmFunc("add_3"/*, required: true*/, 1);
		}

		private WasmerValue[] CallWasmFunc(string funcName, /*bool required,*/ params WasmerValue[] args)
		{
			/* TODO: https://github.com/migueldeicaza/WasmerSharp/issues/12
			var export = _instance.Exports
				.Where(exp => exp.Kind == ImportExportKind.Function)
				.SingleOrDefault(exp => exp.Name == funcName);

			if (!String.IsNullOrWhiteSpace(_instance.LastError)) {
				var err = "instance errored while attempting to find exported func '{0}': {1}".F(funcName, _instance.LastError);
				LogDebugMessage(err);
				return null;
			}

			if (export == null) {
				if (required) {
					LogDebugMessage("required func '{0}' was not found".F(funcName));
				} else {
					LogDebugMessage("optional func '{0}' was not found".F(funcName));
					return new WasmerValue[0];
				}
			}

			var func = export.GetExportFunction();
			var results = new WasmerValue[func.Returns.Length];
			*/
			// TODO: use above lines once above-linked ticket is addressed
			var results = new WasmerValue[1]; // hard-coded support for 1 ret
			mapInstance.Call(funcName, args, results);

			/* Q: Why did I do this?
			var int32Results = results
				.Where(r => r.Tag == WasmerValueType.Int32)
				.Select(r => r.Int32);
			*/

			LogDebugMessage(
				"call '{0}({1})' returned {2} value(s): ({3})".F(
					funcName,
					args.JoinWith(","),
					results.Length,
					results.JoinWith(",")
			));

			return results;
		}

		void Wasm_LogDebugMessage(InstanceContext ctx, int memOffset, int byteCount)
		{
			var mem = ctx.GetMemory(0).Data;
			unsafe
			{
				var str = Encoding.UTF8.GetString((byte*)mem + memOffset, byteCount);
				LogDebugMessage(str);
			}
		}

		void LogDebugMessage(string message)
		{
			Console.WriteLine("WASM debug: {0}", message);
			Log.Write("wasm", message);
		}
	}
}
