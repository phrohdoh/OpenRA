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
using System.Collections.Generic;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	public class OperatorInfo : ITraitInfo
	{
		[Desc("The classification of this operator actor.")]
		public readonly string Type = null;

		[Desc("Cursor to display when targeting an Operated actor that this actor can enter.")]
		public readonly string EnterCursor = "enter";

		[Desc("Cursor to display when targeting an Operated actor that this actor can enter.")]
		public readonly string EnterRestrictedCursor = "enter";

		public object Create(ActorInitializer init) { return new Operator(init.Self, this); }
	}

	public class Operator
	{
		public readonly OperatorInfo Info;

		public Operator(Actor self, OperatorInfo info)
		{
			Info = info;
		}
	}

	class OperateOrderTargeter : IOrderTargeter
	{
		public OperateOrderTargeter(string order, int priority)
		{
			OrderID = order;
			OrderPriority = priority;
		}

		bool CanTargetActor(Actor self, Actor target, TargetModifiers modifiers, ref string cursor)
		{
			if (target.IsDead || !target.IsInWorld)
				return false;

			var allied = self.Owner.IsAlliedWith(target.EffectiveOwner.Owner);

			if (!allied)
				return false;

			var operated = target.TraitOrDefault<Operated>();
			if (operated == null || operated.IsBeingOperated)
				return false;

			if (allied && !operated.Info.AllowFriendlyOperators)
				return false;

			var oper = self.Trait<Operator>();
			var canEnter = operated.Info.OperatorTypes.Contains(oper.Info.Type);

			cursor = canEnter ? oper.Info.EnterCursor : oper.Info.EnterRestrictedCursor;
			return canEnter;
		}

		bool CanTargetActor(Actor self, FrozenActor target, TargetModifiers modifiers, ref string cursor)
		{
			if (!target.IsValid)
				return false;

			var allied = self.Owner.IsAlliedWith(target.Owner);
			if (!allied)
				return false;

			var operated = target.Info.Traits.GetOrDefault<OperatedInfo>();
			if (operated == null)
				return false;

			if (allied && !operated.AllowFriendlyOperators)
				return false;

			var oper = self.Trait<Operator>();
			var canEnter = operated.OperatorTypes.Contains(oper.Info.Type);

			cursor = canEnter ? oper.Info.EnterCursor : oper.Info.EnterRestrictedCursor;
			return canEnter;
		}

		public bool CanTarget(Actor self, Target target, List<Actor> othersAtTarget, TargetModifiers modifiers, ref string cursor)
		{
			switch (target.Type)
			{
				case TargetType.Actor:
					return CanTargetActor(self, target.Actor, modifiers, ref cursor);
				case TargetType.FrozenActor:
					return CanTargetActor(self, target.FrozenActor, modifiers, ref cursor);
				default:
					return false;
			}
		}

		public string OrderID { get; private set; }
		public int OrderPriority { get; private set; }
		public bool OverrideSelection { get { return true; } }
		public bool IsQueued { get; protected set; }
	}
}