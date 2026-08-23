using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
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
public class Watercent : ModMonsterTemplate
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 249, 240);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 258, 249);


    private int BasicBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies,
        10 + CombatState.Players.Count * 15, 10 + CombatState.Players.Count * 10);
    
    // 意图2的数值，重击伤害，根据进阶提高伤害
    private int HeavyDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 1);
    private int BasicDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);

    private int Strength => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);

    
    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: "res://Godot/Monster/watercent.tscn"
    );

    // 自动转换怪物场景，让你不需要手动挂脚本。复制即可。
    protected override NCreatureVisuals? TryCreateCreatureVisuals() =>
        RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.VisualsScenePath!);

    // 战斗开始时，在这里给自己上buff之类
    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<WatercentPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);

    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        // 意图1：连击
        var basicAttack = new MoveState(
            "BASIC_ATTACK",
            async targets =>
            {
                await DamageCmd
                    .Attack(BasicDamage)
                    .FromMonster(this)
                    .WithAttackerAnim("Attack", 0.2f) // 如果有攻击动画，可以取消注释并替换成实际动画名称和延迟
                    .WithAttackerFx(null, AttackSfx)
                    //.WithHitFx("vfx/vfx_attack_blunt")
                    .WithHitCount(2)
                    .Execute(null);
            },
            new MultiAttackIntent(BasicDamage, 2)
        );


        // 意图2：重击
        var heavyAttack = new MoveState(
            "HEAVY_ATTACK",
            async targets =>
            {
                await DamageCmd
                    .Attack(HeavyDamage)
                    .FromMonster(this)
                    .WithAttackerAnim("Attack", 0.2f) // 如果有攻击动画，可以取消注释并替换成实际动画名称和延迟
                    .WithAttackerFx(null, AttackSfx)
                    //.WithHitFx("vfx/vfx_attack_blunt")
                    .WithHitCount(5)
                    .Execute(null);
            },
            new MultiAttackIntent(HeavyDamage, 5)
            
        );
        
        var DEBUFF = new MoveState(
            "DEBUFF",
            async targets =>
            { 
                await PowerCmd.Apply<CorrodePower>(new ThrowingPlayerChoiceContext(), targets, 8, Creature, null);
                await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, Strength, Creature, null);

            },
            new DebuffIntent()
        );
        var Upgrade = new MoveState(
            "UPGRADE",
            async targets => await UpgradeMove(targets),
             new BuffIntent(),new DefendIntent()
        );
        // 或者你也可以创建RandomBranchState（随机意图分支）和ConditionalBranchState（条件意图分支）来实现更复杂的状态转换逻辑
        
        
        // 设置状态转换，意图1后接意图2，意图2后接意图1
        DEBUFF.FollowUpState = basicAttack;
        basicAttack.FollowUpState = Upgrade;
        Upgrade.FollowUpState = heavyAttack;
        heavyAttack.FollowUpState = DEBUFF;

        // 添加2个意图，并且初始意图设成 basicAttack
        return new MonsterMoveStateMachine([basicAttack, heavyAttack, DEBUFF, Upgrade], DEBUFF);
    }


    private async Task UpgradeMove(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, BasicBlock,ValueProp.Move,null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, Strength, Creature, null);
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
