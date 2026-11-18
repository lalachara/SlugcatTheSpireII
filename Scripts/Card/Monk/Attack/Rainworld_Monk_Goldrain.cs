using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 珍珠雨：你的卡组中每有 1 张[稀有]的牌，就对<b>所有敌人</b>造成一次 {Damage} 点伤害，
/// 并获得 <see cref="GoldPerRare" /> 金币；有多人盟友时，盟友也获得<b>等量</b>金币。
/// </summary>
public class Rainworld_Monk_Goldrain : MonkCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    /// <summary>每张稀有牌给的金币（自己与盟友各拿这么多）。</summary>
    private const int GoldPerRare = 2;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { RainworldKeywords.Treasure };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4m, ValueProp.Move)
    ];

    public Rainworld_Monk_Goldrain() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
            return;

        // 「你的卡组」= 整副牌组（Player.Deck），不是战斗中的某个牌堆：
        // 打出的这张自己也算在内（它仍在卡组里），与卡面「卡组中每有 1 张」一致。
        int rareCount = Owner.Deck.Cards.Count(c => c.Rarity == CardRarity.Rare);
        if (rareCount <= 0)
            return;

        // 每张稀有牌：对所有敌人打一段
        if (CombatState.HittableEnemies.Count > 0)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .WithHitCount(rareCount)
                .TargetingAllOpponents(CombatState)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }

        // 金币：自己拿 2 × 稀有数，存活的盟友各拿等量
        int gold = rareCount * GoldPerRare;
        await PlayerCmd.GainGold(gold, Owner);

        foreach (Player ally in CombatState.Players.Where(p => p != Owner && p.Creature.IsAlive))
            await PlayerCmd.GainGold(gold, ally);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
