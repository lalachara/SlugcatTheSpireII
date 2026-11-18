﻿using System.Collections;
   using Godot;
   using MegaCrit.Sts2.Core.Assets;
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
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Resource.Card;
using Rainworld.Scripts;
using Rainworld.Scripts.Card;
using Rainworld.Scripts.Card.Liver.Attack;
using Rainworld.Scripts.Card.RedCat;
using Rainworld.Scripts.Powers;
using Rainworld.relics;

namespace Rainworld.Scripts;

public static class Tools
{
    public static void ChangeEventImg(string img)
    {
        var eventRoom = NEventRoom.Instance;
        if (eventRoom?.Layout == null)
        {
            return;
        }

        eventRoom.SetPortrait(PreloadManager.Cache.GetTexture2D(img));
    }

    /// <summary>
    /// 直接离开当前事件房间并返回地图，等价于点击事件结束页的「继续」按钮。
    /// 用于事件已经结束但不想再让玩家多点一次继续的场景。
    /// </summary>
    public static void LeaveEvent()
    {
        if (NEventRoom.Instance == null || NMapScreen.Instance == null)
        {
            return;
        }

        NEventRoom.Proceed();
    }

    internal static readonly HashSet<System.Type> HpRelics = new()
    {
        typeof(CorrodePower),
        typeof(PoisonPower),
        typeof(DoomPower),
        typeof(ChuangPower),
        typeof(WeakPower),
        typeof(FrailPower),
        typeof(VulnerablePower),
        typeof(ShacklingPotionPower),
        


    };

    /// <summary>蛞蝓猫各颜色卡池（不含 Monk 非颜色卡池）。新增颜色时在此追加，并同步维护 <see cref="SlugcatPoolToCardBase"/>。</summary>
    public static readonly Type[] AllSlugcatCardPools =
    {
        typeof(Rainworld_Liver_CardPool),
        typeof(Rainworld_River_CardPool),
        typeof(Rainworld_Redcat_CardPool),
    };

    /// <summary>卡池类型 → 该颜色卡牌基类，用于判断一张卡属于哪个蛞蝓猫颜色。</summary>
    public static readonly IReadOnlyDictionary<Type, Type> SlugcatPoolToCardBase =
        new Dictionary<Type, Type>
        {
            { typeof(Rainworld_Liver_CardPool), typeof(LiverCardModel) },
            { typeof(Rainworld_River_CardPool), typeof(RiverCardModel) },
            { typeof(Rainworld_Redcat_CardPool), typeof(RedCatCardModel) },
        };

    /// <summary>判断指定类型是否为蛞蝓猫颜色卡池。</summary>
    public static bool IsSlugcatColorPool(Type poolType)
        => AllSlugcatCardPools.Contains(poolType);

    /// <summary>获取指定卡池类型的 CardPoolModel 实例。</summary>
    public static CardPoolModel GetCardPool(Type poolType)
    {
        var method = typeof(ModelDb).GetMethod("CardPool")!.MakeGenericMethod(poolType);
        return (CardPoolModel)method.Invoke(null, null)!;
    }

    /// <summary>获取除当前颜色外的其它蛞蝓猫卡池实例。</summary>
    public static IEnumerable<CardPoolModel> GetOtherSlugcatCardPools(Type currentPoolType)
        => AllSlugcatCardPools.Where(t => t != currentPoolType).Select(GetCardPool);

    /// <summary>是否为可正常掉落的卡牌类型（攻击 / 技能 / 能力）。</summary>
    public static bool IsNormalCardType(CardModel card)
        => card.Type == CardType.Attack || card.Type == CardType.Skill || card.Type == CardType.Power;

    /// <summary>是否为可正常掉落的稀有度（普通 / 罕见 / 稀有）。</summary>
    public static bool IsNormalCardRarity(CardModel card)
        => card.Rarity == CardRarity.Common || card.Rarity == CardRarity.Uncommon || card.Rarity == CardRarity.Rare;

    /// <summary>
    /// 按「颜色优先级」返回全部颜色卡池：其它蛞蝓猫颜色 → 原版角色颜色 → 自身颜色 → 无色。
    /// 蛞蝓猫颜色 + 原版角色颜色 + 无色，去掉自身颜色后最多 9 种，与手牌上限 10 对应。
    /// </summary>
    public static IEnumerable<CardPoolModel> GetColorPoolsInPriorityOrder(Type selfPoolType)
    {
        var pools = new List<CardPoolModel>();

        void Add(CardPoolModel pool)
        {
            if (pool != null && !pools.Contains(pool))
                pools.Add(pool);
        }

        foreach (var pool in AllSlugcatCardPools.Select(GetCardPool))
        {
            if (pool.GetType() != selfPoolType)
                Add(pool);
        }

        foreach (var pool in ModelDb.AllCharacterCardPools)
        {
            if (pool.GetType() != selfPoolType)
                Add(pool);
        }

        // 自身颜色也是这九种颜色之一，放在无色之前
        Add(GetCardPool(selfPoolType));
        Add(ModelDb.CardPool<ColorlessCardPool>());
        return pools;
    }

    /// <summary>
    /// 从所有蛞蝓猫颜色卡池中随机取一张指定类型的牌。<paramref name="upgraded"/> 为 true 时强化。
    /// 没有符合条件的牌时返回 null。
    /// </summary>
    public static CardModel GetRandomSlugcatColorCard(Player player, CardType cardType, bool upgraded)
    {
        var unlockState = player.UnlockState;
        var constraint = player.RunState.CardMultiplayerConstraint;
        var candidates = AllSlugcatCardPools
            .Select(GetCardPool)
            .SelectMany(pool => pool.GetUnlockedCards(unlockState, constraint))
            .Where(c => c.Type == cardType && IsNormalCardRarity(c) && !RainworldCardTags.IsGenerateBlocked(c))
            .ToList();
        return PickRandomCardForCombat(player, candidates, upgraded);
    }

    /// <summary>从指定卡池随机取一张符合条件的牌，池内没有可用的牌时返回 null。</summary>
    public static CardModel GetRandomCardFromPool(Player player, CardPoolModel pool, bool upgraded)
    {
        var unlockState = player.UnlockState;
        var constraint = player.RunState.CardMultiplayerConstraint;
        var candidates = pool.GetUnlockedCards(unlockState, constraint)
            .Where(c => IsNormalCardType(c) && IsNormalCardRarity(c) && !RainworldCardTags.IsGenerateBlocked(c))
            .ToList();
        return PickRandomCardForCombat(player, candidates, upgraded);
    }

    /// <summary>从候选牌里随机抽一张并按战斗用副本返回，可选强化。</summary>
    private static CardModel PickRandomCardForCombat(Player player, List<CardModel> candidates, bool upgraded)
    {
        if (candidates.Count == 0)
            return null;

        var rng = player.RunState.Rng.CombatCardGeneration;
        var picked = candidates[rng.NextInt(0, candidates.Count)];
        var card = CardFactory.GetDistinctForCombat(player, new[] { picked }, 1, rng).FirstOrDefault();
        if (card != null && upgraded && card.IsUpgradable)
            CardCmd.Upgrade(card);
        return card;
    }

    public static async Task  getrandomSpear(Boolean upgraded,Player player)
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
        var roomType = player.RunState.CurrentRoom!.RoomType;
        if (roomType != RoomType.Boss && roomType != RoomType.Monster && roomType != RoomType.Elite)
            return;
        if (player.Creature.HasPower<ExcitePower>())
        {
            await CreatureCmd.Damage( new ThrowingPlayerChoiceContext(),  ((IEnumerable<Creature>) player.Creature.GetPower<ExcitePower>().CombatState.HittableEnemies), player.Creature.GetPower<ExcitePower>().Amount, ValueProp.Unpowered, player.Creature);
        }

        if (player.Creature.HasPower<TwistPower>())
        {
            await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), player.Creature.GetPowerAmount<TwistPower>(),player);
        }
        // 在 WorkReport 方法中，Eatblood 那行后面加上：
        foreach (var card in player.Deck.Cards)
        {
            if (card is Rainworld_Redcat_Eatblood eatblood)
                eatblood.AddMisc(eatblood.IsUpgraded ? 3 : 2);

        }
        foreach (CardModel card in player.PlayerCombatState!.AllCards)
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
        
        var catlostCards = player.PlayerCombatState!.DrawPile.Cards
            .Concat(player.PlayerCombatState.DiscardPile.Cards)
            .OfType<Rainworld_Redcat_Catlost>().ToList();
        if (card is not Rainworld_Redcat_Catlost)
        {
            foreach (var catlost in catlostCards)
            {
                if (!player.PlayerCombatState.Hand.Cards.Contains(catlost))
                {
                    await BackToHand(player, catlost);
                }
            }
        }
        else
        {
            card.DynamicVars["Special"].UpgradeValueBy(1m);
        }
    }

    public static async Task Yinlei(CardPlay cardPlay,PlayerChoiceContext choiceContext)
    {
            choiceContext = new ThrowingPlayerChoiceContext();
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
                    targets = card.CombatState?.GetOpponentsOf(card.Owner.Creature).Where(e => e.IsHittable);
                    break;
                default:
                    targets = cardPlay.Target != null ? new[] { cardPlay.Target } : Array.Empty<Creature>();
                    break;
            }
            if(targets.Count()==0)
                return;

            var lightRowPlayers = card.CombatState?.Players
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