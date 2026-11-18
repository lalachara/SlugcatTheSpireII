using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Rainworld.Scripts.Card.Monk;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Godot;

namespace Rainworld.Scripts.Monster.Minions;

/// <summary>
/// 伙伴：拾荒者（对应塔1 Monkey）。HP10，不攻击：
/// 每回合结束时若玩家金币 ≥ 10 递一张「拾荒者的援助」（6 种效果随机、花费 10~20 且封顶到当前金币）；
/// 金币不足则塞一张「羞耻」（原版诅咒）后离开战场。
/// 骨架只有 idle_loop 与 6 段援助动作（没有 hurt / die），退场不走死亡流程，改用 <see cref="FriendPositions.LoseFriend" />。
/// </summary>
[RegisterMonster]
public sealed class FMonkey : FriendPetBase
{
    /// <summary>专属场景路径：拾荒者骨架 ownScavengerSP（皮肤 01）。</summary>
    public const string MonkeyVisualsPath = "res://Godot/Monster/firendmonkey.tscn";

    private const string FallbackVisualsPath = "res://Godot/Monster/greencab.tscn";

    /// <summary>拾荒者骨架只有 "01" / "02" 两套皮肤，塔1 用 "01"。</summary>
    private const string MonkeySkin = "01";

    /// <summary>
    /// 援助牌 6 种效果各自的动作，下标与 <see cref="Rainworld_Monk_Order" /> 的 EffectXxx 一一对应
    /// （塔1 Monk_MonkeyOrder.use 里的 playOwnerAnim：attack1 / attack2 / gedang / choupai / huifei / xietiao）。
    /// </summary>
    public static readonly string[] OrderAnimations = ["attack1", "attack2", "gedang", "choupai", "huifei", "xietiao"];

    public override int MinInitialHp => 8;
    public override int MaxInitialHp => 8;

    /// <summary>拾荒者不攻击：没有敌人时也应行动（递卡或离场）。</summary>
    public override bool RequiresEnemyTarget => false;

    // ———— 视觉 ————

    public override string? CustomVisualsPath => MonkeyVisualsPath;

    /// <summary>专属场景缺失时退回绿蜥蜴骨架（占位方案，场景就位后自动切换）。</summary>
    protected override NCreatureVisuals? TryCreateCreatureVisuals()
    {
        string path = ResourceLoader.Exists(MonkeyVisualsPath) ? MonkeyVisualsPath : FallbackVisualsPath;
        return RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(path);
    }

    /// <summary>拾荒者骨架有两套皮肤，塔1 固定 "01"。</summary>
    public override void SetupSkins(MegaSprite spine, MegaSkeleton skeleton)
    {
        skeleton.SetSkinByName(MonkeySkin);
        skeleton.SetSlotsToSetupPose();
    }

    /// <summary>
    /// 拾荒者骨架只有 idle_loop 与 6 段援助动作，没有 hurt / die（塔1 leaveBattle 也因此绕开 die/escape）。
    /// 6 段动作各自注册成同名触发器，由援助牌打出时按效果触发；每段播完都回 idle_loop。
    /// </summary>
    protected override CreatureAnimator? SetupCustomCreatureAnimator(MegaSprite controller)
    {
        AnimState idle = new("idle_loop", isLooping: true);

        CreatureAnimator creatureAnimator = new(idle, controller);
        creatureAnimator.AddAnyState("Idle", idle);

        foreach (string animation in OrderAnimations)
        {
            // 骨架缺动作时跳过：注册不存在的动画会让状态机指向空动画并连续打警告
            if (!controller.HasAnimation(animation))
                continue;
            AnimState order = new(animation);
            order.NextState = idle;
            creatureAnimator.AddAnyState(animation, order);
        }

        // 拾荒者没有倒地动画（塔1 被清空血量也是直接离场）：Dead 停在待机，随后由引擎淡出
        creatureAnimator.AddAnyState("Dead", idle);
        return creatureAnimator;
    }

    /// <summary>按援助牌的效果下标播对应动作（越界忽略）。</summary>
    public void PlayOrderAnim(int effect)
    {
        if (effect < 0 || effect >= OrderAnimations.Length)
            return;
        Creature.GetCreatureNode()?.SetAnimationTrigger(OrderAnimations[effect]);
    }

    /// <summary>场上还活着的拾荒者：援助牌优先让塞牌的那只播动作，找不到时退回场上任意一只。</summary>
    public static FMonkey? FindOnField(Player player) =>
        FriendPositions.All(player).Select(friend => friend.Monster).OfType<FMonkey>()
            .FirstOrDefault(monkey => monkey.Creature.IsAlive);

    // ———— 意图：恒为 UNKNOWN（对应塔1 setMove(Intent.UNKNOWN)）————

    private MoveState? _orderState;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState orderState = new("FRIEND_MONKEY_ORDER", _ => Task.CompletedTask, new UnknownIntent());
        orderState.FollowUpState = orderState;

        _orderState = orderState;
        return new MonsterMoveStateMachine([orderState], orderState);
    }

    protected override Task OnSummoned(PlayerChoiceContext choiceContext, Player owner, FriendSummonOptions options)
    {
        // 宠物召唤链路不一定经过 CombatManager.AddCreature，战备状态机可能尚未生成
        if (MoveStateMachine == null)
            SetUpForCombat();
        if (_orderState != null)
            SetMoveImmediate(_orderState, true);
        return Task.CompletedTask;
    }

    // ———— 行动：递「拾荒者的援助」或塞「羞耻」离场 ————

    public override async Task PerformFriendTurn(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> targets)
    {
        Player? player = Creature.PetOwner;
        if (player == null || CombatState is not { } combatState)
            return;

        // 递牌动作（塔1 takeTurn 里 choupai 被注释掉了，这里接回来让「给援助」有表现）
        Creature.GetCreatureNode()?.SetAnimationTrigger("choupai");
        await Cmd.Wait(0.4f);

        if (player.Gold >= Rainworld_Monk_Order.MinGoldCost)
        {
            await GiveOrder(combatState, player);
        }
        else
        {
            // 金币不够门槛：不给援助，塞一张「羞耻」（原版诅咒），然后拾荒者离开战场
            CardModel shame = combatState.CreateCard<Shame>(player);

            // 和援助牌同理：塞牌发生在回合结束阶段，不加单回合保留就会被随后的手牌清空直接弃掉，
            // 这张诅咒也就等于没塞。保留只作用于这一次清手牌，之后它就是普通手牌。
            CardCmd.ApplySingleTurnRetain(shame);

            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(shame, PileType.Hand, player));
            FriendPositions.LoseFriend(player, Creature);
        }
    }

    /// <summary>
    /// 生成一张本次的「拾荒者的援助」：6 种效果等概率随机；花费在 10~20 之间随机，
    /// 并封顶到玩家当前金币——随机值一旦超过手上的钱，这张牌就成了付不起的死牌（同塔1 makeOrder）。
    /// </summary>
    private async Task GiveOrder(ICombatState combatState, Player player)
    {
        var rng = player.RunState.Rng.CombatCardGeneration;
        int effect = rng.NextInt(0, Rainworld_Monk_Order.EffectCount);
        int goldCost = rng.NextInt(Rainworld_Monk_Order.MinGoldCost,
            Math.Min(Rainworld_Monk_Order.MaxGoldCost, player.Gold) + 1);

        Rainworld_Monk_Order card = combatState.CreateCard<Rainworld_Monk_Order>(player);
        card.Configure(effect, goldCost, this);

        // 拾荒者是在「玩家回合结束」阶段塞牌的（FriendCabAttackPower.BeforeSideTurnEndEarly），
        // 不让它保留的话，紧随其后的手牌清空会立刻把这张牌打进弃牌堆，玩家根本看不到。
        // 用原版的单回合保留：只在这一次清手牌时生效，清完由 EndOfTurnCleanup 自动清掉，
        // 所以下一回合它就是一张普通手牌（打出照样消耗），不会变成永久保留。
        CardCmd.ApplySingleTurnRetain(card);

        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player));
    }
}
