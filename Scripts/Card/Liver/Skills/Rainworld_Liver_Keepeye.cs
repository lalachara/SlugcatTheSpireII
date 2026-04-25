using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Keepeye:LiverCardModel

{
    // 基础耗能
    private const int energyCost = 1;
    // 卡牌类型
    private const CardType type = CardType.Skill;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Common;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AnyEnemy;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { };
    public override IEnumerable<CardKeyword> CanonicalKeywords => new []{RainworldKeywords.Huntsign};


    
    public Rainworld_Liver_Keepeye() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, 1, base.Owner);
        await PowerCmd.Apply<HuntsignPower>(choiceContext,cardPlay.Target, 3, base.Owner.Creature, this);

    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}