using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 料敌机先：获得格挡，将1张「擒敌制胜」放入抽牌堆（对应塔1 Monk_Fight2，衍生牌）。
/// 只存在于战斗中的衍生牌，登记在 TokenCardPool，不进任何奖励卡池。
/// 卡牌预览只做单层（FromCard），见 <see cref="Rainworld_Monk_Fight1"/> 的说明。
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public class Rainworld_Monk_Fight2 : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        "res://Resource/Card/Monk/rainworld_monk_fight2.png",
        FramePath: "res://Resource/Card/RedCat/CardSkill1024.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public override bool GainsBlock => true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        new[] { HoverTipFactory.FromCard<Rainworld_Monk_Fight1>(IsUpgraded) };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(9m, ValueProp.Move)
    ];

    public Rainworld_Monk_Fight2() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        // 强化过的「料敌机先」给强化过的「擒敌制胜」（同塔1：生成前把预览牌升级）。
        Rainworld_Monk_Fight1 card = CombatState!.CreateCard<Rainworld_Monk_Fight1>(Owner);
        if (IsUpgraded && card.IsUpgradable)
            CardCmd.Upgrade(card);
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, Owner, CardPilePosition.Random);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
