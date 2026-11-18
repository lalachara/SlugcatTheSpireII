using STS2RitsuLib.Interop.AutoRegistration;

namespace Rainworld.Scripts.Monster.Minions;

/// <summary>
/// 伙伴：焦糖蜥蜴（对应塔1 SugarCab：HP8、攻击6、攻击命中所有敌人）。
/// </summary>
[RegisterMonster]
public sealed class FSugarCab : FriendCabBase
{
    public override int MinInitialHp => 8;
    public override int MaxInitialHp => 8;

    protected override string SkinName => "caramel";

    protected override int AttackDamage => 6;

    /// <summary>焦糖蜥蜴攻击所有敌人（对应塔1 SugarCab 的扫场）。</summary>
    public override bool HitsAllEnemies => true;
}
