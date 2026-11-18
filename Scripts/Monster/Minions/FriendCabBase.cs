using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Scaffolding.Godot;

namespace Rainworld.Scripts.Monster.Minions;

/// <summary>
/// 蜥蜴伙伴的行动契约：在通用伙伴契约（<see cref="IFriendPetTurn" />）之上，
/// 增加休眠/复活流程所需的意图切换（由 <see cref="FriendDormantPower" /> 驱动）。
/// </summary>
public interface IFriendCabCombat : IFriendPetTurn
{
    /// <summary>切换为攻击意图（召唤后与复活后调用）。</summary>
    void SetCabAttackIntent();

    /// <summary>切换为眩晕（休眠）意图（倒地时调用）。</summary>
    void SetCabDormantIntent();

    /// <summary>切到眩晕（休眠）的骨架动作（倒地时调用，对应塔1 的 yun 循环）。</summary>
    void SetCabDormantAnim();
}

/// <summary>
/// 蜥蜴伙伴基类：Friendcabs 骨架（5 皮肤、动画名带皮肤后缀）+ 攻击意图 + 冲撞攻击，
/// 以及「倒地 → 休眠 → 满血复活」的动画接线（对应塔1 AbstractFriend 的伙伴系统）。
/// 通用流程（召唤、伤害公式、冲撞攻击）来自 <see cref="FriendPetBase" />。
/// </summary>
public abstract class FriendCabBase : FriendPetBase, IFriendCabCombat
{
    /// <summary>骨架皮肤名（blue / red / green / caramel / yellow）。</summary>
    protected abstract string SkinName { get; }

    /// <summary>基础攻击力（对应塔1数值；实际伤害额外吃玩家「协调」层数与进化倍率）。</summary>
    protected abstract int AttackDamage { get; }

    public override string? CustomVisualsPath =>
        "res://Godot/Monster/Friendcabs.tscn";

    /// <summary>
    /// 场景未挂脚本，必须经工厂转换才能得到 NCreatureVisuals（写法同 Mushroom.cs）；
    /// 否则原版 Instantiate&lt;NCreatureVisuals&gt; 会抛 InvalidCastException。
    /// </summary>
    protected override NCreatureVisuals? TryCreateCreatureVisuals() =>
        RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(CustomVisualsPath!);

    /// <summary>Friendcabs 骨架共 5 个皮肤（green/yellow/red/caramel/blue），每只固定用其中一个。</summary>
    public override void SetupSkins(MegaSprite spine, MegaSkeleton skeleton)
    {
        skeleton.SetSkinByName(SkinName);
        skeleton.SetSlotsToSetupPose();
    }

    /// <summary>倒下后不淡出：休眠复活期间本体要保持可见、可悬停。</summary>
    public override bool ShouldFadeAfterDeath => false;

    // ———— 护甲面板数据（塔1 protectCapacity / intentTotalOutput）————

    /// <summary>
    /// 用身体替主人挡伤的血量：当前生命；休眠倒计时只剩 1 回合时本回合结束就会满血复活，
    /// 赶在敌人出手前起身，按满血计入（塔1 protectCapacity）。
    /// <para>永久眩晕（腐化蜥蜴）只在真的倒下眩晕时不参与承伤计算——活着的时候照样替主人挡伤，
    /// 不能无条件按 IStunForeverFriend 返回 0。</para>
    /// </summary>
    public override decimal ProtectCapacity
    {
        get
        {
            if (Creature.GetPower<FriendDormantPower>() is { IsDormant: true } dormant)
            {
                // 永久眩晕只能靠回血唤醒，不会自行起身，不计入护甲；
                // 普通蜥蜴倒计时只剩 1 回合时本回合结束就复活，按满血计入
                if (Creature.Monster is IStunForeverFriend)
                    return 0m;

                return dormant.Amount <= 1m ? Creature.MaxHp : 0m;
            }

            return Creature.IsAlive ? Math.Max(Creature.CurrentHp, 0m) : 0m;
        }
    }

    /// <summary>
    /// 攻击意图的预计伤害（单体口径）；休眠 / 眩晕（已倒地）时本轮不行动，返回 0。
    /// 多段攻击（烈焰蜥蜴 4×2）按「单段伤害 × 段数」计入，与头顶意图一致。
    /// </summary>
    public override decimal IntentOutput =>
        Creature.IsAlive ? ComputeAttackDamage() * IntentHitCount : 0m;

    /// <summary>攻击意图的段数（默认 1；烈焰蜥蜴为 2，需与 CreateAttackIntent 保持一致）。</summary>
    protected virtual int IntentHitCount => 1;

    // ———— 动画：idle / attack / hurt / 眩晕(Dormant) / 倒地(Dead) / 复活(Revive) ————

    /// <summary>眩晕（休眠）动作的触发器：塔1 AbstractFriend 的 setanim("yun")。</summary>
    public const string DormantTrigger = "Dormant";

    /// <summary>当前是否处于眩晕（休眠）状态：动画分支用它挡住会覆盖 yun 的动作。</summary>
    protected bool IsDormantNow =>
        Creature.GetPower<FriendDormantPower>() is { IsDormant: true };

    /// <summary>
    /// Friendcabs 的动画名带皮肤后缀（idle_loop_blue / attack_blue / hurt_blue），原版回退用的
    /// 无后缀动画名不存在，因此必须自定义 animator；
    /// 「Dead」先播 hurt 再停在 yun（休眠）循环上，「Revive」切回站立循环（骨架没有 die / revive 动画）。
    /// </summary>
    protected override CreatureAnimator? SetupCustomCreatureAnimator(MegaSprite controller)
    {
        AnimState idle = new($"idle_loop_{SkinName}", isLooping: true);
        AnimState attack = new($"attack_{SkinName}");
        AnimState hurt = new($"hurt_{SkinName}");
        attack.NextState = idle;
        hurt.NextState = idle;

        // 倒地：hurt 播完接休眠动画（yun 皮肤动画可能不存在，存在才接，避免卡在 hurt 末帧）
        AnimState dead = new($"hurt_{SkinName}");
        bool hasYun = controller.HasAnimation($"yun_{SkinName}");
        AnimState dormant = hasYun
            ? new AnimState($"yun_{SkinName}", isLooping: true)
            : idle;
        if (hasYun)
            dead.NextState = dormant;

        // 复活：NCreature.StartReviveAnim 触发 "Revive"，切回站立循环
        AnimState revive = new($"idle_loop_{SkinName}", isLooping: true);

        CreatureAnimator creatureAnimator = new(idle, controller);

        // 眩晕期间引擎仍可能补发 Idle / Hit（倒地动画收尾时会强制回待机），
        // 这些分支在休眠时一律忽略，否则 yun 会被立刻打断成站立
        creatureAnimator.AddAnyState("Idle", idle, () => !IsDormantNow);
        creatureAnimator.AddAnyState("Hit", hurt, () => !IsDormantNow);
        creatureAnimator.AddAnyState("Attack", attack, () => !IsDormantNow);
        creatureAnimator.AddAnyState("Dead", dead);
        creatureAnimator.AddAnyState(DormantTrigger, dormant);
        creatureAnimator.AddAnyState("Revive", revive);

        return creatureAnimator;
    }

    /// <summary>切到眩晕（休眠）的骨架动作（塔1 setanim 的 yun 循环）。</summary>
    public virtual void SetCabDormantAnim() =>
        Creature.GetCreatureNode()?.SetAnimationTrigger(DormantTrigger);

    // ———— 意图：攻击 / 休眠（宠物不会被 RollMove 推进，意图由代码切换）————

    protected MoveState? _attackState;
    protected MoveState? _dormantState;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attackState = new("FRIEND_CAB_ATTACK", _ => Task.CompletedTask, CreateAttackIntents());
        attackState.FollowUpState = attackState;

        MoveState dormantState = new("FRIEND_CAB_DORMANT", _ => Task.CompletedTask, new StunIntent());
        dormantState.FollowUpState = dormantState;

        _attackState = attackState;
        _dormantState = dormantState;

        return new MonsterMoveStateMachine([attackState, dormantState], attackState);
    }

    /// <summary>攻击意图：默认单体，数值随「协调」与力量动态显示；烈焰蜥蜴覆盖为多段。</summary>
    protected virtual AbstractIntent CreateAttackIntent() =>
        new SingleAttackIntent(ComputeAttackDamage);

    /// <summary>
    /// 攻击状态要显示的意图组（默认只有攻击）。绿蜥蜴这类「攻击后还给主人格挡」的伙伴
    /// 覆写它把格挡意图一起加上，头顶就会同时出现剑和盾。
    /// </summary>
    protected virtual AbstractIntent[] CreateAttackIntents() => [CreateAttackIntent()];

    /// <summary>攻击伤害 =（基础值 + 玩家「协调」层数）× 进化倍率（对应塔1 computeFriendDamage）。</summary>
    protected virtual decimal ComputeAttackDamage() => ComputeAttackDamage(AttackDamage);

    public virtual void SetCabAttackIntent() => ApplyIntent(_attackState);

    public virtual void SetCabDormantIntent() => ApplyIntent(_dormantState);


    /// <summary>切换到指定状态（子类按形态扩展状态机时复用）。</summary>
    protected void ApplyIntent(MoveState? state)
    {
        if (state == null)
            return;
        // 宠物召唤链路不一定经过 CombatManager.AddCreature，战备状态机可能尚未生成
        if (MoveStateMachine == null)
            SetUpForCombat();
        SetMoveImmediate(state, true);
    }

    // ———— 攻击音效 ————

    /// <summary>
    /// 攻击音效：默认用原版「双尾鼠撕咬」（event:/sfx/enemy/enemy_attacks/two_tail_rats/…，小型生物近身咬击）。
    /// 腐化蜥蜴有自己的 rotcabatk，覆写此属性即可。
    /// </summary>
    protected virtual string AttackSfx =>
        "event:/sfx/enemy/enemy_attacks/two_tail_rats/two_tail_rats_attack_bite";

    /// <summary>一轮行动只播一次攻击音：多段 / 扫场不跟着段数叠加。</summary>
    protected void PlayAttackSfx() => SfxCmd.Play(AttackSfx);

    // ———— 攻击执行 ————

    /// <summary>通用伙伴动作入口：转发到蜥蜴伙伴的攻击实现（FFireCab 等子类可继续覆写 PerformCabAttack）。</summary>
    public override Task PerformFriendTurn(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> targets)
        => PerformCabAttack(choiceContext, targets);

    public virtual async Task PerformCabAttack(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> targets)
    {
        PlayAttackSfx();
        foreach (Creature target in targets)
        {
            if (target.IsAlive)
                await AttackOnce(choiceContext, target);
        }
        RefreshCabIntents();
    }

    /// <summary>
    /// 攻击命中特效：默认借用原版「咬人卷轴」的咬击特效（vfx/vfx_bite）。
    /// 腐化蜥蜴的腐化形态会把它覆盖成 null，不播这个特效。
    /// </summary>
    protected virtual string? AttackHitVfx => "vfx/vfx_bite";

    /// <summary>单体一次冲撞攻击：先播咬击特效，再播攻击动作并结算伤害。</summary>
    protected async Task AttackOnce(PlayerChoiceContext choiceContext, Creature target)
    {
        // 放在结算前播：目标这时一定还在场上，致命一击也看得到咬击特效
        if (AttackHitVfx is { } vfx)
            VfxCmd.PlayOnCreature(target, vfx);

        await BumpAttackAsync(choiceContext, target, ComputeAttackDamage());
    }

    /// <summary>刷新意图显示（力量、「协调」、进化变化后攻击数字保持最新）。</summary>
    protected void RefreshCabIntents() => RefreshFriendIntents();

    // ———— 召唤 ————

    /// <summary>蜥蜴类专属初始化：挂休眠复活与守护能力并切到攻击意图（其余召唤流程由基类完成）。</summary>
    protected override async Task OnSummoned(PlayerChoiceContext choiceContext, Player owner, FriendSummonOptions options)
    {
        // 守护：随从替玩家承伤（对应塔1伙伴的 Protect；塔1中只有蜥蜴类伙伴挡伤，猫崽/拾荒者不挡）。
        // 用自研 FriendGuardPower 而非 MinionLib 的 MinionGuardianPower：后者硬性要求站在 Front 位，
        // 而伙伴前几只按 FixedOrder 站在 FrontUpper/BackUpper/Upper，会永远不承伤。
        await PowerCmd.Apply<FriendGuardPower>(choiceContext, Creature, 1m, owner.Creature, options.Source);

        // 休眠复活：蜥蜴类伙伴倒下后不退场，休眠数回合后满血复活（仅蜥蜴类拥有）
        await PowerCmd.Apply<FriendDormantPower>(choiceContext, Creature, 1m, owner.Creature, options.Source);

        SetCabAttackIntent();
    }
}

/// <summary>
/// 多段攻击意图：MultiAttackIntent 本身没有 Func 伤害构造，
/// 通过 protected 的 DamageCalc 补上，让「协调」/力量实时反映在 4×2 这样的显示上。
/// </summary>
internal sealed class CabMultiAttackIntent : MultiAttackIntent
{
    public CabMultiAttackIntent(Func<decimal> damageCalc, int repeat) : base(0, repeat)
    {
        DamageCalc = damageCalc;
    }
}