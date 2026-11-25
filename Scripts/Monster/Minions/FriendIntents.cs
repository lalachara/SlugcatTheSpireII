using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Monster.Minions;

/// <summary>
/// 伙伴攻击意图的伤害预览。
/// <para>原版 <c>AttackIntent.GetSingleDamage</c> 预览时把「本地玩家」当成受击目标：
/// <c>Hook.ModifyDamage(..., target: me.Creature, dealer: owner, ...)</c>。
/// 但伙伴打的是敌人，于是任何「以玩家为受击目标」的加成都会虚增显示——
/// 典型的就是感染棱柱给的 [gold]感染[/gold]（<c>TaintedPower</c>：target == 玩家时 +层数），
/// 以及玩家身上的易伤等；实际结算打敌人时这些都不生效，所以只有显示偏高。</para>
/// <para>这里把预览目标换成伙伴自己：既保留伙伴自身的力量/活力/虚弱（dealer 侧）加成，
/// 又不会再吃到「玩家中招」类的加伤，数字与实际伤害一致。</para>
/// </summary>
internal static class FriendIntentPreview
{
    public static decimal GetDamage(Func<decimal> damageCalc, Creature owner)
    {
        decimal amount = damageCalc();
        if (amount < 0m)
            amount = 0m;

        Player? me = owner.CombatState is { } combat ? LocalContext.GetMe(combat) : null;
        if (me == null)
            return amount;

        amount = Hook.ModifyDamage(me.RunState, me.Creature.CombatState, owner, owner, amount, ValueProp.Move,
            null, null, ModifyDamageHookType.All, CardPreviewMode.None, out _);

        return Math.Max(0m, amount);
    }

    public static int GetDamageInt(Func<decimal> damageCalc, Creature owner) =>
        Math.Max(0, (int)GetDamage(damageCalc, owner));
}

/// <summary>伙伴单体攻击意图：数字与说明都只用「伙伴侧」加成（不含玩家中招的加伤）。</summary>
internal sealed class FriendSingleAttackIntent : SingleAttackIntent
{
    public FriendSingleAttackIntent(Func<decimal> damageCalc) : base(damageCalc)
    {
    }

    public override int GetTotalDamage(IEnumerable<Creature> targets, Creature owner) =>
        FriendIntentPreview.GetDamageInt(DamageCalc!, owner);

    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
    {
        // 原版说明是「这个敌人将要……」，伙伴要用自己的本地化文案 + 伙伴侧伤害口径
        LocString description = new("powers", "SLUGCAT_THE_SPIRE_II_FRIEND_INTENT.attack");
        description.Add("Damage", GetTotalDamage(targets, owner));
        description.Add("Repeat", 1);
        return description;
    }
}

/// <summary>伙伴多段攻击意图：同上，同时修正标签与说明里的单段数字。</summary>
internal sealed class FriendMultiAttackIntent : MultiAttackIntent
{
    public FriendMultiAttackIntent(Func<decimal> damageCalc, int repeat) : base(0, repeat)
    {
        DamageCalc = damageCalc;
    }

    public override int GetTotalDamage(IEnumerable<Creature> targets, Creature owner) =>
        FriendIntentPreview.GetDamageInt(DamageCalc!, owner) * Repeats;

    public override LocString GetIntentLabel(IEnumerable<Creature> targets, Creature owner)
    {
        LocString label = base.GetIntentLabel(targets, owner);
        // 原版 MultiAttackIntent 的标签用的是 GetSingleDamage（被玩家侧加成污染过）
        label.Add("Damage", FriendIntentPreview.GetDamageInt(DamageCalc!, owner));
        label.Add("Repeat", Repeats);
        return label;
    }

    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
    {
        LocString description = new("powers", "SLUGCAT_THE_SPIRE_II_FRIEND_INTENT.attackMulti");
        description.Add("Damage", FriendIntentPreview.GetDamageInt(DamageCalc!, owner));
        description.Add("Repeat", Repeats);
        return description;
    }
}

/// <summary>伙伴的负面效果意图：悬停说明用「伙伴」口径，而不是原版的「这个敌人……」。</summary>
internal sealed class FriendDebuffIntent : DebuffIntent
{
    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner) =>
        new("powers", "SLUGCAT_THE_SPIRE_II_FRIEND_INTENT.debuff");
}

/// <summary>伙伴的防御意图：悬停说明用「伙伴」口径。</summary>
internal sealed class FriendDefendIntent : DefendIntent
{
    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner) =>
        new("powers", "SLUGCAT_THE_SPIRE_II_FRIEND_INTENT.defend");
}

/// <summary>伙伴的强化意图：悬停说明用「伙伴」口径。</summary>
internal sealed class FriendBuffIntent : BuffIntent
{
    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner) =>
        new("powers", "SLUGCAT_THE_SPIRE_II_FRIEND_INTENT.buff");
}

/// <summary>伙伴的眩晕（休眠）意图：悬停说明用「伙伴」口径。</summary>
internal sealed class FriendStunIntent : StunIntent
{
    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner) =>
        new("powers", "SLUGCAT_THE_SPIRE_II_FRIEND_INTENT.stun");
}

/// <summary>伙伴的未知意图（拾荒者恒为 UNKNOWN）：悬停说明用「伙伴」口径。</summary>
internal sealed class FriendUnknownIntent : UnknownIntent
{
    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner) =>
        new("powers", "SLUGCAT_THE_SPIRE_II_FRIEND_INTENT.unknown");
}
