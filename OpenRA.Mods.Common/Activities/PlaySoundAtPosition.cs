using OpenRA.Activities;

namespace OpenRA.Mods.Common
{
	public class PlaySoundAtPosition : Activity
	{
		bool playedSound;
		ISound sound;
		int ticks;

		readonly string soundName;
		readonly WPos position;

		/// <summary>Play a sound at a given position after 0 or more ticks.</summary>
		/// <param name="soundName">Sound name.</param>
		/// <param name="position">Position (self.CenterPosition for example).</param>
		/// <param name="waitTicks">Ticks to wait before playing the sound. Default is 0 (immediate).</param>
		public PlaySoundAtPosition(string soundName, WPos position, int waitTicks = 0)
		{
			this.soundName = soundName;
			this.position = position;
			ticks = waitTicks;
		}

		public override void Queue(Activity activity)
		{
			base.Queue(activity);
		}

		public override void Cancel(Actor self)
		{
			Game.Sound.StopSound(sound);
		}

		public override Activity Tick(Actor self)
		{
			if (!playedSound && --ticks <= 0)
			{
				Play(soundName, position);
				return NextActivity;
			}

			return this;
		}

		public virtual void Play(string soundName, WPos position)
		{
			sound = Game.Sound.Play(soundName, position);
			playedSound = true;
		}
	}
}