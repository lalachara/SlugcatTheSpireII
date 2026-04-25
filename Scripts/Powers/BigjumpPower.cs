using System.Diagnostics.Metrics;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
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
public sealed class BigjumpPower : CustomPowerModel
{

  private List<CardModel> cards = new List<CardModel>();
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;
  
  public void addCard(CardModel card)
  {
    cards.Add(card);
  }


  public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
  {
    if (player == base.Owner.Player)
    {
      IReadOnlyList<CardPileAddResult> combat = await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, Owner.Player);
    }
    cards.Clear();
    await PowerCmd.Remove(this);

  }
}
