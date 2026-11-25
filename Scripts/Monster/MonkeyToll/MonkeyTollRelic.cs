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
/// 拾荒者（遗物）——对应塔1 Monkey4：
/// 攻击时偷一件遗物（15 / 18），第 2 回合逃跑。由药猴开场召唤。
/// </summary>
[RegisterMonster]
public sealed class MonkeyTollRelic : MonkeyTollBase
{
	protected override int AttackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 14, 11);

	/// <summary>塔1 Monkey4 的皮肤。</summary>
	protected override string SkinName => "scavenger4";

	protected override async Task ApplyStealPower()
	{
		await PowerCmd.Apply<MonkeyStealRelicPower>(new ThrowingPlayerChoiceContext(), Creature, StealValue, Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState attack = new(
			"ATTACK",
			async targets => await AttackMove(targets),
			new SingleAttackIntent(AttackDamage));

		MoveState escape = new(
			"ESCAPE",
			async targets => await EscapeMove(targets),
			new EscapeIntent());

		attack.FollowUpState = escape;
		InitialState = attack;
		return new MonsterMoveStateMachine([attack, escape], attack);
	}
}
