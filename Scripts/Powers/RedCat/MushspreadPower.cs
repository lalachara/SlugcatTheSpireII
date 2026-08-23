using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
[RegisterPower]
public sealed class MushspreadPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/RedCat/MushspreadPower128.png",
        BigIconPath: "res://Resource/Powers/RedCat/MushspreadPower128.png"
    );
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[]
        { HoverTipFactory.FromPower<MushblockPower>() };


    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != base.Owner.Player)
            return;
        var mushblock = base.Owner.GetPower<MushblockPower>();
        if (mushblock == null )
            return;
        Flash();
        await PowerCmd.Apply<MushblockPower>(choiceContext, base.Owner, (mushblock.Amount / 2) +1, base.Owner, null);
    }
}