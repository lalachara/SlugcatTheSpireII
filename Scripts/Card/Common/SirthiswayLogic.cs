using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Card.Colorless;
using Rainworld.Scripts.Card.Liver.Attack;
using Rainworld.Scripts.Card.Liver.Skills;
using Rainworld.Scripts.Card.Monk;
using Rainworld.Scripts.Card.RedCat;
using Rainworld.Scripts.Card.River.Attack;
using Rainworld.Scripts.Monster.Minions;
using Rainworld.Scripts.Powers;
using SlugcatTheSpireII.Scripts;
using SlugcatTheSpireII.Scripts.Card.Monk.Attack;

namespace Rainworld.Scripts.Card.Common;

/// <summary>
/// 仙人指路（Sirthisway）的通用效果逻辑，供四种蛞蝓猫颜色卡共用，以 Liver 版为准。
/// </summary>
public static class SirthiswayLogic
{
    public static async Task OnPlay(CardModel cardModel, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await RandomEvent(cardModel, choiceContext, cardPlay);
        SfxCmd.Play("event:/Rainworld/sfx/xianren");
        if (cardModel.Keywords.Contains(RainworldKeywords.Backtohand))
            await Tools.BackToHand(cardModel.Owner, cardModel);
    }

    public static void OnUpgrade(CardModel cardModel)
    {
        cardModel.EnergyCost.UpgradeBy(-1);
    }

    private static async Task RandomEvent(CardModel cardModel, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var random = cardModel.Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 21);
        switch (random)
        {
            case 0: //1业力
                if (cardModel.Owner.Character is Slugcat)
                    await SlugcatData.addworklevel(cardModel.Owner, 1);
                else
                    await rock(cardModel, choiceContext, cardModel.IsUpgraded ? 25 : 15);
                break;
            case 1: //抽牌
                await CardPileCmd.Draw(choiceContext, cardModel.IsUpgraded ? 4 : 3, cardModel.Owner);
                break;
            case 2: //制衡
                IEnumerable<CardModel> cards = PileType.Hand.GetPile(cardModel.Owner).Cards;
                var cardsToDraw = cards.Count();
                await CardCmd.DiscardAndDraw(choiceContext, cards, cardsToDraw);
                break;
            case 3: //获得能量
                await PlayerCmd.GainEnergy(cardModel.IsUpgraded ? 3 : 2, cardModel.Owner);
                break;
            case 4: //格挡
                await CreatureCmd.GainBlock(cardModel.Owner.Creature,
                    new BlockVar(cardModel.IsUpgraded ? 16 : 12, ValueProp.Unpowered), cardPlay);
                break;
            case 5: //获得随机牌
                await drawcard(cardModel);
                break;
            case 6: //造成aoe伤害
                await rock(cardModel, choiceContext, cardModel.IsUpgraded ? 30 : 20);
                break;
            case 7: //debuff
                await applydebuff(cardModel, choiceContext);
                break;
            case 8: //金币
                await PlayerCmd.GainGold(cardModel.IsUpgraded ? 20 : 10, cardModel.Owner);
                break;
            case 9: //悔恨和仙人指路
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(
                    cardModel.CreateClone(), PileType.Hand, cardModel.Owner));
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(
                    cardModel.CombatState.CreateCard<Regret>(cardModel.Owner), PileType.Hand, cardModel.Owner));
                break;
            case 10: //弃置所有手牌并获得奇怪的东西
                await discardand(cardModel, choiceContext, cardPlay);
                break;
            case 11: //铸造1召唤1黑暗球1
                await OstyCmd.Summon(choiceContext, cardModel.Owner, 1, cardModel);
                await ForgeCmd.Forge(1, cardModel.Owner, cardModel);
                await OrbCmd.Channel<DarkOrb>(choiceContext, cardModel.Owner);
                break;
            case 12:
                cardModel.BaseReplayCount += 1;
                await discardand(cardModel, choiceContext, cardPlay);
                await drawcard(cardModel);
                break;
            case 13: //这张牌获得重放
                cardModel.BaseReplayCount += 2;
                await drawcard(cardModel);
                break;
            case 14: //巨石
                await rock(cardModel, choiceContext, 1);
                break;
            case 15://释放2次
                await RandomEvent(cardModel, choiceContext, cardPlay);
                await RandomEvent(cardModel, choiceContext, cardPlay);
                await RandomEvent(cardModel, choiceContext, cardPlay);
                break;
            case 16: //对自己造成15伤害
                await CreatureCmd.Damage(choiceContext, cardModel.Owner.Creature,
                    new DamageVar(15m, ValueProp.Unpowered), cardModel.Owner.Creature);
                break;
            case 18: //小概率事件随机
                await bigevnet(cardModel, choiceContext, cardPlay);
                break;
            case 19: //所有手牌+1费获得保留
                foreach (var card in PileType.Hand.GetPile(cardModel.Owner).Cards.ToList())
                    if (card.Type == CardType.Attack || card.Type == CardType.Skill)
                    {
                        card.EnergyCost.SetThisCombat(card.EnergyCost.Canonical + 1);
                        card.AddKeyword(CardKeyword.Retain);
                    }
                break;
            case 17://随机执行一个效果
                await RandomEvent(cardModel, choiceContext, cardPlay);
                await ExEvent(cardModel);
                break;
            case 20: //随机召唤一个伙伴
                await SummonRandomFriend(cardModel, choiceContext);
                break;
        }
    }

    /// <summary>
    /// 随机召唤一只伙伴：等概率从「所有蜥蜴（含腐化蜥蜴）+ 4 种猫崽 + 拾荒者」里抽一只。
    /// 召唤走和普通伙伴卡相同的 <see cref="FriendSummonCmd" /> 链路（等级/站位/召唤能力都被正确初始化）。
    /// </summary>
    private static async Task SummonRandomFriend(CardModel cardModel, PlayerChoiceContext choiceContext)
    {
        var owner = cardModel.Owner;
        var position = FriendPositions.Next(owner);

        switch (owner.RunState.Rng.CombatCardGeneration.NextInt(0, 11))
        {
            case 0:
                await FriendSummonCmd.Summon<FGreenCab>(choiceContext, owner, new FriendSummonOptions(Source: cardModel, Position: position));
                break;
            case 1:
                await FriendSummonCmd.Summon<FBlueCab>(choiceContext, owner, new FriendSummonOptions(Source: cardModel, Position: position));
                break;
            case 2:
                await FriendSummonCmd.Summon<FFireCab>(choiceContext, owner, new FriendSummonOptions(Source: cardModel, Position: position));
                break;
            case 3:
                await FriendSummonCmd.Summon<FSugarCab>(choiceContext, owner, new FriendSummonOptions(Source: cardModel, Position: position));
                break;
            case 4:
                await FriendSummonCmd.Summon<FYellowCab>(choiceContext, owner, new FriendSummonOptions(Source: cardModel, Position: position));
                break;
            case 5:
                await FriendSummonCmd.Summon<FCorruptLizard>(choiceContext, owner, new FriendSummonOptions(Source: cardModel, Position: position));
                break;
            case 6:
                await FriendSummonCmd.Summon<FKittyLiver>(choiceContext, owner, new FriendSummonOptions(Source: cardModel, Position: position));
                break;
            case 7:
                await FriendSummonCmd.Summon<FKittyRedcat>(choiceContext, owner, new FriendSummonOptions(Source: cardModel, Position: position));
                break;
            case 8:
                await FriendSummonCmd.Summon<FKittyMonk>(choiceContext, owner, new FriendSummonOptions(Source: cardModel, Position: position));
                break;
            case 9:
                await FriendSummonCmd.Summon<FKittyRiver>(choiceContext, owner, new FriendSummonOptions(Source: cardModel, Position: position));
                break;
            default:
                await FriendSummonCmd.Summon<FMonkey>(choiceContext, owner, new FriendSummonOptions(Source: cardModel, Position: position));
                break;
        }
    }

    private static async Task ExEvent(CardModel cardModel)
    {
        var random = cardModel.Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 7);
        switch (random)
        {
            case 0:
                await CardPileCmd.AddGeneratedCardToCombat(cardModel.CombatState!.CreateCard<Rainworld_Turtal>(cardModel.Owner), PileType.Draw, cardModel.Owner, CardPilePosition.Random);
                break;
            case 1:
                foreach (var card in cardModel.Owner.Deck.Cards)
                    card.AddKeyword(CardKeyword.Eternal);
                break;
            case 2:
                var bag = cardModel.CombatState!.CreateCard<Rainworld_Liver_Bag>(cardModel.Owner);
                bag.Cards.Clear();
                bag.Cards.Add(cardModel.CombatState.CreateCard<Regret>(cardModel.Owner));
                await CardPileCmd.AddGeneratedCardToCombat(bag, PileType.Draw, cardModel.Owner, CardPilePosition.Random);
                break;
            case 3:
                cardModel.AddKeyword(RainworldKeywords.Backtohand);
                break;
            case 4:
                cardModel.AddKeyword(CardKeyword.Retain);
                break;
            case 5:
                var relics = cardModel.Owner.Relics.ToList();
                if (relics.Count > 0)
                {
                    var randomRelic = relics[cardModel.Owner.RunState.Rng.CombatCardGeneration.NextInt(0, relics.Count)];
                    if (randomRelic.Rarity != RelicRarity.Ancient && randomRelic.Rarity != RelicRarity.Starter && randomRelic.Rarity != RelicRarity.Event)
                        cardModel.Owner.RemoveRelicInternal(randomRelic);
                }
                break;
            case 6: //正负电荷
                await CardPileCmd.AddGeneratedCardToCombat(cardModel.CombatState!.CreateCard<Rainworld_Elec1>(cardModel.Owner), PileType.Draw, cardModel.Owner, CardPilePosition.Random);
                await CardPileCmd.AddGeneratedCardToCombat(cardModel.CombatState!.CreateCard<Rainworld_Elec2>(cardModel.Owner), PileType.Draw, cardModel.Owner, CardPilePosition.Random);
                break;
        }
    }

    private static async Task applydebuff(CardModel cardModel, PlayerChoiceContext choiceContext)
    {
        var random = cardModel.Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 13);
        switch (random)
        {
            case 0: //3层灾厄
                await PowerCmd.Apply<DoomPower>(choiceContext, cardModel.Owner.Creature, 3, cardModel.Owner.Creature, cardModel);
                break;
            case 1: //3层创伤
                await PowerCmd.Apply<ChuangPower>(choiceContext, cardModel.Owner.Creature, 2, cardModel.Owner.Creature, cardModel);
                break;
            case 2: //魂缚
                if (cardModel.Owner.Creature.GetPower<ChainsOfBindingPower>() == null)
                    await PowerCmd.Apply<ChainsOfBindingPower>(choiceContext, cardModel.Owner.Creature, 1, cardModel.Owner.Creature, cardModel);
                else
                    await PowerCmd.Apply<DoomPower>(choiceContext, cardModel.Owner.Creature, 3, cardModel.Owner.Creature, cardModel);
                break;
            case 3: //柔嫩
                if (cardModel.Owner.Creature.GetPower<TenderPower>() == null)
                    await PowerCmd.Apply<TenderPower>(choiceContext, cardModel.Owner.Creature, 1, cardModel.Owner.Creature, cardModel);
                else
                    await PowerCmd.Apply<DoomPower>(choiceContext, cardModel.Owner.Creature, 3, cardModel.Owner.Creature, cardModel);
                break;
            case 4: //烟雾弥漫
                if (cardModel.Owner.Creature.GetPower<SmoggyPower>() == null)
                    await PowerCmd.Apply<SmoggyPower>(choiceContext, cardModel.Owner.Creature, 1, cardModel.Owner.Creature, cardModel);
                else
                    await PowerCmd.Apply<DoomPower>(choiceContext, cardModel.Owner.Creature, 3, cardModel.Owner.Creature, cardModel);
                break;
            case 5: //因果
                await PowerCmd.Apply<BecausePower>(choiceContext, cardModel.Owner.Creature, 1, cardModel.Owner.Creature, cardModel);
                break;
            case 6: //99虚弱易伤脆弱
                await PowerCmd.Apply<WeakPower>(choiceContext, cardModel.Owner.Creature, 99, cardModel.Owner.Creature, cardModel);
                await PowerCmd.Apply<FrailPower>(choiceContext, cardModel.Owner.Creature, 99, cardModel.Owner.Creature, cardModel);
                await PowerCmd.Apply<VulnerablePower>(choiceContext, cardModel.Owner.Creature, 99, cardModel.Owner.Creature, cardModel);
                break;
            case 7: //99层水蚀
                await PowerCmd.Apply<CorrodePower>(choiceContext, cardModel.Owner.Creature, 99, cardModel.Owner.Creature, cardModel);
                break;
            case 8: //-99力量
                await PowerCmd.Apply<StrengthPower>(choiceContext, cardModel.Owner.Creature, -99, cardModel.Owner.Creature, cardModel);
                break;
            case 9://-99敏捷
                await PowerCmd.Apply<DexterityPower>(choiceContext, cardModel.Owner.Creature, -99, cardModel.Owner.Creature, cardModel);
                break;
            case 10: //1层昏眩
                await PowerCmd.Apply<RingingPower>(choiceContext, cardModel.Owner.Creature, 1, cardModel.Owner.Creature, cardModel);
                break;
            case 11://芝士三件套
                await PowerCmd.Apply<SlothPower>(choiceContext, cardModel.Owner.Creature, 3, cardModel.Owner.Creature, cardModel);
                await PowerCmd.Apply<WasteAwayPower>(choiceContext, cardModel.Owner.Creature, 1, cardModel.Owner.Creature, cardModel);
                await PowerCmd.Apply<MindRotPower>(choiceContext, cardModel.Owner.Creature, 1, cardModel.Owner.Creature, cardModel);
                break;
            case 12://乐嘉+沙漏+沙虫的支配
                await PowerCmd.Apply<InsatiableDominionPower>(choiceContext, cardModel.Owner.Creature, 1, cardModel.Owner.Creature, cardModel);
                await PowerCmd.Apply<AeonglassDominionPower>(choiceContext, cardModel.Owner.Creature, 1, cardModel.Owner.Creature, cardModel);
                await PowerCmd.Apply<LagavulinMatriarchDominionPower>(choiceContext, cardModel.Owner.Creature, 1, cardModel.Owner.Creature, cardModel);
                break;
        }
    }

    private static async Task drawcard(CardModel cardModel)
    {
        var random = cardModel.Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 14);
        switch (random)
        {
            case 0: //3张黏液
                for (var i = 0; i < 3; i++)
                    CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(cardModel.CombatState!.CreateCard<Slimed>(cardModel.Owner), PileType.Hand, cardModel.Owner));
                break;
            case 10: //印1张随机牌
                var distinctForCombat = CardFactory.GetDistinctForCombat(cardModel.Owner,
                    from c in cardModel.Owner.Character.CardPool.GetUnlockedCards(cardModel.Owner.UnlockState,
                        cardModel.Owner.RunState.CardMultiplayerConstraint)
                    where (c.Type == CardType.Attack || c.Type == CardType.Skill) && (c.Rarity == CardRarity.Common ||
                        c.Rarity == CardRarity.Uncommon || c.Rarity == CardRarity.Rare)
                    select c, 1, cardModel.Owner.RunState.Rng.CombatCardGeneration);
                await CardPileCmd.AddGeneratedCardsToCombat(distinctForCombat, PileType.Hand, cardModel.Owner);
                break;
            case 1: //印3张相同的随机牌
            {
                CardModel? sameRandomCard = CardFactory.GetDistinctForCombat(cardModel.Owner,
                    from c in cardModel.Owner.Character.CardPool.GetUnlockedCards(cardModel.Owner.UnlockState,
                        cardModel.Owner.RunState.CardMultiplayerConstraint)
                    where (c.Type == CardType.Attack || c.Type == CardType.Skill) && (c.Rarity == CardRarity.Common ||
                        c.Rarity == CardRarity.Uncommon || c.Rarity == CardRarity.Rare)
                    select c, 1, cardModel.Owner.RunState.Rng.CombatCardGeneration).FirstOrDefault();
                if (sameRandomCard != null)
                {
                    for (int i = 0; i < 3; i++)
                        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(
                            sameRandomCard.CreateClone(), PileType.Hand, cardModel.Owner));
                }
                break;
            }
            case 9: //+12 凋萎
            {
                Wither wither = cardModel.CombatState!.CreateCard<Wither>(cardModel.Owner);
                for (int i = 0; i < 12; i++)
                    wither.FakeUpgrade();
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(wither, PileType.Hand, cardModel.Owner));
                break;
            }
#if STS2_0_113_0 || STS2_0_114_0
            case 11: //随机塔罗牌
            {
                var tarots = TarotCards.All;
                CardModel tarotCanonical = cardModel.Owner.RunState.Rng.CombatCardGeneration.NextItem(tarots) ?? tarots[0];
                CardModel tarot = cardModel.CombatState!.CreateCard(tarotCanonical, cardModel.Owner);
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(tarot, PileType.Hand, cardModel.Owner));
                break;
            }
#endif
#if !STS2_0_113_0 && !STS2_0_114_0
            case 11: //111 没有塔罗牌：第 12 档（索引 11）给电流相生
#endif
            case 12: //电流相生（重放9）+ 闪电充能球 + 失去 8 集中
            {
                CardModel voltaic = cardModel.CombatState!.CreateCard<Voltaic>(cardModel.Owner);
                voltaic.BaseReplayCount = 9;
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(voltaic, PileType.Hand, cardModel.Owner));

                var orbContext = new BlockingPlayerChoiceContext();
                await OrbCmd.Channel<LightningOrb>(orbContext, cardModel.Owner);
                await PowerCmd.Apply<FocusPower>(orbContext, cardModel.Owner.Creature, -8, cardModel.Owner.Creature, cardModel);
                break;
            }
            case 2: //3猫咬
                for (var i = 0; i < 3; i++)
                    CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(cardModel.CombatState.CreateCard<Rainworld_Liver_Cateat>(cardModel.Owner), PileType.Hand, cardModel.Owner));
                break;
            case 3: //3wawa
            {
                var card1 = cardModel.CombatState!.CreateCard<Rainworld_Liver_Wawawa>(cardModel.Owner);
                CardCmd.Upgrade(card1);
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card1, PileType.Hand, cardModel.Owner));
                var card2 = cardModel.CombatState.CreateCard<Rainworld_Redcat_Wawawa>(cardModel.Owner);
                CardCmd.Upgrade(card2);
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card2, PileType.Hand, cardModel.Owner));
                var card3 = cardModel.CombatState.CreateCard<Rainworld_River_Wawawa>(cardModel.Owner);
                CardCmd.Upgrade(card3);
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card3, PileType.Hand, cardModel.Owner));
                break;
            }
            case 4://神经元
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(cardModel.CombatState.CreateCard<Rainworld_Liver_Neurone>(cardModel.Owner), PileType.Hand, cardModel.Owner));
                break;
            case 5://强猫和快猫
                var strongercat = cardModel.CombatState!.CreateCard<Rainworld_Liver_Strongercat>(cardModel.Owner);
                var quickercat = cardModel.CombatState!.CreateCard<Rainworld_Liver_Quickercat>(cardModel.Owner);
                CardCmd.Upgrade(strongercat);
                CardCmd.Upgrade(quickercat);
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(strongercat, PileType.Hand, cardModel.Owner));
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(quickercat, PileType.Hand, cardModel.Owner));
                break;
            case 6://3张复制
                for (int i = 0; i < 3; i++)
                    CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(cardModel.CreateClone(), PileType.Hand, cardModel.Owner));
                break;
            case 7://3张飞踢
                await PowerCmd.Apply<VigorPower>(new BlockingPlayerChoiceContext(), cardModel.Owner.Creature, 2, cardModel.Owner.Creature, cardModel);
                await PowerCmd.Apply<NimblePower>(new BlockingPlayerChoiceContext(), cardModel.Owner.Creature, 2, cardModel.Owner.Creature, cardModel);
                await PowerCmd.Apply<AdroitcapPower>(new BlockingPlayerChoiceContext(), cardModel.Owner.Creature, 2, cardModel.Owner.Creature, cardModel);
                await PowerCmd.Apply<TubePower>(new BlockingPlayerChoiceContext(), cardModel.Owner.Creature, 1, cardModel.Owner.Creature, cardModel);
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(cardModel.CombatState!.CreateCard<Rainworld_Liver_Kick>(cardModel.Owner), PileType.Hand, cardModel.Owner));
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(cardModel.CombatState.CreateCard<Rainworld_Redcat_Kick>(cardModel.Owner), PileType.Hand, cardModel.Owner));
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(cardModel.CombatState.CreateCard<Rainworld_River_Kick>(cardModel.Owner), PileType.Hand, cardModel.Owner));
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(cardModel.CombatState.CreateCard<Rainworld_Monk_Kick>(cardModel.Owner), PileType.Hand, cardModel.Owner));

                break;
            case 8://4张轨道
                var ORBIT = cardModel.CombatState!.CreateCard<Orbit>(cardModel.Owner);
                CardCmd.Upgrade(ORBIT);
                for (int i = 0; i < 4; i++)
                    CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(ORBIT.CreateClone(), PileType.Hand, cardModel.Owner));
                break;
        }
    }

    private static async Task bigevnet(CardModel cardModel, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var random = cardModel.Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 15);
        switch (random)
        {
            case 0: //巨石
                await rock(cardModel, choiceContext, 10000);
                break;
            case 1: //药水
                await CreatureCmd.TriggerAnim(cardModel.Owner.Creature, "Cast", cardModel.Owner.Character.CastAnimDelay);
                await PotionCmd.TryToProcure(
                    PotionFactory.CreateRandomPotionInCombat(cardModel.Owner, cardModel.Owner.RunState.Rng.CombatPotionGeneration)
                        .ToMutable(), cardModel.Owner);
                break;
            case 2: //加血上限
                if (cardModel.Owner.Character is Slugcat)
                    await CreatureCmd.GainMaxHp(cardModel.Owner.Creature, 5m);
                break;
            case 3: //神化
                foreach (var allCard in cardModel.Owner.PlayerCombatState!.AllCards)
                    if (allCard != cardModel && allCard.IsUpgradable)
                        CardCmd.Upgrade(allCard);
                break;
            case 4: //回满业力
                if (cardModel.Owner.Character is Slugcat)
                    await SlugcatData.addworklevel(cardModel.Owner, 10);
                break;
            case 5: //5种形态
                await PowerCmd.Apply<EchoFormPower>(choiceContext, cardModel.Owner.Creature, 1, cardModel.Owner.Creature, cardModel);
                await PowerCmd.Apply<VoidFormPower>(choiceContext, cardModel.Owner.Creature, 1, cardModel.Owner.Creature, cardModel);
                await PowerCmd.Apply<DemonFormPower>(choiceContext, cardModel.Owner.Creature, 1, cardModel.Owner.Creature, cardModel);
                await PowerCmd.Apply<ReaperFormPower>(choiceContext, cardModel.Owner.Creature, 1, cardModel.Owner.Creature, cardModel);
                await PowerCmd.Apply<WraithFormPower>(choiceContext, cardModel.Owner.Creature, 1, cardModel.Owner.Creature, cardModel);
                await PowerCmd.Apply<SerpentFormPower>(choiceContext, cardModel.Owner.Creature, 1, cardModel.Owner.Creature, cardModel);
                await PowerCmd.Apply<MythicFormPower>(choiceContext, cardModel.Owner.Creature, 1, cardModel.Owner.Creature, cardModel);
                break;
            case 6:
                await discardand(cardModel, choiceContext, cardPlay);
                await drawcard(cardModel);
                await applydebuff(cardModel, choiceContext);
                break;
            case 7://宇宙冷漠
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(cardModel.CombatState!.CreateCard<CosmicIndifference>(cardModel.Owner), PileType.Hand, cardModel.Owner));
                break;
            case 8: //熟能生巧、猫抓
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(cardModel.CombatState!.CreateCard<Rainworld_Redcat_Study>(cardModel.Owner), PileType.Hand, cardModel.Owner));
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(cardModel.CombatState.CreateCard<Rainworld_Redcat_Woundhit>(cardModel.Owner), PileType.Hand, cardModel.Owner));
                break;
            case 9: //99集中黑球
                await PowerCmd.Apply<FocusPower>(choiceContext, cardModel.Owner.Creature, 99, cardModel.Owner.Creature, cardModel);
                await OrbCmd.Channel<DarkOrb>(choiceContext, cardModel.Owner);
                break;
            case 10:
                await OstyCmd.Summon(choiceContext, cardModel.Owner, 99, cardModel);
                break;
            case 11:
                await ForgeCmd.Forge(99, cardModel.Owner, cardModel);
                break;
            case 12:
                await PowerCmd.Apply<MushblockPower>(choiceContext, cardModel.Owner.Creature, 99, cardModel.Owner.Creature, cardModel);
                break;
            case 13:
                await PowerCmd.Apply<IcewaterPower>(choiceContext, cardModel.Owner.Creature, 99, cardModel.Owner.Creature, cardModel);
                break;
            case 14://4张本我1张驱魔
                for (int i = 0; i < 4; i++)
                {
                    CardModel c = cardModel.CombatState!.CreateCard<Rainworld_Monk_Think>(cardModel.Owner);
                    CardCmd.Upgrade(c);
                    CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(c, PileType.Hand, cardModel.Owner));
                }
                CardModel qm = cardModel.CombatState!.CreateCard<Rainworld_Monk_Quitmagic>(cardModel.Owner);
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(qm, PileType.Hand, cardModel.Owner));
                break;
        }
    }

    private static async Task discardand(CardModel cardModel, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardCmd.Discard(choiceContext, PileType.Hand.GetPile(cardModel.Owner).Cards);

        var random = cardModel.Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 9);
        switch (random)
        {
            case 0: //重启和重放仙人指路
                CardModel card = cardModel.CombatState.CreateCard<Rainworld_Liver_Sirthisway>(cardModel.Owner);
                CardCmd.Upgrade(card);
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, cardModel.Owner));
                CardCmd.PreviewCardPileAdd(
                    await CardPileCmd.AddGeneratedCardToCombat(cardModel.CombatState.CreateCard<Reboot>(cardModel.Owner), PileType.Hand,
                        cardModel.Owner));
                break;
            case 1: //3张蛇咬
                for (var i = 0; i < 3; i++)
                    CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(cardModel.CombatState!.CreateCard<Snakebite>(cardModel.Owner),
                        PileType.Hand, cardModel.Owner));
                break;
            case 2: //区和呼唤
                // 0.113：ICombatState.MultiplayerScalingModel 已移除，多人判定改用 RunState.Players
                if (cardModel.RunState is { } runState && runState.Players.Count > 1)
                    CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(
                        cardModel.CombatState.CreateCard<Rainworld_Liver_Treasurebag>(cardModel.Owner), PileType.Hand, cardModel.Owner));
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(cardModel.CombatState!.CreateCard<Infection>(cardModel.Owner), PileType.Hand,
                    cardModel.Owner));
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(cardModel.CombatState.CreateCard<Beckon>(cardModel.Owner), PileType.Hand,
                    cardModel.Owner));
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(cardModel.CombatState.CreateCard<Infection>(cardModel.Owner), PileType.Hand,
                    cardModel.Owner));
                break;
            case 3: //宝物猫库
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(cardModel.CombatState!.CreateCard<Rainworld_Liver_Catbag>(cardModel.Owner), PileType.Hand, cardModel.Owner));
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(cardModel.CombatState!.CreateCard<Arsenal>(cardModel.Owner), PileType.Hand, cardModel.Owner));
                break;
            case 4: //3随机牌
                var distinctForCombat = CardFactory.GetDistinctForCombat(cardModel.Owner,
                    from c in cardModel.Owner.Character.CardPool.GetUnlockedCards(cardModel.Owner.UnlockState,
                        cardModel.Owner.RunState.CardMultiplayerConstraint)
                    where (c.Type == CardType.Attack || c.Type == CardType.Skill) && (c.Rarity == CardRarity.Common ||
                        c.Rarity == CardRarity.Uncommon || c.Rarity == CardRarity.Rare)
                    select c, 3, cardModel.Owner.RunState.Rng.CombatCardGeneration);
                await CardPileCmd.AddGeneratedCardsToCombat(distinctForCombat, PileType.Hand, cardModel.Owner);
                break;
            case 5: //5拟态
                for (int i = 0; i < 5; i++)
                    CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(cardModel.CombatState!.CreateCard<Rainworld_Redcat_Simulated>(cardModel.Owner), PileType.Hand, cardModel.Owner));
                break;
            case 6://5奇异
                for (int i = 0; i < 5; i++)
                    CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(cardModel.CombatState!.CreateCard<Rainworld_Monk_Strange>(cardModel.Owner), PileType.Hand, cardModel.Owner));
                break;
            case 7://
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(
                    cardModel.CombatState!.CreateCard<Brace>(cardModel.Owner), PileType.Hand, cardModel.Owner));
                await PlayerCmd.GainEnergy(99, cardModel.Owner);

                break;
            case 8://10安可
                for (int i = 0; i < 10; i++)
                    CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(cardModel.CombatState!.CreateCard<Encore>(cardModel.Owner), PileType.Hand, cardModel.Owner));
                break;

        }
    }

    private static async Task rock(CardModel cardModel, PlayerChoiceContext choiceContext, int damage)
    {
        var damageTasks = new List<Task>();
        var vfx = NRollingBoulderVfx.Create(cardModel.CombatState!.HittableEnemies, damage);
        vfx!.Connect(NRollingBoulderVfx.SignalName.HitCreature,
            Callable.From(delegate(NCreature c)
            {
                damageTasks.Add(CreatureCmd.Damage(choiceContext, new[] { c.Entity }, damage, ValueProp.Unpowered,
                    cardModel.Owner.Creature));
            }));
        Callable.From(delegate
        {
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
            if (!vfx.IsInsideTree())
                throw new InvalidOperationException("VFX is not inside tree after adding it to combat room!");
        }).CallDeferred();
        await vfx.ToSignal(vfx, Node.SignalName.TreeExiting);
        await Task.WhenAll(damageTasks);
        foreach (var enemy in cardModel.CombatState.HittableEnemies)
            if (enemy.IsAlive)
                return;
        PlayerCmd.EndTurn(cardModel.Owner, false);
    }
}