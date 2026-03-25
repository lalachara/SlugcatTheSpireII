using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Card.CardVars;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Strangebomb:LiverCardModelAtk

{
    // 基础耗能
    private const int energyCost = 4;
    // 卡牌类型
    private const CardType type = CardType.Attack;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Rare;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AllEnemies;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { };

    public override IEnumerable<CardKeyword> CanonicalKeywords => new []{CardKeyword.Exhaust,CardKeyword.Ethereal};

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HpLossVar(40m),
    ];

    public Rainworld_Liver_Strangebomb() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        foreach (Creature hittableEnemy in base.CombatState.HittableEnemies)
        {
            await CreatureCmd.Damage(choiceContext, hittableEnemy, base.DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this);
        }
        PlayerCmd.EndTurn(base.Owner, canBackOut: false);


    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars.HpLoss.UpgradeValueBy(20m);
    }
}