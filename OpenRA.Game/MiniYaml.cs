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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenRA.FileSystem;

namespace OpenRA
{
	using MiniYamlNodes = List<MiniYamlNode>;

	public static class MiniYamlExts
	{
		public static void WriteToFile(this MiniYamlNodes y, string filename)
		{
			File.WriteAllLines(filename, y.ToLines(true).Select(x => x.TrimEnd()).ToArray());
		}

		public static string WriteToString(this MiniYamlNodes y)
		{
			return y.ToLines(true).Select(x => x.TrimEnd()).JoinWith("\n");
		}

		public static IEnumerable<string> ToLines(this MiniYamlNodes y, bool lowest)
		{
			foreach (var kv in y)
			{
				foreach (var line in kv.Value.ToLines(kv.Key))
					yield return line;

				if (lowest)
					yield return "";
			}
		}
	}

	public class MiniYamlNode
	{
		public struct SourceLocation
		{
			public string Filename;
			public int Line;
			public int Column;
			public override string ToString() { return "{0}:{1}:{2}".F(Filename, Line, Column); }
		}

		public SourceLocation Location;
		public string Key;
		public MiniYaml Value;
		public MiniYamlNode Parent;

		public MiniYamlNode(string k, MiniYaml v)
		{
			Key = k;
			Value = v;
		}

		public MiniYamlNode(string k, MiniYaml v, SourceLocation loc)
			: this(k, v)
		{
			Location = loc;
		}

		public MiniYamlNode(string k, string v)
			: this(k, v, null) { }

		public MiniYamlNode(string k, string v, List<MiniYamlNode> n)
			: this(k, new MiniYaml(v, n)) { }

		public MiniYamlNode(string k, string v, List<MiniYamlNode> n, SourceLocation loc)
			: this(k, new MiniYaml(v, n), loc) { }

		public override string ToString()
		{
			return "{{YamlNode: {0} @ {1}}}".F(Key, Location);
		}

		public MiniYamlNode Clone()
		{
			return new MiniYamlNode(Key, Value.Clone());
		}
	}

	public class MiniYaml
	{
		const int SpacesPerLevel = 4;
		static readonly Func<string, string> StringIdentity = s => s;
		//static readonly Func<MiniYaml, MiniYaml> MiniYamlIdentity = my => my;
		static readonly Func<MiniYaml, MiniYaml> WithoutEmptyKeys = my =>
		{
			var ret = my.Clone();
			ret.Nodes = ret.Nodes.Where(n => !string.IsNullOrWhiteSpace(n.Key)).ToList();
			return my;
		};

		public string Value;
		public List<MiniYamlNode> Nodes;

		public MiniYaml Clone()
		{
			return new MiniYaml(Value, Nodes.Select(n => n.Clone()).ToList());
		}

		public Dictionary<string, MiniYaml> ToDictionary()
		{
			return ToDictionary(WithoutEmptyKeys);
		}

		public Dictionary<string, TElement> ToDictionary<TElement>(Func<MiniYaml, TElement> elementSelector)
		{
			return ToDictionary(StringIdentity, elementSelector);
		}

		public Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(
			Func<string, TKey> keySelector, Func<MiniYaml, TElement> elementSelector)
		{
			var ret = new Dictionary<TKey, TElement>();
			foreach (var y in Nodes.Where(n => !string.IsNullOrWhiteSpace(n.Key)))
			{
				var key = keySelector(y.Key);
				if (string.IsNullOrWhiteSpace(y.Key))
					continue;

				var element = elementSelector(y.Value);
				try
				{
					ret.Add(key, element);
				}
				catch (ArgumentException ex)
				{
					throw new InvalidDataException("Duplicate key '{0}' in {1}".F(y.Key, y.Location), ex);
				}
			}

			return ret;
		}

		public MiniYaml(string value) : this(value, null) { }

		public MiniYaml(string value, List<MiniYamlNode> nodes)
		{
			Value = value;
			Nodes = nodes ?? new List<MiniYamlNode>();
		}

		public static List<MiniYamlNode> NodesOrEmpty(MiniYaml y, string s)
		{
			var nd = y.ToDictionary();
			return nd.ContainsKey(s) ? nd[s].Nodes : new List<MiniYamlNode>();
		}

		static List<MiniYamlNode> FromLines(IEnumerable<string> lines, string filename)
		{
			var levels = new List<List<MiniYamlNode>>();
			levels.Add(new List<MiniYamlNode>());

			var lineNum = 0;

			foreach (var line in lines)
			{
				++lineNum;

				var textStarted = false;
				var commentStarted = false;
				var commentStartPos = -1;

				var text = "";
				var comment = "";
				var column = 0;

				for (var i = 0; i < line.Length; i++)
				{
					var currChar = line[i];
					if (currChar == '#' && !commentStarted)
					{
						commentStarted = true;
						commentStartPos = i;
					}
					else if (!textStarted && currChar != '\t' && currChar != ' ')
						textStarted = true;

					if (commentStarted && commentStartPos < i)
						comment += currChar;
					else if (textStarted && commentStartPos != i)
						text += currChar;
					else
						column++;
				}

				var key = "";
				var val = "";
				var location = new MiniYamlNode.SourceLocation() { Filename = filename ?? "<no filename>", Line = lineNum, Column = column };

				if (!string.IsNullOrWhiteSpace(text))
				{
					var split = text.Split(new[] { ':' }, 2);
					key = split[0].Trim();
					val = split.Length == 2 ? split[1].Trim() : "";
				}

				if (levels.Count <= column)
					throw new YamlException("Bad indent in miniyaml at {0}".F(location));

				while (levels.Count > column + 1)
					levels.RemoveAt(levels.Count - 1);

				var empty = new List<MiniYamlNode>();

				levels[column].Add(new MiniYamlNode(key, val, empty, location));
				levels.Add(empty);
			}

			// Top-level nodes
			return levels[0];
		}

		public static Dictionary<string, MiniYaml> DictFromFile(string path)
		{
			var miniYaml = FromFile(path).Where(node => !string.IsNullOrWhiteSpace(node.Key));
			return miniYaml.ToDictionary(x => x.Key, x => x.Value);
		}

		public static Dictionary<string, MiniYaml> DictFromStream(Stream stream)
		{
			var miniYaml = FromStream(stream).Where(node => !string.IsNullOrWhiteSpace(node.Key));
			return miniYaml.ToDictionary(x => x.Key, x => x.Value);
		}

		public static List<MiniYamlNode> FromFile(string path)
		{
			return FromLines(File.ReadAllLines(path), path);
		}

		public static List<MiniYamlNode> FromStream(Stream s, string fileName = "<no filename available>")
		{
			using (var reader = new StreamReader(s))
				return FromString(reader.ReadToEnd(), fileName);
		}

		public static List<MiniYamlNode> FromString(string text, string fileName = "<no filename available>")
		{
			return FromLines(text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries), fileName);
		}

		public static List<MiniYamlNode> Merge(IEnumerable<List<MiniYamlNode>> sources)
		{
			if (!sources.Any())
				return new List<MiniYamlNode>();

			var tree = sources.Where(s => s != null).Aggregate(MergePartial)
				.ToDictionary(n => n.Key, n => n.Value);

			var resolved = new Dictionary<string, MiniYaml>();
			foreach (var kv in tree)
			{
				var inherited = new Dictionary<string, MiniYamlNode.SourceLocation>();
				inherited.Add(kv.Key, new MiniYamlNode.SourceLocation());

				var children = ResolveInherits(kv.Key, kv.Value, tree, inherited);
				resolved.Add(kv.Key, new MiniYaml(kv.Value.Value, children));
			}

			return resolved.Select(kv => new MiniYamlNode(kv.Key, kv.Value)).ToList();
		}

		static void MergeIntoResolved(MiniYamlNode overrideNode, List<MiniYamlNode> existingNodes,
			Dictionary<string, MiniYaml> tree, Dictionary<string, MiniYamlNode.SourceLocation> inherited)
		{
			var existingNode = existingNodes.FirstOrDefault(n => n.Key == overrideNode.Key);
			if (existingNode != null)
			{
				existingNode.Value = MiniYaml.MergePartial(existingNode.Value, overrideNode.Value);
				existingNode.Value.Nodes = ResolveInherits(existingNode.Key, existingNode.Value, tree, inherited);
			}
			else
				existingNodes.Add(overrideNode.Clone());
		}

		static List<MiniYamlNode> ResolveInherits(string key, MiniYaml node, Dictionary<string, MiniYaml> tree, Dictionary<string, MiniYamlNode.SourceLocation> inherited)
		{
			var resolved = new List<MiniYamlNode>();

			// Inheritance is tracked from parent->child, but not from child->parentsiblings.
			inherited = new Dictionary<string, MiniYamlNode.SourceLocation>(inherited);

			foreach (var n in node.Nodes)
			{
				if (n.Key == "Inherits" || n.Key.StartsWith("Inherits@"))
				{
					MiniYaml parent;
					if (!tree.TryGetValue(n.Value.Value, out parent))
						throw new YamlException(
							"{0}: Parent type `{1}` not found".F(n.Location, n.Value.Value));

					if (inherited.ContainsKey(n.Value.Value))
						throw new YamlException("{0}: Parent type `{1}` was already inherited by this yaml tree at {2} (note: may be from a derived tree)"
							.F(n.Location, n.Value.Value, inherited[n.Value.Value]));

					inherited.Add(n.Value.Value, n.Location);
					foreach (var r in ResolveInherits(n.Key, parent, tree, inherited))
						MergeIntoResolved(r, resolved, tree, inherited);
				}
				else if (n.Key.StartsWith("-"))
				{
					var removed = n.Key.Substring(1);
					if (resolved.RemoveAll(r => r.Key == removed) == 0)
						throw new YamlException("{0}: There are no elements with key `{1}` to remove".F(n.Location, removed));
				}
				else
					MergeIntoResolved(n, resolved, tree, inherited);
			}

			return resolved;
		}

		static MiniYaml MergePartial(MiniYaml existingNodes, MiniYaml overrideNodes)
		{
			if (existingNodes == null)
				return overrideNodes;

			if (overrideNodes == null)
				return existingNodes;

			return new MiniYaml(overrideNodes.Value ?? existingNodes.Value, MergePartial(existingNodes.Nodes, overrideNodes.Nodes));
		}

		static List<MiniYamlNode> MergePartial(List<MiniYamlNode> existingNodes, List<MiniYamlNode> overrideNodes)
		{
			if (existingNodes.Count == 0)
				return overrideNodes;

			if (overrideNodes.Count == 0)
				return existingNodes;

			var ret = new List<MiniYamlNode>();

			var existingDict = existingNodes.ToDictionaryWithConflictLog(x => x.Key, "MiniYaml.Merge", null, x => "{0} (at {1})".F(x.Key, x.Location));
			var overrideDict = overrideNodes.ToDictionaryWithConflictLog(x => x.Key, "MiniYaml.Merge", null, x => "{0} (at {1})".F(x.Key, x.Location));
			var allKeys = existingDict.Keys.Union(overrideDict.Keys);

			foreach (var key in allKeys)
			{
				MiniYamlNode existingNode, overrideNode;
				existingDict.TryGetValue(key, out existingNode);
				overrideDict.TryGetValue(key, out overrideNode);

				var loc = overrideNode == null ? default(MiniYamlNode.SourceLocation) : overrideNode.Location;
				var merged = (existingNode == null || overrideNode == null) ? overrideNode ?? existingNode :
					new MiniYamlNode(key, MergePartial(existingNode.Value, overrideNode.Value), loc);
				ret.Add(merged);
			}

			return ret;
		}

		public IEnumerable<string> ToLines(string name)
		{
			yield return name + ": " + Value;

			if (Nodes != null)
				foreach (var line in Nodes.ToLines(false))
					yield return "\t" + line;
		}

		public static List<MiniYamlNode> Load(IReadOnlyFileSystem fileSystem, IEnumerable<string> files, MiniYaml mapRules)
		{
			if (mapRules != null && mapRules.Value != null)
			{
				var mapFiles = FieldLoader.GetValue<string[]>("value", mapRules.Value);
				files = files.Append(mapFiles);
			}

			var yaml = files.Select(s => MiniYaml.FromStream(fileSystem.Open(s)));
			if (mapRules != null && mapRules.Nodes.Any())
				yaml = yaml.Append(mapRules.Nodes);

			return MiniYaml.Merge(yaml);
		}
	}

	[Serializable]
	public class YamlException : Exception
	{
		public YamlException(string s) : base(s) { }
	}
}
