using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;

namespace Rainworld.relics;
[Pool(typeof(Rainworld_Liver_RelicPool))]
public sealed class Liver_Fruit2 : CustomRelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;
	// 小图标
	public override string PackedIconPath =>  $"res://Resource/Relics/fruit2.png";
	// 轮廓图标
	protected override string PackedIconOutlinePath =>  $"res://Resource/Relics/outline/fruit2.png";
	// 大图标
	protected override string BigIconPath => $"res://Resource/Relics/fruit2.png";

	
	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
	{
		if (player == base.Owner && combatState.RoundNumber == 1)
		{
			Flash();
			CardModel card = combatState.CreateCard<Rainworld_Liver_Fruit>(base.Owner);
			CardCmd.Upgrade(card);			
			CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, addedByPlayer: true, CardPilePosition.Random));
		}
	}
	
	public override bool TryModifyRestSiteHealRewards(Player player, List<Reward> rewards, bool isMimicked)
	{
		if (player != base.Owner)
		{
			return false;
		}

		if (player.Character is Slugcat)
		{
			SlugcatField.GetSlugCatDataByCreature(Owner.Creature).addMaxWorkLevel(1);
			SlugcatField.playerdata.setworklevel(SlugcatField.playerdata.workLevel+1,false);
			Flash();

		}

		return true;
	}

	
	
}
