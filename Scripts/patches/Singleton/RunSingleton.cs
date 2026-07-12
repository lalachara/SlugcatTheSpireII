﻿using Godot;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Rainworld.relics;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;
using SlugcatTheSpireII.Scripts.patches.CampRoomPatch;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace SlugcatTheSpireII.Scripts.patches.Singleton;

[RegisterSingleton]
public class RunSingleton: HookedSingletonModel
{
    
    public static List<PowerModel> Powers = new List<PowerModel>();
    public RunSingleton() : base(HookType.Run)
    {
        GD.Print("[LogSingleton] 构造函数被调用");
    }

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        Powers.Clear();

        if (room.RoomType != RoomType.Monster && room.RoomType != RoomType.Elite && room.RoomType != RoomType.Boss)
        {
            foreach (var player in (CurrentRunState as RunState)?.Players ?? Enumerable.Empty<Player>())
            {
                if(player.Character is Slugcat&&!player.Relics.Any(model => model is Redmushroom))
                    SlugcatData.addfood(player,((Slugcat)player.Character).roomfood);
                // 处理每个玩家
            }
        }
        await base.AfterRoomEntered(room);
    }
    
    public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
    {
        if (player.Character is not Slugcat||SlugcatData.GetChaosheng(player))
            return false;
        options.Add((RestSiteOption) new RainworldShelterOption(player));
        return true;
    }

    public override async Task AfterActEntered()
    {
        foreach (var player in (CurrentRunState as RunState)?.Players ?? Enumerable.Empty<Player>())
        {
            if(player.Character is Slugcat&&!player.Relics.Any(model => model is Redmushroom))
                SlugcatData.setchaosheng(player,false);
            // 处理每个玩家
        }
        await base.AfterActEntered();
    }
    
    public override bool TryModifyRestSiteHealRewards(Player player, List<Reward> rewards, bool isMimicked)
    {

        if (player.Character is Slugcat)
        {
            SlugcatData.addworklevel(player,1);
            return true;
        }
        return false;

    }
    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if(Powers.Count==0)
            return false;
        foreach (var power in Powers)
        {
            if(power is BypassPower bypassPower&&bypassPower.Owner.Player==player)
                rewards.Add(bypassPower.RelicReward);
                if(power.Owner.Player?.Character is  Slugcat)
                    SlugcatData.addworklevel(power.Owner.Player,1);
        }
        return true;
    }


}