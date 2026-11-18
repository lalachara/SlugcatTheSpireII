using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Monster.Minions;

/// <summary>
/// 伙伴模板示例（不注册、不参与游戏）：演示一个最小可用的伙伴需要覆写哪些成员。
/// 召唤链路见 <see cref="FriendSummonCmd.Summon{T}" />，站位见 <see cref="FriendLayout" />。
/// </summary>
public sealed class MyMinion : FriendPetBase
{
    public override int MinInitialHp => 6;
    public override int MaxInitialHp => 6;

    public override string? CustomVisualsPath =>
        "res://Godot/Monster/mushroom.tscn";

    public override async Task OnSummon(
        PlayerChoiceContext choiceContext,
        Player owner,
        FriendSummonOptions options)
    {
        var self = owner.PlayerCombatState?.Pets.FirstOrDefault(p => ReferenceEquals(p.Monster, this));
        if (self == null) return;

        if (options.MaxHp is decimal maxHp)
            await CreatureCmd.SetMaxAndCurrentHp(self, maxHp);

        if (options.PrimaryStatAmount is decimal strength && strength > 0m)
            await PowerCmd.Apply<StrengthPower>(choiceContext, self, strength, owner.Creature, options.Source);

        // 守护：替主人承伤（原 MinionLib 的 MinionGuardianPower，这里换成本 mod 自己的实现）
        await PowerCmd.Apply<FriendGuardPower>(choiceContext, self, 1m, owner.Creature, options.Source);
    }

    public override Task PerformFriendTurn(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> targets)
        => Task.CompletedTask;
}
