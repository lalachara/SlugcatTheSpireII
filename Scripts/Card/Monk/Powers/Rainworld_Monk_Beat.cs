using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts.Powers;
// 0.113 游戏本体新增了 MegaCrit.Sts2.Core.Models.Powers.TunePower，与本 mod 的协调能力同名，这里显式指定
using TunePower = Rainworld.Scripts.Powers.TunePower;

namespace Rainworld.Scripts.Card.Monk;

/// <summary>
/// 捆仙绳（替代原「敲打」）：选择1名盟友，把持有者的协调持续转化为其力量与敏捷；随后获得1层协调。
/// 链接能力挂在<b>被绑定的盟友</b>身上（图标显示在盟友头顶），施放者记录在能力的 Applier 里。
/// 绑定后每次获得/失去协调都会同步增减；已经绑过时再次打出只增加协调，不会重复绑定。
/// </summary>
public class Rainworld_Monk_Beat : MonkCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyAlly;
    private const bool shouldShowInCardLibrary = true;

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[]
    {
        HoverTipFactory.FromPower<TunePower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>()
    };

    public Rainworld_Monk_Beat() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 已经绑过一次（链接能力挂在某个盟友头上）→ 只加协调，不重复绑第二条
        if (BeatLinkPower.FindFor(Owner.Creature) != null)
        {
            await PowerCmd.Apply<TunePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
            return;
        }

        // 链接能力施加在「被绑定的盟友」身上：图标挂在盟友头顶，施放者记在 Applier 里
        await PowerCmd.Apply<BeatLinkPower>(choiceContext, cardPlay.Target, 1m, Owner.Creature, this);
        if (cardPlay.Target.GetPower<BeatLinkPower>() is not { } link)
            return;

        link.Bind();
        await link.SyncInitial(choiceContext);

        // 获得1层协调：BeatLinkPower 的层数监听会把这份协调同步给盟友。
        await PowerCmd.Apply<TunePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
