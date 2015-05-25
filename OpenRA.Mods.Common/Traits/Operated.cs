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
using System.Linq;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("This actor is controlled by an internal actor.")]
	public class OperatedInfo : ITraitInfo
	{
		[ActorReference]
		[Desc("`Operator.Type`s that can operate this actor.")]
		public readonly string[] OperatorTypes = { "any" };

		public object Create(ActorInitializer init) { return new Operated(init.Self, this); }

		public bool CanBeEnteredBy(Actor actor)
		{
			if (actor.IsDead || !actor.IsInWorld)
				return false;

			var oper = actor.Info.Traits.GetOrDefault<OperatorInfo>();
			return oper != null && OperatorTypes.Contains(oper.Type);
		}
	}

	public interface INotifyOperatorChanged
	{
		void OnOperatorEntered(Actor self, Actor oper);
		void OnOperatorRemoved(Actor self, Actor oper);
	}

	public class Operated
	{
		public readonly OperatedInfo Info;

		public Actor Operator { get; private set; }

		public bool HasOperator
		{
			get { return Operator != null; }
		}
	
		public Operated(Actor self, OperatedInfo info)
		{
			Info = info;
		}
	}
}