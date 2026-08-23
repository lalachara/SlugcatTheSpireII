﻿using MegaCrit.Sts2.Core.Entities.Cards;
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
using MegaCrit.Sts2.Core.ValueProps;
using System.Threading.Tasks;

#nullable enable
[RegisterPower]
public sealed class DonttryPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/River/donttry.png",
        BigIconPath: "res://Resource/Powers/River/donttry.png"
    );

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromPower<IcewaterPower>() };

    public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
    {
        if (creature != Owner || cardSource==null )
            return;

        await PowerCmd.Apply<IcewaterPower>(new BlockingPlayerChoiceContext(), Owner, Amount, Owner, null);
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (dealer == Owner || props == ValueProp.Unpowered||target!=Owner)
            return;

        var icewaterAmt = Owner.GetPowerAmount<IcewaterPower>();
        if (icewaterAmt > 0&&dealer!.IsAlive)
        {
            await CreatureCmd.Damage(choiceContext, dealer, (decimal)icewaterAmt, ValueProp.Unpowered, Owner);
        }
    }
}