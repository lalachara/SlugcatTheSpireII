using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Scaffolding.Content;
// 0.113 游戏本体新增了同名 TunePower，这里显式指定本 mod 的协调能力
using TunePower = Rainworld.Scripts.Powers.TunePower;

namespace Rainworld.Scripts.Monster.Minions;

/// <summary>
/// 所有「伙伴」随从的自动行动契约：行动由 <see cref="FriendCabAttackPower" /> 在玩家回合结束时驱动。
/// </summary>
public interface IFriendPetTurn
{
    /// <summary>是否攻击所有敌人（焦糖蜥蜴为 true，其余为单体）。</summary>
    bool HitsAllEnemies { get; }

    /// <summary>行动是否需要敌方目标（拾荒者只发援助卡、不攻击，没有敌人时也应行动）。</summary>
    bool RequiresEnemyTarget { get; }

    /// <summary>执行一次伙伴行动（攻击或辅助，含动画与结算）。</summary>
    Task PerformFriendTurn(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> targets);
}

/// <summary>
/// 伙伴随从基类（对应塔1 AbstractFriend 的公共部分）：
/// 召唤流程（血量/力量/行动驱动）、「协调」与「进化」加成的伤害公式、
/// 冲撞攻击的公共实现，供蜥蜴类（<see cref="FriendCabBase" />）、猫崽与拾荒者共用。
/// <para>
/// 召唤链路已改为**纯原版**：召唤 = <c>PlayerCmd.AddPet&lt;T&gt;</c>（原版 Byrdpip / PaelsLegion / Wuppy
/// 使用的同一个入口：<c>CreateCreature</c> → 登记进 <c>PlayerCombatState.Pets</c> → <c>CreatureCmd.Add</c> 进场），
/// 不再依赖 MinionLib 的 <c>MinionModel</c> / <c>ModMinionTemplate</c>。
/// </para>
/// <para>
/// 基类改成 RitsuLib 的 <see cref="ModMonsterTemplate" />（原版 <c>MonsterModel</c> + 视觉/动画脚手架），
/// 所以各伙伴子类的 <c>CustomVisualsPath</c>、<c>TryCreateCreatureVisuals</c>、
/// <c>SetupCustomCreatureAnimator</c>、<c>SetupSkins</c> 覆写方式与显示效果完全不变。
/// </para>
/// </summary>
public abstract class FriendPetBase : ModMonsterTemplate, IFriendPetTurn
{
    public virtual bool HitsAllEnemies => false;

    /// <summary>默认需要敌方目标（攻击类伙伴没有敌人时不行动）；辅助类伙伴可覆写为 false。</summary>
    public virtual bool RequiresEnemyTarget => true;

    /// <summary>
    /// 本体常态颜色：休眠变暗后还原、以及形态切换时使用。
    /// 需要常态染色的伙伴（如腐化蜥蜴的腐化形态）覆写此属性。
    /// </summary>
    public virtual Color BaseBodyTint => Colors.White;

    /// <summary>
    /// 伙伴的站位槽：由 <see cref="FriendSummonCmd.Summon{T}" /> 在召唤时写入，
    /// 只用于显示与摆位（<see cref="FriendLayout" />），不参与战斗结算。
    /// </summary>
    public FriendPosition Position { get; internal set; } = FriendPosition.Front;

    // ———— 与原版宠物一致的死亡表现（原先由 MinionLib 的 MinionModel 提供）————
    // 不覆写会走 MonsterModel 的默认值：event:/sfx/enemy/enemy_attacks/<id>/<id>_die，
    // 本 mod 的伙伴没有这套音效资源，且塔1 的伙伴共用同一套轻量死亡音。

    /// <summary>死亡音效：沿用原版奥提斯的骨碎声（与大骷髅小型随从一致的轻量死亡音）。</summary>
    public override string DeathSfx => "event:/sfx/characters/osty/osty_die";

    /// <summary>有死亡音效。</summary>
    public override bool HasDeathSfx => true;

#if !STS2_0_113_0 && !STS2_0_114_0
    /// <summary>0.111/0.111.1 的宠物血条开关：不覆盖时友方伙伴不显示血条（0.113/0.114 已移除该属性）。</summary>
    public override bool IsHealthBarVisible => true;
#endif

    // ———— 伙伴护甲面板（CabBlockIndicator）数据，对应塔1 AbstractFriend 的三个查询 ————

    /// <summary>
    /// 用身体替主人扛伤的能力（对应塔1 protectCapacity）：只有蜥蜴类会替主人挡伤，
    /// 数值为当前生命；休眠倒计时只剩 1 回合时按满血计入（本回合结束就会复活）。其余伙伴为 0。
    /// </summary>
    public virtual decimal ProtectCapacity => 0m;

    /// <summary>当前意图额外给主人的护甲（格挡 / 临时生命，对应塔1 intentArmor），没有则为 0。</summary>
    public virtual decimal IntentArmor => 0m;

    /// <summary>当前意图预计造成的伤害（单体口径，对应塔1 intentTotalOutput），本轮不攻击则为 0。</summary>
    public virtual decimal IntentOutput => 0m;

    /// <summary>是否处于休眠（倒地等待复活）状态：护甲面板据此跳过休眠伙伴的意图护甲。</summary>
    public bool IsDormant => Creature.GetPower<FriendDormantPower>() is { IsDormant: true };

    /// <summary>执行一次伙伴行动（攻击或辅助，含动画与结算）：由各伙伴子类实现。</summary>
    public abstract Task PerformFriendTurn(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> targets);

    // ———— 意图状态机：没有覆写的伙伴默认只有一个空转的待机状态 ————

    /// <summary>
    /// 默认意图状态机：只有一段什么都不做的待机（等价 MinionLib 的 MINION_IDLE）。
    /// 各伙伴子类基本都会覆写它来提供攻击 / 防御 / 眩晕意图。
    /// </summary>
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState idle = new("FRIEND_IDLE", _ => Task.CompletedTask, Array.Empty<AbstractIntent>());
        idle.FollowUpState = idle;
        return new MonsterMoveStateMachine([idle], idle);
    }

    // ———— 召唤 ————

    /// <summary>
    /// 召唤初始化（由 <see cref="FriendSummonCmd.Summon{T}" /> 在生物进场后调用）：
    /// 覆盖生命上限、叠加力量、挂上行动驱动能力，最后交给子类做专属初始化。
    /// </summary>
    public virtual async Task OnSummon(PlayerChoiceContext choiceContext, Player owner, FriendSummonOptions options)
    {
        var self = Creature;

        // 腐化蜥蜴在场时，新召唤的伙伴会被它直接吞噬（对应塔1 FriendMonsterController.addFriend 的拦截：
        // 生命上限并进腐化蜥蜴后立刻退场，不挂任何能力、也不留在场上）
        if (await FCorruptLizard.TryDevourAsync(owner, self, options.MaxHp))
            return;

        if (options.MaxHp is decimal maxHp)
            await CreatureCmd.SetMaxAndCurrentHp(self, maxHp);

        if (options.PrimaryStatAmount is decimal strength && strength > 0m)
            await PowerCmd.Apply<StrengthPower>(choiceContext, self, strength, owner.Creature, options.Source);

        // 行动驱动：玩家回合结束时自动行动（隐藏能力，无图标）
        // 蜥蜴类伙伴用可见的「伙伴」buff；其它伙伴（猫崽/拾荒者等）用隐藏驱动，不显示图标
        if (this is FriendCabBase)
            await PowerCmd.Apply<FriendCabAttackPower>(choiceContext, self, 1m, owner.Creature, options.Source);
        else
            await PowerCmd.Apply<FriendPetTurnPower>(choiceContext, self, 1m, owner.Creature, options.Source);

        await OnSummoned(choiceContext, owner, options);
    }

    /// <summary>召唤时在公共流程（血量/力量/行动驱动）之后追加的初始化（挂专属能力、设置初始意图等）。</summary>
    protected virtual Task OnSummoned(PlayerChoiceContext choiceContext, Player owner, FriendSummonOptions options)
        => Task.CompletedTask;

    // ———— 伤害公式 ————

    /// <summary>
    /// 攻击伤害 =（基础值 + 玩家「协调」层数）× 进化倍率（对应塔1 computeFriendDamage 的主动部分）；
    /// 自身力量与活力等由伤害管线另行叠加。
    /// </summary>
    protected decimal ComputeAttackDamage(decimal baseDamage) =>
        Math.Floor((baseDamage + TuneAmount()) * EvolutionMultiplier());

    /// <summary>玩家身上的「协调」层数（塔1 tuneAmount；无则为 0）。</summary>
    protected decimal TuneAmount()
    {
        Creature? playerCreature = Creature.PetOwner?.Creature;
        if (playerCreature == null || !playerCreature.HasPower<TunePower>())
            return 0m;
        return playerCreature.GetPower<TunePower>()?.Amount ?? 0m;
    }

    /// <summary>进化倍率 = 1 + 玩家身上「进化」层数（对应塔1 AbstractFriend.evolutionMultiplier）。</summary>
    protected decimal EvolutionMultiplier()
    {
        Creature? playerCreature = Creature.PetOwner?.Creature;
        if (playerCreature == null || !playerCreature.HasPower<MonkEvolutionPower>())
            return 1m;
        return 1m + (playerCreature.GetPower<MonkEvolutionPower>()?.Amount ?? 0m);
    }

    // ———— 行动公共实现 ————

    /// <summary>单体目标：优先「优先攻击」标记的敌人，否则随机选中一个。</summary>
    internal static Creature PickTarget(Creature pet, IReadOnlyList<Creature> enemies)
    {
        Creature? priority = enemies.FirstOrDefault(e => e.HasPower<PriorityPower>());
        if (priority != null)
            return priority;

        var combatTargets = pet.PetOwner?.RunState.Rng.CombatTargets;
        if (combatTargets == null)
            return enemies[0];
        return enemies[combatTargets.NextInt(0, enemies.Count)];
    }

    /// <summary>让一只伙伴按自动行动规则执行一次行动（需未休眠：目标优先「优先攻击」，否则随机）。</summary>
    internal static async Task PerformAutoTurn(Creature pet, PlayerChoiceContext choiceContext)
    {
        if (!pet.IsAlive || pet.Monster is not IFriendPetTurn turn)
            return;
        IReadOnlyList<Creature> enemies = pet.CombatState?.HittableEnemies ?? [];
        // 攻击类伙伴没有敌人时不行动；辅助类（如拾荒者）不需要目标
        if (enemies.Count == 0 && turn.RequiresEnemyTarget)
            return;
        IReadOnlyList<Creature> targets = turn switch
        {
            { HitsAllEnemies: true } => enemies,
            { RequiresEnemyTarget: true } => [PickTarget(pet, enemies)],
            _ => []
        };
        await turn.PerformFriendTurn(choiceContext, targets);
    }

    /// <summary>单体一次攻击：播自身的攻击动作后直接结算伤害（不做冲撞位移动画）。</summary>
    protected async Task BumpAttackAsync(PlayerChoiceContext choiceContext, Creature target, decimal damage)
    {
        Creature.GetCreatureNode()?.SetAnimationTrigger("Attack");
        await DealDamage(choiceContext, target, damage);
    }

    /// <summary>伤害结算（带「技能来源」标记，吃攻击者的力量/活力等加成）。</summary>
    /// <para>结算后自行处理目标身上的「人体蜂房」(PersonalHivePower)：
    /// 伙伴本身不触发原版能力（原版只认 Osty，伙伴会造出无 owner 的眩晕把回合搞崩），
    /// 改由这里按蜂房层数给<b>伙伴主人</b>塞等量眩晕。</para>
    protected async Task DealDamage(PlayerChoiceContext choiceContext, Creature target, decimal damage)
    {
        await DealHiveAwareDamageAsync(choiceContext, Creature, target, damage, ValueProp.Move);
    }

    /// <summary>
    /// 伙伴造成的伤害统一入口，顺带处理目标身上的「人体蜂房」(PersonalHivePower)。
    /// <para><b>不依赖补丁、也不摘挂能力</b>：目标带蜂房时，这一击改成 <see cref="ValueProp.Unpowered" />，
    /// 原版 <c>PersonalHivePower.AfterDamageReceived</c> 的 <c>props.IsPoweredAttack()</c> 直接为 false，
    /// 不会执行到 <c>CreateCard&lt;Dazed&gt;(dealer.Player)</c> 那个主因为宠物没有 Player 的空引用，
    /// 也不会出现「敌人反复获得/失去能力」的特效闪烁。</para>
    /// <para>代价是这条伤害不再经过能力管线，所以这里手工补上会影响伙伴伤害的加成：
    /// 伙伴自身力量/活力（加法）、目标易伤、伙伴自身虚弱（乘法）；协调与进化已经在
    /// <see cref="ComputeAttackDamage" /> 里算进基础伤害了，不需要重复计算。</para>
    /// </summary>
    public static async Task DealHiveAwareDamageAsync(PlayerChoiceContext choiceContext, Creature pet,
        Creature target, decimal amount, ValueProp props, CardModel? cardSource = null, CardPlay? cardPlay = null)
    {
        PersonalHivePower? hive = target.GetPower<PersonalHivePower>();

        if (hive is null)
        {
            if (cardSource != null)
                await CreatureCmd.Damage(choiceContext, target, amount, props, pet, cardSource, cardPlay);
            else
                await CreatureCmd.Damage(choiceContext, target, amount, props, pet);
            return;
        }

        // 目标带「人体蜂房」：Unpowered 让原版能力直接跳过，加成在这里手工算
        decimal damage = ComputeHivePartyDamage(pet, target, amount);

        if (cardSource != null)
            await CreatureCmd.Damage(choiceContext, target, damage, props | ValueProp.Unpowered, pet, cardSource, cardPlay);
        else
            await CreatureCmd.Damage(choiceContext, target, damage, props | ValueProp.Unpowered, pet);

        // 眩晕按蜂房层数给伙伴的主人（原版那段对非 Osty 宠物会空引用，所以自己发）
        await GrantHiveDazedAsync(choiceContext, pet, (int)decimal.Ceiling(hive.Amount));
    }

    /// <summary>
    /// 目标带「人体蜂房」时的手工伤害计算，顺序与原版 <c>Hook.ModifyDamageInternal</c> 一致：
    /// 先所有加法，再所有乘法，最后取 <c>max(0)</c>。
    /// </summary>
    private static decimal ComputeHivePartyDamage(Creature pet, Creature target, decimal amount)
    {
        decimal value = amount;

        // 加法：伙伴自身的力量 / 活力
        value += pet.GetPower<StrengthPower>()?.Amount ?? 0m;
        value += pet.GetPower<VigorPower>()?.Amount ?? 0m;

        // 乘法：目标的易伤
        if (target.GetPower<VulnerablePower>() is { } vulnerable)
            value *= vulnerable.DynamicVars["DamageIncrease"].BaseValue;

        // 乘法：伙伴自身的虚弱
        if (pet.GetPower<WeakPower>() is { } weak)
            value *= 1m - weak.DynamicVars["DamageDecrease"].BaseValue;

        return Math.Max(0m, value);
    }

    /// <summary>
    /// 给伙伴的主人塞 <paramref name="count"/> 张眩晕（随机插入抽牌堆），对应原版「人体蜂房」的效果。
    /// 主人取自 <c>pet.PetOwner</c>，不依赖 <c>dealer.Player</c>，所以不会出现无 owner 的牌。
    /// </summary>
    public static async Task GrantHiveDazedAsync(PlayerChoiceContext choiceContext, Creature pet, int count)
    {
        if (count <= 0)
            return;

        if (pet.PetOwner is not { } owner)
            return;

        if (pet.CombatState is not { } combatState)
            return;

        var results = new CardPileAddResult[count];
        for (var i = 0; i < count; i++)
        {
            CardModel card = combatState.CreateCard<Dazed>(owner);
            results[i] = await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, null, CardPilePosition.Random);
        }

        CardCmd.PreviewCardPileAdd(results);
        await Cmd.Wait(0.5f);
    }

    /// <summary>刷新意图显示（力量、「协调」、进化变化后攻击数字保持最新）。</summary>
    protected void RefreshFriendIntents()
    {
        if (Creature.CombatState?.IsLiveCombat() != true)
            return;
        NCreature? node = Creature.GetCreatureNode();
        if (node != null)
            TaskHelper.RunSafely(node.RefreshIntents());
    }
}
