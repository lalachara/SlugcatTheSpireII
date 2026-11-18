using STS2RitsuLib.Interop.AutoRegistration;

namespace Rainworld.Scripts.Monster.Minions;

/// <summary>
/// 伙伴：黄蜥蜴（对应塔1 YellowCab：HP4、攻击2）。
/// </summary>
[RegisterMonster]
public sealed class FYellowCab : FriendCabBase
{
    public override int MinInitialHp => 4;
    public override int MaxInitialHp => 4;

    protected override string SkinName => "yellow";

    protected override int AttackDamage => 2;
}
