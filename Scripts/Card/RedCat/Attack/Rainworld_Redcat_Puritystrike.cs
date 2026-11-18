using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
namespace Rainworld.Scripts.Card.RedCat;

public class Rainworld_Redcat_Puritystrike : RedCatCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override HashSet<CardTag> CanonicalTags => new() { CardTag.Strike };

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        RainworldKeywords.Cantcorrupt
    ];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Cantcorrupt)};

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(18m, ValueProp.Move)
    ];

    public Rainworld_Redcat_Puritystrike() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
    {
        if(card != this)
            return true;
        if (SlugcatData.getCorruptValue(Owner) != 0&&Owner.Character is Rainworld_Redcat)
            return false;
        return true;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd
            .Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this,cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
    }
}