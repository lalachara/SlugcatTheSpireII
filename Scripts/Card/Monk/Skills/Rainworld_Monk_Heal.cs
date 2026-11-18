using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Rainworld.Scripts.Monster.Minions;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 治愈：令随机若干伙伴恢复所有生命，优先选择处于眩晕（休眠）的伙伴。
/// 对应塔1 Monk_Heal：先花名额唤醒休眠伙伴（唤醒即满血），名额还有剩再给残血伙伴补满。
/// </summary>
public class Rainworld_Monk_Heal : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Companion)};

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("Special", 1m)
    ];

    public Rainworld_Monk_Heal() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<Creature> friends = FriendPositions.All(Owner).ToList();

        // 休眠（眩晕）的伙伴优先；它们此时 HP 为 0，唤醒时会自动回满。
        List<Creature> dormant = friends
            .Where(f => f.GetPower<FriendDormantPower>() is { IsDormant: true })
            .ToList();

        // 残血伙伴：仍存活、未休眠且未满血（满血的不选，避免白拿名额）。
        List<Creature> injured = friends
            .Where(f => f.IsAlive && f.CurrentHp < f.MaxHp
                        && f.GetPower<FriendDormantPower>() is not { IsDormant: true })
            .ToList();

        var rng = Owner.RunState.Rng.CombatCardGeneration;
        var remaining = (int)DynamicVars["Special"].BaseValue;

        while (remaining > 0 && dormant.Count > 0)
        {
            Creature pick = dormant[rng.NextInt(0, dormant.Count)];
            dormant.Remove(pick);
            remaining--;
            if (pick.GetPower<FriendDormantPower>() is { IsDormant: true } dormantPower)
                await dormantPower.WakeFromDormant();
        }

        while (remaining > 0 && injured.Count > 0)
        {
            Creature pick = injured[rng.NextInt(0, injured.Count)];
            injured.Remove(pick);
            remaining--;
            await CreatureCmd.Heal(pick, pick.MaxHp - pick.CurrentHp, true);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Special"].UpgradeValueBy(1m);
    }
}
