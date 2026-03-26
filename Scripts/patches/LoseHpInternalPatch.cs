using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.ValueProps;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Rainworld.Patches;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;

namespace Rainworld.Scripts.patches;

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
            if (SlugcatField.GetSlugCatDataByCreature(__instance.Player.Creature).canbekill())
            {
                
                return true;
            }
            else
            {
                // 不能被杀：复活并回满
                wasKilled = false;
                unblockedDamage = currentHp; // 依然计算全额伤害
                overkillDamage = 0;
                SlugcatField.GetSlugCatDataByCreature(__instance.Player.Creature).reLive();
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

[HarmonyPatch]
public class ForceKillPatch
{
    [HarmonyTargetMethod]
    public static MethodBase TargetMethod()
    {
        var method = AccessTools.Method(
            typeof(CreatureCmd),
            nameof(CreatureCmd.Kill),
            new[] { typeof(IReadOnlyCollection<Creature>), typeof(bool) }
        );
        return method;
    }

    public static bool Prefix(
        IReadOnlyCollection<Creature> creatures
    )
    {
        foreach (Creature c in creatures)
        {
            if (c.Player?.Character is Slugcat)
            {
                if(c==CombatUiPatch.creature)
                    SlugcatField.playerdata.setworklevel(0,false);
            }
        }
        return true;
    }
}

[HarmonyPatch]
public static class StrangeBomb_ModifyHpLostBeforeOstyPatch
{
    [HarmonyTargetMethod]
    public static MethodBase TargetMethod()
    {
        return AccessTools.Method(
            typeof(Hook),
            nameof(Hook.ModifyHpLostBeforeOsty),
            new Type[]
            {
                typeof(IRunState),
                typeof(CombatState),
                typeof(Creature),
                typeof(decimal),
                typeof(ValueProp),
                typeof(Creature),
                typeof(CardModel),
                typeof(IEnumerable<AbstractModel>).MakeByRefType()
            }
        );
    }

    [HarmonyPrefix]
    public static bool Prefix(
        ref decimal __result,
        decimal amount,
        CardModel? cardSource,
        out IEnumerable<AbstractModel> modifiers)
    {
        modifiers = Array.Empty<AbstractModel>();

        if (cardSource is Rainworld_Liver_Strangebomb)
        {
            __result = cardSource.DynamicVars.HpLoss.BaseValue;
            return false;
        }

        return true;
    }
}