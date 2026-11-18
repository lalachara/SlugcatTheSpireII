using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
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
public class Greencab : ModMonsterTemplate
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 130, 120);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 140, 130);


    // 意图1的数值，伤害和格挡，根据进阶提高伤害
    private int BasicDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 7);

    private int BasicBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies,
        CombatState.Players.Count * 20,  CombatState.Players.Count * 15);
    
    private int HeavyDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 22, 19);

    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: "res://Godot/Monster/greencab.tscn"
    );

    // 自动转换怪物场景，让你不需要手动挂脚本。复制即可。
    protected override NCreatureVisuals? TryCreateCreatureVisuals() =>
        RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.VisualsScenePath!);

    // 战斗开始时，在这里给自己上buff之类
    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<SlowPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);

    }

    public override async Task BeforeCombatStart()
    {
        await CreatureCmd.GainBlock(Creature, BasicBlock, ValueProp.Unpowered, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {

        var basicAttack = new MoveState(
            "BASIC_ATTACK",
            async targets =>
            {
                await DamageCmd
                    .Attack(BasicDamage)
                    .FromMonster(this)
                    .WithAttackerAnim("Attack", 0.2f) // 如果有攻击动画，可以取消注释并替换成实际动画名称和延迟
                    .WithAttackerFx(null, AttackSfx)
                    .WithHitFx("vfx/vfx_bite")
                    .Execute(null);
            },
            new SingleAttackIntent(BasicDamage)
        );


        // 意图2：重击
        var heavyAttack = new MoveState(
            "HEAVY_ATTACK",
            async targets => await HeavyAttackMove(targets),
            new SingleAttackIntent(HeavyDamage)
        );

        var run = new MoveState(
            "RUN",
            async targets => await CreatureCmd.Escape(Creature),
            new EscapeIntent()
        );
        var Sleep = new MoveState(
            "SLEEP",
            async targets => await SleepMove(targets),
            new UnknownIntent()
        );

        var Block = new MoveState(
            "BLOCK",
            async targets => await BlockMove(targets),
            new DefendIntent(), new BuffIntent()
        );
        // 或者你也可以创建RandomBranchState（随机意图分支）和ConditionalBranchState（条件意图分支）来实现更复杂的状态转换逻辑

        var afterbasicAttack = new ConditionalBranchState("afterbasicAttack");
        afterbasicAttack.AddState((MonsterState) run, (Func<bool>) (() => Creature.CurrentHp<Creature.MaxHp*0.15));
        afterbasicAttack.AddState((MonsterState) Block, (Func<bool>) (() => true));
        var aftersleep = new ConditionalBranchState("aftersleep");
        aftersleep.AddState((MonsterState) run, (Func<bool>) (() => Creature.CurrentHp<Creature.MaxHp*0.15));
        aftersleep.AddState((MonsterState) heavyAttack, (Func<bool>) (() => true));
        var afterBlock = new ConditionalBranchState("afterBlock");
        afterBlock.AddState((MonsterState) run, (Func<bool>) (() => Creature.CurrentHp<Creature.MaxHp*0.15));
        afterBlock.AddState((MonsterState) Sleep, (Func<bool>) (() => true));
        var afterheavyAttack = new ConditionalBranchState("afterheavyAttack");
        afterheavyAttack.AddState((MonsterState) run, (Func<bool>) ( () => Creature.CurrentHp<Creature.MaxHp*0.15));
        afterheavyAttack.AddState((MonsterState) basicAttack, (Func<bool>) (() => true));
        
        // 设置状态转换，意图1后接意图2，意图2后接意图1
        basicAttack.FollowUpState = afterbasicAttack;
        Sleep.FollowUpState = aftersleep;
        Block.FollowUpState = afterBlock;
        heavyAttack.FollowUpState = afterheavyAttack;

        // 添加2个意图，并且初始意图设成 basicAttack
        return new MonsterMoveStateMachine([basicAttack, heavyAttack, Block, Sleep, run,afterheavyAttack,afterbasicAttack,aftersleep,afterBlock], basicAttack);
    }
    private Task SleepMove(IReadOnlyList<Creature> targets) => Task.CompletedTask;

    private async Task BlockMove(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, BasicBlock/2, ValueProp.Unpowered, null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 5, Creature, null);
    }

    private async Task HeavyAttackMove(IReadOnlyList<Creature> targets)
    {
        
        await DamageCmd
            .Attack(HeavyDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.5f) // 如果有攻击动画，可以取消注释并替换成实际动画名称和延迟
            .WithAttackerFx(null, AttackSfx) // 攻击音效
            .WithHitFx("vfx/vfx_bite")
            .Execute(null);
        await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, 2, Creature, null);
    }
    
    protected override CreatureAnimator? SetupCustomCreatureAnimator(MegaSprite controller)
    {
        // 定义各动画状态及是否循环
        AnimState idle = new("idle_loop", isLooping: true);
        AnimState attack = new("attack");
        AnimState hurt = new("hurt");


        // 设置播放完后自动回到 idle
        attack.NextState = idle;
        hurt.NextState = idle;
        
        // 创建 CreatureAnimator，idle 为默认状态
        CreatureAnimator creatureAnimator = new(idle, controller);

        // 绑定状态名到动画
        creatureAnimator.AddAnyState("Idle", idle);
        creatureAnimator.AddAnyState("Hit", hurt);
        creatureAnimator.AddAnyState("Attack", attack);

        return creatureAnimator;
    }
}
