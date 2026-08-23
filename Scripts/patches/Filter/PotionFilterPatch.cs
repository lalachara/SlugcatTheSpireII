using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches;

public sealed class PotionFilterPatch : IPatchMethod
{
    public static string PatchId => "rainworld_potion_filter";
    public static string Description => "确保Milktea和Workflower药水只生成给蛞蝓猫角色";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(PotionFactory), nameof(PotionFactory.GetPotionOptions), new[] { typeof(Player) }),
    ];

    public static void Postfix(Player player, ref IEnumerable<PotionModel> __result)
    {
        if (player.Character is not Slugcat)
        {
            __result = __result.Where(p =>
                !p.Id.Entry.EndsWith("MILKTEA") &&
                !p.Id.Entry.EndsWith("WORKFLOWER"));
        }
    }
}