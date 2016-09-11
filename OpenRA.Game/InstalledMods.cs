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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenRA.FileSystem;
using OpenRA.Primitives;

using ModId = System.String;
using ModPath = System.String;

namespace OpenRA
{
	public class InstalledMods : IReadOnlyDictionary<ModId, Manifest>
	{
		readonly Dictionary<string, Manifest> mods;

		public InstalledMods(string customModPath)
		{
			mods = GetInstalledMods(customModPath);
		}

		static IEnumerable<Pair<ModId, ModPath>> GetCandidateMods()
		{
			// Get mods that are in the game folder.
			var basePath = Platform.ResolvePath(Path.Combine(".", "mods"));
			var commonModPath = Path.Combine(basePath, "common");

			var mods = new List<Pair<ModId, ModPath>>();

			foreach (var modDirPath in Directory.GetDirectories(basePath))
			{
				if (modDirPath == commonModPath)
					continue;

				try
				{
					var manifest = new Manifest(new Folder(modDirPath));
					mods.Add(Pair.New(manifest.Id, modDirPath));
				}
				catch (Exception ex)
				{
					var message = "Error loading {0}: {1}".F(modDirPath, ex.Message);
					Log.Write("debug", message);
					Console.WriteLine(message);
					continue;
				}
			}

			foreach (var m in Directory.GetFiles(basePath, "*.oramod"))
				mods.Add(Pair.New(Path.GetFileNameWithoutExtension(m), m));

			// Get mods that are in the support folder.
			var supportPath = Platform.ResolvePath(Path.Combine("^", "mods"));
			if (!Directory.Exists(supportPath))
				return mods;

			foreach (var pair in Directory.GetDirectories(supportPath).ToDictionary(x => x.Substring(supportPath.Length + 1)))
				mods.Add(Pair.New(pair.Key, pair.Value));

			foreach (var m in Directory.GetFiles(supportPath, "*.oramod"))
				mods.Add(Pair.New(Path.GetFileNameWithoutExtension(m), m));

			return mods;
		}

		static Manifest LoadMod(string id, string path)
		{
			IReadOnlyPackage package = null;
			try
			{
				if (Directory.Exists(path))
					package = new Folder(path);
				else
				{
					try
					{
						using (var fileStream = File.OpenRead(path))
							package = new ZipFile(fileStream, path);
					}
					catch (Exception ex)
					{
						throw new InvalidDataException(path + " is not a valid mod package", ex);
					}
				}

				if (!package.Contains("mod.yaml"))
					throw new InvalidDataException(path + " is not a valid mod package (no mod.yaml)");

				// Mods in the support directory and oramod packages (which are listed later
				// in the CandidateMods list) override mods in the main install.
				return new Manifest(package);
			}
			catch (Exception ex)
			{
				if (package != null)
					package.Dispose();

				Log.Write("debug", "Failed to load mod {0}: {1}".F(path, ex.Message);

				return null;
			}
		}

		static Dictionary<string, Manifest> GetInstalledMods(string customModPath)
		{
			var ret = new Dictionary<string, Manifest>();
			var candidates = GetCandidateMods();
			if (customModPath != null)
				candidates = candidates.Append(Pair.New(Path.GetFileNameWithoutExtension(customModPath), customModPath));

			foreach (var pair in candidates)
			{
				var id = pair.First;
				var path = pair.Second;
				var mod = LoadMod(id, path);

				// Mods in the support directory and oramod packages (which are listed later
				// in the CandidateMods list) override mods in the main install.
				if (mod != null)
					ret[pair.First] = mod;
			}

			return ret;
		}

		public Manifest this[string key] { get { return mods[key]; } }
		public int Count { get { return mods.Count; } }
		public ICollection<string> Keys { get { return mods.Keys; } }
		public ICollection<Manifest> Values { get { return mods.Values; } }
		public bool ContainsKey(string key) { return mods.ContainsKey(key); }
		public IEnumerator<KeyValuePair<string, Manifest>> GetEnumerator() { return mods.GetEnumerator(); }
		public bool TryGetValue(string key, out Manifest value) { return mods.TryGetValue(key, out value); }
		IEnumerator IEnumerable.GetEnumerator() { return mods.GetEnumerator(); }
	}
}