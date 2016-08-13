using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.AI
{
	class RepairTarget
	{
		internal readonly Actor Actor;
		internal readonly EngineerRepairableInfo Info;
		internal readonly int Distance;
		internal readonly Health Health;

		/// <summary>The order string given to the capturer so they can capture this actor.</summary>
		/// <example>ExternalCaptureActor</example>
		internal readonly string OrderString;

		internal RepairTarget(Actor actor, int distance, string orderString)
		{
			Actor = actor;
			Info = actor.TraitsImplementing<EngineerRepairable>().FirstOrDefault(er => er.IsTraitEnabled())?.Info;
			Health = actor.TraitOrDefault<Health>();
			Distance = distance;
			OrderString = orderString;
		}
	}

	public class AIEngineerRepairInfo : ITraitInfo, Requires<HackyAIInfo>
	{
		[Desc("Actor types that can repair other actors via `EngineerRepair`.")]
		public HashSet<string> ReparingActorTypes = new HashSet<string>();

		[Desc("Actor types that should be considered for repair.",
		      "Leave this empty to include all actors.")]
		public HashSet<string> RepairableActorTypes = new HashSet<string>();

		[Desc("Minimum delay (in ticks) between trying to repair nearby actors.")]
		public readonly int MinimumRepairDelay = 375;

		[Desc("Only attempt to repair nearby actors with this stance.")]
		public readonly Stance ValidStances = Stance.Ally;

		[Desc("This trait is only enabled if the bot's Name is one of these.")]
		public HashSet<string> EnabledForBotNames = new HashSet<string>();

		[Desc("Delay (in ticks) to clear all reservations.")]
		public readonly int CleanReservationsDelay = 500;

		object ITraitInfo.Create(ActorInitializer init) => new AIEngineerRepair(init.Self, this);
	}

	public class AIEngineerRepair : AITraitBase,
		ITick
	{
		readonly AIEngineerRepairInfo info;
		readonly Player player;
		readonly World world;
		readonly Dictionary<Actor, RepairTarget> reservations = new Dictionary<Actor, RepairTarget>();

		HackyAI ai;
		int minRepairDelayTicks;
		int cleanReservationDelayTicks;
		bool isEnabled;

		public AIEngineerRepair(Actor self, AIEngineerRepairInfo info)
		{
			this.info = info;
			player = self.Owner;
			world = self.World;
		}

		internal override void PostActivate(Player p, HackyAI hackyAi)
		{
			ai = hackyAi;
			isEnabled = info.EnabledForBotNames.Contains(ai.Info.Name)
				&& world.Type == WorldType.Regular
				&& info.ReparingActorTypes.Any();

			minRepairDelayTicks = ai.Random.Next(0, info.MinimumRepairDelay);
			cleanReservationDelayTicks = ai.Random.Next(0, info.CleanReservationsDelay);
			base.PostActivate(p, ai);
		}

		IEnumerable<Actor> GetEngineerRepairers()
		{
			foreach (var actor in world.ActorsHavingTrait<EngineerRepair>())
				if (actor.Owner == player)
					yield return actor;
		}

		void QueueRepairOrders()
		{
			if (player.WinState != WinState.Undefined)
				return;

			if (!info.ReparingActorTypes.Any())
			{
				isEnabled = false;
				return;
			}

			var idleRepairers = GetEngineerRepairers().Where(a => a.IsIdle && !a.IsDead && a.IsInWorld).ToArray();
			if (idleRepairers.Length == 0)
				return;

			var targetsFound = 0;
			foreach (var repairer in idleRepairers)
			{
				var target = GetRepairTargetClosestToOrDefault(repairer);
				if (target != null && target.Actor != null)
				{
					targetsFound++;
					reservations.Add(target.Actor, target);
					QueueRepairOrderFor(repairer, target);
				}
			}
		}

		void QueueRepairOrderFor(Actor repairer, RepairTarget target)
		{
			if (target.Actor.IsDead || !target.Actor.IsInWorld)
				return;

			ai.QueueOrder(new Order(target.OrderString, repairer, true) { TargetActor = target.Actor });
			HackyAI.BotDebug("{0} ({1}): Ordered {2} to repair {3}", ai.Info.Name, player.ClientIndex, repairer, target.Actor);
			reservations.Remove(target.Actor);
		}

		RepairTarget GetRepairTargetClosestToOrDefault(Actor repairer)
		{
			var targets = world.FindActorsInCircle(repairer.CenterPosition, WDist.FromCells(15))
				.Select(t => new RepairTarget(t, (t.CenterPosition - repairer.CenterPosition).HorizontalLength, "EngineerRepair"))
				.Where(target => target.Health != null
					&& target.Health.HP != target.Health.MaxHP
					&& target.Info != null
					&& !reservations.ContainsKey(target.Actor));

			if (info.RepairableActorTypes.Any())
				targets = targets.Where(t => info.RepairableActorTypes.Contains(t.Actor.Info.Name));

			if (!targets.Any())
				return null;

			return targets.MinBy(t => t.Health.HP + t.Distance * 2);
		}

		void ITick.Tick(Actor self)
		{
			if (!isEnabled)
				return;

			if (--cleanReservationDelayTicks <= 0)
			{
				cleanReservationDelayTicks = info.CleanReservationsDelay;
				reservations.Clear();
			}

			if (--minRepairDelayTicks <= 0)
			{
				minRepairDelayTicks = info.MinimumRepairDelay;
				QueueRepairOrders();
			}
		}
	}
}