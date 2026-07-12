using System.Globalization;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.RedCat;

public class Rainworld_Redcat_Firewind : RedCatCardModel

{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    protected override HashSet<CardTag> CanonicalTags => new() { };
    public override int MaxUpgradeLevel => 999999999;

    public override string Title
    {
        get
        {
            var baseTitle = TitleLocString.GetFormattedText();
            if (!IsUpgraded)
                return baseTitle;
            if (CultureInfo.CurrentUICulture.Name.StartsWith("zh"))
                return $"{baseTitle}第{CurrentUpgradeLevel}层";
            return $"{baseTitle}+{CurrentUpgradeLevel}";
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4m, ValueProp.Move),
        new CardsVar(2)
    ];

    public Rainworld_Redcat_Firewind() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd
            .Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this,cardPlay)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_slash")
            .WithHitCount(DynamicVars.Cards.IntValue)
            .Execute(choiceContext);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        if (CurrentUpgradeLevel % 2 != 1)
            DynamicVars.Cards.UpgradeValueBy(1m);
        DynamicVars.Damage.UpgradeValueBy(1m);
    }
}