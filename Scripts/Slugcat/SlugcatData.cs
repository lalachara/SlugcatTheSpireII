using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts.Powers;

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
        // 你的逻辑方法
        public Creature creature;

        public SlugcatData(Creature creature)
        {
            this.creature = creature;
        }

        public void Initialize()
        {
            if (creature.Player.Character is Slugcat slugcat)
            {
                workLevel = slugcat.workLevel;
                maxWorkLevel = slugcat.maxWorkLevel;
                food = slugcat.food;
                maxFood = slugcat.maxFood;
                sleepFood = slugcat.sleepfood;
            }
        }

        public void SaveDataToCharacter()
        {
            if (creature.Player.Character is Slugcat slugcat)
            {
                 slugcat.workLevel =  workLevel;
                 slugcat.maxWorkLevel = maxWorkLevel;
                 slugcat.food = food;
                 slugcat.maxFood = maxFood;
                 slugcat.sleepfood = sleepFood;
            }
        }
        
        public bool canbekill()
        {
            if (creature.HasPower<WorklockPower>())
                return false;
            return workLevel <= 0;
        }

        public async void addfood(int amount)
        {
            //音效
            //bool full = food==maxFood;
            food+=amount;
            int exfood = 0;
            // if(!full)  饱食度增加的特效文本，之后补上
            // 	AbstractDungeon.effectsQueue.add(new TextAboveCreatureEffect(this.hb.cX - this.animX, this.hb.cY, characterStrings.TEXT[2] + Integer.toString(amount), Settings.GREEN_TEXT_COLOR));
            if(food>maxFood)
            {
                exfood = food-maxFood;
                //长腿菌块逻辑
                // int damage = food-maxFood;
                // if(hasRelic(Liver_LongLegMushroom.ID)&&AbstractDungeon.getCurrRoom().phase == AbstractRoom.RoomPhase.COMBAT){
                // 	boolean isdamageself = false;
                // 	for (AbstractMonster mo : AbstractDungeon.getMonsters().monsters) {
                // 		if (!mo.isDeadOrEscaped()&&mo.currentHealth>0) {
                // 			AbstractDungeon.actionManager.addToBottom(new DamageAction(mo,new DamageInfo(this,damage, DamageInfo.DamageType.THORNS)));
                // 			isdamageself = true;
                // 		}
                // 	}
                // 	if(isdamageself){
                // 		AbstractDungeon.actionManager.addToBottom(new DamageAction(this,new DamageInfo(this,damage, DamageInfo.DamageType.THORNS)));
                // 	}
                // }
                
                if(creature.HasPower<FatworldPower>())
                    await PowerCmd.Apply<StrengthPower>(creature, exfood, creature, null);
            }
            if(food<0)
                food=0;
            food = Math.Min(food,maxFood);
        }

        public async void reLive()
        {
            if(creature.HasPower<WorklockPower>())
                await PowerCmd.Decrement(creature.GetPower<WorklockPower>());
            else
                this.addworklevel(-1);
        }
        public void addworklevel(int level)
        {
            if(level>0&&creature.HasPower<WorkerrorPower>())
                return;
            int result = workLevel+level;
            setworklevel(result);
        }
        public void setworklevel(int level)
        {
            GD.PrintErr("业力变化：变化后为" +level);

            if(level>maxWorkLevel)
                level=maxWorkLevel;
            if(level<0)
                level=0;
            workLevel=level;
		
        }
    }
}