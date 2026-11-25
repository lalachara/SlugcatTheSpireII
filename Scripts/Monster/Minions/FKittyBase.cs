using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Godot;

namespace Rainworld.Scripts.Monster.Minions;

/// <summary>
/// 猫崽伙伴基类（对应塔1 Kitty_*）：HP4，三态轮换行动——攻击 / 防御（给玩家格挡或临时生命）/
/// 增益（给玩家能力），对应塔1 的 actIndex % 3 轮换。视觉统一用猫崽专用骨架 kittys.tscn（按皮肤区分颜色）。
/// 猫崽不替玩家挡伤、也不休眠复活（塔1：只有蜥蜴类伙伴挡伤，猫崽被打死直接离场）。
/// </summary>
public abstract class FKittyBase : FriendPetBase
{
    public override int MinInitialHp => 4;
    public override int MaxInitialHp => 4;

    // ———— 各猫崽数值（塔1 基础值；实际生效值 = 基础值 + 玩家「协调」层数）————

    /// <summary>视觉场景路径：四只猫崽统一使用猫崽专用骨架 kittys.tscn（塔1 setkittyskin 的同一套 slugpup）。</summary>
    protected abstract string VisualsScenePath { get; }

    /// <summary>
    /// 猫崽骨架皮肤：slugpup 提供 white / red / blue / yellow 四套，对应塔1 的 setkittyskin。
    /// 主角角色场景各自只有一套贴图，不能直接套用到猫崽骨架上。
    /// </summary>
    protected abstract string KittySkin { get; }

    /// <summary>攻击基础伤害（实际伤害另乘进化倍率）。</summary>
    protected abstract int AttackDamage { get; }

    /// <summary>防御态给玩家的数值基础值（格挡或临时生命）。</summary>
    protected abstract int DefendAmount { get; }

    /// <summary>增益态给玩家的能力基础层数。</summary>
    protected abstract int BuffAmount { get; }

    /// <summary>攻击是否附带减益（白猫创伤 / 蓝猫侵蚀的攻击回合会显示减益意图）。</summary>
    protected virtual bool AttackHasDebuff => false;

    // ———— 护甲面板数据（塔1 Kitty_* 只统计当前意图；猫崽不替玩家挡伤，protectCapacity 保持 0）————

    /// <summary>攻击态（三轮中的第一轮）的预计伤害，其余两轮不攻击为 0。</summary>
    public override decimal IntentOutput =>
        Creature.IsAlive && _actIndex % 3 == 0 ? ComputeAttackDamage(AttackDamage) : 0m;

    /// <summary>
    /// 防御态的护甲：白猫 / 蓝猫给格挡，红猫给临时生命，两者都算护甲（塔1 intentArmor = blockBase + getTune()）。
    /// </summary>
    public override decimal IntentArmor =>
        Creature.IsAlive && _actIndex % 3 == 1 ? DefendAmount + TuneAmount() : 0m;

    // ———— 视觉：四只猫崽共用猫崽专用骨架 kittys.tscn，只是皮肤不同；经工厂转成 NCreatureVisuals ————

    public override string? CustomVisualsPath => VisualsScenePath;

    protected override NCreatureVisuals? TryCreateCreatureVisuals() =>
        RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(VisualsScenePath);

    /// <summary>按 KittySkin 切换猫崽骨架皮肤（白 / 红 / 蓝 / 黄），再把槽位摆回 setup pose。</summary>
    public override void SetupSkins(MegaSprite spine, MegaSkeleton skeleton)
    {
        skeleton.SetSkinByName(KittySkin);
        skeleton.SetSlotsToSetupPose();
    }

    /// <summary>
    /// 猫崽动作名在塔1 里是写死的 idle_loop / attack（各猫崽 takeTurn 直接 setAnimation）。
    /// 受击只在骨架确实有 hurt 时才接：猫崽骨架 slugpup 只有 attack 与 idle_loop，
    /// 塔1 setHurtAnimation 也是先 findAnimation("hurt_…") 判空后直接跳过；
    /// 这里若不判断就注册，CreatureAnimator 会把 _currentState 指向不存在的动画并连续打警告。
    /// </summary>
    protected override CreatureAnimator? SetupCustomCreatureAnimator(MegaSprite controller)
    {
        AnimState idle = new("idle_loop", isLooping: true);
        AnimState attack = new("attack");
        attack.NextState = idle;

        CreatureAnimator creatureAnimator = new(idle, controller);
        creatureAnimator.AddAnyState("Idle", idle);
        creatureAnimator.AddAnyState("Attack", attack);

        if (controller.HasAnimation("hurt"))
        {
            AnimState hurt = new("hurt");
            hurt.NextState = idle;
            creatureAnimator.AddAnyState("Hit", hurt);
        }

        // 猫崽没有倒地 / 复活动画（塔1 被打死直接离场）：Dead 只停在待机，随后由引擎淡出
        creatureAnimator.AddAnyState("Dead", idle);
        return creatureAnimator;
    }

    // ———— 意图：攻击 / 防御 / 增益三态轮换 ————

    private int _actIndex;
    private MoveState? _attackState;
    private MoveState? _defendState;
    private MoveState? _buffState;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attackState = new("FRIEND_KITTY_ATTACK", _ => Task.CompletedTask, CreateAttackIntents());
        attackState.FollowUpState = attackState;

        MoveState defendState = new("FRIEND_KITTY_DEFEND", _ => Task.CompletedTask, new FriendDefendIntent());
        defendState.FollowUpState = defendState;

        MoveState buffState = new("FRIEND_KITTY_BUFF", _ => Task.CompletedTask, new FriendBuffIntent());
        buffState.FollowUpState = buffState;

        _attackState = attackState;
        _defendState = defendState;
        _buffState = buffState;

        return new MonsterMoveStateMachine([attackState, defendState, buffState], attackState);
    }

    private AbstractIntent[] CreateAttackIntents()
    {
        AbstractIntent attack = new FriendSingleAttackIntent(() => ComputeAttackDamage(AttackDamage));
        return AttackHasDebuff ? [attack, new FriendDebuffIntent()] : [attack];
    }

    /// <summary>按轮换计数切换意图（攻击 → 防御 → 增益，循环）。</summary>
    private void ApplyIntent()
    {
        MoveState? state = (_actIndex % 3) switch
        {
            0 => _attackState,
            1 => _defendState,
            _ => _buffState
        };
        if (state == null)
            return;
        // 宠物召唤链路不一定经过 CombatManager.AddCreature，战备状态机可能尚未生成
        if (MoveStateMachine == null)
            SetUpForCombat();
        SetMoveImmediate(state, true);
    }

    protected override Task OnSummoned(PlayerChoiceContext choiceContext, Player owner, FriendSummonOptions options)
    {
        ApplyIntent();
        return Task.CompletedTask;
    }

    // ———— 行动：三态轮换 ————

    public override async Task PerformFriendTurn(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> targets)
    {
        switch (_actIndex % 3)
        {
            case 0:
                await PerformKittyAttack(choiceContext, targets);
                break;
            case 1:
                await PerformKittyDefend(choiceContext);
                break;
            default:
                await PerformKittyBuff(choiceContext);
                break;
        }

        _actIndex++;
        ApplyIntent();
        RefreshFriendIntents();
    }

    /// <summary>攻击态：冲撞一个目标，命中后追加各猫崽的特效（白猫创伤 / 蓝猫侵蚀）。</summary>
    protected virtual async Task PerformKittyAttack(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> targets)
    {
        Creature? target = targets.FirstOrDefault(t => t.IsAlive);
        if (target == null)
            return;

        await BumpAttackAsync(choiceContext, target, ComputeAttackDamage(AttackDamage));
        await AfterKittyAttack(choiceContext, target);
    }

    /// <summary>攻击命中后的追加效果（默认无）。</summary>
    protected virtual Task AfterKittyAttack(PlayerChoiceContext choiceContext, Creature target) => Task.CompletedTask;

    /// <summary>防御态：默认给玩家格挡（基础值 + 协调层数）；红猫覆写为临时生命。</summary>
    protected virtual async Task PerformKittyDefend(PlayerChoiceContext choiceContext)
    {
        Creature? playerCreature = Creature.PetOwner?.Creature;
        if (playerCreature == null)
            return;
        await CreatureCmd.GainBlock(playerCreature, DefendAmount + TuneAmount(), ValueProp.Move, null);
    }

    /// <summary>增益态：为玩家（及可能的自身）施加对应能力。</summary>
    protected abstract Task PerformKittyBuff(PlayerChoiceContext choiceContext);
}
