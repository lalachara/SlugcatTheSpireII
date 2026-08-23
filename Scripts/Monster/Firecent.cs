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
using Rainworld.Scripts.Card.Colorless;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Godot;

namespace Rainworld.Scripts.Monster;

// 创建一个简单的怪物，意图1和意图2循环
// 意图1：造成伤害，获得格挡
// 意图2：重击
[RegisterMonster]
public class Firecent : ModMonsterTemplate
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 155, 145);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 160, 150);

    private static readonly string[] _skinOptions = new string[5]
    {
        "all",
        "hit1",
        "hit2",
        "hit3",
        "hit4"
    };
// 在 _skinOptions 数组后面加：
    private MegaSkeleton? _skeleton;

// 在 SetupSkins 里第一行加 _skeleton = skeleton;
    public override void SetupSkins(MegaSprite spine, MegaSkeleton skeleton)
    {
        _skeleton = skeleton;  // <-- 新增
        MegaSkin skin = spine.NewSkin("custom-skin");
        MegaSkeletonDataResource data = skeleton.GetData();
        skin.AddSkin(data.FindSkin(_skinOptions[0]));
        skeleton.SetSkin(skin);
        skeleton.SetSlotsToSetupPose();
    }

// 在 SetupSkins 方法后面加：
    public void SetSkin(string skinName)
    {
        _skeleton?.SetSkinByName(skinName);
        _skeleton?.SetSlotsToSetupPose();
    }
    // 意图1的数值，伤害和格挡，根据进阶提高伤害

    private int BasicBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies,
        15 + CombatState.Players.Count * 25, 10 + CombatState.Players.Count * 20);

    public int blocks = 4;

    // 意图2的数值，重击伤害，根据进阶提高伤害
    private int HeavyDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 21, 16);
    private int BasicDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 5);

    private int Cards => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 4);

    
    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: "res://Godot/Monster/Firecent.tscn"
    );

    // 自动转换怪物场景，让你不需要手动挂脚本。复制即可。
    protected override NCreatureVisuals? TryCreateCreatureVisuals() =>
        RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.VisualsScenePath!);

    // 战斗开始时，在这里给自己上buff之类
    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<FirecentPower>(new ThrowingPlayerChoiceContext(), Creature, BasicBlock, Creature, null);
        //await CreatureCmd.GainBlock(Creature, BasicBlock, ValueProp.Unpowered, null);

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
                    .WithAttackerAnim("Attack2", 0.2f) // 如果有攻击动画，可以取消注释并替换成实际动画名称和延迟
                    .WithAttackerFx(null, AttackSfx)
                    .WithHitFx("vfx/vfx_attack_blunt")
                    .WithHitCount(2)
                    .Execute(null);
            },
            new MultiAttackIntent(BasicDamage, 2)
        );


        // 意图2：重击
        var heavyAttack = new MoveState(
            "HEAVY_ATTACK",
            async targets => await HeavyAttackMove(targets),
            new SingleAttackIntent(HeavyDamage), new DebuffIntent()
        );


        var Dazed = new MoveState(
            "DAZED",
            async targets =>
            { 
                await CardPileCmd.AddToCombatAndPreview<Rainworld_Elec1>(targets, PileType.Draw, 1, null);
                await CardPileCmd.AddToCombatAndPreview<Rainworld_Elec2>(targets, PileType.Discard, 1, null);
            },
            new StatusIntent(2)
        );
        var Upgrade = new MoveState(
            "UPGRADE",
            async targets => await UpgradeMove(targets),
             new BuffIntent()
        );
        // 或者你也可以创建RandomBranchState（随机意图分支）和ConditionalBranchState（条件意图分支）来实现更复杂的状态转换逻辑
        
        
        // 设置状态转换，意图1后接意图2，意图2后接意图1
        basicAttack.FollowUpState = Dazed;
        Dazed.FollowUpState = heavyAttack;
        heavyAttack.FollowUpState = Upgrade;
        Upgrade.FollowUpState = basicAttack;

        // 添加2个意图，并且初始意图设成 basicAttack
        return new MonsterMoveStateMachine([basicAttack, heavyAttack, Dazed, Upgrade], basicAttack);
    }


    private async Task UpgradeMove(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, Cards, Creature, null);
    }

    private async Task HeavyAttackMove(IReadOnlyList<Creature> targets)
    {
        //TalkCmd.Play(L10NMonsterLookup("TEST_MONSTER_TEST_MONSTER.moves.BASIC_ATTACK.banter"), Creature, VfxColor.Blue);
        await DamageCmd
            .Attack(HeavyDamage)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.5f) // 如果有攻击动画，可以取消注释并替换成实际动画名称和延迟
            .WithAttackerFx(null, AttackSfx) // 攻击音效
            .WithHitFx("vfx/vfx_attack_blunt") // 攻击特效
            .Execute(null);
        await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), targets, Cards/2, Creature, null);
        await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, Cards/2, Creature, null);
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
