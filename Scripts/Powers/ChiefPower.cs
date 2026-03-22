using System.Diagnostics.Metrics;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;

namespace Rainworld.Scripts.Powers;


using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
public sealed class ChiefPower : CustomPowerModel
{
  public override string? CustomPackedIconPath => "res://Resource/Powers/Chief48.png";
  public override string? CustomBigIconPath => "res://Resource/Powers/Chief128.png";
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;

  public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
  {
    modifiedCost = originalCost;
    if (card.Owner.Creature != base.Owner)
    {
      return false;
    }
    if (!card.Keywords.Contains(RainworldKeywords.Treasure))
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
  
  public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
  {
    if (player == base.Owner.Player && base.AmountOnTurnStart >= 1)
    {
      Flash();
      IEnumerable<CardModel> distinctForCombat = CardFactory.GetDistinctForCombat(base.Owner.Player, from c in base.Owner.Player.Character.CardPool.GetUnlockedCards(base.Owner.Player.UnlockState, base.Owner.Player.RunState.CardMultiplayerConstraint)
        
        where ((c.Type==CardType.Attack||c.Type==CardType.Skill)&&(c.Rarity==CardRarity.Common||c.Rarity==CardRarity.Uncommon||c.Rarity==CardRarity.Rare))
        select c, base.AmountOnTurnStart, base.Owner.Player.RunState.Rng.CombatCardGeneration);
      foreach (CardModel c in distinctForCombat)
      {
        c.AddKeyword(RainworldKeywords.Treasure);
        c.AddKeyword(CardKeyword.Exhaust);
      }

      await CardPileCmd.AddGeneratedCardsToCombat(distinctForCombat, PileType.Hand, addedByPlayer: true);
    }
  }
  
 
}
