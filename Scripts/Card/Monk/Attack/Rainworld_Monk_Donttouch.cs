using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 别碰！：造成伤害，然后选择 1 张手牌，其在本回合获得保留。
/// 已有保留的牌、虚无牌、<b>诅咒牌和状态牌</b>不可选；只剩 1 张可选时直接生效（与塔1口径一致）。
/// </summary>
public class Rainworld_Monk_Donttouch : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(CardKeyword.Retain) };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move)
    ];

    public Rainworld_Monk_Donttouch() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 战斗已经结束就不弹选牌了（塔1 里战斗结束会清空行动队列，选择动作不会执行）
        if (CombatManager.Instance.IsOverOrEnding)
            return;

        var candidates = PileType.Hand.GetPile(Owner).Cards.Where(RetainFilter).ToList();
        if (candidates.Count == 0)
            return;
        // 与塔1一致：只剩 1 张可选时直接给它保留，不再打开选择界面
        if (candidates.Count == 1)
        {
            if(IsUpgraded)
                CardCmd.ApplyKeyword(candidates[0], CardKeyword.Retain);
            else
                CardCmd.ApplySingleTurnRetain(candidates[0]);
            return;
        }

        var selected = await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(SelectionScreenPrompt, 1),
            context: choiceContext, player: Owner, filter: RetainFilter, source: this);
        foreach (var card in selected)
        {
            if(IsUpgraded)
                CardCmd.ApplyKeyword(card, CardKeyword.Retain);
            else
                CardCmd.ApplySingleTurnRetain(card);
        }

    }

    /// <summary>
    /// 不可选的牌：已有保留（含本回合保留）、虚无、<b>诅咒</b>和<b>状态</b>牌。
    /// 给诅咒/状态牌保留没有意义（它们本来就不该被留住），所以直接从候选里排除。
    /// </summary>
    private static bool RetainFilter(CardModel card)
        => !card.ShouldRetainThisTurn
           && !card.Keywords.Contains(CardKeyword.Ethereal)
           && card.Type != CardType.Curse
           && card.Type != CardType.Status;

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}