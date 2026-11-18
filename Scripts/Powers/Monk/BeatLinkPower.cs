using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 捆仙绳：绑定1名盟友。<b>能力施加在被绑定的盟友身上</b>，图标因此挂在盟友头顶，
/// 由施放者的「协调」持续转化为盟友的力量与敏捷：施放者每获得/失去1层协调，
/// 盟友同步获得/失去等量力量与敏捷。用"上次同步的协调层数"做差值同步，
/// 覆盖获得、失去与协调被整体移除三种情况。
/// </summary>
[RegisterPower]
public sealed class BeatLinkPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/shengzi.png",
        BigIconPath: "res://Resource/Powers/Monk/shengzi.png");

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>施放者（协调的持有者）：由 <see cref="PowerModel.Applier" /> 取，联机时随能力同步。</summary>
    private Creature? Caster => Applier;

    /// <summary>被绑定的盟友就是能力的主人（图标显示在其头顶）。</summary>
    private Creature LinkedAlly => Owner;

    /// <summary>上次同步时施放者的协调层数。</summary>
    private decimal _syncedTune;

    /// <summary>在场上找这条链接（施放者 = <paramref name="caster" />）：用于「再次打出只加协调」的判定。</summary>
    public static BeatLinkPower? FindFor(Creature caster)
    {
        if (caster.CombatState is not { } state)
            return null;

        foreach (var creature in state.Creatures)
        {
            if (creature.GetPower<BeatLinkPower>() is { } link && link.Applier == caster)
                return link;
        }

        return null;
    }

    /// <summary>绑定完成：把施放者当前协调层数记为已同步。</summary>
    public void Bind()
    {
        _syncedTune = Caster?.GetPowerAmount<TunePower>() ?? 0m;
    }

    /// <summary>把绑定瞬间已有的协调层数一次性给盟友。</summary>
    public Task SyncInitial(PlayerChoiceContext choiceContext)
        => ApplyDelta(choiceContext, _syncedTune);

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is TunePower && power.Owner == Caster)
            await SyncTune(choiceContext);
        await base.AfterPowerAmountChanged(choiceContext, power, amount, applier, cardSource);
    }

    // 协调被整体移除（PowerCmd.Remove）等路径不一定走 AfterPowerAmountChanged，
    // 在每次出牌与己方回合开始时按差值补同步。
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SyncTune(choiceContext);
        await base.AfterCardPlayed(choiceContext, cardPlay);
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        // 用施放者的阵营判断（双方玩家通常同在 Player 侧）
        if (side == (Caster?.Side ?? Owner.Side))
            await SyncTune(new BlockingPlayerChoiceContext());
        await base.AfterSideTurnStart(side, participants, combatState);
    }

    private async Task SyncTune(PlayerChoiceContext choiceContext)
    {
        if (Caster is not { } caster)
            return;

        decimal current = caster.GetPowerAmount<TunePower>();
        decimal delta = current - _syncedTune;
        if (delta == 0m)
            return;

        _syncedTune = current;
        await ApplyDelta(choiceContext, delta);
    }

    private async Task ApplyDelta(PlayerChoiceContext choiceContext, decimal delta)
    {
        if (delta == 0m || !LinkedAlly.IsAlive)
            return;

        await PowerCmd.Apply<StrengthPower>(choiceContext, LinkedAlly, delta, Caster, null);
        await PowerCmd.Apply<DexterityPower>(choiceContext, LinkedAlly, delta, Caster, null);
    }
}
