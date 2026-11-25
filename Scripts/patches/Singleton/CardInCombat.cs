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

//     // === 猫粮使用遥测（仅统计本机玩家每场战斗打出的猫粮次数）===
//     // 蓝色果实 / 蓝色？果实 / 泡水果 / 辣条 / 神经元。
//     // 单例实例跨战斗复用，故每场战斗开始（BeforeCombatStart）先清零，战斗结束发送后自然归零。
//     private int _fruitCount;
//     private int _fruit2Count;
//     private int _waterFruitCount;
//     private int _smallCentCount;
//     private int _neuroneCount;
//
//     public override Task BeforeCombatStart()
//     {
//         // 猫粮计数清零
//         _fruitCount = 0;
//         _fruit2Count = 0;
//         _waterFruitCount = 0;
//         _smallCentCount = 0;
//         _neuroneCount = 0;
//         return Task.CompletedTask;
//     }
//
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
//         // 猫粮使用遥测：仅当打出者为本机玩家时累计对应猫粮次数（多人模式不统计他人）。
//         if (LocalContext.IsMe(cardPlay.Player))
//         {
//             switch (cardPlay.Card)
//             {
//                 case Rainworld_Liver_Fruit:
//                     _fruitCount++;
//                     break;
//                 case Rainworld_Liver_Fruit2:
//                     _fruit2Count++;
//                     break;
//                 case Rainworld_River_Waterfruit:
//                     _waterFruitCount++;
//                     break;
//                 case Rainworld_Redcat_Smallcent:
//                     _smallCentCount++;
//                     break;
//                 case Rainworld_Liver_Neurone:
//                     _neuroneCount++;
//                     break;
//             }
//         }
//
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

        // 奇异物质/拟态物质互斥：两者互相生成会让同一张牌同时带两个标签，
        // 打出牌时「拟态」和「奇异」两条逻辑会对同一张牌各 Transform 一次，导致卡死。
        bool playedCardHasStrange = cardPlay.Card.Keywords.Contains(RainworldKeywords.Strange);

        foreach (var card in PileType.Hand.GetPile(cardPlay.Card.Owner).Cards.ToList())
        {
            bool transformed = false;

            // 拟态物质：变化为刚打出的牌；刚打出的牌带「奇异物质」时不复制，避免复制出奇异物质
            if (!playedCardHasStrange && card.Keywords.Contains(RainworldKeywords.Simulated) && card != cardPlay.Card)
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
                transformed = true;
            }

            // === 新增：奇异物质逻辑 ===
            // 同一张牌本轮已经变化过就不再走奇异逻辑（即使意外带上了双标签也只会变化一次）
            if (!transformed && card.Keywords.Contains(RainworldKeywords.Strange) && card != cardPlay.Card)
            {
                var allCards = ModelDb.AllCardPools
                    .SelectMany(pool => pool.GetUnlockedCards(card.Owner.UnlockState, card.Owner.RunState.CardMultiplayerConstraint))
                    .Where(c => c.Rarity != CardRarity.Token && c.Type != CardType.Power)
                    // 奇异物质不能变成拟态物质，否则会变成同时带两个标签的牌
                    .Where(c => !c.CanonicalKeywords.Contains(RainworldKeywords.Simulated));
                var distinctForCombat = CardFactory.GetDistinctForCombat(card.Owner, allCards, 1, card.Owner.RunState.Rng.CombatCardGeneration);
                // 去重工厂可能返回空序列或过滤后没有可选牌，避免 .First() 抛异常卡死
                var randomCard = distinctForCombat.FirstOrDefault();
                if (randomCard == null || randomCard.Keywords.Contains(RainworldKeywords.Simulated))
                    continue;   // 这张牌这次不变化，继续处理手中其它牌（原来用 return 会漏掉后面的牌）
                randomCard.AddKeyword(RainworldKeywords.Strange);
                if(card.Keywords.Contains(CardKeyword.Exhaust))
                    randomCard.AddKeyword(CardKeyword.Exhaust);
                else
                    randomCard.RemoveKeyword(CardKeyword.Exhaust);
                await CardCmd.Transform(card, randomCard);
            }
        }
    }

//     // 每场战斗结束：若本局至少打出过一种猫粮则上报各猫粮次数；一场都没打出则不发送。
//     public override Task AfterCombatEnd(CombatRoom room)
//     {
//         int total = _fruitCount + _fruit2Count + _waterFruitCount + _smallCentCount + _neuroneCount;
//         if (total <= 0)
//         {
//             return Task.CompletedTask;
//         }
//
//         TelemetrySetup.CaptureGameplay("combat_catfood_usage", new Dictionary<string, object?>
//         {
//             ["fruit"] = _fruitCount,
//             ["fruit2"] = _fruit2Count,
//             ["waterfruit"] = _waterFruitCount,
//             ["smallcent"] = _smallCentCount,
//             ["neurone"] = _neuroneCount,
//         });
//
//         return Task.CompletedTask;
//     }
//    
 
}