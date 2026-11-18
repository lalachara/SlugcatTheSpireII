using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Rainworld.Scripts.Monster.MonkeyToll;

/// <summary>
/// 拾荒者（卡）——对应塔1 Monkey3：
/// 攻击时按稀有度优先级偷一张牌（6 / 8），随后全体格挡 10、逃跑。由金猴中途召唤。
/// </summary>
[RegisterMonster]
public sealed class MonkeyTollCard : MonkeyTollBase
{
	protected override int AttackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 6);

	/// <summary>塔1 Monkey3 的皮肤。</summary>
	protected override string SkinName => "scavenger3";

	protected override async Task ApplyStealPower()
	{
		await PowerCmd.Apply<MonkeyStealCardPower>(new ThrowingPlayerChoiceContext(), Creature, StealValue, Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState attack = new(
			"ATTACK",
			async targets => await AttackMove(targets),
			new SingleAttackIntent(AttackDamage));

		// 塔1 case1「格挡」：全体敌人各获得 10 格挡
		MoveState defend = new(
			"DEFEND",
			async targets => await DefendAllMove(targets),
			new DefendIntent());

		MoveState escape = new(
			"ESCAPE",
			async targets => await EscapeMove(targets),
			new EscapeIntent());

		attack.FollowUpState = defend;
		defend.FollowUpState = escape;
		InitialState = attack;
		return new MonsterMoveStateMachine([attack, defend, escape], attack);
	}
}
