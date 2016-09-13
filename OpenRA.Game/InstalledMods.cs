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
using ModIdAtVersion = System.String;
using MaxMind.GeoIP2.Model;

namespace OpenRA
{
	public class InstalledMods : IReadOnlyDictionary<ModId, Manifest>
	{
		readonly Dictionary<ModIdAtVersion, Manifest> mods;

		public InstalledMods(string customModPath)
		{
			mods = GetInstalledMods(customModPath);
		}

		static ModIdAtVersion GetIdAtVersion(string fileOrDirectoryPath)
		{
			Manifest manifest;

			if (File.Exists(fileOrDirectoryPath))
			{
				using (var fileStream = File.OpenRead(fileOrDirectoryPath))
				{
					var package = new ZipFile(fileStream, fileOrDirectoryPath);

					try
					{
						manifest = new Manifest(package);
					}
					catch (FieldLoader.MissingFieldsException ex)
					{
						Log.Write("debug", $"{fileOrDirectoryPath}/mod.yaml is missing fields:{Environment.NewLine}\t{ex.Missing.JoinWith(Environment.NewLine + "\t")}");
						return null;
					}
				}
			}
			else if (Directory.Exists(fileOrDirectoryPath))
			{
				try
				{
					manifest = new Manifest(new Folder(fileOrDirectoryPath));
				}
				catch (FieldLoader.MissingFieldsException ex)
				{
					Log.Write("debug", $"{fileOrDirectoryPath}/mod.yaml is missing fields:{Environment.NewLine}\t{ex.Missing.JoinWith(Environment.NewLine + "\t")}");
					return null;
				}
			}
			else
				return null;

			return manifest.Id + "@" + manifest.Metadata.Version;
		}

		static IEnumerable<Pair<ModIdAtVersion, ModPath>> GetCandidateMods()
		{
			// Get mods that are in the game folder.
			var basePath = Platform.ResolvePath(Path.Combine(".", "mods"));
			var commonModPath = Path.Combine(basePath, "common");

			var mods = new List<Pair<ModIdAtVersion, ModPath>>();

			foreach (var modDirPath in Directory.GetDirectories(basePath))
			{
				if (modDirPath == commonModPath)
					continue;

				var modIdAtVersion = GetIdAtVersion(modDirPath);
				if (modIdAtVersion == null)
					continue;

				mods.Add(Pair.New(modIdAtVersion, modDirPath));
			}

			foreach (var m in Directory.GetFiles(basePath, "*.oramod"))
			{
				var modIdAtVersion = GetIdAtVersion(m);
				if (modIdAtVersion == null)
					continue;

				mods.Add(Pair.New(modIdAtVersion, m));
			}

			// Get mods that are in the support folder.
			var supportPath = Platform.ResolvePath(Path.Combine("^", "mods"));
			if (!Directory.Exists(supportPath))
				return mods;

			foreach (var pair in Directory.GetDirectories(supportPath).ToDictionary(x => x))
			{
				var modIdAtVersion = GetIdAtVersion(pair.Key);
				if (modIdAtVersion == null)
					continue;

				mods.Add(Pair.New(modIdAtVersion, pair.Value));
			}

			foreach (var m in Directory.GetFiles(supportPath, "*.oramod"))
			{
				var modIdAtVersion = GetIdAtVersion(m);
				if (modIdAtVersion == null)
					continue;

				mods.Add(Pair.New(modIdAtVersion, m));
			}

			return mods;
		}

		static Manifest LoadMod(string idAtVersion, string path)
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
				var version = idAtVersion.Split(new[] { '@' }, 2)?[1] ?? "*";
				var manifest = new Manifest(package);
				if (manifest.Metadata.Version == version || version == "*")
					return manifest;

				return null;
			}
			catch (Exception ex)
			{
				if (package != null)
					package.Dispose();

				Log.Write("debug", "Failed to load mod {0}: {1}".F(path, ex.Message));

				return null;
			}
		}

		static Dictionary<string, Manifest> GetInstalledMods(string customModPath)
		{
			var ret = new Dictionary<string, Manifest>();
			var candidates = GetCandidateMods();
			if (customModPath != null)
				candidates = candidates.Append(Pair.New(GetIdAtVersion(customModPath), customModPath));

			foreach (var pair in candidates)
			{
				var idAtVersion = pair.First;
				var path = pair.Second;
				var mod = LoadMod(idAtVersion, path);

				// Mods in the support directory and oramod packages (which are listed later
				// in the CandidateMods list) override mods in the main install.
				if (mod != null)
					ret[idAtVersion] = mod;
			}

			return ret;
		}

		public Manifest this[string key] { get { return mods[key]; } }
		public int Count { get { return mods.Count; } }
		public ICollection<string> Keys { get { return mods.Keys; } }
		public ICollection<Manifest> Values { get { return mods.Values; } }
		public bool ContainsKey(string key) => mods.ContainsKey(key);
		public IEnumerator<KeyValuePair<string, Manifest>> GetEnumerator() { return mods.GetEnumerator(); }
		public bool TryGetValue(string key, out Manifest value) => mods.TryGetValue(key, out value);
		IEnumerator IEnumerable.GetEnumerator() { return mods.GetEnumerator(); }

		public bool ContainsVersionedMod(string modId, string modVersion)
		{
			foreach (var k in mods.Keys)
			{
				var split = k.Split(new[] { '@' });
				if (split[0] == modId && (split[1] == "*" || split[1] == modVersion))
					return true;
			}

			return false;
		}

		public bool TryGetVersionedMod(string modId, string modVersion, out Manifest value)
		{
			value = null;

			foreach (var k in mods.Keys)
			{
				var split = k.Split(new[] { '@' });
				Console.WriteLine(split.JoinWith(","));
				if (split[0] == modId && (split[1] == "*" || split[1] == modVersion))
				{
					value = mods[k];
					return true;
				}
			}

			return false;
		}
	}
}