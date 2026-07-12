﻿﻿﻿﻿﻿using System.Reflection;
     using Godot;
     using HarmonyLib;
     using MegaCrit.Sts2.Core.Combat;
     using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Patches;
using Rainworld.relics;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches;

public sealed class LoseHpInternalPatch : IPatchMethod
{
	public static string PatchId => "rainworld_lose_hp_internal";
	public static string Description => "蛞蝓猫致死伤害时复活";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
		[new(typeof(Creature), nameof(Creature.LoseHpInternal), new[] { typeof(decimal), typeof(ValueProp) })];


	public static bool Prefix(
		Creature __instance,
		ref decimal amount,
		ValueProp props,
		out DamageResult __result
	)
{
    // if (props.Card is Rainworld_Liver_Strangebomb bomb)
    // {
    //     amount = bomb.DynamicVars.HpLoss.BaseValue;
    // }
    
    __result = default;
    if (__instance.Player?.Character is not Slugcat||__instance.CurrentHp <= 0||__instance.Player.GetRelic<Redmushroom>()!=null)
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
            if (SlugcatData.canbekill(__instance.Player))
            {
                
                return true;
            }
            else
            {
                // 不能被杀：复活并回满
                wasKilled = false;
                unblockedDamage = currentHp; // 依然计算全额伤害
                overkillDamage = 0;
                SlugcatData.reLive(__instance.Player);
                
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
public sealed class ForceKillPatch : IPatchMethod
{
	public static string PatchId => "rainworld_force_kill";
	public static string Description => "蛞蝓猫被杀时重置业力";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
		[new(typeof(CreatureCmd), nameof(CreatureCmd.Kill), new[] { typeof(IReadOnlyCollection<Creature>), typeof(bool) })];

	public static bool Prefix(IReadOnlyCollection<Creature> creatures)
	{
		foreach (Creature c in creatures)
		{
			if (c.Player?.Character is Slugcat)
			{
				
                SlugcatData.setworklevel(c.Player, 0, false);
                Log.Info($"Slugcat {c.Player} 强制击杀");
                if(c.HasPower<WorklockPower>())
	               PowerCmd.Remove(c.GetPower<WorklockPower>());
			}
		}
		return true;
	}
}


public sealed class StrangeBomb_ModifyHpLostBeforeOstyPatch : IPatchMethod
{
	public static string PatchId => "rainworld_strange_bomb_hp_loss";
	public static string Description => "怪炸弹的HP损失穿透所有能力";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
		[new(typeof(Hook), nameof(Hook.ModifyHpLost))];

	public static bool Prefix(
		ref Decimal __result,
		Decimal amount,
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
     
// [HarmonyPatch]
// public static class StrangeBomb_ModifyHpLostBeforeOstyPatch
// {
//     [HarmonyTargetMethod]
//     public static MethodBase TargetMethod()
//     {
//         return AccessTools.Method(
//             typeof(Hook),
//             nameof(Hook.AfterModifyingHpLostBeforeOsty),
//             new Type[]
//             {
//                 typeof(IRunState),
//                 typeof(CombatState),
//                 typeof(IEnumerable<AbstractModel>)
//             }
//         );
//     }
//
//     [HarmonyPrefix]
//     public static bool Prefix(
//         ref decimal __result,
//         decimal amount,
//         CardModel? cardSource,
//         out IEnumerable<AbstractModel> modifiers)
//     {
//         modifiers = Array.Empty<AbstractModel>();
//
//         if (cardSource is Rainworld_Liver_Strangebomb)
//         {
//             __result = cardSource.DynamicVars.HpLoss.BaseValue;
//             return false;
//         }
//
//         return true;
//     }
// }
