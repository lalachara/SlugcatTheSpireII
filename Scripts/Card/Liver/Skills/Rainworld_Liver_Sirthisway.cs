﻿using Godot;
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
using Rainworld.Scripts.Card.Liver.Skills;
using Rainworld.Scripts.Card.RedCat;
using Rainworld.Scripts.Card.River.Attack;
using Rainworld.Scripts.Powers;
using SlugcatTheSpireII.Scripts;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Sirthisway : LiverCardModel

{
    // 基础耗能
    private const int energyCost = 1;

    // 卡牌类型
    private const CardType type = CardType.Skill;

    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Uncommon;

    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;
    
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    protected override HashSet<CardTag> CanonicalTags => new() { };
    public override CardAssetProfile AssetProfile => new(
        GetLocalizedCardImage(),
        FramePath: Type switch
        {
            CardType.Attack => "res://Resource/Card/Liver/CardAtk1024.png",
            CardType.Skill => "res://Resource/Card/Liver/CardSkill1024.png",
            CardType.Power => "res://Resource/Card/Liver/CardPower1024.png",
            _ => ""
        }
    );

    private string GetLocalizedCardImage()
    {
        var cardName = GetType().Name.ToLowerInvariant()
            .Replace("slugcat_the_spire_ii_card_", "");
        var locale = TranslationServer.GetLocale(); // 如 "zh", "en"
        string localizedPath = $"res://Resource/Card/Liver/{cardName}.png";
        if (locale == "ko")
        {
            localizedPath = $"res://Resource/Card/diff/KR/{cardName}.png";
        }

        return ResourceLoader.Exists(localizedPath) 
            ? localizedPath 
            : $"res://Resource/Card/Liver/{cardName}.png"; // 回退到默认
    }

    // 卡牌的基础属性（例如这里是12点伤害）
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];

    public Rainworld_Liver_Sirthisway() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
     protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await RandomEvent(choiceContext, cardPlay);
        SfxCmd.Play("event:/Rainworld/sfx/xianren");
        if (Keywords.Contains(RainworldKeywords.Backtohand))
             await Tools.BackToHand(Owner,this);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }


    private async Task RandomEvent(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var random = Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 20);
        switch (random)
        {
            case 0: //1业力
                if (Owner.Character is Slugcat)
                    await SlugcatData.addworklevel(Owner, 1);
                else
                    await rock(choiceContext, IsUpgraded ? 25 : 15);
                break;
            case 1: //抽牌
                await CardPileCmd.Draw(choiceContext, IsUpgraded ? 4 : 3, Owner);
                break;
            case 2: //制衡
                IEnumerable<CardModel> cards = PileType.Hand.GetPile(Owner).Cards;
                var cardsToDraw = cards.Count();
                await CardCmd.DiscardAndDraw(choiceContext, cards, cardsToDraw);
                break;
            case 3: //获得能量
                await PlayerCmd.GainEnergy(IsUpgraded ? 3 : 2, Owner);
                break;
            case 4: //格挡
                await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(IsUpgraded ? 16 : 12, ValueProp.Unpowered),
                    cardPlay);
                break;
            case 5: //获得随机牌
                await drawcard();
                break;
            case 6: //造成aoe伤害
                await rock(choiceContext, IsUpgraded ? 30 : 20);
                break;
            case 7: //debuff
                await applydebuff(choiceContext);
                break;
            case 8: //金币
                await PlayerCmd.GainGold(IsUpgraded ? 20 : 10, Owner);
                break;
            case 9: //悔恨和仙人指路
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(
                    CreateClone(), PileType.Hand, Owner));
                CardCmd.PreviewCardPileAdd(
                    await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Regret>(Owner), PileType.Hand,
                        Owner));
                break;
            case 10: //弃置所有手牌并获得奇怪的东西
                await discardand(choiceContext, cardPlay);
                break;
            case 11: //铸造1召唤1黑暗球1
                await OstyCmd.Summon(choiceContext, Owner, 1, this);
                await ForgeCmd.Forge(1, Owner, this);
                await OrbCmd.Channel<DarkOrb>(choiceContext, Owner);
                break;
            case 12:
                BaseReplayCount += 1;
                await discardand(choiceContext, cardPlay);
                await drawcard();
                break;
            case 13: //这张牌获得重放
                BaseReplayCount += 2;
                await drawcard();
                break;
            case 14: //巨石
                await rock(choiceContext, 1);
                break;
            case 15://释放2次
                await RandomEvent(choiceContext, cardPlay);
                await RandomEvent(choiceContext, cardPlay);
                await RandomEvent(choiceContext, cardPlay);
                break;
            case 16: //对自己造成15伤害
                await CreatureCmd.Damage(choiceContext, Owner.Creature, new DamageVar(15m, ValueProp.Unpowered),
                    Owner.Creature);
                break;
            case 18: //小概率事件随机
                await bigevnet(choiceContext, cardPlay);
                break;
            case 19: //所有手牌+1费获得保留
                foreach (var card in PileType.Hand.GetPile(Owner).Cards.ToList())
                    if (card.Type == CardType.Attack || card.Type == CardType.Skill)
                    {
                        card.EnergyCost.SetThisCombat(card.EnergyCost.Canonical + 1);
                        card.AddKeyword(CardKeyword.Retain);
                    }

                break;
            case 17://随机执行一个效果
                await RandomEvent(choiceContext, cardPlay);
                await ExEvent();
                break;
        }
    }

    private async Task ExEvent()
    {
        var random = Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 7);
        switch (random)
        {
            case 0:
                await CardPileCmd.AddGeneratedCardToCombat(CombatState!.CreateCard<Rainworld_Turtal>(Owner), PileType.Draw, Owner, CardPilePosition.Random);
                break;
            case 1:
                foreach (var card in Owner.Deck.Cards)
                {
                    card.AddKeyword(CardKeyword.Eternal);
                }
                break;
            case 2:
                var bag = CombatState!.CreateCard<Rainworld_Liver_Bag>(Owner);
                bag.Cards.Clear();
                bag.Cards.Add(CombatState.CreateCard<Regret>(Owner));
                await CardPileCmd.AddGeneratedCardToCombat(bag, PileType.Draw, Owner, CardPilePosition.Random);
                break;
            case 3:
                AddKeyword(RainworldKeywords.Backtohand);
                break;
            case 4:
                AddKeyword(CardKeyword.Retain);
                break;
            case 5:
                var relics = Owner.Relics.ToList();
                if (relics.Count > 0)
                {
                    var randomRelic = relics[Owner.RunState.Rng.CombatCardGeneration.NextInt(0, relics.Count)];
                    if(randomRelic.Rarity!=RelicRarity.Ancient&&randomRelic.Rarity!=RelicRarity.Starter&&randomRelic.Rarity!=RelicRarity.Event)
                        Owner.RemoveRelicInternal(randomRelic);
                }
                break;
            case 6: //正负电荷
                await CardPileCmd.AddGeneratedCardToCombat(CombatState!.CreateCard<Rainworld_Elec1>(Owner), PileType.Draw, Owner, CardPilePosition.Random);
                await CardPileCmd.AddGeneratedCardToCombat(CombatState!.CreateCard<Rainworld_Elec2>(Owner), PileType.Draw, Owner, CardPilePosition.Random);

                break;
        }
    }

    private async Task applydebuff(PlayerChoiceContext choiceContext)
    {
        var random = Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 11);

        switch (random)
        {
            case 0: //3层灾厄
                await PowerCmd.Apply<DoomPower>(choiceContext, Owner.Creature, 3, Owner.Creature, this);
                break;
            case 1: //3层创伤
                await PowerCmd.Apply<ChuangPower>(choiceContext, Owner.Creature, 2, Owner.Creature, this);
                break;
            case 2: //魂缚
                if (Owner.Creature.GetPower<ChainsOfBindingPower>() == null)
                    await PowerCmd.Apply<ChainsOfBindingPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
                else
                    await PowerCmd.Apply<DoomPower>(choiceContext, Owner.Creature, 3, Owner.Creature, this);
                break;
            case 3: //柔嫩
                if (Owner.Creature.GetPower<TenderPower>() == null)
                    await PowerCmd.Apply<TenderPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
                else
                    await PowerCmd.Apply<DoomPower>(choiceContext, Owner.Creature, 3, Owner.Creature, this);
                break;
            case 4: //烟雾弥漫
                if (Owner.Creature.GetPower<SmoggyPower>() == null)
                    await PowerCmd.Apply<SmoggyPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
                else
                    await PowerCmd.Apply<DoomPower>(choiceContext, Owner.Creature, 3, Owner.Creature, this);
                break;
            case 5: //因果
                await PowerCmd.Apply<BecausePower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
                break;
            case 6: //99虚弱易伤脆弱
                await PowerCmd.Apply<WeakPower>(choiceContext, Owner.Creature, 99, Owner.Creature, this);
                await PowerCmd.Apply<FrailPower>(choiceContext, Owner.Creature, 99, Owner.Creature, this);
                await PowerCmd.Apply<VulnerablePower>(choiceContext, Owner.Creature, 99, Owner.Creature, this);
                break;
            case 7: //99层水蚀
                await PowerCmd.Apply<CorrodePower>(choiceContext, Owner.Creature, 99, Owner.Creature, this);
                break;
            case 8: //-99力量
                await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, -99, Owner.Creature, this);
                break;
            case 9://-99敏捷
                await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, -99, Owner.Creature, this);
                break;
            case 10: //1层昏眩
                await PowerCmd.Apply<RingingPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
                break;
            // case 11: //1层流电
            //     await PowerCmd.Apply<GalvanicPower>(choiceContext, Owner.Creature, 8, Owner.Creature, this);
            //     break;
        }
    }

    private async Task drawcard()
    {
        var random = Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 11);

        switch (random)
        {
            case 0: //3张黏液
                for (var i = 0; i < 3; i++)
                    CardCmd.PreviewCardPileAdd(
                        await CardPileCmd.AddGeneratedCardToCombat(CombatState!.CreateCard<Slimed>(Owner), PileType.Hand,
                            Owner));
                break;
            case 1: //印随机牌
            case 9:
            case 10:
                var distinctForCombat = CardFactory.GetDistinctForCombat(Owner,
                    from c in Owner.Character.CardPool.GetUnlockedCards(Owner.UnlockState,
                        Owner.RunState.CardMultiplayerConstraint)
                    where (c.Type == CardType.Attack || c.Type == CardType.Skill) && (c.Rarity == CardRarity.Common ||
                        c.Rarity == CardRarity.Uncommon || c.Rarity == CardRarity.Rare)
                    select c, 1, Owner.RunState.Rng.CombatCardGeneration);
                await CardPileCmd.AddGeneratedCardsToCombat(distinctForCombat, PileType.Hand, Owner);
                break;
            case 2: //3猫咬
                for (var i = 0; i < 3; i++)
                    CardCmd.PreviewCardPileAdd(
                        await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Rainworld_Liver_Cateat>(Owner), PileType.Hand,
                            Owner));
                break;
            case 3: //3wawa
            {
                var card1 = CombatState!.CreateCard<Rainworld_Liver_Wawawa>(Owner);
                CardCmd.Upgrade(card1);
                CardCmd.PreviewCardPileAdd(
                    await CardPileCmd.AddGeneratedCardToCombat(card1, PileType.Hand, Owner));
                var card2 = CombatState.CreateCard<Rainworld_Redcat_Wawawa>(Owner);
                CardCmd.Upgrade(card2);
                CardCmd.PreviewCardPileAdd(
                    await CardPileCmd.AddGeneratedCardToCombat(card2, PileType.Hand, Owner));
                var card3 = CombatState.CreateCard<Rainworld_River_Wawawa>(Owner);
                CardCmd.Upgrade(card3);
                CardCmd.PreviewCardPileAdd(
                    await CardPileCmd.AddGeneratedCardToCombat(card3, PileType.Hand, Owner));
                break;
            }
            case 4://神经元
                CardCmd.PreviewCardPileAdd(
                    await CardPileCmd.AddGeneratedCardToCombat( CombatState.CreateCard<Rainworld_Liver_Neurone>(Owner), PileType.Hand, Owner));
                break;
            case 5://强猫和快猫
                var strongercat = CombatState!.CreateCard<Rainworld_Liver_Strongercat>(Owner);
                var quickercat = CombatState!.CreateCard<Rainworld_Liver_Quickercat>(Owner);
                CardCmd.Upgrade(strongercat);
                CardCmd.Upgrade(quickercat);
                CardCmd.PreviewCardPileAdd(
                    await CardPileCmd.AddGeneratedCardToCombat( strongercat, PileType.Hand, Owner));
                CardCmd.PreviewCardPileAdd(
                    await CardPileCmd.AddGeneratedCardToCombat( quickercat, PileType.Hand, Owner));
                break;
            case 6://3张复制
                for (int i = 0; i < 3; i++)
                {
                    CardCmd.PreviewCardPileAdd(
                                        await CardPileCmd.AddGeneratedCardToCombat( CreateClone(), PileType.Hand, Owner));
                }
                break;
            case 7://3张飞踢
                await PowerCmd.Apply<VigorPower>(new BlockingPlayerChoiceContext(), Owner.Creature, 2, Owner.Creature, this);
                await PowerCmd.Apply<NimblePower>(new BlockingPlayerChoiceContext(), Owner.Creature, 2, Owner.Creature, this);
                await PowerCmd.Apply<AdroitcapPower>(new BlockingPlayerChoiceContext(), Owner.Creature, 2, Owner.Creature, this);
                CardCmd.PreviewCardPileAdd(
                    await CardPileCmd.AddGeneratedCardToCombat(CombatState!.CreateCard<Rainworld_Liver_Kick>(Owner), PileType.Hand, Owner));
                CardCmd.PreviewCardPileAdd(
                    await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Rainworld_Redcat_Kick>(Owner), PileType.Hand, Owner));
                CardCmd.PreviewCardPileAdd(
                    await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Rainworld_River_Kick>(Owner), PileType.Hand, Owner));
                break;
            case 8://4张轨道
                var ORBIT = CombatState!.CreateCard<Orbit>(Owner);
                CardCmd.Upgrade(ORBIT);
                for (int i = 0; i < 4; i++)
                {
                    CardCmd.PreviewCardPileAdd(
                                        await CardPileCmd.AddGeneratedCardToCombat(ORBIT.CreateClone(), PileType.Hand, Owner));
                }
                break;
           
        }
    }

    private async Task bigevnet(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var random = Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 12);
        switch (random)
        {
            case 0: //巨石
                await rock(choiceContext, 10000);
                break;
            case 1: //药水
                await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
                await PotionCmd.TryToProcure(
                    PotionFactory.CreateRandomPotionInCombat(Owner, Owner.RunState.Rng.CombatPotionGeneration)
                        .ToMutable(), Owner);
                break;
            case 2: //加血上限
                if (Owner.Character is Slugcat)
                    await CreatureCmd.GainMaxHp(Owner.Creature, 5m);
                break;
            case 3: //神化
                foreach (var allCard in Owner.PlayerCombatState!.AllCards)
                    if (allCard != this && allCard.IsUpgradable)
                        CardCmd.Upgrade(allCard);

                break;
            case 4: //回满业力
                if (Owner.Character is Slugcat)
                    await SlugcatData.addworklevel(Owner, 10);
                break;
            case 5: //5种形态
                await PowerCmd.Apply<EchoFormPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
                await PowerCmd.Apply<VoidFormPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
                await PowerCmd.Apply<DemonFormPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
                await PowerCmd.Apply<ReaperFormPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
                await PowerCmd.Apply<WraithFormPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
                await PowerCmd.Apply<SerpentFormPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
                break;
            case 6:
                await discardand(choiceContext, cardPlay);
                await drawcard();
                await applydebuff(choiceContext);
                break;
            case 7://宇宙冷漠
                CardCmd.PreviewCardPileAdd(
                    await CardPileCmd.AddGeneratedCardToCombat( CombatState!.CreateCard<CosmicIndifference>(Owner), PileType.Hand, Owner));
                break;
            case 8: //熟能生巧、猫抓
                CardCmd.PreviewCardPileAdd(
                    await CardPileCmd.AddGeneratedCardToCombat( CombatState!.CreateCard<Rainworld_Redcat_Study>(Owner), PileType.Hand, Owner));
                CardCmd.PreviewCardPileAdd(
                    await CardPileCmd.AddGeneratedCardToCombat( CombatState.CreateCard<Rainworld_Redcat_Woundhit>(Owner), PileType.Hand, Owner));
                break;
            case 9: //99集中黑球
                await PowerCmd.Apply<FocusPower>(choiceContext, Owner.Creature, 99, Owner.Creature, this);
                await OrbCmd.Channel<DarkOrb>(choiceContext, Owner);
                break;
            case 10:
                await OstyCmd.Summon(choiceContext, Owner, 99, this);
                break;
            case 11:
                await ForgeCmd.Forge(99, Owner, this);
                break;
            case 12:
                await PowerCmd.Apply<MushblockPower>(choiceContext, Owner.Creature, 99, Owner.Creature, this);
                break;
            case 13:
                await PowerCmd.Apply<IcewaterPower>(choiceContext, Owner.Creature, 99, Owner.Creature, this);
                break;
        }
    }

    private async Task discardand(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardCmd.Discard(choiceContext, PileType.Hand.GetPile(Owner).Cards);

        var random = Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 6);
        switch (random)
        {
            case 0: //重启和重放仙人指路
                CardModel card = CombatState.CreateCard<Rainworld_Liver_Sirthisway>(Owner);
                CardCmd.Upgrade(card);
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner));
                CardCmd.PreviewCardPileAdd(
                    await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Reboot>(Owner), PileType.Hand,
                        Owner));
                break;
            case 1: //3张蛇咬
                for (var i = 0; i < 3; i++)
                    CardCmd.PreviewCardPileAdd(
                        await CardPileCmd.AddGeneratedCardToCombat(CombatState!.CreateCard<Snakebite>(Owner),
                            PileType.Hand, Owner));
                break;
            case 2: //区和呼唤
                if (CombatState?.MultiplayerScalingModel != null)
                    CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(
                        CombatState.CreateCard<Rainworld_Liver_Treasurebag>(Owner), PileType.Hand, Owner));
                CardCmd.PreviewCardPileAdd(
                    await CardPileCmd.AddGeneratedCardToCombat(CombatState!.CreateCard<Infection>(Owner), PileType.Hand,
                        Owner));
                CardCmd.PreviewCardPileAdd(
                    await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Beckon>(Owner), PileType.Hand,
                        Owner));
                CardCmd.PreviewCardPileAdd(
                    await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Infection>(Owner), PileType.Hand,
                        Owner));
                break;
            case 3: //宝物猫库
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(CombatState!.CreateCard<Rainworld_Liver_Catbag>(Owner), PileType.Hand, Owner));
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(CombatState!.CreateCard<Arsenal>(Owner), PileType.Hand, Owner));
                break;
            case 4: //3随机牌
                var distinctForCombat = CardFactory.GetDistinctForCombat(Owner,
                    from c in Owner.Character.CardPool.GetUnlockedCards(Owner.UnlockState,
                        Owner.RunState.CardMultiplayerConstraint)
                    where (c.Type == CardType.Attack || c.Type == CardType.Skill) && (c.Rarity == CardRarity.Common ||
                        c.Rarity == CardRarity.Uncommon || c.Rarity == CardRarity.Rare)
                    select c, 3, Owner.RunState.Rng.CombatCardGeneration);
                await CardPileCmd.AddGeneratedCardsToCombat(distinctForCombat, PileType.Hand, Owner);
                break;
            case 5: //5拟态
                for (int i = 0; i < 5; i++)
                {
                    CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat( CombatState!.CreateCard<Rainworld_Redcat_Simulated>(Owner), PileType.Hand, Owner));
                }
                break;
        }
    }

    private async Task rock(PlayerChoiceContext choiceContext, int damage)
    {
        var damageTasks = new List<Task>();
        var vfx = NRollingBoulderVfx.Create(CombatState!.HittableEnemies, damage);
        vfx!.Connect(NRollingBoulderVfx.SignalName.HitCreature,
            Callable.From(delegate(NCreature c)
            {
                damageTasks.Add(CreatureCmd.Damage(choiceContext, new[] { c.Entity }, damage, ValueProp.Unpowered,
                    Owner.Creature));
            }));
        Callable.From(delegate
        {
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
            if (!vfx.IsInsideTree())
                throw new InvalidOperationException("VFX is not inside tree after adding it to combat room!");
        }).CallDeferred();
        await vfx.ToSignal(vfx, Node.SignalName.TreeExiting);
        await Task.WhenAll(damageTasks);
        foreach (var enemy in CombatState.HittableEnemies)
            if (enemy.IsAlive)
                return;
        PlayerCmd.EndTurn(Owner, false);
    }
}