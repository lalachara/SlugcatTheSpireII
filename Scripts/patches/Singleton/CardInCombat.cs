﻿﻿﻿﻿using Godot;
 using MegaCrit.Sts2.Core.Commands;
 using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using Rainworld.Scripts; using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.ValueProps;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using Rainworld.Scripts.Powers;

namespace SlugcatTheSpireII.Scripts.patches.Singleton;

[RegisterSingleton]
public class CardInCombat: HookedSingletonModel
{
    public CardInCombat() : base(HookType.Combat)
    {
    }
    
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Keywords.Contains(RainworldKeywords.Immunity))
        {
            if(card.Owner.Creature.HasPower<RedmushroomPower>())
                await PlayerCmd.LoseEnergy(1, card.Owner);
            else
            { 
                await PlayerCmd.GainEnergy(1, card.Owner);

            }
        }

    }
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Keywords.Contains(RainworldKeywords.Treasurespear))
        {
            Tools.getrandomSpear(cardPlay.Card.IsUpgraded, cardPlay.Card.Owner);
        }

        if (cardPlay.Card.Keywords.Contains(RainworldKeywords.Lightning))
        {
            Tools.Yinlei( cardPlay,choiceContext);
        }
        if (cardPlay.Card.Owner.Character is Rainworld_Redcat)
        {
            if(cardPlay.Card.Keywords.Contains(RainworldKeywords.Corrupt))
               await  SlugcatData.addCorruptValue(cardPlay.Card.Owner, 1);

        }

        foreach (var card in PileType.Hand.GetPile(cardPlay.Card.Owner).Cards.ToList())
        {
            if (card.Keywords.Contains(RainworldKeywords.Simulated) && card != cardPlay.Card)
            {
                var copy = cardPlay.Card.CreateClone();
                copy.AddKeyword(RainworldKeywords.Simulated);
                if(card.Keywords.Contains(CardKeyword.Exhaust))
                    copy.AddKeyword(CardKeyword.Exhaust);
                else
                {
                    copy.RemoveKeyword(CardKeyword.Exhaust);
                }
                await CardCmd.Transform(card, copy);
            }
        }
    }
    
 
}