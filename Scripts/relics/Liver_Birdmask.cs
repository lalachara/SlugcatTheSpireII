using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;
using Rainworld.Scripts.Powers;

namespace Rainworld.relics;
[Pool(typeof(Rainworld_Liver_RelicPool))]
public sealed class Liver_Birdmask : CustomRelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;
	// 小图标
	public override string PackedIconPath =>  $"res://Resource/Relics/birdmask.png";
	// 轮廓图标
	protected override string PackedIconOutlinePath =>  $"res://Resource/Relics/outline/birdmask.png";
	// 大图标
	protected override string BigIconPath => $"res://Resource/Relics/birdmask.png";

	public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (CombatManager.Instance.IsInProgress && target == base.Owner.Creature && result.UnblockedDamage==0)
		{
			Flash();
			await PowerCmd.Apply<NimblePower>(Owner.Creature, 2, Owner.Creature, null);

		}
	}

}
