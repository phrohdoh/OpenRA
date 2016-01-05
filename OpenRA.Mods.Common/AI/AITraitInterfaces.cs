using System;

namespace OpenRA.Mods.Common.AI
{
	///<summary>Notify the AI player actor that it has trained a new unit (non-structure).</summary>
	public interface INotifyTrainingComplete
	{
		/// <summary>Called once the training is complete and `trained` has entered the world.</summary>
		/// <param name="self">The AI player actor.</param>
		/// <param name="trained">The newly trained actor.</param>
		/// <param name="factory">The factory (barracks, war factory, etc) that produced `trained`.</param>
		void OnTrainingComplete(Actor self, Actor trained, Actor factory);
	}
}