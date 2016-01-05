using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.AI
{
	public class AITeamTypeInfo : ITraitInfo
	{
		[Desc("Actor types to train and group together into teams. Duplicates allowed: 'e1, e1, medic' means 2 e1s and 1 medic.")]
		public readonly string[] ActorComposition = { };
		
		[Desc("Factory actor types (barr, nahand, etc) required to produce the AllowedActorTypes.")]
		public readonly string[] RequiredFactoryTypes = { };
		
		[Desc("The category this team belongs to. This helps decided what teams are chosen for which task.")]
		public readonly string Category;
		
		[Desc("Remove dead/disposed actors from this team every X ticks.")]
		public readonly int CleanupReferencesDelay = 100;
		
		public object Create(ActorInitializer init) { return new AITeamType(init.Self, this); }
	}
	
	public class AITeamType : ITick
	{
		public readonly AITeamTypeInfo Info;
		public readonly HashSet<Actor> CurrentActors;
		public readonly Dictionary<string, int> CompositionCountsDict;
		
		int cleanupTicks;
		
		public AITeamType(Actor self, AITeamTypeInfo info)
		{
			Info = info;
			CurrentActors = new HashSet<Actor>();
			
			CompositionCountsDict = new Dictionary<string, int>();
			
			var groups = info.ActorComposition.GroupBy(item => item);
			foreach (var group in groups)
				CompositionCountsDict.Add(group.Key, group.Count());
			
			cleanupTicks = info.CleanupReferencesDelay;
		}
		
		public Actor[] CurrentActorsOfType(string infoName)
		{
			return CurrentActors.Where(a => a.Info.Name == infoName).ToArray();
		}
		
		public int GetMissingActorTypeCount(string actorInfoName)
		{
			var missing = GetMissingActorTypeCount();
			return missing.ContainsKey(actorInfoName) ? missing[actorInfoName] : -1;
		}
		
		public Dictionary<string, int> GetMissingActorTypeCount()
		{
			var ret = new Dictionary<string, int>();
			foreach (var actorType in CompositionCountsDict.Keys)
			{
				var maxCount = CompositionCountsDict[actorType];
				var currCount = CurrentActorsOfType(actorType).Length;
				
				if (maxCount - currCount > 0)
				{
					if (ret.ContainsKey(actorType))
						ret[actorType]++;
					else
						ret.Add(actorType, 1);
				}
			}
			
			return ret;
		}
		
		/// <summary>
		/// Adds the given actor.
		/// </summary>
		/// <returns>Whether or not the actor was added.</returns>
		/// <param name="actor">The actor instance to add.</param>
		/// <param name="force">If this is `true` then add `actor` even if the number of types has been met.</param>
		public bool AddActor(Actor actor, bool force = false)
		{
			return CurrentActors.Add(actor);
		}
		
		/// <summary>
		/// Removes the given actor.
		/// </summary>
		/// <returns>Whether or not the actor was removed.</returns>
		/// <param name="actor">The actor instance to remove.</param>
		public bool RemoveActor(Actor actor)
		{
			return CurrentActors.Remove(actor);
		}

		void ITick.Tick(Actor self)
		{
			if (--cleanupTicks > 0)
				return;
				
			cleanupTicks = Info.CleanupReferencesDelay;
			CleanupReferences();
		}
		
		void CleanupReferences()
		{
			foreach (var act in CurrentActors)
				if (act == null || act.Disposed)
					RemoveActor(act);
		}
	}
}