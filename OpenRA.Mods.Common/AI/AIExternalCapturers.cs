using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.AI
{
	public class AITraitBase
	{
		internal virtual void PostActivate(Player p, HackyAI hackyAi)
		{
			Game.Debug("Activating {0} on bot {1}", GetType().Name, hackyAi.Info.Name);
		}
	}

	class ExternalCaptureTarget
	{
		internal readonly Actor Actor;
		internal readonly ExternalCapturableInfo Info;

		/// <summary>The order string given to the capturer so they can capture this actor.</summary>
		/// <example>ExternalCaptureActor</example>
		internal readonly string OrderString;

		internal ExternalCaptureTarget(Actor actor, string orderString)
		{
			Actor = actor;
			Info = actor.TraitsImplementing<ExternalCapturable>().FirstOrDefault(er => er.IsTraitEnabled())?.Info;
			OrderString = orderString;
		}
	}

	public class AIExternalCapturersInfo : ITraitInfo, Requires<HackyAIInfo>
	{
		[Desc("Actor types that can capture other actors via `ExternalCaptures`.")]
		public HashSet<string> CapturingActorTypes = new HashSet<string>();

		[Desc("Actor types that can be targeted for capturing.",
		      "Leave this empty to include all actors.")]
		public HashSet<string> CapturableActorTypes = new HashSet<string>();

		[Desc("Minimum delay (in ticks) between trying to capture with CapturingActorTypes.")]
		public readonly int MinimumCaptureDelay = 375;

		[Desc("Maximum number of options to consider for capturing.",
		      "If a value less than 1 is given 1 will be used instead.")]
		public readonly int MaximumCaptureTargetOptions = 10;

		[Desc("Should visibility (Shroud, Fog, Cloak, etc) be considered when searching for capturable targets?")]
		public readonly bool CheckCaptureTargetsForVisibility = true;

		[Desc("Player stances that capturers should attempt to target.")]
		public readonly Stance CapturableStances = Stance.Enemy | Stance.Neutral;

		[Desc("This trait is only enabled if the bot's Name is one of these.")]
		public HashSet<string> EnabledForBotNames = new HashSet<string>();

		object ITraitInfo.Create(ActorInitializer init) => new AIExternalCapturers(init.Self, this);
	}

	public class AIExternalCapturers : AITraitBase,
		ITick
	{
		readonly AIExternalCapturersInfo info;
		readonly Player player;
		readonly World world;
		readonly int maximumCaptureTargetOptions;
		readonly HashSet<Actor> trackedCapturers = new HashSet<Actor>();
		readonly Dictionary<Actor, ExternalCaptureTarget> reservations = new Dictionary<Actor, ExternalCaptureTarget>();

		HackyAI ai;
		int minCaptureDelayTicks;
		bool isEnabled;

		public AIExternalCapturers(Actor self, AIExternalCapturersInfo info)
		{
			this.info = info;
			player = self.Owner;
			world = self.World;

			maximumCaptureTargetOptions = Math.Max(1, info.MaximumCaptureTargetOptions);
		}

		internal override void PostActivate(Player p, HackyAI hackyAi)
		{
			ai = hackyAi;
			isEnabled = info.EnabledForBotNames.Contains(ai.Info.Name)
				&& world.Type == WorldType.Regular;

			minCaptureDelayTicks = ai.Random.Next(0, info.MinimumCaptureDelay);

			base.PostActivate(p, ai);
		}

		IEnumerable<Actor> GetExternalCapturers()
		{
			foreach (var actor in world.ActorsHavingTrait<ExternalCaptures>())
				if (actor.Owner == player)
					yield return actor;
		}

		IEnumerable<Actor> GetVisibleExternalCapturables(Player target)
		{
			foreach (var actor in GetExternalCapturables(target))
				if (actor.CanBeViewedByPlayer(player))
					yield return actor;
		}

		IEnumerable<Actor> GetExternalCapturables(Player target)
		{
			return world.Actors.Where(a => a.Owner == target && a.Info.HasTraitInfo<ExternalCapturableInfo>());
		}

		void QueueCaptureOrders()
		{
			if (player.WinState != WinState.Undefined)
				return;

			if (!info.CapturingActorTypes.Any())
			{
				isEnabled = false;
				return;
			}

			var idleCapturers = GetExternalCapturers().Where(a => a.IsIdle && a.IsInWorld).ToArray();
			if (idleCapturers.Length == 0)
				return;

			var randPlayer = world.Players.Where(p => !p.Spectating
				&& info.CapturableStances.HasStance(player.Stances[p])).Random(ai.Random);

			var targetOptions = (info.CheckCaptureTargetsForVisibility
				? GetVisibleExternalCapturables(randPlayer)
				: GetExternalCapturables(randPlayer))
				.Where(a => info.CapturableActorTypes.Contains(a.Info.Name));

			var externalCapturableTargetOptions = targetOptions
				.Select(a => new ExternalCaptureTarget(a, "ExternalCaptureActor"))
				.Where(target => target.Info != null
				       && idleCapturers.Any(capturer => target.Info.CanBeTargetedBy(capturer, target.Actor.Owner)))
				.OrderByDescending(target => target.Actor.GetSellValue())
				.Take(maximumCaptureTargetOptions);

			if (!externalCapturableTargetOptions.Any())
				return;

			foreach (var capturer in idleCapturers)
			{
				var target = GetCapturerTargetClosestToOrDefault(capturer, externalCapturableTargetOptions);
				if (!QueueCaptureOrderFor(capturer, target))
					HackyAI.BotDebug("{0} ({1}): {2} failed to capture {3}", ai.Info.Name, player.ClientIndex, capturer, target.Actor);
			}
		}

		bool QueueCaptureOrderFor(Actor capturer, ExternalCaptureTarget target)
		{
			if (target.Actor.IsDead || !target.Actor.IsInWorld)
				return false;

			ai.QueueOrder(new Order(target.OrderString, capturer, true) { TargetActor = target.Actor });
			HackyAI.BotDebug("{0} ({1}): Ordered {2} to capture {3}", ai.Info.Name, player.ClientIndex, capturer, target.Actor);
			//trackedCapturers.Remove(capturer);
			return true;
		}

		ExternalCaptureTarget GetCapturerTargetClosestToOrDefault(Actor capturer, IEnumerable<ExternalCaptureTarget> targets)
		{
			return targets.MinByOrDefault(target => (target.Actor.CenterPosition - capturer.CenterPosition).LengthSquared);
		}

		void ITick.Tick(Actor self)
		{
			if (!isEnabled)
				return;

			if (--minCaptureDelayTicks <= 0)
			{
				minCaptureDelayTicks = info.MinimumCaptureDelay;
				QueueCaptureOrders();
			}
		}
	}
}
