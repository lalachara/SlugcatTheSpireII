using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Laphurt : LiverCardModel

{
    // 基础耗能
    private const int energyCost = 1;

    // 卡牌类型
    private const CardType type = CardType.Skill;

    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Uncommon;

    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;

    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    protected override HashSet<CardTag> CanonicalTags => new() { };

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust};
    protected override bool IsPlayable => Owner.Character is Slugcat && SlugcatData.GetFood(Owner) >= 1;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        new[] { HoverTipFactory.FromPower<PlatingPower>() ,HoverTipFactory.FromKeyword(RainworldKeywords.Food) };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];

    public Rainworld_Liver_Laphurt() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Character is Slugcat && SlugcatData.GetFood(Owner) >= 1)
        {
            SlugcatData.addfood(Owner, -1);
            var temphp = Owner.Creature.MaxHp - Owner.Creature.CurrentHp;
            await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature, temphp, Owner.Creature, this);
            await CreatureCmd.Heal(Owner.Creature, temphp);
        }
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}