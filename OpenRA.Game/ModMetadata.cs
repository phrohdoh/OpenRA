#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenRA
{
	public class ModMetadata
	{
		/// <summary>
		/// <para>Input: modId</para>
		/// Output: Absolute path to mod's directory.
		/// </summary>
		public static readonly Dictionary<string, string> ModPaths = GetModPaths();
		public static readonly Dictionary<string, ModMetadata> AllMods = ValidateMods();

		public string Id;
		public string Title;
		public string Description;
		public string Version;
		public string Author;
		public bool Hidden;
		public ContentInstaller Content;
		public string BaseFilePath;

		static Dictionary<string, ModMetadata> ValidateMods()
		{
			var ret = new Dictionary<string, ModMetadata>();
			foreach (var modId in ModMetadata.ModPaths.Keys)
			{
				var modPath = ModMetadata.ModPaths[modId];
				try
				{
					var yamlPath = Path.Combine(modPath, "mod.yaml");
					var yaml = new MiniYaml(null, MiniYaml.FromFile(yamlPath));
					var nd = yaml.ToDictionary();
					if (!nd.ContainsKey("Metadata"))
						continue;

					var metadata = FieldLoader.Load<ModMetadata>(nd["Metadata"]);
					metadata.Id = modId;
					metadata.BaseFilePath = modPath;

					if (nd.ContainsKey("ContentInstaller"))
						metadata.Content = FieldLoader.Load<ContentInstaller>(nd["ContentInstaller"]);

					ret.Add(modId, metadata);
				}
				catch (Exception ex)
				{
					Console.WriteLine("An exception occurred when trying to load ModMetadata for `{0}` ({1}):".F(modId, modPath));
					Console.WriteLine(ex.Message);
				}
			}

			return ret;
		}

		static Dictionary<string, string> GetModPaths()
		{
			var relativePath = Platform.ResolvePath(".", "mods");
			var supportPath = Platform.ResolvePath("^", "mods");

			var modPaths = Directory.GetDirectories(relativePath)
				.ToDictionary(x => x.Substring(relativePath.Length + 1));

			foreach (var path in Directory.GetDirectories(supportPath).ToDictionary(x => x.Substring(supportPath.Length + 1)))
				modPaths.Add(path.Key, path.Value);

			foreach (var k in modPaths.Keys.ToArray())
			{
				var v = modPaths[k];
				var yamlPath = Path.Combine(v, "mod.yaml");
				if (!File.Exists(yamlPath))
					modPaths.Remove(k);
			}

			return modPaths;
		}
	}
}
