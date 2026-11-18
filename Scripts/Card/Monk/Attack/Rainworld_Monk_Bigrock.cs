using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Monk;

public class Rainworld_Monk_Bigrock : MonkCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[]
    {
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<FreespearPower>()
    };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12m, ValueProp.Move)
    ];

    public Rainworld_Monk_Bigrock() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 原版「下砸」的落地表现：目标脚下先溅起尖刺，再打出从底部生成的重击特效
        if (cardPlay.Target.GetCreatureNode() != null)
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NSpikeSplashVfx.Create(cardPlay.Target));

        var result = await DamageCmd
            .Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_heavy_blunt", null, "heavy_attack.mp3")
            .WithHitVfxSpawnedAtBase()
            .Execute(choiceContext);
        var unblockedDamage = result.Results.SelectMany(r => r).Sum(r => r.UnblockedDamage);
        if (unblockedDamage > 0)
            await PowerCmd.Apply<BigrockTempStrength>(choiceContext, cardPlay.Target,
                (int)unblockedDamage, Owner.Creature, this);
        await PowerCmd.Apply<FreespearPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}