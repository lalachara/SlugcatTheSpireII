using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.River.Skills;

public class Rainworld_River_Skyzed : RiverCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromKeyword(RainworldKeywords.Lightning),HoverTipFactory.FromPower<CorrodePower>() };
        
    
    public Rainworld_River_Skyzed() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        while (cardPlay.Target.HasPower<CorrodePower>() && !cardPlay.Target.IsDead)
        {
            await Tools.Yinlei(cardPlay,choiceContext);
            for (int i = 0; i < 4; i++)
            {
                if(cardPlay.Target.HasPower<CorrodePower>())
                    await PowerCmd.Decrement(cardPlay.Target.GetPower<CorrodePower>());  // 第二次 Decrement ← 这里崩了！
            }
           
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}