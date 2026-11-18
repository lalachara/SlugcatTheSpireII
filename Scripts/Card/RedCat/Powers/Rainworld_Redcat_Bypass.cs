using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Card.RedCat;

public class Rainworld_Redcat_Bypass : RedCatCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    public override CardAssetProfile AssetProfile => new(
        GetLocalizedCardImage(),
        FramePath: Type switch
        {
            CardType.Attack => "res://Resource/Card/RedCat/CardAtk1024.png",
            CardType.Skill => "res://Resource/Card/RedCat/CardSkill1024.png",
            CardType.Power => "res://Resource/Card/RedCat/CardPower1024.png",
            _ => ""
        }
    );

    private string GetLocalizedCardImage()
    {
        var cardName = GetType().Name.ToLowerInvariant()
            .Replace("slugcat_the_spire_ii_card_", "");
        var locale = TranslationServer.GetLocale(); // 如 "zh", "en"
        string localizedPath = $"res://Resource/Card/RedCat/{cardName}.png";
        if (locale == "ko")
        {
            localizedPath = $"res://Resource/Card/diff/KR/{cardName}.png";
        }

        return ResourceLoader.Exists(localizedPath) 
            ? localizedPath 
            : $"res://Resource/Card/RedCat/{cardName}.png"; // 回退到默认
    }
    protected override HashSet<CardTag> CanonicalTags => new() { RainworldCardTags.CantGenerate };
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Ethereal,CardKeyword.Innate};
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Worklevel)};

    public Rainworld_Redcat_Bypass() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if(CombatState?.RoundNumber<=(IsUpgraded ? 4 : 3))
             await PowerCmd.Apply<BypassPower>(choiceContext, Owner.Creature, (IsUpgraded ? 4 : 3)+1-CombatState.RoundNumber, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
            EnergyCost.UpgradeBy(-1);

    }
}