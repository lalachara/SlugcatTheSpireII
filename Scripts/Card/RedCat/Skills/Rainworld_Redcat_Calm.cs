using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.RedCat;

public class Rainworld_Redcat_Calm : RedCatCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(4m, ValueProp.Move),
        new("Summon", 2m)
    ];

    public Rainworld_Redcat_Calm() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        RainworldKeywords.Cantcorrupt
    ];
    public override bool GainsBlock => true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] {  HoverTipFactory.FromPower<RedmushroomPower>(),HoverTipFactory.FromPower<MushblockPower>(),HoverTipFactory.FromKeyword(RainworldKeywords.Corrupt)};

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var corruptValue = (int)SlugcatData.getCorruptValue(Owner);
        var block = DynamicVars.Block.BaseValue + DynamicVars["Summon"].BaseValue * corruptValue;
        await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(block, ValueProp.Unpowered), cardPlay);
        await SlugcatData.addCorruptValue(Owner, -5);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars["Summon"].UpgradeValueBy(1m);
    }
}