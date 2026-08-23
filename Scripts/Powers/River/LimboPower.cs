using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;


using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
[RegisterPower]
public sealed class LimboPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Resource/Powers/River/{GetType().Name}128.png",
        BigIconPath: $"res://Resource/Powers/River/{GetType().Name}128.png"
    );

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<AdroitcapPower>() ,HoverTipFactory.FromPower<AdroitPower>() };

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult _, ValueProp props, Creature? dealer, CardModel? __)
    {
        if (target != Owner || dealer == null || props == ValueProp.Unpowered)
            return;

        if (_.UnblockedDamage == 0)
        {
            await PowerCmd.Apply<AdroitcapPower>(choiceContext, Owner, (int)Amount, Owner, null);
        }
        else
        {
            if (Owner.HasPower<AdroitPower>())
            {
                var adroit = (AdroitcapPower)Owner.GetPower<AdroitcapPower>()!;
                if (adroit.Amount > 0)
                {
                    await PowerCmd.Decrement(adroit);
                }
            }
        }
    }
}