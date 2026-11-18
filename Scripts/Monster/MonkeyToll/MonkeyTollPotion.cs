using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Rainworld.Scripts.Monster.MonkeyToll;

/// <summary>
/// 拾荒者（药）——对应塔1 Monkey2：
/// 开场直接召唤「拾荒者（遗物）」，随后攻击（7 / 9）、给全体敌人格挡 10、逃跑。
/// </summary>
[RegisterMonster]
public sealed class MonkeyTollPotion : MonkeyTollBase
{
	protected override int AttackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 7);

	/// <summary>塔1 Monkey2 的皮肤。</summary>
	protected override string SkinName => "scavenger2";

	protected override async Task ApplyStealPower()
	{
		await PowerCmd.Apply<MonkeyStealPotionPower>(new ThrowingPlayerChoiceContext(), Creature, StealValue, Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		// 塔1 case0「召唤」：开场就召唤 Monkey4（遗物）
		MoveState summon = new(
			"SUMMON",
			async targets => await SummonMonkey(ModelDb.Monster<MonkeyTollRelic>()),
			new SummonIntent());

		MoveState attack = new(
			"ATTACK",
			async targets => await AttackMove(targets),
			new SingleAttackIntent(AttackDamage));

		// 塔1 case3「格挡」：全体敌人各获得 10 格挡
		MoveState defend = new(
			"DEFEND",
			async targets => await DefendAllMove(targets),
			new DefendIntent());

		MoveState escape = new(
			"ESCAPE",
			async targets => await EscapeMove(targets),
			new EscapeIntent());

		summon.FollowUpState = attack;
		attack.FollowUpState = defend;
		defend.FollowUpState = escape;
		InitialState = summon;
		return new MonsterMoveStateMachine([summon, attack, defend, escape], summon);
	}
}
