// Rainworld_River_Limbo.cs

using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Card.River.Powers;

public class Rainworld_River_Limbo : RiverCardModel
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
            CardType.Attack => "res://Resource/Card/River/CardAtk1024.png",
            CardType.Skill => "res://Resource/Card/River/CardSkill1024.png",
            CardType.Power => "res://Resource/Card/River/CardPower1024.png",
            _ => ""
        }
    );
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Ethereal };

    private string GetLocalizedCardImage()
    {
        var cardName = GetType().Name.ToLowerInvariant()
            .Replace("slugcat_the_spire_ii_card_", "");
        var locale = TranslationServer.GetLocale(); // 如 "zh", "en"
        string localizedPath = $"res://Resource/Card/River/{cardName}.png";
        if (locale == "ko")
        {
            localizedPath = $"res://Resource/Card/diff/KR/{cardName}.png";
        }
        else if(locale != "zh_Hans")
        {
            localizedPath = $"res://Resource/Card/diff/ENG/{cardName}.png";
        }

        return ResourceLoader.Exists(localizedPath) 
            ? localizedPath 
            : $"res://Resource/Card/River/{cardName}.png"; // 回退到默认
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<AdroitPower>() ,HoverTipFactory.FromPower<AdroitcapPower>()};
    public Rainworld_River_Limbo() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<LimboPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
        
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
}