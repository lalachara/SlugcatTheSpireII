using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.RedCat;

public class Rainworld_Redcat_Silent : RedCatCardModel

{
    // 基础耗能
    private const int energyCost = 3;

    // 卡牌类型
    private const CardType type = CardType.Skill;

    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Rare;

    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AnyEnemy;

    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    protected override HashSet<CardTag> CanonicalTags => new() { };
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new []{HoverTipFactory.FromPower<SilentPower>()};
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust ,CardKeyword.Ethereal};


    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("Special", 2m)
    ];


    public Rainworld_Redcat_Silent() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // var list = cardPlay.Target.Powers.ToList();
        // foreach (var power in list)
        //     if (power.Type == PowerType.Buff && power is not SandpitPower&&power is not AdaptablePower && power is not ReattachPower)
        //     {
        //         await PowerCmd.Remove(power);
        //         
        //     }
        
        await PowerCmd.Apply<SilentPower>(choiceContext, cardPlay.Target!,DynamicVars["Special"].BaseValue, Owner.Creature,this);
        

    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars["Special"].UpgradeValueBy(1m);
    }
}