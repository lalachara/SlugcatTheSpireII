using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.ValueProps;
using Godot;
using Rainworld.Scripts;

namespace demo.Scripts.patches;

[HarmonyPatch]
public class LoseHpInternalPatch
{
    [HarmonyTargetMethod]
    public static MethodBase TargetMethod()
    {
        var method = AccessTools.Method(
            typeof(Creature),
            nameof(Creature.LoseHpInternal),
            new[] { typeof(decimal), typeof(ValueProp) }
        );
        return method;
    }

    public static bool Prefix(
    Creature __instance,          
    decimal amount,               
    ValueProp props,              
    out DamageResult __result
)
{
    __result = default;
    if (__instance.Player?.Character is not Slugcat||__instance.CurrentHp <= 0)
    {
        return true;
    }
    
    //↑非蛞蝓猫执行原版方法
    
    // 1. 初始化变量
    bool wasKilled = false;
    int currentHp = __instance.CurrentHp;
    int unblockedDamage = 0;
    int overkillDamage = 0;
    
    bool isLethalDamage = amount >= __instance.CurrentHp;

        if (isLethalDamage)
        {
            // 伤害致死：判断 canbekill()
            if (SlugcatField.GetSlugCatData[__instance.Player.Creature].canbekill())
            {
                return true;
            }
            else
            {
                // 不能被杀：复活并回满
                wasKilled = false;
                unblockedDamage = currentHp; // 依然计算全额伤害
                overkillDamage = 0;
                SlugcatField.GetSlugCatData[__instance.Player.Creature].reLive();
                __instance.SetCurrentHpInternal(__instance.MaxHp);
            }
        }
        else
        {
            return true;
        }
    

    // 3. 统一设置返回值
    __result = new DamageResult(__instance, props)
    {
        UnblockedDamage = unblockedDamage,
        WasTargetKilled = wasKilled, 
        OverkillDamage = overkillDamage
    };

    // 4. 阻止原方法执行
    return false;
}
}