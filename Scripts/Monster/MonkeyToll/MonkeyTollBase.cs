using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Godot;

namespace Rainworld.Scripts.Monster.MonkeyToll;

/// <summary>
/// 「拾荒者收费站」四只拾荒者的共同基类（对应塔1 Monkey1~4）：
/// HP 38-45 / 41-48 / 44-51（塔1 三档折成 sts2 两档）；
/// 攻击命中时靠偷窃能力自动拾取（金/药/卡/遗物），死亡时归还 + 发放击杀补给；
/// 战斗中途可召唤同伴（五步链加入战斗 + 显式意图 + 全体敌人重排 + 从右侧滑入）；
/// 逃跑的拾荒者会让事件直接结束（<see cref="MonkeyTollLoot.MarkEscaped" />）。
/// </summary>
public abstract class MonkeyTollBase : ModMonsterTemplate
{
	// ———— 数值（塔1 三档 [38-45 / 41-48 / 44-51] 折成 sts2 两档）————

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 39, 33);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 46, 40);

	/// <summary>攻击伤害（子类按塔1 数值实现，进阶 DeadlyEnemies 取高阶值）。</summary>
	protected abstract int AttackDamage { get; }

	/// <summary>偷窃能力层数：金猴 = 每次偷的金币数，其余 = 1（件）。</summary>
	protected virtual int StealValue => 1;

	/// <summary>格挡值（塔1 固定 10）。</summary>
	protected virtual int DefendAmount => 10;

	/// <summary>由子类在 <see cref="GenerateMoveStateMachine" /> 里记录初始意图（召唤时强制设置用）。</summary>
	protected MoveState? InitialState;

	// ———— 视觉（拾荒者骨架：四只共用一个 spine，用塔1 的 scavenger1~4 皮肤区分）————

	/// <summary>专属场景：拾荒者（塔1 敌方 Monkey1~4 的 allScavenger 骨架）。</summary>
	
	private string ResolvedVisualsPath => "res://Godot/Monster/monkey.tscn";

	public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: ResolvedVisualsPath);

	protected override NCreatureVisuals? TryCreateCreatureVisuals() =>
		RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.VisualsScenePath!);

	/// <summary>皮肤名：塔1 的 Monkey1~4 依次用 scavenger1~4，四只外观互不相同。</summary>
	protected abstract string SkinName { get; }

	public override void SetupSkins(MegaSprite spine, MegaSkeleton skeleton)
	{
		// scavenger1~4 都是整套立绘（骨架自带的 default 皮肤只有一个脖子挂件），
		// 换皮肤后再同步一次 setup pose，让所有挂件都来自新皮肤
		skeleton.SetSkin(skeleton.GetData().FindSkin(SkinName));
		skeleton.SetSlotsToSetupPose();
	}

	/// <summary>
	/// 塔1 敌方拾荒者的动作集：待机 idle_loop_1、攻击 attack1、受击 hurt1、
	/// 格挡/召唤 point_1、逃跑 run1（骨架没有死亡动作，死亡沿用 hurt1 停在最后一帧）。
	/// </summary>
	protected override CreatureAnimator? SetupCustomCreatureAnimator(MegaSprite controller)
	{
		AnimState idle = new("idle_loop_1", isLooping: true);
		AnimState attack = new("attack1");
		AnimState hurt = new("hurt1");
		AnimState point = new("point_1");
		AnimState run = new("run1", isLooping: true);
		AnimState dead = new("hurt1");

		attack.NextState = idle;
		hurt.NextState = idle;
		point.NextState = idle;

		CreatureAnimator creatureAnimator = new(idle, controller);
		creatureAnimator.AddAnyState("Idle", idle);
		creatureAnimator.AddAnyState("Hit", hurt);
		creatureAnimator.AddAnyState("Attack", attack);
		creatureAnimator.AddAnyState("Point", point);
		creatureAnimator.AddAnyState("Run", run);
		creatureAnimator.AddAnyState("Dead", dead);
		return creatureAnimator;
	}

	/// <summary>塔1 格挡 / 召唤时播 point_1（抬手示意），等一小段好让后续结算接着进行。</summary>
	protected async Task PlayPointAnim(float wait = 0.35f)
	{
		await CreatureCmd.TriggerAnim(Creature, "Point", wait);
	}

	// ———— 偷窃能力（开场生成与召唤两条路径，幂等挂载）————

	private bool _stealPowerApplied;

	public override async Task AfterAddedToRoom()
	{
		await EnsureStealPower();
	}

	/// <summary>挂上偷窃能力（幂等）。开场由战斗流程触发，召唤时由召唤方显式调用。</summary>
	public async Task EnsureStealPower()
	{
		if (_stealPowerApplied)
			return;
		_stealPowerApplied = true;
		await ApplyStealPower();
	}

	/// <summary>子类挂上自己的偷窃能力（层数用 <see cref="StealValue" />）。</summary>
	protected abstract Task ApplyStealPower();

	// ———— 召唤同伴（对应塔1 MonkeySummonAction）————

	/// <summary>
	/// 召唤一只拾荒者：加入战斗 → 挂偷窃能力 → 设置初始意图 → 全体敌人重排（新怪从右侧滑入）。
	/// </summary>
	protected async Task SummonMonkey(MonsterModel blueprint, bool playPointAnim = true)
	{
		if (Creature.CombatState is not CombatState combatState)
			return;
		CombatManager? manager = CombatManager.Instance;
		if (manager == null)
			return;
		if (blueprint.ToMutable() is not MonkeyTollBase mutable)
			return;

		// 塔1 召唤前先播 point_1；同一回合已经播过（如金猴的格挡召唤）就跳过
		if (playPointAnim)
			await PlayPointAnim();

		Creature summoned = combatState.CreateCreature(mutable, CombatSide.Enemy, slot: null);
		combatState.AddCreature(summoned);
		manager.AddCreature(summoned);
		NCombatRoom.Instance?.AddCreature(summoned);
		await manager.AfterCreatureAdded(summoned);

		// 引擎可能已触发 AfterAddedToRoom；这里再兜底一次（幂等）
		await mutable.EnsureStealPower();
		mutable.SetInitialIntent();

		RearrangeEnemies(combatState, summoned);
	}

	/// <summary>把当前意图强制设为初始 move（召唤后调用，保证新怪显示正确意图）。</summary>
	public void SetInitialIntent()
	{
		if (InitialState != null)
			SetMoveImmediate(InitialState, true);
	}

	/// <summary>
	/// 把所有存活敌人按 NCombatRoom.PositionEnemies 的算法重排（960 宽、间距 70、y=200），
	/// 新召唤的拾荒者从屏幕右侧滑入（塔1 animX = 1200）。
	/// </summary>
	private void RearrangeEnemies(CombatState combatState, Creature summoned)
	{
		NCombatRoom? room = NCombatRoom.Instance;
		if (room == null)
			return;

		List<NCreature> nodes = [];
		foreach (Creature enemy in combatState.Enemies)
		{
			if (enemy.IsDead || enemy.PetOwner != null)
				continue;
			NCreature? node = room.GetCreatureNode(enemy);
			if (node != null)
				nodes.Add(node);
		}
		if (nodes.Count == 0)
			return;

		const float areaWidth = 960f;
		float gap = 70f;
		float totalWidth = nodes.Sum(n => n.Visuals.Bounds.Size.X);
		float span = totalWidth + (nodes.Count - 1) * gap;
		float startX = Math.Max((areaWidth - span) * 0.5f, 150f);
		float stagger = 0f;
		if (startX + span > areaWidth && nodes.Count > 1)
		{
			gap = Math.Max((areaWidth - 150f - totalWidth) / (nodes.Count - 1), 5f);
			span = totalWidth + (nodes.Count - 1) * gap;
			startX = (areaWidth - span) * 0.5f;
			if (gap < 30f)
				stagger = Mathf.Lerp(60f, 40f, (gap - 5f) / 25f);
		}

		for (int i = 0; i < nodes.Count; i++)
		{
			NCreature node = nodes[i];
			Vector2 target = new(startX + node.Visuals.Bounds.Size.X * 0.5f,
				200f - (i % 2 != 0 ? stagger : 0f));
			if (node.Entity == summoned)
			{
				node.Position = target + new Vector2(1200f, 0f);
				node.CreateTween().TweenProperty(node, "position", target, 0.6f);
			}
			else
			{
				node.CreateTween().TweenProperty(node, "position", target, 0.45f);
			}
			startX += node.Visuals.Bounds.Size.X + gap;
		}
	}

	// ———— 通用行动 ————

	/// <summary>普通攻击（塔1 case 0）。</summary>
	protected async Task AttackMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd
			.Attack(AttackDamage)
			.FromMonster(this)
			.WithAttackerAnim("Attack", 0.2f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	/// <summary>给全体存活敌人（含自己）各加一次格挡（塔1 Monkey2/3 的 case 3）。</summary>
	protected async Task DefendAllMove(IReadOnlyList<Creature> targets)
	{
		if (Creature.CombatState is not CombatState combatState)
			return;
		await PlayPointAnim();
		foreach (Creature enemy in combatState.Enemies.ToList())
		{
			if (enemy.IsDead || !enemy.IsAlive)
				continue;
			await CreatureCmd.GainBlock(enemy, DefendAmount, ValueProp.Unpowered, null);
		}
	}

	/// <summary>自己获得格挡（塔1 Monkey1 的 case 1 前半段）。</summary>
	protected async Task GainSelfBlock()
	{
		await PlayPointAnim();
		await CreatureCmd.GainBlock(Creature, DefendAmount, ValueProp.Unpowered, null);
	}

	/// <summary>逃跑（塔1 case 2 / escape 覆写）：标记「有猴子逃走」，事件会直接结束。</summary>
	protected async Task EscapeMove(IReadOnlyList<Creature> targets)
	{
		MonkeyTollLoot.MarkEscaped();

		NCreature? node = NCombatRoom.Instance?.GetCreatureNode(Creature);
		if (node == null)
		{
			await Cmd.CustomScaledWait(0.3f, 0.3f);
			await CreatureCmd.Escape(Creature);
			return;
		}

		// 拾荒者原本朝左站，往右逃跑要水平翻转（沿 Y 轴翻转），否则看起来像在倒退
		node.Visuals.Scale = new Vector2(-Mathf.Abs(node.Visuals.Scale.X), node.Visuals.Scale.Y);

		// 逃跑动画：animator 触发 + 直接播 spine 双保险（只走其中一条都可能被空闲状态覆盖）
		node.SetAnimationTrigger("Run");
		PlayRunAnim(node);

		// 平移出屏；期间持续重播 run1，避免被引擎的空闲动画顶掉
		// 逃跑移动速度比之前慢 50%（时长 ×2）
		const float duration = 1.4f;
		Tween tween = node.CreateTween();
		for (float elapsed = 0f; elapsed < duration; elapsed += 0.1f)
			tween.TweenCallback(Callable.From(() => PlayRunAnim(node)));

		tween.TweenProperty(node, "position", node.Position + new Vector2(1200f, 0f), duration);

		// 动画等待必须带超时兜底：逃跑过程中若战斗结束 / 节点被释放，tween 不会再发 Finished，
		// 直接 await 会让这只怪的回合永远停在这里（逃走导致整局卡死的已知来源）。
		Task finished = node.ToSignal(tween, Tween.SignalName.Finished).ToTask();
		Task timeout = node.ToSignal(node.GetTree().CreateTimer(duration + 1.5f), "timeout").ToTask();
		await Task.WhenAny(finished, timeout);

		// 战斗已结束 / 本体已被移除时不要再走 Escape（避免操作已释放的对象）
		if (Creature.CombatState is not { } combatState || !combatState.IsLiveCombat() || !Creature.IsAlive)
			return;

		await CreatureCmd.Escape(Creature);
	}

	/// <summary>让骨架保持播放 run1 循环（没有 spine 动画时安静跳过）。</summary>
	private static void PlayRunAnim(NCreature node)
	{
		if (!GodotObject.IsInstanceValid(node) || !node.HasSpineAnimation)
			return;

		node.Visuals.SpineAnimation.SetAnimation("run1", loop: true);
	}
}
