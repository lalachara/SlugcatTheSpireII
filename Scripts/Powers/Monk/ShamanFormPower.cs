﻿using MegaCrit.Sts2.Core.Combat;
 using MegaCrit.Sts2.Core.Commands;
 using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Resource.Card;
using Rainworld.Scripts;
using Rainworld.Scripts.Card;
using Rainworld.Scripts.Card.Liver.Attack;
using Rainworld.Scripts.Card.RedCat;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;



#nullable enable
[RegisterPower]
public sealed class ShamanFormPower : ModPowerTemplate
{
  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/Chief128.png",
    BigIconPath: "res://Resource/Powers/Chief128.png"
  );
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;
  
  private static readonly Type[] AllSlugcatCardPools =
  {
    typeof(Rainworld_Liver_CardPool),
    typeof(Rainworld_River_CardPool),
    typeof(Rainworld_Redcat_CardPool),
  };

  private List<CardModel> GetOtherSlugcatColorCards()
  {
    var unlockState = Owner.Player.UnlockState;
    var constraint = Owner.Player.RunState.CardMultiplayerConstraint;
    var currentPoolType = Owner.Player.Character.CardPool.GetType();

    var allCards = new List<CardModel>();
    foreach (var poolType in AllSlugcatCardPools)
    {
      if (poolType == currentPoolType)
        continue;
      var getPoolMethod = typeof(ModelDb).GetMethod("CardPool")!.MakeGenericMethod(poolType);
      var pool = getPoolMethod.Invoke(null, null);
      var getCardsMethod = pool!.GetType().GetMethod("GetUnlockedCards")!;
      var cards = (IEnumerable<CardModel>)getCardsMethod.Invoke(pool, new object[] { unlockState, constraint });
      allCards.AddRange(cards);
    }

    return allCards
      .Where(c => (c.Type == CardType.Attack || c.Type == CardType.Skill)
                  && (c.Rarity == CardRarity.Common || c.Rarity == CardRarity.Uncommon || c.Rarity == CardRarity.Rare)
                  && notSpecialCard(c))
      .ToList();
  }
  
  
  public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
  {
    modifiedCost = originalCost;
    if (card.Owner.Creature != base.Owner)
    {
      return false;
    }
    if (!IsOtherSlugcatColor(card))
    {
      return false;
    }
    bool flag;
    switch (card.Pile?.Type)
    {
      case PileType.Hand:
      case PileType.Play:
        flag = true;
        break;
      default:
        flag = false;
        break;
    }
    if (!flag)
    {
      return false;
    }
    modifiedCost = default(decimal);
    return true;
  }
  public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
  {
    if (player == base.Owner.Player && base.AmountOnTurnStart >= 1)
    {
      Flash();
      var otherCards = GetOtherSlugcatColorCards();
      if (otherCards.Count == 0)
        return;
      IEnumerable<CardModel> distinctForCombat = CardFactory.GetDistinctForCombat(
        base.Owner.Player, otherCards, base.AmountOnTurnStart, 
        base.Owner.Player.RunState.Rng.CombatCardGeneration);
      foreach (CardModel c in distinctForCombat)
      {
        CardCmd.Upgrade(c);
        await CardPileCmd.AddGeneratedCardToCombat(c, PileType.Hand, Owner.Player);
      }
    }
  }
  

  private static readonly Dictionary<Type, Type> PoolToCardBase = new()
  {
    { typeof(Rainworld_Liver_CardPool), typeof(LiverCardModel) },
    { typeof(Rainworld_River_CardPool), typeof(RiverCardModel) },
    { typeof(Rainworld_Redcat_CardPool), typeof(RedCatCardModel) },
  };

  private bool IsOtherSlugcatColor(CardModel card)
  {
    var currentPoolType = Owner.Player.Character.CardPool.GetType();
    if (!PoolToCardBase.TryGetValue(currentPoolType, out var myBase))
      return false;
    return PoolToCardBase.Values.Where(t => t != myBase).Any(t => t.IsAssignableFrom(card.GetType()));
  }
  
  public Boolean notSpecialCard(CardModel card)
  {
    return card is not Rainworld_Liver_Neurone &&  card is not Rainworld_Liver_Pearl && card is not Rainworld_Redcat_Besame && card is not Rainworld_Redcat_Leave && card is not Rainworld_Redcat_Eat;
  }


}