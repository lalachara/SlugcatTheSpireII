using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Rainworld.Scripts.Monster.MonkeyToll;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Event;

/// <summary>
/// 拾荒者收费站（SCAVENGER_EVENT，对应塔1 事件 Monkeys）：
/// INITIAL 页三选项——多给一些（200 金 → 随机遗物）/ 少给一些（100 金）/
/// 强行通过（进入收费站战斗）；**没有「离开」**，玩家必须选一个（金币不足时付费项锁定置灰）。
/// 战斗结束回到事件（Resume）：<b>全部歼灭</b>才经奖励屏返还被偷走的东西 + 发放击杀补给
/// （偷金那只额外 20 金 / 偷药那只随机药水 / 偷卡那只三选一卡牌 / 偷遗物那只随机遗物），
/// 然后显示战后文本与「离开」；<b>只要有一只逃走</b> → 什么都不发、不弹奖励屏，直接结束事件。
/// </summary>
[RegisterActEvent(typeof(Hive))] 
public sealed class ScavengerEvent : ModEventTemplate
{
	// 塔1 Monkeys.PAY_GOLD / BARGAIN_GOLD
	private const int PayMoreGold = 200;

	private const int PayLessGold = 100;

	// 事件图占位：资源未就绪前回退默认图（等场景/图片资源做好后直接放上去即可）。
	private const string PortraitPath = "res://Resource/Event/Monkeys.png";

	public override EventAssetProfile AssetProfile => new(
		InitialPortraitPath: ResourceLoader.Exists(PortraitPath) ? PortraitPath : null
	);

	public override bool IsShared => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => [];

	public override bool IsAllowed(IRunState runState)
	{
		var cfg = SpawnConfigRuntime.GetEffectiveConfig();
		return SpawnConfigRuntime.ModeShouldAppear(cfg.ScavengerEventSpawnMode);
	}

	// 初始选项（塔1 generateInitialOptions）：金币不足的付费选项换成锁定文案（置灰不可点）。
	// 这里刻意不给「离开」：玩家必须从三选一里挑一个，不能白嫖跳过事件。
	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		int gold = Owner!.Gold;
		return
		[
			gold >= PayMoreGold
				? new EventOption(this, GiveMore, $"{Id.Entry}.pages.INITIAL.options.GIVE_MORE")
				: new EventOption(this, null, $"{Id.Entry}.pages.INITIAL.options.GIVE_MORE_LOCKED", true),
			gold >= PayLessGold
				? new EventOption(this, GiveLess, $"{Id.Entry}.pages.INITIAL.options.GIVE_LESS")
				: new EventOption(this, null, $"{Id.Entry}.pages.INITIAL.options.GIVE_LESS_LOCKED", true),
			new EventOption(this, ForceThrough, $"{Id.Entry}.pages.INITIAL.options.FORCE_THROUGH"),
		];
	}

	// 多给一些：失去 200 金，获得一件随机遗物（塔1：loseGold(200) + 随机稀有度无屏遗物）。
	private async Task GiveMore()
	{
		await PlayerCmd.LoseGold(PayMoreGold, Owner!, GoldLossType.Spent);
		// 0.113：PullNextRelicFromFront(Player) 重载已移除，奖励用的等价方法是 PullNextRelicForRewardFromFront
		RelicModel relic = RelicFactory.PullNextRelicForRewardFromFront(Owner!);
		await RelicCmd.Obtain(relic.ToMutable(), Owner!, -1);
		ShowLeavePage("GIVE_MORE");
	}

	// 少给一些：失去 100 金（塔1：loseGold(100)）。
	private async Task GiveLess()
	{
		await PlayerCmd.LoseGold(PayLessGold, Owner!, GoldLossType.Spent);
		ShowLeavePage("GIVE_LESS");
	}

	// 强行通过：进入收费站战斗，结束后回到本事件结算（塔1：enterCombatFromImage + reopen）。
	private Task ForceThrough()
	{
		EnterCombatWithoutExitingEvent<MonkeyTollEncounter>(
			Array.Empty<Reward>(),
			shouldResumeAfterCombat: true
		);
		return Task.CompletedTask;
	}

	// 离开（含战斗后的战后页）：结束事件。
	private Task Run()
	{
		SetEventFinished(L10NLookup($"{Id.Entry}.pages.RUN.description"));
		return Task.CompletedTask;
	}

	/// <summary>付费后的「离开」页（塔1 switchToLeave：正文换成结果，只剩离开）。</summary>
	private void ShowLeavePage(string page)
	{
		SetEventState(L10NLookup($"{Id.Entry}.pages.{page}.description"),
		[
			new EventOption(this, Run, $"{Id.Entry}.pages.{page}.options.RUN"),
		]);
	}

	/// <summary>
	/// 战斗结束回到事件（塔1 Monkeys.reopen）：
	/// 1）先把本玩家名下的赃物 + 击杀补给经奖励屏发放（OfferCustom 领完后奖励屏自动消失，
	///    对应塔1 的 isScreenUp 等待）；
	/// 2）全部歼灭 → 战后文本 + 离开选项；
	/// 3）有拾荒者逃走 → 直接结束事件（不显示战后页）。
	/// 注意：这里不做全表 Reset——Resume 会按玩家事件实例各执行一次，各取各的赃物；
	/// 全量清空放在遭遇开场（<see cref="MonkeyTollEncounter" /> 的 GenerateMonsters）。
	/// </summary>
	public override async Task Resume(AbstractRoom exitedRoom)
	{
		List<Reward> loot = MonkeyTollLoot.TakeFor(Owner!);

		// 有拾荒者逃走：不显示战后页、直接结束事件，但击杀所得（击杀补给 + 被偷物品返还）照常发放。
		if (MonkeyTollLoot.AnyEscaped)
		{
			if (loot.Count > 0)
				await RewardsCmd.OfferCustom(Owner!, loot);
			SetEventFinished(L10NLookup($"{Id.Entry}.pages.RUN.description"));
			return;
		}

		if (loot.Count > 0)
			await RewardsCmd.OfferCustom(Owner!, loot);
		SetEventState(L10NLookup($"{Id.Entry}.pages.FORCE_THROUGH.description"),
		[
			new EventOption(this, Run, $"{Id.Entry}.pages.FORCE_THROUGH.options.RUN"),
		]);
	}
}
