using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;
using Rainworld.Scripts.Card.CardVars;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Workerror:LiverCardModelPower

{
    // 基础耗能
    private const int energyCost =0;
    // 卡牌类型
    private const CardType type = CardType.Power;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Rare;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    protected override bool ShouldGlowRedInternal => true;

    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> {  };
    //public override IEnumerable<CardKeyword> CanonicalKeywords => new []{};
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
    ];
    
    public Rainworld_Liver_Workerror() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Character is Slugcat)
        {
            SlugcatField.playerdata.setworklevel(0);
            await PowerCmd.Apply<StrengthPower>( Owner.Creature, (IsUpgraded ? 3 : 2)*(SlugcatField.GetSlugCatDataByCreature(Owner.Creature).maxWorkLevel+1), Owner.Creature, this);
            await PowerCmd.Apply<WorkerrorPower>( Owner.Creature, 1, Owner.Creature, this);
            await PowerCmd.Apply<WorklockPower>( Owner.Creature, 1, Owner.Creature, this);

        }

    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
    }
}