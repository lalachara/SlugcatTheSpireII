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
using MegaCrit.Sts2.Core.ValueProps;
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
    public static bool needmeow = false;
    public static List<PowerModel> Powers = new List<PowerModel>();
    public RunSingleton() : base(HookType.Run)
    {
    }

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        

            foreach (var player in (CurrentRunState as RunState)?.Players ?? Enumerable.Empty<Player>())
            {
                if (player.Character is Slugcat)
                {
                    if(player.GetRelic<Worklevelrepair>()==null)
                    {
                       await RelicCmd.Obtain<Worklevelrepair>(player);
                    }
                    else
                    {
                        player.GetRelic<Worklevelrepair>()!.Initialize();
                    }
                }
                
            }
        
        
        Powers.Clear();
        needmeow = true;
        if (room.RoomType != RoomType.Monster && room.RoomType != RoomType.Elite && room.RoomType != RoomType.Boss)
        {
            foreach (var player in (CurrentRunState as RunState)?.Players ?? Enumerable.Empty<Player>())
            {
                if(player.Character is Slugcat&&!player.Relics.Any(model => model is Redmushroom))
                    await SlugcatData.addfood(player,((Slugcat)player.Character).roomfood);
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
        // 只做决策判定（是否采纳该修改），不在此处执行副作用（addworklevel 含治疗，
        // 属结算后行为；放在 Try* 里可能被预览/多次调用反复触发）。
        // 真正的业力+1 副作用已移到 AfterRestSiteHeal（结算后只触发一次）。
        return player.Character is Slugcat;
    }

    public override async Task AfterRestSiteHeal(Player player, bool isMimicked)
    {
        if (player.Character is Slugcat)
            await SlugcatData.addworklevel(player, 1);
        await base.AfterRestSiteHeal(player, isMimicked);
    }
    
    public override bool ShouldDie(Creature creature)
    {
        return creature.Player?.Character is not Slugcat || SlugcatData.canbekill(creature.Player);
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        await SlugcatData.reLive(creature.Player!);
        if(creature.HasPower<SleepPower>()&&creature.Player?.GetRelic<Liver_Sleeppotion>()==null)
            await PowerCmd.Remove<SleepPower>(creature);
        await DispatchKarmaReviveDamageHooks(creature);
    }

    /// <summary>
    /// 蛞蝓猫因“不可被杀”被业力复活后，原版对致命伤害（<c>WasTargetKilled=true</c>）不派发
    /// <see cref="Creature.LoseHpInternal"/> 之后的受伤 hook，导致部分受击类 power 不触发。
    /// 这里仅对三类“只看 target==Owner / UnblockedDamage&gt;0 / props 非 Unpowered、不读受击后血量、
    /// 不弹玩家选择”的安全 power 手动补发，避免走原版派发器带来的端口风险。
    /// </summary>
    private async Task DispatchKarmaReviveDamageHooks(Creature creature)
    {
        if (creature.Player is not { } player || player.Character is not Slugcat || SlugcatData.canbekill(player))
            return;
        if (creature.CombatState is not { } combat || !combat.IsLiveCombat())
            return;

        const ValueProp props = ValueProp.Move;
        var result = new DamageResult(creature, props)
        {
            UnblockedDamage = 1,    // 这三类 power 只判 >0，不读实际伤害值
            WasTargetKilled = true, // 本是一次致命一击
            OverkillDamage = 0,
        };
        var ctx = new ThrowingPlayerChoiceContext();

        // 先做一次同步快照，避免 await 期间 creature.Powers 集合被增删
        // （power 生效/消耗后自我移除）导致“Collection was modified”枚举异常。
        var targets = creature.Powers
            .Where(power => power is FairyformPower or FlexibilityPower or TubePower)
            .ToArray();
        foreach (var power in targets)
            await power.AfterDamageReceived(ctx, creature, result, props, null, null);
    }

    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if(Powers.Count==0)
            return false;
        foreach (var power in Powers)
        {
            if (power is BypassPower bypassPower && bypassPower.Owner.Player == player)
            {
                rewards.Add(bypassPower.RelicReward);
                // 业力+1 副作用（含治疗）挪到 AfterModifyingRewards，避免在 Try*（决策阶段）被预测/多次调用触发。
            }
                
        }
        return true;
    }

    public override async Task AfterModifyingRewards()
    {
        foreach (var power in Powers)
        {
            if (power is BypassPower bypassPower && bypassPower.Owner.Player is { } p && p.Character is Slugcat)
                await SlugcatData.addworklevel(p, 1);
        }
        await base.AfterModifyingRewards();
    }


}