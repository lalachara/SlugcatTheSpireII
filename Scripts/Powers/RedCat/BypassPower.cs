using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;
using SlugcatTheSpireII.Scripts.patches.Singleton;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;


using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
[RegisterPower]
public sealed class BypassPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/RedCat/Bypasspower128.png",
        BigIconPath: "res://Resource/Powers/RedCat/Bypasspower128.png"
    );
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override PowerType Type => PowerType.Buff;
    public required Reward RelicReward;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        RunSingleton.Powers.Add(this);
        RelicReward = new RelicReward(Owner.Player!).SetRng(Owner.Player!.RunState.Rng.TreasureRoomRelics);
        await base.AfterApplied(applier, cardSource);
    }

    public override async Task AfterSideTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player)
            await PowerCmd.Decrement(this);
        // if (side == CombatSide.Player && CombatState.RoundNumber >= (upgrade ? 4 : 3))
        // {
        //     await PowerCmd.Remove(this);
        // }
        await base.AfterSideTurnEndLate(choiceContext, side, participants);
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        RunSingleton.Powers.Remove(this);
        await base.AfterRemoved(oldOwner);
    }
    


}