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

using OpenRA.Mods.Common.Traits;
using OpenRA.Scripting;

namespace OpenRA.Mods.Common.Scripting
{
	[ScriptGlobal("Weather")]
	public class WeatherGlobal : ScriptGlobal
	{
		readonly WeatherOverlay weather;
		public bool IsAvailable { get; private set; }

		public WeatherGlobal(ScriptContext context)
			: base(context)
		{
			weather = context.World.WorldActor.TraitOrDefault<WeatherOverlay>();
			IsAvailable = weather != null;
		}

		public double ParticleDensityFactor
		{
			get { return IsAvailable ? weather.ParticleDensityFactor : 0.0007625f; }
			set { if (IsAvailable) weather.ParticleDensityFactor = (float)value; }
		}

		public bool ChangingWindLevel
		{
			get { return IsAvailable ? weather.ChangingWindLevel : true; }
			set { if (IsAvailable) weather.ChangingWindLevel = value; }
		}

		public bool InstantWindChanges
		{
			get { return IsAvailable ? weather.InstantWindChanges : false; }
			set { if (IsAvailable) weather.InstantWindChanges = value; }
		}

		public bool UseSquares
		{
			get { return IsAvailable ? weather.UseSquares : true; }
			set { if (IsAvailable) weather.UseSquares = value; }
		}
	}
}
