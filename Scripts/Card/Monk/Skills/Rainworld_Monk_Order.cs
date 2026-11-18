using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Monster.Minions;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 拾荒者的援助：只能由伙伴「拾荒者」塞进手牌的一次性援助卡，不进入任何卡池（消耗、虚无）。
/// 每张牌在生成时就定下两件事，卡面显示的就是打出去会发生的事：
/// 1. 效果（6 选 1）—— 描述 = 基础描述 + 对应那条 .effectN 文本；
/// 2. 花费的金币 —— 描述里的 {GoldCost} 与扣款读的是同一份数据。
/// 全部数据只写在 DynamicVars 里：渲染与结算同源，克隆（如复制手牌效果）时也随 DynamicVars 一起传递。
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public class Rainworld_Monk_Order : ModCardTemplate
{
    // ———— 随机效果（下标与本地化条目 .effect0~5 一一对应）————
    /// <summary>对随机敌人造成伤害（{Damage}）。</summary>
    public const int EffectAttackOne = 0;

    /// <summary>对所有敌人造成伤害（{Damage}）。</summary>
    public const int EffectAttackAll = 1;

    /// <summary>获得格挡（{Block}）。</summary>
    public const int EffectBlock = 2;

    /// <summary>抽牌。</summary>
    public const int EffectDraw = 3;

    /// <summary>获得能量。</summary>
    public const int EffectEnergy = 4;

    /// <summary>获得协调。</summary>
    public const int EffectTune = 5;

    /// <summary>效果种类数，随机下标的上界。</summary>
    public const int EffectCount = 6;

    // ———— 花费的金币 ————
    /// <summary>拾荒者塞牌的门槛，也是花费的下限。</summary>
    public const int MinGoldCost = 10;

    /// <summary>花费的上限。</summary>
    public const int MaxGoldCost = 20;

    private const int DmgOne = 11;
    private const int DmgAll = 7;
    private const int BlockAmount = 9;
    private const int DrawAmount = 2;
    private const int EnergyAmount = 1;
    private const int TuneAmount = 1;

    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInCardLibrary = true;

    /// <summary>塞这张牌的拾荒者：打出时由它播对应动作（塔1 playOwnerAnim 的 owner）。</summary>
    private FMonkey? _ownerMonkey;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    /// <summary>格挡效果要显示在卡面数值上。</summary>
    public override bool GainsBlock => true;

    public override CardAssetProfile AssetProfile => new(
        "res://Resource/Card/Monk/rainworld_monk_order.png",
        FramePath: "res://Resource/Card/RedCat/CardSkill1024.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // 默认按「对随机敌人造成9点伤害」的一套数值（图鉴里显示的就是这套）；生成时由 Configure 覆写为随机到的效果
        new DamageVar(DmgOne, ValueProp.Move),
        new BlockVar(BlockAmount, ValueProp.Move),
        new EnergyVar(EnergyAmount),
        new("GoldCost", MinGoldCost),
        new("Special", EffectAttackOne)
    ];

    public Rainworld_Monk_Order() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    /// <summary>
    /// 由生成方（拾荒者）在加入手牌前调用：定下这张牌的效果与花费的金币，
    /// 并记下主人后面由它在打出时播对应动作。
    /// </summary>
    public void Configure(int effect, int goldCost, FMonkey? owner = null)
    {
        DynamicVars["Special"].BaseValue = effect;
        DynamicVars["GoldCost"].BaseValue = goldCost;
        _ownerMonkey = owner;
        switch (effect)
        {
            case EffectAttackOne:
                DynamicVars.Damage.BaseValue = DmgOne;
                break;
            case EffectAttackAll:
                DynamicVars.Damage.BaseValue = DmgAll;
                break;
            case EffectBlock:
                DynamicVars.Block.BaseValue = BlockAmount;
                break;
        }

        // 生成时就按「基础值 + 当前协调」显示预览
        RefreshPreview();
    }

    /// <summary>
    /// 描述 = 基础描述 + 随机到的那条效果文本：把 .effectN 整条格式化后注入 {Effect}。
    /// 效果条目里的 {Damage}/{Block} 占位符要用本卡的变量格式化，所以先把当前描述的变量复制过去再拼。
    /// </summary>
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        var effectIndex = (int)DynamicVars["Special"].BaseValue;
        if (effectIndex < 0 || effectIndex >= EffectCount)
            effectIndex = EffectAttackOne;
        var effectString = new LocString("cards", $"{Id.Entry}.effect{effectIndex}");
        effectString.AddVariablesFrom(description);
        description.Add("Effect", effectString);
    }

    /// <summary>绑定的拾荒者：优先用生成这张牌的伙伴，找不到时退回场上任意一只存活拾荒者。</summary>
    private FMonkey? BoundMonkey()
    {
        if (_ownerMonkey is { Creature: { IsAlive: true } } bound)
            return bound;
        return FMonkey.FindOnField(Owner);
    }

    /// <summary>来源伙伴已倒下时这张牌打不出去（塔1：援助牌绑定 playOwner）。</summary>
    protected override bool IsPlayable => BoundMonkey() != null;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var effect = (int)DynamicVars["Special"].BaseValue;
        FMonkey? monkey = BoundMonkey();

        // 打出前再刷新一次：卡面显示多少就打多少（基础值 + 协调）
        RefreshPreview();

        // 先付钱：卡面写着花费多少金币就扣多少（金币不足时按现有金币付，不欠账也不白送）
        var goldToPay = Math.Min(DynamicVars["GoldCost"].BaseValue, Owner.Gold);
        if (goldToPay > 0)
            await PlayerCmd.LoseGold(goldToPay, Owner);

        // 拾荒者按这张牌的效果播对应动作（0 单体攻击 / 1 全体攻击 / 2 格挡 / 3 抽牌 / 4 能量 / 5 协调）
        PlayOwnerAnim(effect);

        switch (effect)
        {
            case EffectAttackOne:
                // 来源是拾荒者：直接以伙伴为 dealer 结算（不吃玩家力量）
                await DealOrderDamage(choiceContext, monkey, RandomHittableEnemy() is { } singleTarget ? [singleTarget] : null, cardPlay,
                    DynamicVars.Damage.BaseValue, "event:/Rainworld/sfx/spear");
                break;
            case EffectAttackAll:
                await DealOrderDamage(choiceContext, monkey, HittableEnemies(), cardPlay,
                    DynamicVars.Damage.BaseValue, "event:/Rainworld/sfx/bombspear");
                break;
            case EffectBlock:
                // 格挡加在玩家身上，但只吃协调：用 Unpowered 绕过敏捷加成
                if (monkey != null)
                    await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Unpowered, cardPlay);
                break;
            case EffectDraw:
                await CardPileCmd.Draw(choiceContext, DrawAmount, Owner);
                break;
            case EffectEnergy:
                await PlayerCmd.GainEnergy(EnergyAmount, Owner);
                break;
            case EffectTune:
                await PowerCmd.Apply<TunePower>(choiceContext, Owner.Creature, TuneAmount, Owner.Creature, this);
                break;
        }
    }

    private IReadOnlyList<Creature> HittableEnemies() => CombatState?.HittableEnemies ?? [];

    private Creature? RandomHittableEnemy()
    {
        IReadOnlyList<Creature> list = HittableEnemies();
        return list.Count == 0 ? null : Owner.RunState.Rng.CombatCardGeneration.NextItem(list) ?? list[0];
    }

    /// <summary>
    /// 以拾荒者伙伴为来源结算伤害：dealer = 伙伴生物（所以不吃玩家力量），
    /// cardSource 仍是本卡（战斗历史/卡牌来源类效果照常识别）。数值已经含过协调加成。
    /// </summary>
    private async Task DealOrderDamage(PlayerChoiceContext choiceContext, FMonkey? monkey,
        IReadOnlyList<Creature>? targets, CardPlay cardPlay, decimal amount, string sfx)
    {
        if (monkey == null || targets == null)
            return;

        List<Creature> list = targets.Where(c => c.IsAlive).ToList();
        if (list.Count == 0)
            return;

        SfxCmd.Play(sfx);
        foreach (Creature target in list)
            VfxCmd.PlayOnCreature(target, "vfx/vfx_attack_slash");

        await CreatureCmd.Damage(choiceContext, list, amount, ValueProp.Move, monkey.Creature, this, cardPlay);
    }

    /// <summary>本次打出额外获得的协调加成：伤害与格挡都吃它（替代力量/敏捷）。</summary>
    private decimal TuneBonus() => Owner.Creature.GetPower<TunePower>()?.Amount ?? 0m;

    /// <summary>
    /// 把「基础值 + 当前协调」写回卡面数值，让玩家在打出前就看到加成后的伤害/格挡。
    /// 幂等，可随时调用（与能屈能伸的 RefreshDynamicVars 同一套写法）。
    /// </summary>
    private void RefreshPreview()
    {
        if (CombatState == null || Owner?.Creature == null)
            return;

        decimal tune = TuneBonus();
        switch ((int)DynamicVars["Special"].BaseValue)
        {
            case EffectAttackOne:
                DynamicVars.Damage.BaseValue = DmgOne + tune;
                break;
            case EffectAttackAll:
                DynamicVars.Damage.BaseValue = DmgAll + tune;
                break;
            case EffectBlock:
                DynamicVars.Block.BaseValue = BlockAmount + tune;
                break;
        }
    }

    /// <summary>协调变化通常来自打出别的牌，这里统一刷新卡面。</summary>
    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        RefreshPreview();
        return Task.CompletedTask;
    }

    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card == this)
            RefreshPreview();
        return Task.CompletedTask;
    }

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card == this && !IsClone)
            RefreshPreview();
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner)
            RefreshPreview();
        return Task.CompletedTask;
    }

    /// <summary>
    /// 让塞这张牌的拾荒者播对应动作（塔1 playOwnerAnim）：
    /// 主人可能找不回来（卡被克隆、主人已离场），这时退回现场找一只还在场的拾荒者；
    /// 实在没有就安静跳过 —— 效果照常结算，不影响出牌。
    /// </summary>
    private void PlayOwnerAnim(int effect)
    {
        FMonkey? monkey = _ownerMonkey;
        if (monkey == null || !monkey.Creature.IsAlive)
            monkey = FMonkey.FindOnField(Owner);
        monkey?.PlayOrderAnim(effect);
    }

    // 随机援助牌不进任何卡池，也不可升级
    public override int MaxUpgradeLevel => 0;

    protected override void OnUpgrade()
    {
    }
}
