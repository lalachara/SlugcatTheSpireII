using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// PUA（带薪假日）：拥有者在本次玩家回合结束时获得<b>一个</b>额外回合。
/// <para><b>层数 = 额外回合里盟友每打出一张牌给施放者的格挡</b>（卡牌未强化 2 层、强化 3 层）；
/// 反复打出卡牌会叠加层数，所以每张牌给的格挡越来越多。层数不增加额外回合数（回合不叠加）。</para>
/// <para>格挡结算用 <c>canGainBlock</c> 开关：获得能力时为 false（本回合出牌不给格挡），
/// 判定要进入额外回合时置 true，之后盟友在额外回合里每打出一张牌，施放者获得等同于层数的格挡；
/// 额外回合结束（<see cref="AfterSideTurnEnd" />）时移除本能力。</para>
/// </summary>
[RegisterPower]
public sealed class PuaExtraTurnPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/goodrest.png",
        BigIconPath: "res://Resource/Powers/Monk/goodrest.png");

    public override PowerType Type => PowerType.Buff;

    /// <summary>层数就是每张牌的格挡值，所以要显示数字。</summary>
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>能力自带的运行时数据（只剩格挡开关）。</summary>
    private sealed class Data
    {
        /// <summary>是否已开启格挡结算（获得能力时 false，进入额外回合时 true）。</summary>
        public bool canGainBlock;
    }

    protected override object InitInternalData() => new Data();

    /// <summary>拥有者此刻是否正处在自己的额外回合里（与原版额外回合队列同源）。</summary>
    private bool IsTakingExtraTurn()
    {
        CombatManager? manager = CombatManager.Instance;
        Player? owner = Owner.Player;
        if (manager == null || owner == null)
            return false;

        return manager.PlayersTakingExtraTurn.Contains(owner);
    }

    /// <summary>
    /// 额外回合内拥有者每打出一张牌，给施放者（打出 PUA 的那名玩家）加格挡 = 当前层数。
    /// 施放者从 <see cref="PowerModel.Applier" /> 取（<c>PowerCmd.Apply</c> 时传的主人生物），
    /// 不额外存 Player 引用，联机时各端都能还原。
    /// </summary>
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!GetInternalData<Data>().canGainBlock || Amount <= 0m)
            return;

        Creature? applier = Applier;
        if (applier is not { IsAlive: true })
        {
            Entry.Logger.Info($"[PUA] 跳过格挡：施加者不可用（applierPlayer={applier?.Player?.NetId}, alive={applier?.IsAlive}）");
            return;
        }

        // 只算拥有者自己打出的牌
        if (cardPlay.Card.Owner != Owner.Player)
            return;

        Entry.Logger.Info($"[PUA] 额外回合内出牌：ownerPlayer={Owner?.Player?.NetId}, block={Amount}, inExtra={IsTakingExtraTurn()}");
        await CreatureCmd.GainBlock(applier, Amount, ValueProp.Move, cardPlay);
    }

    /// <summary>
    /// 本次玩家回合结束时判定是否进入额外回合：为真时顺手把格挡结算打开。
    /// 无论层数多少都只给 1 个额外回合（层数只影响格挡量）。
    /// </summary>
    public override bool ShouldTakeExtraTurn(Player player)
    {
        bool result = Owner.IsAlive && Amount > 0m && player == Owner.Player;
        if (result)
            GetInternalData<Data>().canGainBlock = true;

        Entry.Logger.Info($"[PUA] ShouldTakeExtraTurn player={player?.NetId}, ownerPlayer={Owner?.Player?.NetId}, amount={Amount}, alive={Owner?.IsAlive}, result={result}");
        return result;
    }

    /// <summary>
    /// 原版在「即将开始额外回合」时调用（不是结束），只用于记录。
    /// </summary>
    public override Task AfterTakingExtraTurn(Player player)
    {
        if (player == Owner.Player)
            Entry.Logger.Info($"[PUA] 进入额外回合（ownerPlayer={Owner?.Player?.NetId}, canGainBlock={GetInternalData<Data>().canGainBlock}）");

        return Task.CompletedTask;
    }

    /// <summary>
    /// 回合结束钩子：普通回合结束时 canGainBlock 还是 false（此时 ShouldTakeExtraTurn 还没跑），
    /// 所以只有「额外回合结束」这一次会走到移除分支——下一回合起不再给格挡。
    /// </summary>
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != Owner.Side)
            return;

        if (!GetInternalData<Data>().canGainBlock)
            return;

        Entry.Logger.Info($"[PUA] 额外回合结束，移除能力（ownerPlayer={Owner?.Player?.NetId}, amount={Amount}）");
        await PowerCmd.Remove(this);
    }
}
