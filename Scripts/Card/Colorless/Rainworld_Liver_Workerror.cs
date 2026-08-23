using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Resource.Card;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Card.Liver.Attack;
[RegisterCard(typeof(Rainworld_Future_CardPool))]

public class Rainworld_Liver_Workerror : ModCardTemplate

{
    // 基础耗能
    private const int energyCost = 0;

    // 卡牌类型
    private const CardType type = CardType.Power;

    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Ancient;

    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;

    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    protected override bool ShouldGlowRedInternal => true;
    public override CardAssetProfile AssetProfile => new(
        $"res://Resource/Card/Liver/{GetType().Name.ToLowerInvariant().Replace("slugcat_the_spire_ii_card_", "")}.png",
        FramePath: Type switch
        {
            CardType.Attack => "res://Resource/Card/Liver/CardAtk1024.png",
            CardType.Skill => "res://Resource/Card/Liver/CardSkill1024.png",
            CardType.Power => "res://Resource/Card/Liver/CardPower1024.png",
            _ => ""
        }

        // 卡框等，有需求自己添加。需要自行判断卡牌类型（攻击、技能、能力等）设置，建议写在基类里。
        // 如果使用自定义卡池，需要改下material，看添加人物章节的添加卡池部分
        // FramePath: "", // 卡牌背景
        // PortraitBorderPath: "", // 边框（状态牌感染使用的）
        // BannerTexturePath: "" // 横幅（不同类型）
    );
    protected override HashSet<CardTag> CanonicalTags => new() { };
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<WorkerrorPower>(),HoverTipFactory.FromPower<WorklockPower>() };

    public override IEnumerable<CardKeyword> CanonicalKeywords => new []{RainworldKeywords.Worklevel,CardKeyword.Innate};

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];

    public Rainworld_Liver_Workerror() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Character is Slugcat)
        {
           await SlugcatData.setworklevel(Owner, 0);
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature,
                (IsUpgraded ? 3 : 2) * (SlugcatData.GetMaxWorkLevel(Owner) + 1), Owner.Creature, this);
            await PowerCmd.Apply<WorkerrorPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
            await PowerCmd.Apply<WorklockPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
            SfxCmd.Play("event:/Rainworld/sfx/workerror");

        }
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
    }
}