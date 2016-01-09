using System;
using System.Collections.Generic;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Scripting
{
	public class ScriptTagsInfo : ITraitInfo
	{
		public object Create(ActorInitializer init) { return new ScriptTags(init, this); }
	}
	
	public class ScriptTags
	{
		readonly HashSet<string> tags;
		readonly Actor self;
		
		public ScriptTags(ActorInitializer init, ScriptTagsInfo info)
		{
			tags = init.Contains<ScriptTagsInit>() ? init.Get<ScriptTagsInit, string[]>().ToHashSet() : new HashSet<string>();
			self = init.Self;
		}
		
		public bool AddTag(string tag)
		{
			Game.Debug("Added {0} to {1}", tag, self);
			return tags.Add(tag);
		}
		
		public bool AddTags(string[] tags)
		{
			var addedAll = true;

			foreach (var tag in tags)
				if (!AddTag(tag))
					addedAll = false;

			return addedAll;
		}
		
		public bool RemoveTag(string tag)
		{
			return tags.Remove(tag);
		}
		
		public bool RemoveTags(string[] tags)
		{
			var removedAll = true;

			foreach (var tag in tags)
				if (!RemoveTag(tag))
					removedAll = false;

			return removedAll;
		}
		
		public bool HasTag(string tag)
		{
			return tags.Contains(tag);
		}
		
		public bool HasTags(string[] tags)
		{
			foreach (var tag in tags)
				if (!HasTag(tag))
					return false;

			return true;
		}
	}
	
	public class ScriptTagsInit : IActorInit<string[]>
	{
		[FieldFromYamlKey] readonly string[] value;
		public ScriptTagsInit() { }
		public ScriptTagsInit(string[] init)
		{
			value = init;
		}

		public string[] Value(World world)
		{
			return value;
		}
	}
}