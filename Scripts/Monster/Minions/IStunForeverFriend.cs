namespace Rainworld.Scripts.Monster.Minions;

/// <summary>
/// 永久眩晕型伙伴标记：倒下后不会随回合自动复活，只能靠任意一次生命回复唤醒
/// （对应塔1 AbstractFriend.stunForeverOnDeath）。
/// 由 <see cref="Rainworld.Scripts.Powers.FriendDormantPower" /> 识别，腐化蜥蜴实现。
/// </summary>
public interface IStunForeverFriend
{
}
