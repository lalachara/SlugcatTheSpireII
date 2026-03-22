using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Rainworld.Scripts
{
    /// <summary>
    /// 静态访问类，封装 SpireField
    /// </summary>
    public static class SlugcatField
    {
        // 【核心】创建 SpireField，挂载在 Creature 上
        // 传入默认值工厂：每次创建新 SlugcatData 时自动调用 Initialize()
        public static List<SlugcatData> needsaves = new List<SlugcatData>();
        public static readonly SpireField<Creature, SlugcatData> GetSlugCatData = new(
            creature => 
            {
                var data = new SlugcatData(creature);
                data.Initialize(); // 自动初始化
                if(creature.Player.Character is Slugcat)
                    needsaves.Add(data);
                return data;
            }
        );
        
        // 辅助方法：判断是不是 Slugcat
        public static bool IsSlugcat(this Creature creature)
        {
            return creature.Player?.Character is Slugcat;
        }

        public static SlugcatData GetSlugcat(this Creature creature)
        {
            SlugcatData data = SlugcatField.GetSlugCatData[creature];

            return data;
        }

        public static void SaveAllData()
        {
            foreach (SlugcatData data in needsaves)
            {
                GD.PrintErr("执行保存数据：" );
                    data.SaveDataToCharacter();

            }

        }
    }
}