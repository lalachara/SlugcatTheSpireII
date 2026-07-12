using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.RedCat;

public class Rainworld_Redcat_Leave : RedCatCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust};

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] {   HoverTipFactory.FromKeyword(RainworldKeywords.Corrupt),HoverTipFactory.FromPower<RedmushroomPower>(), HoverTipFactory.FromPower<MushblockPower>(),HoverTipFactory.FromCard<Rainworld_Redcat_Corrupt>() };

    public Rainworld_Redcat_Leave() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Character is Rainworld_Redcat)
        {
            int amount = SlugcatData.getCorruptValue(Owner);
            await SlugcatData.setCorruptValue(Owner, 0);
            if(amount > 0)
                for (int i = 0; i < amount; i++)
                {
                    var corrupt = CombatState.CreateCard<Rainworld_Redcat_Corrupt>(Owner);
                    await CardPileCmd.AddGeneratedCardToCombat(corrupt, PileType.Hand, Owner);
                }
        }
    }

    protected override void OnUpgrade()
    {
       RemoveKeyword(CardKeyword.Exhaust);
    }
}