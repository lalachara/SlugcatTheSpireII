using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Friendship : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // 描述中 {Energy:energyIcons()} 占位：每回合开始时获得 1 点能量（由 FriendshipPower 提高能量上限实现）。
        new EnergyVar(1)
    ];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<StrengthPower>() };

    public Rainworld_Monk_Friendship() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<FriendshipPower>(choiceContext, Owner.Creature,1, Owner.Creature, this);
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature,-2, Owner.Creature, this);

    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}