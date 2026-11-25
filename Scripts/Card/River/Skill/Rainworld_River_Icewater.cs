// Rainworld_River_IceWater.cs

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.River.Skills;

public class Rainworld_River_Icewater : RiverCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override bool GainsBlock => true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<IcewaterPower>() };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(4m, ValueProp.Move)
    ];

    public Rainworld_River_Icewater() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var owner = Owner.Creature;

        // ① 先按正常流程「获得格挡」：走引擎的完整格挡管线
        //    （Hook.ModifyBlock 里的敏捷/格挡加成、BeforeBlockGained、AfterModifyingBlockAmount、
        //    格挡音效与特效都会正常触发），所以对格挡敏感的能力都会响应。
        //    以前直接用 DynamicVars.Block.BaseValue 自己算，等于绕开了这条管线，吃不到任何格挡加成。
        await CreatureCmd.GainBlock(owner, DynamicVars.Block, cardPlay);

        // ② 再把「现有格挡 + 刚获得的格挡」全部转成冰晶的减伤层（和卡面描述一致）。
        decimal total = owner.Block;
        if (total <= 0m)
            return;

        await CreatureCmd.LoseBlock(choiceContext, owner, total, owner);
        await PowerCmd.Apply<IcewaterPower>(choiceContext, owner, total, owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}