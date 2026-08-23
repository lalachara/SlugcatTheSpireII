using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
[RegisterPower]
public sealed class WetPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Resource/Powers/River/{GetType().Name}128.png",
        BigIconPath: $"res://Resource/Powers/River/{GetType().Name}128.png"
    );

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    private bool drawToggle = false;

    public async Task ReduceAdroit(PlayerChoiceContext choiceContext, int reduceAmount)
    {
        if (reduceAmount > 20)
        {
            await CardPileCmd.Draw(choiceContext, 10, Owner.Player!);
            return;
        }

        if (reduceAmount > 2)
        {
            await CardPileCmd.Draw(choiceContext, reduceAmount / 2, Owner.Player!);
            reduceAmount %= 2;
        }

        while (reduceAmount > 0)
        {
            if (drawToggle)
            {
                drawToggle = false;
                await CardPileCmd.Draw(choiceContext, 1, Owner.Player!);
            }
            else
            {
                drawToggle = true;
            }

            reduceAmount--;
        }
    }
}