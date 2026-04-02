using System.Collections;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Resource.Card;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;

namespace Rainworld.Scripts;

public static class Tools
{
    public const CardTag CatStrike_Tag = (CardTag)4532;
    public static async void  getrandomSpear(Boolean upgraded,Player player)
    {
        
        
        IEnumerable<CardModel> distinctForCombat = CardFactory.GetDistinctForCombat(player, from c in ModelDb.CardPool<Rainworld_Liver_CardPool>().GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
            where c.Keywords.Contains(RainworldKeywords.Spear)
            select c, 1, player.RunState.Rng.CombatCardGeneration);
        
        
        foreach (CardModel c in distinctForCombat)
        {
            if(upgraded)
                CardCmd.Upgrade(c);
            await CardPileCmd.AddGeneratedCardToCombat(c, PileType.Hand, true);
            c.AddKeyword(RainworldKeywords.Treasurespear);
            c.AddKeyword(CardKeyword.Exhaust);
            c.AddKeyword(CardKeyword.Retain);

        }

    }
}