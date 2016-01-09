using System;
using OpenRA.Scripting;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Scripting
{
	[ScriptPropertyGroup("Utilities")]
	public class ScriptTagsProperties : ScriptActorProperties, Requires<ScriptTagsInfo>
	{
		protected readonly ScriptTags tags;
		
		public ScriptTagsProperties(ScriptContext context, Actor self)
			: base(context, self)
		{
			tags = self.Trait<ScriptTags>();
		}
		
		/// <summary>Adds the given tag.</summary>
		/// <returns>True if `tag` was added, false otherwise.</returns>
		/// <param name="tags">The tags to add.</param>
		[ScriptActorPropertyActivity]
		public bool AddTag(string tag)
		{
			return tags.AddTag(tag);
		}

		/// <summary>Adds the given tags.</summary>
		/// <returns>True if every tag was added, false if any wasn't.</returns>
		/// <param name="tags">The tags to add.</param>
		[ScriptActorPropertyActivity]
		public bool AddTags(string[] tags)
		{
			var addedAll = true;

			foreach (var tag in tags)
				if (!AddTag(tag))
					addedAll = false;

			return addedAll;
		}

		/// <summary>Removes the given tag, if it exists.</summary>
		/// <returns>True if `tag` was removed, false otherwise.</returns>
		/// <param name="tag">The tag to remove.</param>
		[ScriptActorPropertyActivity]
		public bool RemoveTag(string tag)
		{
			return tags.RemoveTag(tag);
		}

		/// <summary>Removes the given tags, if they exist.</summary>
		/// <returns>True if every tag was removed, false if any wasn't.</returns>
		/// <param name="tags">The tags to remove.</param>
		[ScriptActorPropertyActivity]
		public bool RemoveTags(string[] tags)
		{
			var removedAll = true;

			foreach (var tag in tags)
				if (!RemoveTag(tag))
					removedAll = false;

			return removedAll;
		}

		/// <summary>Indicates whether or not this actor has the given tag.</summary>
		/// <returns>True if this actor has the tag, false otherwise.</returns>
		/// <param name="tag">The tag to check for.</param>
		[ScriptActorPropertyActivity]
		public bool HasTag(string tag)
		{
			return tags.HasTag(tag);
		}

		/// <summary>Indicates whether or not this actor has all of the given tags</summary>
		/// <returns>True if all this actor has all `tags`, false otherwise.</returns>
		/// <param name="tags">The tags to check for.</param>
		[ScriptActorPropertyActivity]
		public bool HasTags(string[] tags)
		{
			foreach (var tag in tags)
				if (!HasTag(tag))
					return false;

			return true;
		}
	}
}