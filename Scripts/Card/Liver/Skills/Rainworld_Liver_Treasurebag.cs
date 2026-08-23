﻿﻿﻿using Godot;
   using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Treasurebag : LiverCardModel

{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    // 基础耗能
    private const int energyCost = 0;

    // 卡牌类型
    private const CardType type = CardType.Skill;

    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Uncommon;

    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AnyAlly;

    // 是否在卡牌图鉴中显示
    public override CardAssetProfile AssetProfile => new(
        GetLocalizedCardImage(),
        FramePath: Type switch
        {
            CardType.Attack => "res://Resource/Card/Liver/CardAtk1024.png",
            CardType.Skill => "res://Resource/Card/Liver/CardSkill1024.png",
            CardType.Power => "res://Resource/Card/Liver/CardPower1024.png",
            _ => ""
        }
    );

    private string GetLocalizedCardImage()
    {
        var cardName = GetType().Name.ToLowerInvariant()
            .Replace("slugcat_the_spire_ii_card_", "");
        var locale = TranslationServer.GetLocale(); // 如 "zh", "en"
        string localizedPath = $"res://Resource/Card/Liver/{cardName}.png";
        if (locale == "ko")
        {
            localizedPath = $"res://Resource/Card/diff/KR/rainworld_liver_bag.png";
        }
        
        return ResourceLoader.Exists(localizedPath) 
            ? localizedPath 
            : $"res://Resource/Card/Liver/{cardName}.png"; // 回退到默认
    }
    protected override HashSet<CardTag> CanonicalTags => new() { };
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust ,RainworldKeywords.Treasure };


    // 卡牌的基础属性（例如这里是12点伤害）
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1)
    ];

    public Rainworld_Liver_Treasurebag() : base(energyCost, type, rarity, targetType, true)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        var list = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(SelectionScreenPrompt, 0, 999),
            context: choiceContext, player: Owner, filter: null, source: this)).ToList();
        var bag = CombatState!.CreateCard<Rainworld_Liver_Bag>(cardPlay.Target.Player!);
        bag.Cards = new List<CardModel>();  
        foreach (var card in list)
        {
                    var clone = card.CreateCloneForPlayer(cardPlay.Target.Player!);
                    await CardPileCmd.RemoveFromCombat(card);
                    bag.Cards.Add(clone);

        }

        var results = await CardPileCmd.AddGeneratedCardsToCombat(new List<CardModel> { bag }, PileType.Draw, Owner,
            CardPilePosition.Random);
        if (LocalContext.IsMe(cardPlay.Target.Player)) CardCmd.PreviewCardPileAdd(results);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}