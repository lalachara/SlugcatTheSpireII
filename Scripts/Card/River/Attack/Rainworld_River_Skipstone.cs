using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.River.Attack;

public class Rainworld_River_Skipstone : RiverCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5m, ValueProp.Move)
    ];

    public Rainworld_River_Skipstone() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        var handPile = PileType.Hand.GetPile(Owner);
        if (handPile.Cards.Count == 0)
            return;

        var selected = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(SelectionScreenPrompt, 1),
            context: choiceContext, player: Owner, filter: null, source: this)).FirstOrDefault();

        if (selected == null)
            return;

        var cost = (int)selected.EnergyCost.Canonical;

        await CardCmd.Exhaust(choiceContext, selected);

        if(cost<=0)
            return;

        await DamageCmd
                .Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this,cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitCount(cost)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}