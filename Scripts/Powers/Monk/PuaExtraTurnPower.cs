using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
/// <para><b>层数 = 额外回合里拥有者每打出一张牌，<u>每个施放者</u>获得的格挡</b>
/// （卡牌未强化 2 层、强化 3 层；多名玩家各自打过就给各自算）。</para>
/// <para>多人要点：能力用 <see cref="PowerInstanceType.Instanced" />，<b>每名玩家的施放各是一个实例</b>，
/// 各自保留自己的 <see cref="PowerModel.Applier" />。旧实现是同一个实例叠层，
/// 只有第一个施放者能拿到格挡（A、B 都给 C 时永远只有 A 加格挡）。</para>
/// <para>额外回合只由原版队列发放一次（<c>Hook.ShouldTakeExtraTurn</c> 取第一个为真的来源），
/// 因此格挡判定不再依赖「谁被选为额外回合来源」，统一看
/// <see cref="CombatManager.PlayersTakingExtraTurn" />：只有拥有者真正处在额外回合里，
/// 他打出的牌才会让<i>每</i>个实例的施放者各自获得自己那份格挡。</para>
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

    /// <summary>每个玩家施放的「带薪假日」各自独立成实例，避免叠层时丢掉后面施放者。</summary>
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

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
    /// 额外回合内拥有者每打出一张牌，给<b>本实例的施放者</b>加格挡 = 当前层数。
    /// 有几个施放者就有几个实例，所以 A、B 各自都会拿到自己那份（2+2 = 4）。
    /// 施放者从 <see cref="PowerModel.Applier" /> 取（<c>PowerCmd.Apply</c> 时传的主人生物），
    /// 不额外存 Player 引用，联机时各端都能还原。
    /// </summary>
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Amount <= 0m)
            return;

        // 必须是拥有者自己打出的牌，且此刻真的在额外回合里
        if (cardPlay.Card.Owner != Owner.Player)
            return;
        if (!IsTakingExtraTurn())
            return;

        Creature? applier = Applier;
        if (applier is not { IsAlive: true })
        {
            Entry.Logger.Info($"[PUA] 跳过格挡：施加者不可用（applierPlayer={applier?.Player?.NetId}, alive={applier?.IsAlive}）");
            return;
        }

        Entry.Logger.Info($"[PUA] 额外回合内出牌：ownerPlayer={Owner?.Player?.NetId}, applierPlayer={applier.Player?.NetId}, block={Amount}");
        await CreatureCmd.GainBlock(applier, Amount, ValueProp.Move, cardPlay);
    }

    /// <summary>
    /// 本次玩家回合结束时判定是否进入额外回合。
    /// 无论层数多少都只给 1 个额外回合（层数只影响格挡量）；
    /// 多个实例都返回 true 也没关系，原版只会采用第一个来源、每人至多一个额外回合。
    /// </summary>
    public override bool ShouldTakeExtraTurn(Player player)
    {
        bool result = Owner.IsAlive && Amount > 0m && player == Owner.Player;
        Entry.Logger.Info($"[PUA] ShouldTakeExtraTurn player={player?.NetId}, ownerPlayer={Owner?.Player?.NetId}, amount={Amount}, alive={Owner?.IsAlive}, result={result}");
        return result;
    }

    /// <summary>原版在「即将开始额外回合」时调用（只对来源模型调用，这里仅记录）。</summary>
    public override Task AfterTakingExtraTurn(Player player)
    {
        if (player == Owner.Player)
            Entry.Logger.Info($"[PUA] 进入额外回合（ownerPlayer={Owner?.Player?.NetId}, amount={Amount}）");

        return Task.CompletedTask;
    }

    /// <summary>
    /// 额外回合结束时移除本实例。用 <see cref="IsTakingExtraTurn" /> 判断而不是记录开关：
    /// 普通回合结束时队列里没有拥有者，只有额外回合结束那一次才会移除。
    /// </summary>
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != Owner.Side)
            return;

        if (!IsTakingExtraTurn())
            return;

        Entry.Logger.Info($"[PUA] 额外回合结束，移除能力（ownerPlayer={Owner?.Player?.NetId}, amount={Amount}）");
        await PowerCmd.Remove(this);
    }
}
