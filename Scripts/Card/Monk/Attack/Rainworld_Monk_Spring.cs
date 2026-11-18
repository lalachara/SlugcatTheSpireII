using System;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 能屈能伸：造成伤害并获得格挡；本回合你每打出 1 张其它牌，伤害降低、格挡提高（幅度 = Special）。
/// <para>数值改成「先攻 / 乘胜追击」那套口径：<b>直接按本回合已打出的牌数从战斗历史重算</b>，
/// 而不是靠 AfterCardPlayed 在自己身上累加。这样即使这张牌当时还在抽牌堆/弃牌堆（不在手牌里），
/// 被抽到或打出时也能按已打出的张数给出正确的数值。</para>
/// </summary>
public class Rainworld_Monk_Spring : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override bool GainsBlock => true;

    private readonly decimal _initialDamage = 8m;
    private readonly decimal _initialBlock = 0m;

    /// <summary>回合开始重置用的伤害基础值（升级后的基础值 = 初始值 + 升级加成，不能被重置覆盖）。</summary>
    private decimal DamageBase => IsUpgraded ? _initialDamage + 2m : _initialDamage;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(_initialDamage, ValueProp.Move),
        new BlockVar(_initialBlock, ValueProp.Move),
        new("Special", 2m)
    ];

    public Rainworld_Monk_Spring() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    /// <summary>本回合该玩家已打出的牌数（不含这张自己）。</summary>
    private decimal OtherCardsPlayedThisTurn()
    {
        if (CombatState == null || Owner == null)
            return 0m;

        return CombatManager.Instance.History.CardPlaysFinished.Count(e =>
            e.CardPlay.Card != this &&
            e.CardPlay.Card.Owner == Owner &&
            e.HappenedThisTurn(CombatState));
    }

    /// <summary>
    /// 按「本回合已打出的其它牌数量」重算卡面数值：伤害 = 基础 − 张数×Special（最低 0），格挡 = 基础 + 张数×Special。
    /// 幂等——随时可调用，重复调用不会叠加。
    /// </summary>
    private void RefreshDynamicVars()
    {
        decimal count = OtherCardsPlayedThisTurn();
        decimal step = DynamicVars["Special"].BaseValue;

        DynamicVars.Damage.BaseValue = Math.Max(0m, DamageBase - count * step);
        DynamicVars.Block.BaseValue = _initialBlock + count * step;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // 新回合还没出牌 → 按历史重算会自然回到基础值，不需要再手工记「重置基准」
        if (player == Owner)
            RefreshDynamicVars();
        return Task.CompletedTask;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 打出前按当前张数重算一次：保证「不在手牌时打出的这张」也是正确数值
        RefreshDynamicVars();

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 只刷新显示：真正的数值由历史重算，不再在这里累加
        RefreshDynamicVars();
        return Task.CompletedTask;
    }

    /// <summary>进战斗时可能已经打出过若干张（例如本回合中途生成的牌）。</summary>
    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card == this && !IsClone)
            RefreshDynamicVars();
        return Task.CompletedTask;
    }

    /// <summary>抽到手牌时立刻按当前张数刷新显示，避免「抽上来还显示旧数值」。</summary>
    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card == this)
            RefreshDynamicVars();
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        // 塔1 升级加的是每张牌的增幅（magic 2 → 3），不是格挡基础值（基础格挡为 0）
        DynamicVars["Special"].UpgradeValueBy(1m);
    }
}
