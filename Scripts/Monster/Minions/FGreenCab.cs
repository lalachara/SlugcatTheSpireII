using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Rainworld.Scripts.Monster.Minions;

/// <summary>
/// 伙伴：绿蜥蜴（HP12、攻击 3、攻击后给玩家 3 点格挡；对应塔1 GreenCab，数值按本 mod 平衡取 3）。
/// </summary>
[RegisterMonster]
public sealed class FGreenCab : FriendCabBase
{
    public override int MinInitialHp => 12;
    public override int MaxInitialHp => 12;

    protected override string SkinName => "green";

    protected override int AttackDamage => 3;

    /// <summary>绿蜥蜴的意图是「攻击 + 格挡」：剑（单体伤害）+ 盾（攻击后给主人的格挡）。</summary>
    protected override AbstractIntent[] CreateAttackIntents() =>
        [CreateAttackIntent(), new FriendDefendIntent()];
    /// <summary>攻击意图顺带给主人 3 + 「协调」层格挡（塔1 GreenCab intentArmor = getblock()）。</summary>
    public override decimal IntentArmor => Creature.IsAlive ? 3m + TuneAmount() : 0m;

    /// <summary>塔1 GreenCab：攻击后给玩家格挡（3 + 「协调」层数）。</summary>
    public override async Task PerformCabAttack(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> targets)
    {
        await base.PerformCabAttack(choiceContext, targets);

        Creature? playerCreature = Creature.PetOwner?.Creature;
        if (playerCreature != null)
            await CreatureCmd.GainBlock(playerCreature, 3m + TuneAmount(), ValueProp.Move, null);
    }
}
