using BaseLib;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using Rainworld.Scripts.Powers;
using Rainworld;
using Rainworld.Patches;
using Rainworld.relics;
using Rainworld.Scripts.patches;
using SlugcatTheSpireII.Scripts.patches;

namespace Rainworld.Scripts
{
    /// <summary>
    /// Slugcat 专属数据类（纯数据 + 逻辑）
    /// </summary>
    public class SlugcatData
    {
        // 你的状态变量
        public int workLevel = 0;
        public int maxWorkLevel = 0;
        public int food = 0;
        public int maxFood = 0;
        public int sleepFood = 99;
        public int sleepCD = 0;
        public NAutoButton Nbutton;

        // 你的逻辑方法
        public Player Player;
        public WorkLevelIndicator workLevelIndicator;

        public SlugcatData(Player player)
        {
            this.Player = player;
            Initialize();

        }

       
        public void Initialize()
        {
           

            try
            {
                if (Player.Character is Slugcat slugcat)
                {
                    //
                    // workLevel = slugcat.workLevel;
                    // maxWorkLevel = slugcat.maxWorkLevel;
                    // food = slugcat.food;
                    // maxFood = slugcat.maxFood;
                    // sleepFood = slugcat.sleepfood;
                    if (!RainworldData.Current.isinit)
                    {
                        workLevel = slugcat.workLevel;
                        maxWorkLevel = slugcat.maxWorkLevel;
                        food = slugcat.food;
                        maxFood = slugcat.maxFood;
                        sleepFood = slugcat.sleepfood;
                        SaveDataToCharacter();
                    }
                    else
                    {
                        workLevel = RainworldData.Current.WorkLevel;
                        maxWorkLevel = RainworldData.Current.MaxWorkLevel;
                        food = RainworldData.Current.Food;
                        maxFood = RainworldData.Current.MaxFood;
                        sleepFood = RainworldData.Current.SleepFood;
                    }

                

                }
            }
            catch (Exception e)
            {
                GD.PrintErr("SlugCatDataInitialize报错"+e);
                throw;
            }
           
        }

        public void SaveDataToCharacter()
        {
        
            if (Player.Character is Slugcat)
            {
                RainworldData.Current.WorkLevel =  workLevel;
                RainworldData.Current.MaxWorkLevel = maxWorkLevel;
                RainworldData.Current.Food = food;
                RainworldData.Current.MaxFood = maxFood;
                RainworldData.Current.SleepFood = sleepFood;
                RainworldData.Current.isinit = true;
            }
        }
        
        public bool canbekill()
        {
            if (Player.Creature.HasPower<WorklockPower>())
                return false;
            return workLevel <= 0;
        }

        public async void addfood(int amount)
        {
            int tempfood = food;
            //音效
            //bool full = food==maxFood;
            food+=amount;
            int exfood = 0;
            // if(!full)  饱食度增加的特效文本，之后补上
            // 	AbstractDungeon.effectsQueue.add(new TextAboveCreatureEffect(this.hb.cX - this.animX, this.hb.cY, characterStrings.TEXT[2] + Integer.toString(amount), Settings.GREEN_TEXT_COLOR));
            if(food>maxFood)
            {
                exfood = food-maxFood;
                
                if(Player.Creature.HasPower<FatworldPower>())
                    await PowerCmd.Apply<StrengthPower>(Player.Creature, exfood, Player.Creature, null);
            }
            if(food<0)
                food=0;
            food = Math.Min(food,maxFood);
            if (food != tempfood)
            {
                if(sleepCD==0)
                    CombatUiPatch.SetSleepButton(cansleep());
                CombatUiPatch.Setfood(food);
            }
        }

        public void addMaxWorkLevel(int amount)
        {
            int temp =  maxWorkLevel+amount;
            if (temp > 9)
                temp = 9;
            maxWorkLevel = Math.Max(temp,0);
            if(workLevel>maxWorkLevel)
                setworklevel(maxWorkLevel);
        }

        public async void reLive()
        {
            if(Player.Creature.HasPower<WorklockPower>())
                await PowerCmd.Decrement(Player.Creature.GetPower<WorklockPower>());
            else
                this.addworklevel(-1);
        }
        public void addworklevel(int level)
        {
            if(level>0&&Player.Creature.HasPower<WorkerrorPower>())
                return;
            int result = workLevel+level;
            setworklevel(result);
        }
        public void setworklevel(int level,Boolean show = true)
        {
            if(level>maxWorkLevel)
                level=maxWorkLevel;
            if(level<0)
                level=0;
            workLevel=level;
            if (workLevelIndicator != null&&show&&Player.RunState.CurrentRoom is CombatRoom)
            {
                
                workLevelIndicator.UpdateWorkLevel(workLevel);
                Player.Creature.SetCurrentHpInternal(Player.Creature.MaxHp);
            }

        }
        

        public bool cansleep()
        {
            
            return sleepCD==0&&food >= sleepFood;
        }

        public  void trysleep()
        {
            if ( cansleep())
            {
                sleepCD = 3;
                addfood(-sleepFood);
                sleep();
                
            }
        }

        public async void sleep()
        {
            await PowerCmd.Apply<SleepPower>(Player.Creature,1, Player.Creature, null);
            

            NCreature creatureNode = NCombatRoom.Instance?.GetCreatureNode(Player.Creature);
            if (creatureNode != null)
            {
                creatureNode.SetAnimationTrigger("sleep");
            }

            PlayerCmd.EndTurn(Player, canBackOut: false);

        }

        public void callTurnStart()
        {
            if (Player.Creature.CombatState == null)
            {
                Player = CombatUiPatch.creature.Player;
            }

            if (Player.Creature.CombatState?.CurrentSide == CombatSide.Player)
            {
                
                if (sleepCD > 0)
                            {
                                sleepCD--;
                                if (Player.Creature.CombatState.RoundNumber == 1)
                                    sleepCD = 0;
                                if(sleepCD == 0)
                                    CombatUiPatch.SetSleepButton(cansleep());
                                if (sleepCD == 1)
                                    CombatUiPatch.SetSleepButtonDown1();
                            }
                            else
                            {
                                CombatUiPatch.SetSleepButton(cansleep());
                            }
            }
            else
            {
                GD.PrintErr("非玩家回合");
            }

            

        }

        public void getRewordFood(RoomType roomType)
        {
            
            if(roomType == RoomType.Monster)
                addfood(3);
            if(roomType == RoomType.Elite)
                addfood(5);
            if (roomType == RoomType.Boss)
            {
                addfood(7); ;
                addworklevel(1);
                addMaxWorkLevel(1);
            }

        }
        

        public void callVictory()
        {
            sleepCD = 0;
            getRewordFood(Player.RunState.CurrentRoom.RoomType);
        }
    }
}