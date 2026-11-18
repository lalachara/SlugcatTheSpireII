using STS2RitsuLib.Interop.AutoRegistration;

namespace Rainworld.Scripts.Monster.Minions;

/// <summary>
/// 伙伴：蓝蜥蜴（对应塔1 BlueCab：HP8、攻击3）。
/// 攻击意图、自动攻击、守护承伤与休眠复活均由 <see cref="FriendCabBase" /> 统一实现。
/// </summary>
[RegisterMonster]
public sealed class FBlueCab : FriendCabBase
{
    public override int MinInitialHp => 8;
    public override int MaxInitialHp => 8;

    protected override string SkinName => "blue";

    protected override int AttackDamage => 3;
}
