using Rainworld.relics;
using Rainworld.Scripts.Card.Liver.Attack;

namespace SlugcatTheSpireII.Scripts.patches;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using System.Collections.Generic;
using System.Reflection;


    [HarmonyPatch]
    public static class TouchOfOrobasPatch
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(TouchOfOrobas),
                nameof(TouchOfOrobas.GetUpgradedStarterRelic),
                new Type[]
                {
                    typeof(RelicModel)
                }
            );
        }

        [HarmonyPrefix]
        public static bool Prefix(
            ref RelicModel __result,
            RelicModel starterRelic
            )
        {
            if (starterRelic.Id == ModelDb.Relic<Liver_Fruit1>().Id)
            {
                __result = ModelDb.Relic<Liver_Fruit2>();
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ArchaicTooth), "TranscendenceUpgrades", MethodType.Getter)]
    public static class ArchaicTooth_TranscendenceUpgrades_Patch
    {
        // 只执行一次，防止重复添加

        [HarmonyPostfix]
        public static void Postfix(ref Dictionary<ModelId, CardModel> __result)
        {
            
            __result.TryAdd(ModelDb.Card<Rainworld_Liver_Spear>().Id, ModelDb.Card<Rainworld_Liver_Spearfire>());
                
        }
    }