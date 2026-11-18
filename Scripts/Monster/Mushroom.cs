using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Godot;

namespace Rainworld.Scripts.Monster;

// 创建一个简单的怪物，意图1和意图2循环
// 意图1：造成伤害，获得格挡
// 意图2：重击
[RegisterMonster]
public class Mushroom : ModMonsterTemplate
{
    // 根据进阶提高最小血量，进阶8及以上为20，否则为15
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 666, 630);

    // 根据进阶提高最大血量，进阶8及以上为30，否则为20
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 666, 630);

    // 意图1的数值，伤害和格挡，根据进阶提高伤害
    private int BasicDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 7);
    private int DazedCount
    {
        get => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    }
    private int HealCount
    {
        get => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 8);
    }
    // 意图2的数值，重击伤害，根据进阶提高伤害
    private int HeavyDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 31, 25);
    private bool _isAwake;
    private bool _healthed = false;

    public bool IsAwake
    {
        get => this._isAwake;
        set
        {
            this.AssertMutable();
            this._isAwake = value;
        }
    }
    public bool needHealth
    {
        get => !this._healthed&&Creature.CurrentHp<=Creature.MaxHp*0.5;
        set
        {
            this.AssertMutable();
            this._healthed = value;
        }
    }
    
    public MoveState WakeState { get; private set; }

    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: "res://Godot/Monster/mushroom.tscn"
    );

    // 自动转换怪物场景，让你不需要手动挂脚本。复制即可。
    protected override NCreatureVisuals? TryCreateCreatureVisuals() => RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.VisualsScenePath!);

    // 战斗开始时，在这里给自己上buff之类
    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<MushroomstartPower>(new ThrowingPlayerChoiceContext(), Creature, 2, Creature, null);
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var sleep = new MoveState(
            "SLEEP",
            SleepMove,
            new SleepIntent()
        );

        var heavyAttack = new MoveState(
            "HEAVY_ATTACK",
            async targets => await DamageCmd
                .Attack(HeavyDamage)
                .FromMonster(this)
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null),
            new SingleAttackIntent(HeavyDamage)
        );

        var basicAttack = new MoveState(
            "BASIC_ATTACK",
            async targets => await DamageCmd
                .Attack(BasicDamage)
                .FromMonster(this)
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_blunt")
                .WithHitCount(2)
                .Execute(null),
            new MultiAttackIntent(BasicDamage, 2)
        );

        var daze = new MoveState(
            "Mushroom_xuanyun",
            DazedMove,
            new StatusIntent(DazedCount*2),new BuffIntent()
        );

        var health = new MoveState(
            "Mushroom_health",
            HealthMove,
            new HealIntent()
        );

        var wake = new MoveState(
            "Mushroom_wake",
            WakeMove,
            new BuffIntent()
        );

        var afterBasic = new ConditionalBranchState("AfterBasic");
        afterBasic.AddState((MonsterState) heavyAttack, (Func<bool>) (() => !needHealth));
        afterBasic.AddState((MonsterState) health, (Func<bool>) (() => needHealth));

        var afterHeavy = new ConditionalBranchState("AfterHeavy");
        afterHeavy.AddState((MonsterState) daze, (Func<bool>) (() => !needHealth));
        afterHeavy.AddState((MonsterState) health, (Func<bool>) (() => needHealth));

        var afterDaze = new ConditionalBranchState("AfterDaze");
        afterDaze.AddState((MonsterState) basicAttack, (Func<bool>) (() => !needHealth));
        afterDaze.AddState((MonsterState) health, (Func<bool>) (() => needHealth));

        var aftersleep = new ConditionalBranchState("AfterSleep");
        aftersleep.AddState((MonsterState) sleep, (Func<bool>) (() => !_isAwake));
        aftersleep.AddState((MonsterState) wake, (Func<bool>) (() => _isAwake));
        
        basicAttack.FollowUpState = (MonsterState) afterBasic;
        heavyAttack.FollowUpState = (MonsterState) afterHeavy;
        daze.FollowUpState = (MonsterState) afterDaze;
        health.FollowUpState = (MonsterState) basicAttack;
        wake.FollowUpState = (MonsterState) afterDaze;
        sleep.FollowUpState = (MonsterState) aftersleep;
        
        WakeState = wake;

        
// 当前：只注册了 MoveState，没有注册 ConditionalBranchState
// 修复：把三个 ConditionalBranchState 也加进去
        return new MonsterMoveStateMachine([basicAttack, heavyAttack, daze, wake, health, sleep, aftersleep, afterBasic, afterHeavy, afterDaze], sleep);
    }
    private Task SleepMove(IReadOnlyList<Creature> targets) => Task.CompletedTask;
    private async Task DazedMove(IReadOnlyList<Creature> targets)
    {
        //await CreatureCmd.TriggerAnim(Creature, "Vomit", 0.7f);
        await CardPileCmd.AddToCombatAndPreview<Dazed>( targets, PileType.Discard, DazedCount,  null);
        await CardPileCmd.AddToCombatAndPreview<Dazed>(targets, PileType.Draw, DazedCount,  null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 6+2*DazedCount, Creature, null);

    }
    private async Task HealthMove(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.Heal(Creature,(int)(Creature.MaxHp));
        needHealth = true;
    }
    public async Task WakeMove(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<MushroomPower>(new ThrowingPlayerChoiceContext(),Creature,HealCount,Creature,null);
    }
    
    protected override CreatureAnimator? SetupCustomCreatureAnimator(MegaSprite controller)
    {
        // 定义各动画状态及是否循环
        AnimState idle = new("idle_loop", isLooping: true);
        AnimState attack = new("attack");
        AnimState hurt = new("hurt");
        AnimState sleep = new("sleep", isLooping: true);  // 新增 sleep，循环播放
        AnimState sleeptoidle = new("sleeptoidle");

        // 设置播放完后自动回到 idle
        attack.NextState = idle;
        hurt.NextState = idle;
        sleeptoidle.NextState = idle;
        
        // 创建 CreatureAnimator，idle 为默认状态
        CreatureAnimator creatureAnimator = new(sleep, controller);

        // 绑定状态名到动画
        creatureAnimator.AddAnyState("Idle", idle);
        creatureAnimator.AddAnyState("Hit", hurt,() => _isAwake);
        creatureAnimator.AddAnyState("Attack", attack,() => _isAwake);
        creatureAnimator.AddAnyState("Sleep", sleep);  // 注册 sleep 状态
        creatureAnimator.AddAnyState("sleeptoidle", sleeptoidle);  // 注册 sleeptoidle 状态

        return creatureAnimator;
    }

}
