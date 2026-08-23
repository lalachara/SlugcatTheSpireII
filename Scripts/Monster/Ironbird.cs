using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
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
public class Ironbird : ModMonsterTemplate
{
    // 根据进阶提高最小血量，进阶8及以上为20，否则为15
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 155, 150);

    // 根据进阶提高最大血量，进阶8及以上为30，否则为20
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 165, 160);

    // 意图1的数值，伤害和格挡，根据进阶提高伤害
    private int BasicDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

    private int baseflytimes
    {
        
        get => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2+ CombatState.Players.Count*3, 2+ CombatState.Players.Count*4);
    }
    // 意图2的数值，重击伤害，根据进阶提高伤害
    private int HeavyDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 19, 17);
    private int flytimes = 0;
    private bool _isDazed;
    public MoveState sleepmove;
    public bool IsDazed
    {
        get => this._isDazed;
        set
        {
            this.AssertMutable();
            this._isDazed = value;
        }
    }
    
    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: "res://Godot/Monster/ironbird.tscn"
    );

    // 自动转换怪物场景，让你不需要手动挂脚本。复制即可。
    protected override NCreatureVisuals? TryCreateCreatureVisuals() => RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.VisualsScenePath!);

    // 战斗开始时，在这里给自己上buff之类

    public override async Task AfterAddedToRoom()
    {
        int playerCount = CombatState.Players.Count;
        await PowerCmd.Apply<IronflyPower>(
            new ThrowingPlayerChoiceContext(), 
            Creature, 
            baseflytimes ,   // 多人模式下层数翻倍
            Creature, null);
    }
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var sleep = new MoveState(
            "SLEEP",
            SleepMove,
            new StunIntent()
        );

        var heavyAttack = new MoveState(
            "HEAVY_ATTACK",
            async targets => await DamageCmd
                .Attack(HeavyDamage)
                .FromMonster(this)
                .WithAttackerFx(null, AttackSfx)
                .WithHitCount(2)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null),
            new MultiAttackIntent(HeavyDamage, 2)
        );

        var basicAttack = new MoveState(
            "BASIC_ATTACK",
            async targets => await DamageCmd
                .Attack(BasicDamage)
                .FromMonster(this)
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_blunt")
                .WithHitCount(5)
                .Execute(null),
            new MultiAttackIntent(BasicDamage, 5)
        );
        

        var upgrade = new MoveState(
            "UPGRADE",
            Upgrademove,
            new BuffIntent()
        );

        var wake = new MoveState(
            "WAKE",
            WakeMove,
            new BuffIntent()
        );
        
        var debuff = new MoveState(
            "DEBUFF",
            DebuffMove,
            new DebuffIntent()
        );
        
        
        

        basicAttack.FollowUpState = debuff;
        heavyAttack.FollowUpState = upgrade;
        debuff.FollowUpState = heavyAttack;
        upgrade.FollowUpState =  basicAttack;
        
        sleep.FollowUpState = wake;
        wake.FollowUpState = basicAttack;
   
        
        sleepmove = sleep;
        return new MonsterMoveStateMachine([basicAttack, heavyAttack, wake, sleep,debuff,upgrade], basicAttack);
    }
    private async Task SleepMove(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, 5+15*CombatState.Players.Count, ValueProp.Unpowered, null);
        await CreatureCmd.TriggerAnim(Creature, "sleeptoidle", 0);

    }



    private async Task Upgrademove(IReadOnlyList<Creature> targets)
    {
        if (Creature.HasPower<IronflyPower>())
        {
            
            flytimes++;
            int playerCount = CombatState.Players.Count;
            Creature.GetPower<IronflyPower>().basenum = flytimes*(playerCount>2?2:1)+baseflytimes;
            
        }
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(),Creature,3,Creature,null);
    }
    private async Task DebuffMove(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(),targets,2,Creature,null);
        await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(),targets,2,Creature,null);


    }
    public async Task WakeMove(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.TriggerAnim(Creature, "Idle", 0.6f);

        flytimes++;
        _isDazed = false;
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(),Creature,3,Creature,null);
        int playerCount = CombatState.Players.Count;
        await PowerCmd.Apply<IronflyPower>(new ThrowingPlayerChoiceContext(),Creature,flytimes*(playerCount>2?2:1)+baseflytimes,Creature,null);
    }
    
    
    protected override CreatureAnimator? SetupCustomCreatureAnimator(MegaSprite controller)
    {
        // 定义各动画状态及是否循环
        AnimState idle = new("idle_loop", isLooping: true);
        AnimState attack = new("attack");
        AnimState hurt = new("hurt");
        
        AnimState sleep = new("fall_loop", isLooping: true);  // 新增 sleep，循环播放
        AnimState fallidle = new("fallidle_loop", isLooping: true);  // 新增 sleep，循环播放

        AnimState falltoidle = new("fall-to-idel");

        // 设置播放完后自动回到 idle
        attack.NextState = idle;
        hurt.NextState = idle;
        falltoidle.NextState = fallidle;
        
        // 创建 CreatureAnimator，idle 为默认状态
        CreatureAnimator creatureAnimator = new(idle, controller);

        // 绑定状态名到动画
        creatureAnimator.AddAnyState("Idle", idle);
        creatureAnimator.AddAnyState("Hit", hurt,() => !IsDazed);
        creatureAnimator.AddAnyState("Attack", attack,() => !IsDazed);
        creatureAnimator.AddAnyState("Sleep", sleep);  // 注册 sleep 状态
        creatureAnimator.AddAnyState("sleeptoidle", falltoidle);  // 注册 sleeptoidle 状态

        return creatureAnimator;
    }

}
