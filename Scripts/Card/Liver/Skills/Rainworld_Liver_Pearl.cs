using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Pearl : LiverCardModel

{

#if STS2_0_113_0 || STS2_0_114_0
    /// <summary>快速模式（Blitz）没有金币概念：与原版贪婪之手 / 我的小钱钱一致，不在该模式生成。</summary>
    public override bool IsAllowedInBlitz => false;
#endif
    // 基础耗能
    private const int energyCost = -2;

    // 卡牌类型
    private const CardType type = CardType.Skill;

    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Rare;

    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;

    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    // 不可被生成类效果抽到：这张牌不能打出，只有从卡组移除时才换金币。
    protected override HashSet<CardTag> CanonicalTags => new() { RainworldCardTags.CantGenerate };
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
    protected override bool IsPlayable => false;
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
        { RainworldKeywords.Treasure };
    // 卡牌的基础属性（例如这里是12点伤害）
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new GoldVar(300)
    ];

    public Rainworld_Liver_Pearl() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public override async Task BeforeCardRemoved(CardModel card)
    {
        if (card == this)
            await PlayerCmd.GainGold(DynamicVars["Gold"].IntValue, Owner);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars.Gold.UpgradeValueBy(100);
    }
}