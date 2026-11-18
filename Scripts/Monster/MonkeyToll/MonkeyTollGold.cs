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
/// 拾荒者（金）——对应塔1 Monkey1：
/// 攻击时偷金币（20 / 20），第 2 回合原地格挡 10 并固定召唤「拾荒者（卡）」，第 3 回合逃跑。
/// </summary>
[RegisterMonster]
public sealed class MonkeyTollGold : MonkeyTollBase
{
	protected override int AttackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 14, 11);

	/// <summary>塔1 Monkey1 的皮肤。</summary>
	protected override string SkinName => "scavenger1";

	/// <summary>每次攻击偷走的金币数（塔1 goldAmount：10 / 15 / 20）。</summary>
	protected override int StealValue => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 20, 20);

	protected override async Task ApplyStealPower()
	{
		await PowerCmd.Apply<MonkeyStealGoldPower>(new ThrowingPlayerChoiceContext(), Creature, StealValue, Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState attack = new(
			"ATTACK",
			async targets => await AttackMove(targets),
			new SingleAttackIntent(AttackDamage));

		// 塔1 case1「格挡召唤」：格挡 10 + 固定召唤 Monkey3（卡）
		MoveState summon = new(
			"SUMMON",
			async targets =>
			{
				// 格挡已经播过 point_1，召唤时不再重复播
				await GainSelfBlock();
				await SummonMonkey(ModelDb.Monster<MonkeyTollCard>(), playPointAnim: false);
			},
			new SummonIntent(),
			new DefendIntent());

		MoveState escape = new(
			"ESCAPE",
			async targets => await EscapeMove(targets),
			new EscapeIntent());

		attack.FollowUpState = summon;
		summon.FollowUpState = escape;
		InitialState = attack;
		return new MonsterMoveStateMachine([attack, summon, escape], attack);
	}
}
