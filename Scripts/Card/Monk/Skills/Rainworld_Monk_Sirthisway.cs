﻿using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Card.Colorless;
using Rainworld.Scripts.Card.Common;
using Rainworld.Scripts.Card.Liver.Skills;
using Rainworld.Scripts.Card.RedCat;
using Rainworld.Scripts.Card.River.Attack;
using Rainworld.Scripts.Powers;
using SlugcatTheSpireII.Scripts;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Monk_Sirthisway : MonkCardModel
{
    // 基础耗能
    private const int energyCost = 1;

    // 卡牌类型
    private const CardType type = CardType.Skill;

    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Uncommon;

    // 目标类型
    private const TargetType targetType = TargetType.Self;

    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    protected override HashSet<CardTag> CanonicalTags => new() { };

    // 只替换卡图，卡框、横幅等继续沿用 MonkCardModel 里的设置。
    public override CardAssetProfile AssetProfile =>
        base.AssetProfile with { PortraitPath = GetLocalizedCardImage() };

    private string GetLocalizedCardImage()
    {
        var cardName = GetType().Name.ToLowerInvariant()
            .Replace("slugcat_the_spire_ii_card_", "");
        var locale = TranslationServer.GetLocale(); // 如 "zh", "en"
        string localizedPath = $"res://Resource/Card/Liver/rainworld_liver_sirthisway.png";
        if (locale == "ko")
            localizedPath = $"res://Resource/Card/diff/KR/{cardName}.png";

        return ResourceLoader.Exists(localizedPath)
            ? localizedPath
            : $"res://Resource/Card/Liver/rainworld_liver_sirthisway.png"; // 回退到默认
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public Rainworld_Monk_Sirthisway() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SirthiswayLogic.OnPlay(this, choiceContext, cardPlay);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        SirthiswayLogic.OnUpgrade(this);
    }
}
