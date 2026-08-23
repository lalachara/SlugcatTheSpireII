using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Resource.Card;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Card.RedCat;
[RegisterCard(typeof(Rainworld_Future_CardPool), Inherit = true)]

public class Rainworld_Redcat_Corrupt : ModCardTemplate

{
    public override CardAssetProfile AssetProfile => new(
        $"res://Resource/Card/RedCat/{GetType().Name.ToLowerInvariant().Replace("slugcat_the_spire_ii_card_", "")}.png",
        
        FramePath: Type switch
        {
            CardType.Attack => "res://Resource/Card/RedCat/CardAtk1024.png",
            CardType.Skill => "res://Resource/Card/RedCat/CardSkill1024.png",
            CardType.Power => "res://Resource/Card/RedCat/CardPower1024.png",
            _ => ""
        }

        // 卡框等，有需求自己添加。需要自行判断卡牌类型（攻击、技能、能力等）设置，建议写在基类里。
        // 如果使用自定义卡池，需要改下material，看添加人物章节的添加卡池部分
        // FramePath: "", // 卡牌背景
        // PortraitBorderPath: "", // 边框（状态牌感染使用的）
        // BannerTexturePath: "" // 横幅（不同类型）
    );
    
    private const int energyCost = 0;
    private const CardType type = CardType.Status;
    private const CardRarity rarity = CardRarity.Status;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override HashSet<CardTag> CanonicalTags => new() { };
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[]
    {
        HoverTipFactory.FromPower<RedmushroomPower>(),
        HoverTipFactory.FromPower<MushblockPower>(),
    };
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { RainworldKeywords.Corrupt, CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(2m),
        new CalculationExtraVar(1m),
        new CalculatedVar("Mushblock").WithMultiplier((CardModel card, Creature? _) =>
            // card.Owner.Creature?.GetPower<SubstratePower>()?.Amount ??
            0)
    ];

    public Rainworld_Redcat_Corrupt() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public override int MaxUpgradeLevel { get; } = 0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var mushblock = (int)((CalculatedVar)DynamicVars["Mushblock"]).Calculate(null);
        await PowerCmd.Apply<MushblockPower>(choiceContext, Owner.Creature, mushblock, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
    }
}