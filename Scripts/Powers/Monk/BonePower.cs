using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 蚀骨：本回合施加者每打出 1 张牌，该目标失去与层数等量的生命。
/// 每个施加者拥有独立实例，只有施加者自己打出的牌会触发。
/// </summary>
[RegisterPower]
public sealed class BonePower : ModPowerTemplate
{
    private sealed class Data
    {
        /// <summary>记录每张牌在打出前的层数，避免打出蚀骨本身触发新施加的层数。</summary>
        public readonly Dictionary<CardModel, int> AmountsForPlayedCards = new();
    }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/Bone128.png",
        BigIconPath: "res://Resource/Powers/Monk/Bone128.png");

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>不同玩家施加的蚀骨分别记录层数，互不干扰。</summary>
    public override PowerInstanceType InstanceType => PowerInstanceType.InstancedPerApplier;

    protected override object InitInternalData() => new Data();

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        // 只有施加者本人打出的牌才计数
        if (Applier?.Player == null || cardPlay.Player != Applier.Player)
            return Task.CompletedTask;

        GetInternalData<Data>().AmountsForPlayedCards[cardPlay.Card] = Amount;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!GetInternalData<Data>().AmountsForPlayedCards.Remove(cardPlay.Card, out var amount))
            return;
        if (!Owner.IsAlive || amount <= 0)
            return;

        Flash();
        // 生命流失：不可格挡、不受能力增伤；来源记为施加者玩家
        await CreatureCmd.Damage(choiceContext, Owner, amount,
            ValueProp.Unblockable | ValueProp.Unpowered, Applier);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player)
            return;

        // 在施加者的回合结束时移除；多人模式下其他玩家的额外回合不提前清除
        if (Applier?.Player == null || participants.Contains(Applier))
            await PowerCmd.Remove(this);
    }
}
