using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Monster.MonkeyToll;

/// <summary>
/// 「拾荒者收费站」事件专用遭遇（对应塔1 Monkeys 事件的 Rainworld:4Monkey）：
/// 开场为拾荒者（金）+ 拾荒者（药），战斗中分别召唤拾荒者（卡）/ 拾荒者（遗物）。
/// 与塔1 一致：本场战斗不发常规奖励（塔1 rewards.clear() + noCardsInRewards），
/// 战利品全部由拾荒者死亡的偷窃能力结算（<see cref="MonkeyTollLoot" /> 暂存），
/// 战斗结束回到事件后由 <see cref="Rainworld.Scripts.Event.ScavengerEvent" /> 统一发放。
/// 注意：本遭遇仅供事件调用，需在 EncounterFilterHelpers 中从随机池剔除（同 ThreeChickenEvent）。
/// </summary>
[RegisterGlobalEncounter]
public class MonkeyTollEncounter : ModEncounterTemplate
{
	public override IEnumerable<MonsterModel> AllPossibleMonsters =>
	[
		ModelDb.Monster<MonkeyTollGold>(),
		ModelDb.Monster<MonkeyTollPotion>(),
		ModelDb.Monster<MonkeyTollCard>(),
		ModelDb.Monster<MonkeyTollRelic>(),
	];

	public override RoomType RoomType => RoomType.Monster;

	/// <summary>不发常规战斗奖励（塔1：奖励全部由 StealPower.onDeath 按打死的猴子补上）。</summary>
	public override bool ShouldGiveRewards => false;

	protected override bool UseActCombatBackground => true;

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
		// 每场收费站战斗开始前清空上一场的残留（战利品 + 逃走标记）
		MonkeyTollLoot.Reset();
		return
		[
			(ModelDb.Monster<MonkeyTollGold>().ToMutable(), null),
			(ModelDb.Monster<MonkeyTollPotion>().ToMutable(), null),
		];
	}
}
