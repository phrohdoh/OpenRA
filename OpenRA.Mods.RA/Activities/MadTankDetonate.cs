using System;
using System.Linq;
using OpenRA.Activities;
using OpenRA.GameRules;
using OpenRA.Mods.RA.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.RA.Activities
{
	public class MadTankDetonate : Activity
	{
		readonly Target target;
		readonly MadTankInfo info;

		bool playedChargeSound;
		bool playedDetonateSound;
		int chargeTicks;
		int detonateTicks;

		bool hasDetonated;

		/// <summary><para>Requires the `MadTank` trait!</para>
		/// Ejects a new actor (driver), then plays a charge sound and detonate sound after given delays.</summary>
		/// <param name="self">The detonating MadTank actor.</param>
		/// <param name="target">The target (terrain, actor).</param>
		/// <param name="info">The `MadTankInfo` of the trait queuing this activity.</param>
		public MadTankDetonate(Actor self, Target target, MadTankInfo info)
		{
			this.target = target;
			this.info = info;

			chargeTicks = info.ChargeDelay;
			detonateTicks = info.DetonationDelay;
		}

		public override Activity Tick(Actor self)
		{
			if (!playedChargeSound && --chargeTicks <= 0)
			{
				Game.Sound.Play(info.ChargeSound, self.CenterPosition);
				playedChargeSound = true;
			}

			if (playedChargeSound && !playedDetonateSound && --detonateTicks <= 0)
			{
				Game.Sound.Play(info.DetonationSound, self.CenterPosition);
				playedDetonateSound = true;
			}

			if (playedChargeSound && playedDetonateSound)
				DetonateInFrameEndTask(self);

			return hasDetonated ? NextActivity : this;
		}

		void EjectDriver()
		{
			// ...
		}

		void DetonateInFrameEndTask(Actor self)
		{
			self.World.AddFrameEndTask(w => Detonate(self));
		}

		void Detonate(Actor self)
		{
			if (self.IsDead || hasDetonated)
				return;

			if (info.DetonationWeaponInfo != null)
			{
				// Use .FromPos since this actor is killed we cannot use Target.FromActor().
				info.DetonationWeaponInfo.Impact(Target.FromPos(self.CenterPosition), self, Enumerable.Empty<int>());
			}

			self.Kill(self);
			hasDetonated = true;
		}
	}
}
