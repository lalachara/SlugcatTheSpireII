using Godot;
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
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using Rainworld.Scripts.Powers;
using Rainworld.Scripts.Card.Liver.Attack;
using Rainworld.Scripts.Card.River.Skills;
using Rainworld.Scripts.Telemetry;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Rooms;
using System.Collections.Generic;

namespace SlugcatTheSpireII.Scripts.patches.Singleton;

[RegisterSingleton]
public class CardInCombat: HookedSingletonModel
{
    public CardInCombat() : base(HookType.Combat)
    {
    }

    // === 猫粮使用遥测（仅统计本机玩家每场战斗打出的猫粮次数）===
    // 蓝色果实 / 蓝色？果实 / 泡水果 / 辣条 / 神经元。
    // 单例实例跨战斗复用，故每场战斗开始（BeforeCombatStart）先清零，战斗结束发送后自然归零。
    private int _fruitCount;
    private int _fruit2Count;
    private int _waterFruitCount;
    private int _smallCentCount;
    private int _neuroneCount;

    public override Task BeforeCombatStart()
    {
        // 猫粮计数清零
        _fruitCount = 0;
        _fruit2Count = 0;
        _waterFruitCount = 0;
        _smallCentCount = 0;
        _neuroneCount = 0;
        return Task.CompletedTask;
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
        // 猫粮使用遥测：仅当打出者为本机玩家时累计对应猫粮次数（多人模式不统计他人）。
        if (LocalContext.IsMe(cardPlay.Player))
        {
            switch (cardPlay.Card)
            {
                case Rainworld_Liver_Fruit:
                    _fruitCount++;
                    break;
                case Rainworld_Liver_Fruit2:
                    _fruit2Count++;
                    break;
                case Rainworld_River_Waterfruit:
                    _waterFruitCount++;
                    break;
                case Rainworld_Redcat_Smallcent:
                    _smallCentCount++;
                    break;
                case Rainworld_Liver_Neurone:
                    _neuroneCount++;
                    break;
            }
        }

        if (cardPlay.Card.Keywords.Contains(RainworldKeywords.Treasurespear))
        {
            await Tools.getrandomSpear(cardPlay.Card.IsUpgraded, cardPlay.Card.Owner);
        }

        if (cardPlay.Card.Keywords.Contains(RainworldKeywords.Lightning))
        {
            await Tools.Yinlei( cardPlay,choiceContext);
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

            // === 新增：奇异物质逻辑 ===
            if (card.Keywords.Contains(RainworldKeywords.Strange) && card != cardPlay.Card)
            {
                var allCards = ModelDb.AllCardPools
                    .SelectMany(pool => pool.GetUnlockedCards(card.Owner.UnlockState, card.Owner.RunState.CardMultiplayerConstraint))
                    .Where(c => c.Rarity != CardRarity.Token && c.Type != CardType.Power);
                var distinctForCombat = CardFactory.GetDistinctForCombat(card.Owner, allCards, 1, card.Owner.RunState.Rng.CombatCardGeneration);
                // 去重工厂可能返回空序列，避免 .First() 抛异常卡死
                var randomCard = distinctForCombat.FirstOrDefault();
                if (randomCard == null)
                    return;
                randomCard.AddKeyword(RainworldKeywords.Strange);
                if(card.Keywords.Contains(CardKeyword.Exhaust))
                    randomCard.AddKeyword(CardKeyword.Exhaust);
                else
                    randomCard.RemoveKeyword(CardKeyword.Exhaust);
                await CardCmd.Transform(card, randomCard);
            }
        }
    }

    // 每场战斗结束：若本局至少打出过一种猫粮则上报各猫粮次数；一场都没打出则不发送。
    public override Task AfterCombatEnd(CombatRoom room)
    {
        int total = _fruitCount + _fruit2Count + _waterFruitCount + _smallCentCount + _neuroneCount;
        if (total <= 0)
        {
            return Task.CompletedTask;
        }

        TelemetrySetup.CaptureGameplay("combat_catfood_usage", new Dictionary<string, object?>
        {
            ["fruit"] = _fruitCount,
            ["fruit2"] = _fruit2Count,
            ["waterfruit"] = _waterFruitCount,
            ["smallcent"] = _smallCentCount,
            ["neurone"] = _neuroneCount,
        });

        return Task.CompletedTask;
    }
    
 
}