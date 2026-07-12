﻿﻿﻿using System.Collections;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Resource.Card;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;
using Rainworld.Scripts.Card.RedCat;
using Rainworld.Scripts.Powers;
using Rainworld.relics;

namespace Rainworld.Scripts;

public static class Tools
{
    public static async void  getrandomSpear(Boolean upgraded,Player player)
    {
        
        
        IEnumerable<CardModel> distinctForCombat = CardFactory.GetDistinctForCombat(player, from c in ModelDb.CardPool<Rainworld_Liver_CardPool>().GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
            where c.Keywords.Contains(RainworldKeywords.Spear)
            select c, 1, player.RunState.Rng.CombatCardGeneration);
        
        
        foreach (CardModel c in distinctForCombat)
        {
            if(upgraded)
                CardCmd.Upgrade(c);
            c.AddKeyword(RainworldKeywords.Treasurespear);
            c.AddKeyword(CardKeyword.Exhaust);
            c.AddKeyword(CardKeyword.Retain);
            await CardPileCmd.AddGeneratedCardToCombat(c, PileType.Hand, player);
            

        }

    }

    public static async Task CorruptCard(Player player, CardModel card)
    {
        if(card.Keywords.Contains(RainworldKeywords.Cantcorrupt)||card.Keywords.Contains(RainworldKeywords.Immunity))
            return ;
        if ( player.Creature.HasPower<MisfortunePower>() && card.Type == CardType.Status&& card.CombatState!=null&&card is not Rainworld_Redcat_Corrupt)
        {
            CardModel cardModel = card.CombatState.CreateCard<Rainworld_Redcat_Corrupt>(player);
            await CardCmd.Transform(card, cardModel);
            return;
        }
        CardCmd.ApplyKeyword(card,RainworldKeywords.Corrupt);
    }

    public static async Task WorkReport(Player player)
    {
        if (player.Creature.HasPower<ExcitePower>())
        {
            await CreatureCmd.Damage( new ThrowingPlayerChoiceContext(),  ((IEnumerable<Creature>) player.Creature.GetPower<ExcitePower>().CombatState.HittableEnemies), player.Creature.GetPower<ExcitePower>().Amount, ValueProp.Unpowered, player.Creature);
        }

        if (player.Creature.HasPower<TwistPower>())
        {
            await CardPileCmd.Draw(new BlockingPlayerChoiceContext(), player.Creature.GetPowerAmount<TwistPower>(),player);
        }
        // 在 WorkReport 方法中，Eatblood 那行后面加上：
        foreach (var card in player.Deck.Cards)
        {
            if (card is Rainworld_Redcat_Eatblood eatblood)
                eatblood.AddMisc(eatblood.IsUpgraded ? 3 : 2);

        }
        foreach (CardModel card in player.PlayerCombatState.AllCards)
        {
            if (card is Rainworld_Redcat_Eatblood eatblood)
                eatblood.AddMisc(eatblood.IsUpgraded ? 3 : 2);
            if (card is Rainworld_Redcat_Willflower willflower)       // ← 新增
                willflower.AddMisc( 1);    // ← 新增
            if(card is Rainworld_Redcat_Save save)
                save.EnergyCost.UpgradeBy(-1);    // ← 新增
        }
    }


    public static async Task BackToHand(Player player, CardModel card)
    {
        await CardPileCmd.Add(card, PileType.Hand);
        await BackToHandAciton(player, card);
        
    }

    public static async Task BackToHandAciton(Player player, CardModel card)
    {
        if (player.Creature.HasPower<StudyPower>() &&card.Type==CardType.Attack && card.DynamicVars.Damage.BaseValue != 0)
        {
            card.DynamicVars.Damage.UpgradeValueBy(player.Creature.GetPowerAmount<StudyPower>());
            card.GiveSingleTurnRetain();
        }
    }

    public static async void Yinlei(CardPlay cardPlay,PlayerChoiceContext choiceContext)
    {
       
            var card = cardPlay.Card;
            var cardTargetType = card.TargetType;

            IEnumerable<Creature> targets;
            switch (cardTargetType)
            {
                case TargetType.Self:
                case TargetType.None:
                    targets = new[] { card.Owner.Creature };
                    break;
                case TargetType.AllEnemies:
                    targets = card.CombatState.GetOpponentsOf(card.Owner.Creature).Where(e => e.IsHittable);
                    break;
                default:
                    targets = cardPlay.Target != null ? new[] { cardPlay.Target } : Array.Empty<Creature>();
                    break;
            }

            var lightRowPlayers = card.CombatState.Players
                .Where(p => p.GetRelic<River_Lightrow>() != null)
                .ToList();

            if (targets.Any())
            {
                foreach (var t in targets)
                    VfxCmd.PlayOnCreature(t, "vfx/vfx_attack_lightning");

                var lightRowTargets = targets.Where(t => lightRowPlayers.Any(p => p.Creature == t)).ToList();
                var normalTargets = targets.Where(t => !lightRowPlayers.Any(p => p.Creature == t)).ToList();

                if (normalTargets.Any())
                    await CreatureCmd.Damage(choiceContext, normalTargets, 8m, ValueProp.Unpowered, card.Owner.Creature);
                if (lightRowTargets.Any())
                    await CreatureCmd.Damage(choiceContext, lightRowTargets, 0m, ValueProp.Unpowered, card.Owner.Creature);
            }

            if (targets.Any(t => t.HasPower<CorrodePower>()))
            {
                var corrodeTargets = card.CombatState
                    .GetOpponentsOf(card.Owner.Creature)
                    .Where(e => e.HasPower<CorrodePower>() && e.IsHittable)
                    .ToList();
                if (corrodeTargets.Count > 0)
                {
                    foreach (var t in corrodeTargets)
                        VfxCmd.PlayOnCreature(t, "vfx/vfx_attack_lightning");

                    var lightRowCorrodeTargets = corrodeTargets.Where(t => lightRowPlayers.Any(p => p.Creature == t)).ToList();
                    var normalCorrodeTargets = corrodeTargets.Where(t => !lightRowPlayers.Any(p => p.Creature == t)).ToList();

                    if (normalCorrodeTargets.Any())
                        await CreatureCmd.Damage(choiceContext, normalCorrodeTargets, 8m, ValueProp.Unpowered, card.Owner.Creature);
                    if (lightRowCorrodeTargets.Any())
                        await CreatureCmd.Damage(choiceContext, lightRowCorrodeTargets, 0m, ValueProp.Unpowered, card.Owner.Creature);
                }
            }

            foreach (var player in lightRowPlayers)
                await PowerCmd.Apply<VigorPower>(choiceContext, player.Creature, 8, player.Creature, null);
    }
}