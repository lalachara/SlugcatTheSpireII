﻿﻿﻿using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Patches;
using Rainworld.Scripts.Powers;
using Rainworld;
using Rainworld.Patches;
using Rainworld.relics;
using Rainworld.Scripts.patches;
using SlugcatTheSpireII.Scripts;
using SlugcatTheSpireII.Scripts.patches;
using STS2RitsuLib.Combat.SecondaryResources;

namespace Rainworld.Scripts
{
    public class SlugcatData
    {
        public static int GetWorkLevel(Player player)
        {
            return SlugcatResources.Data.Get(player).WorkLevel;
        }
        public static bool GetChaosheng(Player player)
        {
            return SlugcatResources.Data.Get(player).HaveChaoshengt;
        }

        public static void setchaosheng(Player player,bool  chaosheng)
        {
            SlugcatResources.Data.Modify(player, d => d.HaveChaoshengt = chaosheng);
        }

        public static int GetFood(Player player)
        {
            return SlugcatResources.Data.Get(player).Food;
        }
        public static int GetMaxFood(Player player)
        {
            return SlugcatResources.Data.Get(player).MaxFood;
        }

        public static int GetSleepFood(Player player)
        {
            return SlugcatResources.Data.Get(player).SleepFood;
        }

        public static int GetMaxWorkLevel(Player player)
        {
            return SlugcatResources.Data.Get(player).MaxWorkLevel;
        }

        public SlugcatData(Creature creature)
        {
            if (creature.Player == null)
                return;
        }

        public static void Initialize(Player player)
        {
            try
            {
                if (player?.Creature == null || player.Character is not Slugcat slugcat)
                {
                    GD.Print("Player或Creature不是Slugcat");
                    return;
                }

                var data = SlugcatResources.Data.Get(player);

                if (!data.IsInitialized)
                {
                    GD.Print($"首次初始化 {player}，hash={player.GetHashCode()}，从角色类读取初始值");
                    SlugcatResources.Data.Modify(player, d =>
                    {
                        d.WorkLevel = slugcat.workLevel;
                        d.MaxWorkLevel = slugcat.maxWorkLevel;
                        d.Food = slugcat.food;
                        d.MaxFood = slugcat.maxFood;
                        d.SleepFood = slugcat.sleepfood;
                        d.IsInitialized = true;
                    });
                }

                var finalData = SlugcatResources.Data.Get(player);
                GD.Print($"初始化完成{player}，hash={player.GetHashCode()}，" +
                    $"workLevel={finalData.WorkLevel}，food={finalData.Food}，" +
                    $"maxWorkLevel={finalData.MaxWorkLevel}，maxFood={finalData.MaxFood}，" +
                    $"sleepFood={finalData.SleepFood}");
            }
            catch (Exception e)
            {
                GD.PrintErr("SlugCatDataInitialize报错" + e);
            }
        }

        public static bool canbekill(Player player)
        {
            if (player.Creature.HasPower<WorklockPower>())
                return false;
            if(player.Creature.HasPower<RedmushroomPower>()&&!player.Creature.HasPower<RefusePower>())
                return true;
            return SlugcatResources.Data.Get(player).WorkLevel <= 0;
        }

        public static async Task addfood(Player player, int amount)
        {
            GD.Print($"添加食物{amount}，hash={player.GetHashCode()}");
            var data = SlugcatResources.Data.Get(player);
            int currentFood = data.Food;
            int newFood = currentFood + amount;
            if(player.Creature == CombatUiPatch.creature)
                SfxCmd.Play("event:/Rainworld/sfx/foodup");
            int exfood = 0;
            if (newFood > data.MaxFood)
            {
                exfood = newFood - data.MaxFood;
                if (player.Creature.HasPower<FatworldPower>())
                    await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), player.Creature, exfood, player.Creature, null);
            }
            if (newFood < 0)
                newFood = 0;
            newFood = Math.Min(newFood, data.MaxFood);
            if (newFood != currentFood)
            {
                SlugcatResources.Data.Modify(player, d => d.Food = newFood);
                GD.Print($"存储food{newFood}");
                CombatUiPatch.Setfood(newFood,player);
            }
        }

        public static void addMaxWorkLevel(Player player, int amount)
        {
            var data = SlugcatResources.Data.Get(player);
            int temp = data.MaxWorkLevel + amount;
            if (temp > 9)
                temp = 9;
            temp = Math.Max(temp, 0);
            SlugcatResources.Data.Modify(player, d => d.MaxWorkLevel = temp);
            if (data.WorkLevel > temp)
                setworklevel(player, temp);
        }

        public async static Task reLive(Player player)
        {
            if(player.GetRelic<Redmushroom>()!=null)
                return;
            if(player.Creature.HasPower<RefusePower>())
                await setCorruptValue(player, 0);

            if (player.Creature.HasPower<WorklockPower>())
            {
                await CreatureCmd.Heal(player.Creature,player.Creature.MaxHp,false);
                await PowerCmd.Decrement(player.Creature.GetPower<WorklockPower>());

            }

            else
                addworklevel(player, -1);
            Tools.WorkReport(player);
            //
        }

        public async static Task addworklevel(Player player, int level)
        {
            if(player.GetRelic<Redmushroom>()!=null)
                return;
            if(player.Creature == CombatUiPatch.creature&&level>0)
                SfxCmd.Play("event:/Rainworld/sfx/worklevelup");
            if (player.Creature == CombatUiPatch.creature&&level<0)
                SfxCmd.Play("event:/Rainworld/sfx/workleveldown");
            await CreatureCmd.Heal(player.Creature,player.Creature.MaxHp-player.Creature.CurrentHp,false);
            if (level > 0 && player.Creature.HasPower<WorkerrorPower>())
                return;
            int result = SlugcatResources.Data.Get(player).WorkLevel + level;
            await setworklevel(player, result);
        }

        public async static Task setworklevel(Player player, int level, Boolean show = true)
        {
            if(player.GetRelic<Redmushroom>()!=null)
                return;
            var data = SlugcatResources.Data.Get(player);
            if (level > data.MaxWorkLevel)
            {
                level = data.MaxWorkLevel;
               

            }

            if (level < 0)
            {
                level = 0;
                await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), player.Creature, 9999,ValueProp.Move, player.Creature);
            }
            SlugcatResources.Data.Modify(player, d => d.WorkLevel = level);
            CombatUiPatch.SetWorklevel(level, player);

        }

        public static bool cansleep(Player player)
        {
            if(player.GetRelic<Redmushroom>()!=null)
                return false;
            var data = SlugcatResources.Data.Get(player);
            return data.Food >= data.SleepFood&&player.Creature.IsAlive&&!player.Creature.HasPower<FakedeathPower>() ;
        }

        public static void trysleep(Player player)
        {
            if (cansleep(player))
            {
                
                sleep(player);
            }
        }
        public static int getCorruptValue(Player player)
        {
            return SecondaryResourceCmd.Get(player, SlugcatResources.CorruptValueID);
        }
        public async static Task setCorruptValue(Player player, int value)
        {
            int old = getCorruptValue(player);
            if (value < 0)
                value = 0;
            if (value > 5)
            {   //畸变形态加力量
                if(player.Creature.HasPower<TwistformPower>())
                {
                    await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), player.Creature, (value-5)*player.Creature.GetPowerAmount<TwistformPower>(), player.Creature, null);
                }
                value = 5;
            }
            
            
            
            //融合逻辑
            if (old != value && player.Creature.HasPower<MixPower>())
            {
                if (old - value > 0)
                {
                    await PowerCmd.Apply<VigorPower>(new ThrowingPlayerChoiceContext(), player.Creature, (old-value)*player.Creature.GetPowerAmount<MixPower>(), player.Creature, null);
                }
                else
                {
                    await PowerCmd.Apply<MushblockPower>(new ThrowingPlayerChoiceContext(), player.Creature, (value-old)*player.Creature.GetPowerAmount<MixPower>(), player.Creature, null);
                }
            }

            //如果增加腐化，创造者之歌扣血
            if (value > old && player.Creature.HasPower<CreatesongPower>())
            {
                IEnumerable<DamageResult> damageResults = await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), player.Creature, player.Creature.GetPowerAmount<CreatesongPower>(),ValueProp.Unpowered, player.Creature);

            }

            if (value == 5 )
            {
                if (!player.Creature.HasPower<RedmushroomPower>())
                {
                     await PowerCmd.Apply<RedmushroomPower>(new ThrowingPlayerChoiceContext(), player.Creature, 1, player.Creature, null);
                     
                     GD.Print($"执行菌化{value}");
                }
                else
                {
                    GD.Print($"执行菌化失败，已有菌化{value}");
                }

               
                GD.Print($"GD增加腐化值{value}");
            }

            Log.Debug($"腐化值set{value}");

            //退出菌化
            if (value == 0)
            {
                if (player.Creature.HasPower<RedmushroomPower>())
                {
                    await PowerCmd.Remove<RedmushroomPower>(player.Creature);
                    // if (!player.Creature.HasPower<InveteracyPower>())
                    // {
                    //     await PowerCmd.Remove<CorruptStrengthPower>(player.Creature);
                    //     if(player.Creature.HasPower<MushblockPower>())
                    //         await CreatureCmd.GainBlock(player.Creature,player.Creature.GetPowerAmount<MushblockPower>(),ValueProp.Unpowered, null);
                    //     await PowerCmd.Remove<MushblockPower>(player.Creature);
                    // }
                }
            }



            await SecondaryResourceCmd.Set(player, SlugcatResources.CorruptValueID, value);
            CombatUiPatch.SetCorruptValue(value, player);
        }
        public async static Task addCorruptValue(Player player, int amount)
        {
            if(player.Creature.HasPower<RestrainPower>())
                amount=0;
            if(player.Creature.HasPower<TwistformPower>()&&amount>0)
            {
                amount*=2;
            }
            Log.Debug($"增加腐化值{amount}");

            await setCorruptValue(player, getCorruptValue(player) + amount);
        }
        
        public static async void sleep(Player player)
        {
            
            RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(
                new SleepAction(player));
            
        }
        
        public static void callTurnStart(Player player)
        {
        }

        public static async Task getRewordFood(Player player, RoomType roomType)
        {
            if (player.Character is Slugcat slugcat)
            {
                
                if (roomType == RoomType.Monster)
                  await  addfood(player, slugcat.enemyfood);
                if (roomType == RoomType.Elite)
                {
                  await  addfood(player, slugcat.jyFood);
                }
                if (roomType == RoomType.Boss)
                {
                   await addfood(player, slugcat.bossfood);
                  await  addworklevel(player, 1);
                    if(player.Character is Rainworld_Liver)
                        addMaxWorkLevel(player, 1);
                }
            }

            
        }

        public static async Task callVictory(Player player)
        {
           await getRewordFood(player, player.RunState.CurrentRoom.RoomType);
        }
    }
}