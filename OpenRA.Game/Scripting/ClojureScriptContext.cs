using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Eluant;
using OpenRA.Graphics;
using OpenRA.Primitives;
using OpenRA.Support;
using OpenRA.Traits;
// using clojure.lang;
using Clj = clojure.clr.api.Clojure;
using CljVar = clojure.lang.Var;
using CljSym = clojure.lang.Symbol;
using CljIFn = clojure.lang.IFn;
using CljNs = clojure.lang.Namespace;
using RT = clojure.lang.RT;

namespace OpenRA.Scripting
{
	public sealed class ClojureScriptContext : IDisposable
	{
		public World World { get; private set; }
		public WorldRenderer WorldRenderer { get; private set; }

		bool disposed;

		private readonly CljIFn clojure_core__require;
		private readonly CljIFn clojure_core__load_string;
		private readonly CljIFn clojure_core__star_ns_star;
		private readonly CljIFn clojure_core__prn;
		private readonly CljIFn clojure_core__deref;

		private readonly CljIFn map_fn_tick;

		public ClojureScriptContext(World world, WorldRenderer worldRenderer, string scriptFileName)
		{
			Log.AddChannel("clj", "clj.log");

			World = world;
			WorldRenderer = worldRenderer;

			try {
				clojure_core__require = Clj.var("clojure.core", "require");
				clojure_core__load_string = Clj.var("clojure.core", "load-string");

				clojure_core__star_ns_star = Clj.var("clojure.core", "*ns*");
				clojure_core__prn = Clj.var("clojure.core", "prn");
				clojure_core__deref = Clj.var("clojure.core", "deref");
				// clojure_core__prn.invoke(clojure_core__deref.invoke(clojure_core__star_ns_star));
			} catch (Exception e) {
				LogDebugMessage($"Failed to prepare Clojure scripting environment: {e}");
				throw;
			}

			try {
				clojure_core__require.invoke(Clj.read("clojure.core.server"));
				var fnStartReplServer = Clj.var("clojure.core.server", "start-server");
				var ret = fnStartReplServer.invoke(Clj.read("{:name ora-repl :port 5555 :accept clojure.core.server/repl}"));
				// LogDebugMessage($"Started Clojure REPL server: {ret}");
			} catch (Exception e) {
				LogDebugMessage($"Failed to start Clojure REPL server: {e}");
				throw;
			}

			// eval map script
			try {
				// todo: set up namespace, doesn't seem to have any affect
				//clojure_core__load_string.invoke("(ns openra.map.script)");

				var cljFileName = scriptFileName ?? "map2.clj";
				var mapClj = world.Map.Open(cljFileName).ReadAllText();
				clojure_core__load_string.invoke(mapClj);
				var ns = (clojure_core__deref.invoke(clojure_core__star_ns_star) as CljNs).Name;
				map_fn_tick = Clj.var(ns, "tick");
			} catch (Exception e) {
				LogDebugMessage(e.ToString());
				throw;
			}
		}

		void LogDebugMessage(string message)
		{
			Console.WriteLine("Clojure debug: {0}", message);
			Log.Write("clj", message);
		}

		public void WorldLoaded()
		{
			CljNs currNsObj = clojure_core__deref.invoke(clojure_core__star_ns_star) as CljNs;
			var currNs = currNsObj.Name;
			Clj.var(currNs, "on-world-loaded").invoke();
		}

		public void Tick(Actor self)
		{
			if (disposed)
				return;

			map_fn_tick.invoke();
		}

		public void Dispose()
		{
			if (disposed)
				return;

			disposed = true;
		}
	}
}
