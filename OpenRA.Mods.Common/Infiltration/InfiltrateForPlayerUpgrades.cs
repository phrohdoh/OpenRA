#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{

	[Desc("Grants upgrades to either the infiltrating actor's owner, or this actor's owner.")]
	public class InfiltrateForPlayerUpgradesInfo : ITraitInfo
	{
		[Desc("Upgrades to grant when this actor is infiltrated."), UpgradeGrantedReference, FieldLoader.Require]
		public readonly string[] Upgrades = { };

		[DescAttribute("Apply the listed Upgrades to this actor's owner? If false, apply to the infiltrator's owner.")]
		public readonly bool ApplyToSelf = true;

		public object Create(ActorInitializer init) { return new InfiltrateForPlayerUpgrades(this); }
	}

	public class InfiltrateForPlayerUpgrades : INotifyInfiltrated
	{
		readonly InfiltrateForPlayerUpgradesInfo info;

		public InfiltrateForPlayerUpgrades(InfiltrateForPlayerUpgradesInfo info)
		{
			this.info = info;
		}

		public void Infiltrated(Actor self, Actor infiltrator)
		{
			var pa = info.ApplyToSelf ? self.Owner.PlayerActor : infiltrator.Owner.PlayerActor;
			var um = pa.TraitOrDefault<UpgradeManager>();

			if (um == null)
				return;

			foreach (var up in info.Upgrades)
				um.GrantUpgrade(pa, up, pa);
		}
	}
}