using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 共振：场上每有 1 个单位，就令所有敌人失去 {CalculatedDamage} 点<b>生命</b>（生命流失，不是伤害）。
/// <para>「单位」（也就是段数）= 战斗场景里还占着位置的生物，与卡面文案「场上每有 1 个单位」一致：
/// 玩家本人与队友、伙伴（自己的、队友的、眩晕倒地的、原版异鸟 / 奥斯提这类非本 mod 宠物）、
/// 敌人以及其它召唤物都算。</para>
/// <para>每段生命流失固定（2，升级后 3）：不吃力量、不被格挡、也不吃菌壁一类的伤害减免，只按段数叠加。</para>
/// </summary>
public class Rainworld_Monk_Syntony : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    /// <summary>
    /// 计数单位：**按战斗场景里还存在的生物节点**数，因此
    /// <list type="bullet">
    /// <item>玩家算：自己 + 队友；</item>
    /// <item>伙伴全算：自己的、队友的、原版异鸟（Byrdpip）/ 奥斯提（Osty）这类非本 mod 宠物，
    /// 以及被打倒后还在场上等复活的（`IsAlive == false` 但场景节点没被移除）——
    /// 卡面说的是「场上」，不是「存活」；</item>
    /// <item>敌人算；已经阵亡并被移出场景的单位（例如直接退场的猫崽）不算。</item>
    /// </list>
    /// 不能按 `IsAlive` 过滤（会漏掉眩晕倒地的伙伴），也不能用 `PlayerCombatState.Pets`
    /// （多人客机上未必完整，而且原版 Osty 根本不是「宠物」——没有 PetOwner）。
    /// 战斗外没有场景，返回 0：卡库里只显示基础伤害，不显示段数。
    /// </summary>
    private static decimal CountUnits(CardModel card, Creature? creature)
    {
        if (card is not Rainworld_Monk_Syntony)
            return 0m;

        ICombatState? combatState = card.CombatState;
        if (combatState == null)
            return 0m;

        NCombatRoom? room = NCombatRoom.Instance;

        decimal count = 0m;
        foreach (Creature target in combatState.Creatures)
        {
            // 正常战斗里一定有场景节点；取不到战斗场景时退回「存活」口径，避免整套计算失效
            bool onField = room != null ? room.GetCreatureNode(target) != null : target.IsAlive;
            if (onField)
                count += 1m;
        }

        return count;
    }

    /// <summary>
    /// 攻击次数变量：默认用 CalculationBase/CalculationExtra，
    /// 这里指向独立的 HitsBase / HitsExtra，避免与伤害的 CalculationBase 共用一个值。
    /// </summary>
    private sealed class HitCountVar : CalculatedVar
    {
        private readonly DynamicVar _hitsBase;
        private readonly DynamicVar _hitsExtra;

        public HitCountVar(DynamicVar hitsBase, DynamicVar hitsExtra) : base("CalculatedHits")
        {
            _hitsBase = hitsBase;
            _hitsExtra = hitsExtra;
        }

        protected override DynamicVar GetBaseVar() => _hitsBase;
        protected override DynamicVar GetExtraVar() => _hitsExtra;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            var hitsBase = new DynamicVar("HitsBase", 0m);
            var hitsExtra = new DynamicVar("HitsExtra", 1m);
            return
            [
                new CalculationBaseVar(2m),
                // 伤害不随单位数增长：单位数只决定段数，ExtraDamage 置 0 后 CalculatedDamage 恒为基础值。
                new ExtraDamageVar(0m),
                // Unpowered：跳过力量等增减伤修正，卡面写多少就造成多少。
                new CalculatedDamageVar(ValueProp.Move | ValueProp.Unpowered | ValueProp.Unblockable).WithMultiplier(CountUnits),
                hitsBase,
                hitsExtra,
                new HitCountVar(hitsBase, hitsExtra).WithMultiplier(CountUnits)
            ];
        }
    }

    public Rainworld_Monk_Syntony() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
            return;

        // 现算段数：当前场上单位数；场上没单位（理论上不会）时不生效。
        var hits = (int)((CalculatedVar)DynamicVars["CalculatedHits"]).Calculate(null);
        if (hits <= 0)
            return;

        // 生命流失（不是伤害）：不可格挡 + 不受能力增伤，来源记为打出这张牌的玩家，
        // 这样「你造成生命流失」类能力（思考 / 识破 / 自然能量 / 幻象计数）都能正常响应，
        // 而力量、易伤、格挡、菌壁这类作用于「伤害」的东西都不参与。
        decimal lossPerHit = DynamicVars.CalculatedDamage.BaseValue;

        for (var i = 0; i < hits; i++)
        {
            List<Creature> enemies = CombatState.HittableEnemies.Where(e => e.IsAlive).ToList();
            if (enemies.Count == 0)
                break;

            await CreatureCmd.Damage(choiceContext, enemies, lossPerHit,
                ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature, this, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级后每段生命流失 2 → 3
        DynamicVars.CalculationBase.UpgradeValueBy(1m);
    }
}
