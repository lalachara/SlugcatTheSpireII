using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Pullout : LiverCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[]
    {
        HoverTipFactory.FromPower<ChuangPower>(),};

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("Special", 2m)
    ];

    public Rainworld_Liver_Pullout() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
            return;

        var discardPile = PileType.Discard.GetPile(Owner);
        var spears = discardPile.Cards
            .Where(c => c.Keywords.Contains(RainworldKeywords.Spear))
            .ToList();

        if (spears.Count == 0)
            return;

        await PowerCmd.Apply<ChuangPower>(choiceContext, cardPlay.Target,
            DynamicVars["Special"].BaseValue * spears.Count, Owner.Creature, this);

        foreach (var spear in spears)
            await CardPileCmd.Add(spear, PileType.Hand.GetPile(Owner), CardPilePosition.Top);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Special"].UpgradeValueBy(1m);
    }
}