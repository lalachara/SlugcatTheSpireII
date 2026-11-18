using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// PUA：结束你的回合，令1名盟友在本回合结束后获得1个额外回合。
/// 其于该额外回合每次打出牌时，你获得 <see cref="BlockPerCard" /> 点格挡。
/// </summary>
public class Rainworld_Monk_Pua : MonkCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyAlly;
    private const bool shouldShowInCardLibrary = true;
    public override bool GainsBlock => true;

    /// <summary>额外回合里盟友每打出一张牌，你获得的格挡（卡面 {Special}）。</summary>
    private const decimal BlockPerCard = 2m;

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("Special", BlockPerCard)
    ];

    public Rainworld_Monk_Pua() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        if (cardPlay.Target.IsAlive)
        {
            // 层数 = 额外回合里盟友每打出一张牌给你的格挡（未强化 2 / 强化 3）。
            // 反复打出会叠加层数 → 每张牌给的格挡变多；但只会给 1 个额外回合（不叠加）。
            await PowerCmd.Apply<PuaExtraTurnPower>(choiceContext, cardPlay.Target,
                DynamicVars["Special"].BaseValue, Owner.Creature, this);
        }

        // 结束自己的回合；额外回合在本次玩家回合结束后统一结算。
        PlayerCmd.EndTurn(Owner, canBackOut: false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Special"].UpgradeValueBy(1m);

    }
}
