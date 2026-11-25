using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Rainworld.Scripts.Monster.Minions;

/// <summary>
/// 伙伴：腐化蜥蜴（对应塔1 PurpleLeggy / 紫色菌块召唤的紫长腿菌）。
/// 与普通蜥蜴伙伴的区别（全部照搬塔1）：
/// 1. 登场吞噬——场上其它伙伴当场退场，它们的生命上限之和并进自己；
///    只要它在场，之后召唤的伙伴也会被直接吞噬（拦截在 <see cref="FriendPetBase.OnSummon" />）。
/// 2. 生命上限超过 <see cref="RotThreshold" /> 进入腐化形态：攻击从单体变为打全场。
/// 3. 眩晕而不是休眠——0 血后进入永久眩晕，不会随回合自动复活，只能靠回血唤醒。
/// 4. 抗伤：继承蜥蜴伙伴的守护，只替自己的主人承伤，多模式下不会替别人的宠物挡刀。
/// 5. 视觉：直接用塔1 的 LizardROTSP 骨架（rotcab.tscn），普通 / 腐化两套动画按生命上限切换。
/// </summary>
[RegisterMonster]
public sealed class FCorruptLizard : FriendCabBase, IStunForeverFriend
{
    /// <summary>塔1 BASE_MAX_HP：召唤出来就这么多，之后全靠吞噬成长。</summary>
    public const int BaseMaxHp = 10;

    /// <summary>塔1 DAMAGE_PERCENT：攻击伤害 = 生命上限的 30%（至少 1）。</summary>
    public const decimal DamagePercent = 30m;

    /// <summary>塔1 GROWTH_STEP：生命上限超过它进入腐化形态。</summary>
    public const decimal RotThreshold = 40m;

    /// <summary>塔1 GROWTH_HP_PER_STEP：腐化后生命上限每多 5 点长一小档。</summary>
    public const decimal GrowthHpPerStep = 5m;

    /// <summary>塔1 GROWTH_RATE：每小档放大 0.03125（合计每 40 点生命上限 +0.25 倍）。</summary>
    public const float GrowthRate = 0.03125f;

    /// <summary>塔1 MAX_GROWTH：体型最多放大到 2 倍。</summary>
    public const float MaxGrowth = 2f;

    public override int MinInitialHp => BaseMaxHp;
    public override int MaxInitialHp => BaseMaxHp;

    /// <summary>腐化蜥蜴专属场景：塔1 的 LizardROTSP 骨架，普通与腐化两套动画都在这个文件里。</summary>
    public override string? CustomVisualsPath => "res://Godot/Monster/rotcab.tscn";

    /// <summary>ROT 骨架只有 default 一个皮肤（形态靠动画名区分），保留抽象成员占位。</summary>
    protected override string SkinName => "default";

    /// <summary>ROT 骨架没有换肤需求，直接把槽位摆回 setup pose。</summary>
    public override void SetupSkins(MegaSprite spine, MegaSkeleton skeleton)
    {
        skeleton.SetSlotsToSetupPose();
    }

    /// <summary>抽象成员要求的基础攻击力；实际伤害按当前生命上限计算（见 ComputeAttackDamage）。</summary>
    protected override int AttackDamage => (int)(BaseMaxHp * DamagePercent / 100m);

    /// <summary>腐化形态：生命上限超过阈值（塔1 rotForm）。</summary>
    public bool IsRotForm => Creature.MaxHp > RotThreshold;

    /// <summary>
    /// 腐化蜥蜴专属音效事件（FMOD：event:/Rainworld/sfx/rotcab*）。
    /// 命名里的 full 指倒地（spine 的 hurt_to_full → full_idle），rot 指腐化形态的变身。
    /// </summary>
    public static class Sfx
    {
        /// <summary>攻击。</summary>
        public const string Attack = "event:/Rainworld/sfx/rotcabatk";

        /// <summary>受击至眩晕（倒地）。</summary>
        public const string Fall = "event:/Rainworld/sfx/rotcabfull";

        /// <summary>治疗（消耗牌回血 / 被回血唤醒）。</summary>
        public const string Heal = "event:/Rainworld/sfx/rotcabheal";

        /// <summary>受击。</summary>
        public const string Hurt = "event:/Rainworld/sfx/rotcabhurt";

        /// <summary>生命上限跨过阈值变身（塔1 nor_to_rot）。</summary>
        public const string Rot = "event:/Rainworld/sfx/rotcabrot";
    }

    /// <summary>
    /// 塔1 growthScale：普通形态恒为 1；腐化后生命上限每多 5 点放大一小档，封顶 2 倍
    /// （45 → 1.03、80 → 1.25、120 → 1.50、200 及以上 → 2.00）。
    /// </summary>
    public float GrowthScale
    {
        get
        {
            if (!IsRotForm)
                return 1f;

            int steps = (int)((Creature.MaxHp - RotThreshold) / GrowthHpPerStep);
            return Math.Min(1f + steps * GrowthRate, MaxGrowth);
        }
    }

    /// <summary>腐化形态攻击所有敌人（塔1 rotForm 的 takeTurn 分支）。</summary>
    public override bool HitsAllEnemies => IsRotForm;

    /// <summary>腐化形态（生命上限 &gt; 40 变身后）不播咬击特效，普通形态照旧。</summary>
    protected override string? AttackHitVfx => IsRotForm ? null : base.AttackHitVfx;

    /// <summary>
    /// 腐化形态的全体攻击额外来一次屏幕震动（普通形态不加）。
    /// 基类 <c>PerformCabAttack</c> 会对所有目标各打一段，这里在整轮打完后震一次。
    /// </summary>
    public override async Task PerformCabAttack(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> targets)
    {
        await base.PerformCabAttack(choiceContext, targets);

        if (IsRotForm)
            NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Normal);
    }

    private MoveState? _corruptAttackState;

    /// <summary>上一次同步的形态；null 表示尚未同步过。</summary>
    private bool? _lastRotForm;

    /// <summary>已经套用到视觉上的体型倍率（塔1 currentGrowth）。</summary>
    private float _appliedGrowth = 1f;

    // ———— 意图：单体攻击 / 腐化扫场 / 眩晕 ————

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attackState = new("FRIEND_CAB_ATTACK", _ => Task.CompletedTask, CreateAttackIntent());
        attackState.FollowUpState = attackState;

        // 腐化形态：实际打全场，悬停说明同步改成「对所有敌人造成伤害」
        MoveState corruptState = new("FRIEND_CORRUPT_ATTACK", _ => Task.CompletedTask, CreateCorruptAttackIntent());
        corruptState.FollowUpState = corruptState;

        MoveState dormantState = new("FRIEND_CAB_DORMANT", _ => Task.CompletedTask, new FriendStunIntent());
        dormantState.FollowUpState = dormantState;

        _attackState = attackState;
        _corruptAttackState = corruptState;
        _dormantState = dormantState;

        return new MonsterMoveStateMachine([attackState, corruptState, dormantState], attackState);
    }

    private AbstractIntent CreateCorruptAttackIntent() => new CorruptAllEnemiesAttackIntent(ComputeAttackDamage);

    /// <summary>按形态选择攻击意图（腐化形态 → 扫场状态）。</summary>
    public override void SetCabAttackIntent()
    {
        _lastRotForm = IsRotForm;
        ApplyIntent(IsRotForm ? _corruptAttackState : _attackState);
    }

    /// <summary>
    /// 同步腐化形态：生命上限跨过阈值时切换攻击意图并切到对应的动画（塔1 的 nor_to_rot 变身），
    /// 未跨阈值时只刷新意图数字（上限变了，头顶伤害要跟着变）。
    /// </summary>
    public void SyncRotForm()
    {
        if (_lastRotForm != IsRotForm)
        {
            // 首次同步（刚召唤）时 _lastRotForm 为 null：骨架本来就是按当前生命上限新建的，
            // 只需切成形态对应的待机；只有确实见过普通形态，才算「变身」并播 nor_to_rot。
            bool crossedIntoRot = _lastRotForm == false && IsRotForm;
            SetCabAttackIntent();
            // 生命上限第一次跨过阈值：播变身音
            if (crossedIntoRot)
                SfxCmd.Play(Sfx.Rot);
            Creature.GetCreatureNode()?.SetAnimationTrigger(crossedIntoRot ? RotFormTrigger : "Idle");
        }
        else
        {
            RefreshCabIntents();
        }

        // 体型：塔1 每次吞噬后都重算，贴图 / 碰撞箱 / 意图位置一起长
        SyncBodyScale();
    }

    // ———— 伤害：生命上限的 30% ————

    /// <summary>
    /// 塔1 damageOutput：伤害 = max(1, 生命上限 × 30%)，再按本 mod 惯例叠加「协调」与「进化」。
    /// 意图与实战共用这一处数值，保证显示与结算一致。
    /// </summary>
    protected override decimal ComputeAttackDamage() =>
        ComputeAttackDamage(Math.Max(1m, Math.Floor(Creature.MaxHp * DamagePercent / 100m)));

    // ———— 攻击 ————

    /// <summary>攻击音：腐化蜥蜴用自己的 rotcabatk（基类默认是普通蜥蜴的咬击音）。</summary>
    protected override string AttackSfx => Sfx.Attack;

    // ———— 召唤：挂形态能力并吞噬场上其它伙伴 ————

    protected override async Task OnSummoned(PlayerChoiceContext choiceContext, Player owner, FriendSummonOptions options)
    {
        await base.OnSummoned(choiceContext, owner, options);

        // 形态能力：主人消耗牌时回血 + 形态描述（塔1 挂在紫蘑菇遗物上，塔2 挂在宠物自身）
        await PowerCmd.Apply<CorruptLizardPower>(choiceContext, Creature, 1m, owner.Creature, options.Source);

        await DevourAllOthersAsync(owner);
        SyncRotForm();
    }

    /// <summary>
    /// 登场吞噬（塔1 devourAllOthers）：场上其它伙伴全部当场退场，生命上限之和并进自己。
    /// 只处理主人自己的伙伴，多人模式下不会影响别人的宠物。
    /// </summary>
    private async Task DevourAllOthersAsync(Player owner)
    {
        decimal sum = 0m;
        // 遍历副本：循环里会往原列表里删元素
        foreach (Creature friend in FriendPositions.All(owner).ToList())
        {
            if (friend == Creature || friend.PetOwner != owner)
                continue;

            // 只吞本 mod 的伙伴：原版宠物（异鸟宝宝等）没有血条、生命上限是占位值（9999），
            // 吞了会直接给腐化蜥蜴一堆假血量，也会把原版宠物错误移出场
            if (friend.Monster is not FriendPetBase)
                continue;

            sum += Math.Max(friend.MaxHp, 0m);
            FriendPositions.LoseFriend(owner, friend);
        }

        if (sum > 0m)
            await AbsorbAsync(sum);
    }

    /// <summary>
    /// 把 amount 点生命并进自己（塔1 absorb）：生命上限与当前生命一起涨。
    /// <see cref="CreatureCmd.GainMaxHp" /> 内部会 Heal(amount)，所以眩晕中被喂食会直接醒来，
    /// 并把这份营养吃进去——与塔1 increaseMaxHp → heal → wakeUpOnHeal 完全一致。
    /// </summary>
    public async Task AbsorbAsync(decimal amount)
    {
        if (amount <= 0m)
            return;

        await CreatureCmd.GainMaxHp(Creature, amount);
        SyncRotForm();
    }

    // ———— 吞噬后续召唤 ————

    /// <summary>主人场上那只腐化蜥蜴；没有时返回 null。眩晕中的仍算在场（塔1 findOnField）。</summary>
    public static FCorruptLizard? FindOnField(Player owner, Creature? exclude = null)
    {
        foreach (Creature pet in FriendPositions.All(owner))
        {
            if (pet == exclude)
                continue;
            if (pet.Monster is FCorruptLizard lizard)
                return lizard;
        }

        return null;
    }

    /// <summary>
    /// 场上已有腐化蜥蜴时，把新召唤的伙伴喂给它：生命上限并进腐化蜥蜴，然后让这只伙伴直接退场。
    /// 对应塔1 FriendMonsterController.addFriend 里对 PurpleLeggy.findOnField() 的拦截。
    /// </summary>
    /// <returns>true 表示已被吞噬，调用方（召唤流程）应立即结束。</returns>
    public static async Task<bool> TryDevourAsync(Player owner, Creature newcomer, decimal? overrideMaxHp)
    {
        // 只处理本 mod 的伙伴召唤；原版宠物（异鸟宝宝等）不参与吞噬
        if (newcomer.Monster is not FriendPetBase)
            return false;

        if (FindOnField(owner, newcomer) is not { } lizard)
            return false;

        // 生命上限以召唤参数为准（部分召唤会临时覆盖初始生命上限）
        await lizard.AbsorbAsync(overrideMaxHp ?? newcomer.MaxHp);
        FriendPositions.LoseFriend(owner, newcomer);
        return true;
    }

    // ———— 视觉：普通 / 腐化两套动画（塔1 PurpleLeggy + AbstractFriend）————

    /// <summary>变身触发器：生命上限第一次跨过阈值时播放 nor_to_rot。</summary>
    public const string RotFormTrigger = "RotForm";

    /// <summary>
    /// 同步体型（塔1 applyGrowth / applyHitboxScale）：贴图、碰撞箱、血条、意图位置一起放大。
    /// </summary>
    /// <remarks>
    /// 引擎的 <see cref="NCreature.SetScaleAndHue" /> 缩的是视觉根节点，并立刻重算碰撞箱：
    /// <c>UpdateBounds</c> 里尺寸乘根缩放，位置取 %Bounds 的 GlobalPosition（同样含根缩放），
    /// 意图高度也乘根缩放 —— 所以只要改根缩放，四者天然等比跟随，不需要额外补偏移。
    /// 塔1 只在跳档时重建骨架（容差半档），这里同样只在跨过一小档时动手，
    /// 避免每点生命上限都触发一次碰撞箱 / 血条重算。
    /// </remarks>
    private void SyncBodyScale()
    {
        NCreature? node = Creature.GetCreatureNode();
        if (node == null)
            return;

        float target = GrowthScale;
        if (Math.Abs(target - _appliedGrowth) < GrowthRate / 2f)
            return;

        _appliedGrowth = target;
        // hue 传 0：不改材质，只改缩放
        node.SetScaleAndHue(target, 0f);
    }

    /// <summary>
    /// 塔1 的 ROT 骨架把普通与腐化两套动画放在同一个文件里，用动画名区分形态。
    /// 腐化变体的名字在塔1 里就多写了一个 i（rotihurt / rotifull_idle / rotifull_idle_toidle /
    /// rotihurt_to_full），spine 里是什么就照抄什么，不要「修正」成拼写正确的形式。
    /// 每个动作播完接哪个待机用条件分支在播放时判断，复刻塔1 每次播放前 rotForm() 的写法。
    /// </summary>
    protected override CreatureAnimator? SetupCustomCreatureAnimator(MegaSprite controller)
    {
        // 普通形态
        AnimState idleNormal = new("idle_loop", isLooping: true);
        AnimState attackNormal = new("attack");
        AnimState hurtNormal = new("hurt");
        AnimState fallNormal = new("hurt_to_full");
        AnimState fullIdleNormal = new("full_idle", isLooping: true);
        AnimState getUpNormal = new("full_to_idle");

        // 腐化形态
        AnimState idleRot = new("rotidle_loop", isLooping: true);
        AnimState attackRot = new("rotattack");
        AnimState hurtRot = new("rotihurt");
        AnimState fallRot = new("rotihurt_to_full");
        AnimState fullIdleRot = new("rotifull_idle", isLooping: true);
        AnimState getUpRot = new("rotifull_idle_toidle");
        AnimState transform = new("nor_to_rot");

        // 攻击 / 受击播完，回到「当时的形态」的待机（塔1 MushroomsetAtkAnimation / setHurtAnimation）
        attackNormal.AddNextState(idleNormal, () => !IsRotForm);
        attackNormal.AddNextState(idleRot, () => IsRotForm);
        hurtNormal.AddNextState(idleNormal, () => !IsRotForm);
        hurtNormal.AddNextState(idleRot, () => IsRotForm);
        attackRot.NextState = idleRot;
        hurtRot.NextState = idleRot;

        // 倒地：塔1 enterStunForever —— 先播 hurt_to_full，再停在 full_idle 循环上
        fallNormal.NextState = fullIdleNormal;
        fallRot.NextState = fullIdleRot;

        // 起身：塔1 playWakeUpAnimation —— full_to_idle 播完接回站立循环
        getUpNormal.NextState = idleNormal;
        getUpRot.NextState = idleRot;

        // 变身：塔1 跨过阈值时 nor_to_rot → rotidle_loop
        transform.NextState = idleRot;

        // 初始状态按生成时的形态选（同一次召唤里，骨架可能是在吞噬之后才创建的）
        CreatureAnimator creatureAnimator = new(IsRotForm ? idleRot : idleNormal, controller);

        // 触发器按形态二选一：条件先判断当前形态，再决定播哪一套；
        // 眩晕（休眠）期间引擎仍可能补发 Idle / Hit，这些分支一律挡掉，否则会从 full_idle 站回去
        creatureAnimator.AddAnyState("Idle", idleNormal, () => !IsRotForm && !IsDormantNow);
        creatureAnimator.AddAnyState("Idle", idleRot, () => IsRotForm && !IsDormantNow);
        creatureAnimator.AddAnyState("Hit", hurtNormal, () => !IsRotForm && !IsDormantNow);
        creatureAnimator.AddAnyState("Hit", hurtRot, () => IsRotForm && !IsDormantNow);
        creatureAnimator.AddAnyState("Attack", attackNormal, () => !IsRotForm && !IsDormantNow);
        creatureAnimator.AddAnyState("Attack", attackRot, () => IsRotForm && !IsDormantNow);
        creatureAnimator.AddAnyState("Dead", fallNormal, () => !IsRotForm);
        creatureAnimator.AddAnyState("Dead", fallRot, () => IsRotForm);
        creatureAnimator.AddAnyState("Revive", getUpNormal, () => !IsRotForm);
        creatureAnimator.AddAnyState("Revive", getUpRot, () => IsRotForm);
        creatureAnimator.AddAnyState(RotFormTrigger, transform, () => IsRotForm);
        // 眩晕（休眠）：永久眩晕没有倒计时，直接停在倒地循环上（塔1 enterStunForever 的 full_idle）
        creatureAnimator.AddAnyState(FriendCabBase.DormantTrigger, fullIdleNormal, () => !IsRotForm);
        creatureAnimator.AddAnyState(FriendCabBase.DormantTrigger, fullIdleRot, () => IsRotForm);

        return creatureAnimator;
    }
}

/// <summary>
/// 腐化形态的攻击意图：图标与数字沿用普通攻击（塔2 的 IntentType 没有 AOE 攻击类型），
/// 但悬停说明改成「对所有敌人造成伤害」，与实际的扫场结算一致。
/// </summary>
internal sealed class CorruptAllEnemiesAttackIntent : SingleAttackIntent
{
    public CorruptAllEnemiesAttackIntent(Func<decimal> damageCalc) : base(damageCalc)
    {
    }

    public override int GetTotalDamage(IEnumerable<Creature> targets, Creature owner) =>
        FriendIntentPreview.GetDamageInt(DamageCalc!, owner);

    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
    {
        LocString description =
            new("powers", "SLUGCAT_THE_SPIRE_II_POWER_CORRUPT_LIZARD_POWER.rotIntentDescription");
        description.Add("Damage", GetTotalDamage(targets, owner));
        return description;
    }
}
