using System.Collections;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Goldcat : LiverCardModel

{
    // 基础耗能
    private const int energyCost = 0;

    // 卡牌类型
    private const CardType type = CardType.Skill;

    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Uncommon;

    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;

    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    protected override HashSet<CardTag> CanonicalTags => new() { };
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword( RainworldKeywords.Food )};

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust};

    protected override bool IsPlayable => Owner.Character is Slugcat && SlugcatData.GetFood(Owner) >= 1;


    // 卡牌的基础属性（例如这里是12点伤害）
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];

    public Rainworld_Liver_Goldcat() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Character is Slugcat && SlugcatData.GetFood(Owner) >= 1) await SlugcatData.addfood(Owner, -1);

        var debuffs = new List<PowerModel>();
        foreach (var power in Owner.Creature.Powers)
            if (power.Type == PowerType.Debuff&&power.StackType == PowerStackType.Counter)
                debuffs.Add(power);

        if (debuffs.Count > 0)
        {
            if (debuffs.Count == 1)
                await PowerCmd.Remove(debuffs[0]);
            else
                await PowerCmd.Remove(Owner.RunState.Rng.Shuffle.NextItem(debuffs));
        }
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}