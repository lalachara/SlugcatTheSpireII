using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Runs.History;
using Rainworld.Scripts;

namespace demo.Scripts.patches;

[HarmonyPatch]
public class MaxhpFixPatch
{
    [HarmonyTargetMethod]
    public static MethodBase TargetMethod()
    {
        return AccessTools.Method(
            typeof(CreatureCmd), 
            nameof(CreatureCmd.GainMaxHp),
            new[] { typeof(Creature), typeof(decimal) } // 方法参数类型
        );
    }

    // 2. Prefix 补丁：核心条件判断
    // 返回值说明：true = 执行原方法；false = 跳过原方法
    public static bool Prefix(
        Creature creature,          // 原方法参数1：目标生物
        decimal amount,             // 原方法参数2：增加的最大血量
        ref Task __result  // 原方法返回值（async Task<decimal>）
    )
    {
        
        // ========== 第二步：根据条件执行逻辑 ==========
        if (creature.Player.Character is Slugcat||amount>10000)
        {
            // 跳过原方法，执行你的自定义逻辑
            __result = CustomGainMaxHpLogic(creature, amount);
            return false; // 返回false，阻止原方法执行
        }
        else
        {
            return true; 
        }
    }
    private static async Task CustomGainMaxHpLogic(Creature creature, decimal amount)
    {
        if (amount > 10000)
        {
            amount-=10000;
            Decimal amount1 = await CreatureCmd.SetMaxHp(creature, (Decimal) creature.MaxHp + amount);
            MapPointHistoryEntry pointHistoryEntry = creature.Player?.RunState.CurrentMapPointHistoryEntry;
            if (pointHistoryEntry != null)
                pointHistoryEntry.GetEntry(creature.Player.NetId).MaxHpGained += (int) amount1;
            await CreatureCmd.Heal(creature, amount1);
        }
        else
        {
            SlugcatField.GetSlugCatData[creature].addfood((int)amount);
            
        }

       
    }
}