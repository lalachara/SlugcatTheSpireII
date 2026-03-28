using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using SlugcatTheSpireII.Scripts.patches;

namespace Rainworld.Scripts
{
    
    
    /// <summary>
    /// 静态访问类，封装 SpireField
    /// </summary>
    public static class SlugcatField
    {
        public static SlugcatData? playerdata;


        public static SlugcatData GetSlugCatDataByCreature( Creature creature)
        {
            if(playerdata == null)
                playerdata = new SlugcatData(creature.Player);
            return playerdata;
        }
        

        public static void SaveAllData()
        {
            GD.PrintErr("执行保存数据");
            if(playerdata != null)
                playerdata.SaveDataToCharacter();
            
        }
    }
}