﻿﻿using Godot;
  using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Resource.Card;
using Rainworld.Scripts;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Void = MegaCrit.Sts2.Core.Models.Cards.Void;

namespace Rainworld.Scripts.Card.Liver.Attack;
[RegisterCard(typeof(Rainworld_Future_CardPool), Inherit = true)]
public class Rainworld_Liver_Bag : ModCardTemplate

{


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
                localizedPath = $"res://Resource/Card/diff/KR/{cardName}.png";
            }
    
            return ResourceLoader.Exists(localizedPath) 
                ? localizedPath 
                : $"res://Resource/Card/Liver/{cardName}.png"; // 回退到默认
        }
    // 基础耗能
    private const int energyCost = 0;

    // 卡牌类型
    private const CardType type = CardType.Skill;

    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Token;

    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;

    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    public override int MaxUpgradeLevel => 0;
    protected override HashSet<CardTag> CanonicalTags => new() { };

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new[] { CardKeyword.Exhaust, RainworldKeywords.Treasure };

    public List<CardModel> Cards = new();


    // 卡牌的基础属性（例如这里是12点伤害）
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];

    public Rainworld_Liver_Bag() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public Rainworld_Liver_Bag(List<CardModel> cards, Player owner) : base(energyCost, type, rarity, targetType,
        shouldShowInCardLibrary)
    {
        Cards = cards;
        Owner = owner;
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Cards.Count == 0)
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Void>(Owner), PileType.Hand, Owner));
        else
            foreach (var card in Cards)
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner));

        Cards = new();
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
    }
}