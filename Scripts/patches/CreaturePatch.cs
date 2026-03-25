using BaseLib;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Rainworld.Patches;

namespace SlugcatTheSpireII.Scripts.patches;

[HarmonyPatch(typeof(Creature))]
public class CreaturePatch
{
    [HarmonyPatch("AfterTurnStart")]
    [HarmonyPostfix]
    public static void Postfix_AfterTurnStart() {
        CombatUiPatch.callTurnStart();	
    }
}
