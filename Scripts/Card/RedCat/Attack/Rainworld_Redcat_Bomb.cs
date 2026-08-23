using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Card.RedCat;

public class Rainworld_Redcat_Bomb : RedCatCardModel

{
    // 基础耗能
    private const int energyCost = 1;

    // 卡牌类型
    private const CardType type = CardType.Attack;

    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Common;

    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AllEnemies;
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
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<RedmushroomPower>(), HoverTipFactory.FromPower<MushblockPower>()};

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
        { CardKeyword.Exhaust, RainworldKeywords.Corrupt};


    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9m, ValueProp.Move)
    ];

    public Rainworld_Redcat_Bomb() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        //ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this,cardPlay)
            .WithHitFx(sfx: "event:/Rainworld/sfx/bomb")
            .TargetingAllOpponents(CombatState)
            .Execute(choiceContext);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m); // 升级后增加4点伤害
    }
}