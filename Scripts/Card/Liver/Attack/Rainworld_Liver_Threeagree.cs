using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.CardVars;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Threeagree:LiverCardModel

{
    // 基础耗能
    private const int energyCost = 3;
    // 卡牌类型
    private const CardType type = CardType.Attack;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Rare;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AllEnemies;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;
    
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { };
    public override IEnumerable<CardKeyword> CanonicalKeywords => new []{CardKeyword.Retain,CardKeyword.Exhaust,RainworldKeywords.Worklevel};


    protected override IEnumerable<DynamicVar> CanonicalVars => [
    ];
    protected override bool IsPlayable => Owner.Character is Slugcat&&SlugcatField.GetSlugCatData[Owner.Creature].workLevel>=9;

    public Rainworld_Liver_Threeagree() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }


    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        if (!(Owner.Character is Slugcat && SlugcatField.GetSlugCatData[Owner.Creature].workLevel >= 9))
        {
            return;
        }

        if (base.CombatState.HittableEnemies.Count == 0)
        {
            return;
        }
        foreach (Creature creature in base.CombatState.HittableEnemies)
        {
            await CreatureCmd.Kill(creature);
        }

        SlugcatField.GetSlugCatData[Owner.Creature].setworklevel(4); 
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}