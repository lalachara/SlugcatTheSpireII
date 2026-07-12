using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.relics;

[RegisterRelic(typeof(Rainworld_River_RelicPool))]
public sealed class River_Cream : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Relics/Cream.png",
        IconOutlinePath: "res://Resource/Relics/outline/Cream.png",
        BigIconPath: "res://Resource/Relics/Cream.png"
    );

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<CorrodePower>() };

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal delta,
        Creature source,
        CardModel? cardSource)
    {
        if (power is not CorrodePower || power.Owner != Owner.Creature || delta <= 0)
            return;

        var debuffs = Owner.Creature.Powers
            .Where(p => p.Type == PowerType.Debuff && p != power)
            .ToList();

        if (debuffs.Count == 0)
            return;

        var toRemove = debuffs[Owner.RunState.Rng.CombatTargets.NextInt(debuffs.Count)];
        await PowerCmd.Remove(toRemove);
    }
}