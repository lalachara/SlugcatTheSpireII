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

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Kitty : LiverCardModel

{
    // 基础耗能
    private const int energyCost = 3;

    // 卡牌类型
    private const CardType type = CardType.Power;

    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Rare;

    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;

    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
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
    protected override HashSet<CardTag> CanonicalTags => new() { };
    //public override IEnumerable<CardKeyword> CanonicalKeywords => new []{};

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1)
    ];

    public Rainworld_Liver_Kitty() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<KittyPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}