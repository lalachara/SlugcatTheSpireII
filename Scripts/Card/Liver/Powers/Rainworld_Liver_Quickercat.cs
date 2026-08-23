using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Liver.Attack;

public class Rainworld_Liver_Quickercat : LiverCardModel

{
    // 基础耗能
    private const int energyCost = 2;

    // 卡牌类型
    private const CardType type = CardType.Power;

    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Rare;

    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;

    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    protected override HashSet<CardTag> CanonicalTags => new() { };
    //public override IEnumerable<CardKeyword> CanonicalKeywords => new []{};

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("Replay", 2m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        new[] { HoverTipFactory.FromCard<Rainworld_Liver_Strike>(),HoverTipFactory.Static(StaticHoverTip.ReplayStatic) };

    public Rainworld_Liver_Quickercat() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (CardModel card in base.Owner!.PlayerCombatState.AllCards)
        {
            if (card.Keywords.Contains(RainworldKeywords.CatStrike))
            {
                card.BaseReplayCount += DynamicVars["Replay"].IntValue;
            }
        }
        await PowerCmd.Apply<QuickercatPower>(choiceContext, Owner.Creature, DynamicVars["Replay"].IntValue,
            Owner.Creature, this);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars["Replay"].UpgradeValueBy(1);
    }
}