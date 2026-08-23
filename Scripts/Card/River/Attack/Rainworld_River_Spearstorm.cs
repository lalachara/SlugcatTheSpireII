﻿// Rainworld_River_SpearStorm.cs

 using Godot;
 using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Card.River.Attack;

public class Rainworld_River_Spearstorm : RiverCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] {  HoverTipFactory.FromPower<CorrodePower>(),HoverTipFactory.FromKeyword(RainworldKeywords.Lightning) };

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

    private string GetLocalizedCardImage()
    {
        var cardName = GetType().Name.ToLowerInvariant()
            .Replace("slugcat_the_spire_ii_card_", "");
        var locale = TranslationServer.GetLocale(); // 如 "zh", "en"
        string localizedPath = $"res://Resource/Card/River/{cardName}.png";
        if (locale == "ko")
        {
            localizedPath = $"res://Resource/Card/diff/KR/rainworld_liver_spearelec.png";
        }

        return ResourceLoader.Exists(localizedPath) 
            ? localizedPath 
            : $"res://Resource/Card/River/{cardName}.png"; // 回退到默认
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { RainworldKeywords.Spear };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(18m, ValueProp.Move)
    ];

    public Rainworld_River_Spearstorm() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        var dmg = DynamicVars.Damage.BaseValue;
        if (cardPlay.Target.HasPower<CorrodePower>())
            dmg *= 2;

        await DamageCmd
            .Attack(dmg)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx(sfx:"event:/Rainworld/sfx/spearelec")
            .Execute(choiceContext);
    }


    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6m);
    }
}