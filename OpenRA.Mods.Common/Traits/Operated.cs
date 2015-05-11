// #region Copyright & License Information
// /*
//  * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
//  * This file is part of OpenRA, which is free software. It is made
//  * available to you under the terms of the GNU General Public License
//  * as published by the Free Software Foundation. For more information,
//  * see COPYING.
//  */
// #endregion
using System;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("This actor is controlled by an internal actor.")]
	public class OperatedInfo : ITraitInfo
	{
		[ActorReference]
		[Desc("Actor types that can operate this actor.")]
		public readonly string OperatorTypes = { };

		[Desc("Can friendly players order their Operator actors to enter this actor?")]
		public readonly bool AllowFriendlyOperators = true;

		public object Create(ActorInitializer init) { return new Operated(init.Self, this); }
	}

	public class Operated
	{
		public readonly OperatedInfo Info;
	
		public Operated(Actor self, OperatedInfo info)
		{
			Info = info;
		}

		public bool IsBeingOperated
		{
			get { }
		}
	}
}