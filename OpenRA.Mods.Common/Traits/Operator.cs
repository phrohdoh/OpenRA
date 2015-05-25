#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using OpenRA.Mods.Common.Orders;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	public class OperatorInfo : ITraitInfo
	{
		[Desc("The classification of this operator actor.")]
		public readonly string Type = "any";

		[Desc("Cursor to display when targeting a valid Operated actor.")]
		public readonly string EnterCursor = "enter";

		[Desc("Cursor to display when targeting an invalid Operated actor.")]
		public readonly string EnterRestrictedCursor = "enter-blocked";

		public object Create(ActorInitializer init) { return new Operator(init.Self, this); }
	}

	public class Operator : IIssueOrder, IOrderVoice
	{
		public readonly OperatorInfo Info;
		public Actor Operating { get; private set; }

		public Operator(Actor self, OperatorInfo info)
		{
			Info = info;
		}

		public Order IssueOrder(Actor self, IOrderTargeter order, Target target, bool queued)
		{
			if (order.OrderID == "Enter")
				return new Order(order.OrderID, self, queued) { TargetActor = target.Actor };

			return null;
		}

		public IEnumerable<IOrderTargeter> Orders
		{
			get
			{
				yield return new OperatorOrderTargeter("Enter", 5,
					target => CanOperate(target) ? Info.EnterCursor : Info.EnterRestrictedCursor);
			}
		}

		static bool CanOperate(Actor target)
		{
			var operated = target.TraitOrDefault<Operated>();
			if (operated == null)
				return false;

			return !operated.HasOperator;
		}

		public string VoicePhraseForOrder(Actor self, Order order)
		{
			var target = order.TargetActor;
			return target != null && CanOperate(target) ? "Enter" : null;
		}
	}

	public class OperatorOrderTargeter : UnitOrderTargeter
	{
		readonly Func<Actor, string> cursor;

		public OperatorOrderTargeter(string order, int priority, Func<Actor, string> cursor)
			: base(order, priority, "enter", true, true)
		{
			this.cursor = cursor;
		}
		public override bool CanTargetActor(Actor self, Actor target, TargetModifiers modifiers, ref string cursor)
		{
			if (target.IsDead || !target.IsInWorld)
				return false;
			
			var operated = target.TraitOrDefault<Operated>();
			if (operated == null)
				return false;

			if (operated.HasOperator)
				return false;

			IsQueued = modifiers.HasModifier(TargetModifiers.ForceQueue);
			cursor = this.cursor(target);
			return true;
		}

		public override bool CanTargetFrozenActor(Actor self, FrozenActor target, TargetModifiers modifiers, ref string cursor)
		{
			var info = target.Info.Traits.GetOrDefault<OperatedInfo>();
			return info != null && info.CanBeEnteredBy(self);
		}
	}
}