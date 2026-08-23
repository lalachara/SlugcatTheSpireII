using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Event;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Godot;

namespace Rainworld.Scripts.Monster;

// 创建一个简单的怪物，意图1和意图2循环
// 意图1：造成伤害，获得格挡
// 意图2：重击
[RegisterMonster]
public class Threechicken : ModMonsterTemplate
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 245, 235);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 255, 245);

    private static readonly Func<CardModel, bool>[] _stealPriorities = new Func<CardModel, bool>[4]
    {
        (Func<CardModel, bool>) (c => !(c.Enchantment is Imbued) && c.Rarity == CardRarity.Uncommon),
        (Func<CardModel, bool>) (c =>
        {
            bool flag1 = !(c.Enchantment is Imbued);
            if (flag1)
            {
                bool flag2;
                switch (c.Rarity)
                {
                    case CardRarity.Common:
                    case CardRarity.Rare:
                    case CardRarity.Event:
                        flag2 = true;
                        break;
                    default:
                        flag2 = false;
                        break;
                }
                flag1 = flag2;
            }
            return flag1;
        }),
        (Func<CardModel, bool>) (c =>
        {
            bool flag3 = !(c.Enchantment is Imbued);
            if (flag3)
            {
                bool flag4;
                switch (c.Rarity)
                {
                    case CardRarity.Basic:
                    case CardRarity.Quest:
                        flag4 = true;
                        break;
                    default:
                        flag4 = false;
                        break;
                }
                flag3 = flag4;
            }
            return flag3;
        }),
        (Func<CardModel, bool>) (c => c.Rarity == CardRarity.Ancient || c.Enchantment is Imbued)
    };

    // 意图2的数值，重击伤害，根据进阶提高伤害
    private int BasicDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);


    
    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: "res://Godot/Monster/threechicken.tscn"
    );

    // 自动转换怪物场景，让你不需要手动挂脚本。复制即可。
    protected override NCreatureVisuals? TryCreateCreatureVisuals() =>
        RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.VisualsScenePath!);

    // 战斗开始时，在这里给自己上buff之类
    public override async Task AfterAddedToRoom()
    {

    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        // 意图1：连击
        var basicAttack = new MoveState(
            "ATTACK",
            async targets =>
            {
                await DamageCmd
                    .Attack(BasicDamage)
                    .FromMonster(this)
                    .WithAttackerAnim("Attack", 0.2f) // 如果有攻击动画，可以取消注释并替换成实际动画名称和延迟
                    .WithAttackerFx(null, AttackSfx)
                    //.WithHitFx("vfx/vfx_attack_blunt")
                    .WithHitCount(WatcherEvent.times)
                    .Execute(null);
            },
            new MultiAttackIntent(BasicDamage, WatcherEvent.times)
        );

        
        
        var DEBUFF = new MoveState(
            "GET",
            async targets =>
            {
                foreach (var target in targets)
                {if (!target.IsDead)
                    {
                        
                    await CreatureCmd.LoseMaxHp(new BlockingPlayerChoiceContext(),target,5,false);
                    await CreatureCmd.LoseMaxHp(new BlockingPlayerChoiceContext(),target,5,false);
                    
                    List<CardModel> list = CardPile.GetCards(target.Player ?? target.PetOwner, PileType.Draw, PileType.Discard).Where<CardModel>((Func<CardModel, bool>) (c => c.DeckVersion != null)).ToList<CardModel>();
                    IEnumerable<CardModel> cardModels = (IEnumerable<CardModel>) list;
                    foreach (Func<CardModel, bool> stealPriority in _stealPriorities)
                    {
                        IEnumerable<CardModel> source = list.Where<CardModel>(stealPriority);
                        if (source.Any<CardModel>())
                        {
                            cardModels = source;
                            break;
                        }
                    }
                    if (cardModels.Any<CardModel>())
                    {
                        CardModel cardToSteal = RunRng.CombatCardGeneration.NextItem<CardModel>(cardModels);
                        await CardPileCmd.RemoveFromCombat(cardToSteal);

                    }
                    }
                    
                }

            },
            new UnknownIntent()
        );
        var Upgrade = new MoveState(
            "WATCH",
            async targets => await UpgradeMove(targets),
             new BuffIntent()
        );
        // 或者你也可以创建RandomBranchState（随机意图分支）和ConditionalBranchState（条件意图分支）来实现更复杂的状态转换逻辑
        
        
        // 设置状态转换，意图1后接意图2，意图2后接意图1
        Upgrade.FollowUpState = basicAttack;

        basicAttack.FollowUpState = DEBUFF;
        DEBUFF.FollowUpState = basicAttack;


        // 添加2个意图，并且初始意图设成 basicAttack
        return new MonsterMoveStateMachine([basicAttack, DEBUFF, Upgrade], Upgrade);
    }


    private async Task UpgradeMove(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<CatchPower>(new ThrowingPlayerChoiceContext(), Creature, WatcherEvent.times, Creature, null);
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
