using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches;

/// <summary>
/// 本 mod 的药水只允许蛞蝓猫角色获得。
/// <para>
/// 原版角色（铁甲战士、静默猎手……）的 <see cref="PotionModel"/> 候选项来自
/// 「角色药水池 + 通用药水池」，我们注册到 <c>SharedPotionPool</c> 的药水
/// （兴奋剂、珍珠奶茶、瓶装业力花……）本来也会出现在原版角色那里。
/// </para>
/// <para>
/// 经 IL 确认：<see cref="PotionFactory.CreateRandomPotionInCombat"/>、
/// <see cref="PotionFactory.CreateRandomPotionOutOfCombat"/>、
/// <see cref="PotionFactory.CreateRandomPotionsOutOfCombat"/> 全部经由
/// <see cref="PotionFactory.GetPotionOptions"/> 取候选列表，
/// 所以只需要在这一处把「本 mod 程序集里的药水」过滤掉即可，
/// 商店、战斗奖励、事件等随机药水来源会一并生效。
/// </para>
/// <para>
/// 用程序集判断而不是写死 id 列表，以后新增药水不用再改这里。
/// </para>
/// <para>
/// 版本差异：0.107 的 <c>GetPotionOptions</c> 是
/// <c>(Player, IEnumerable&lt;PotionModel&gt; blacklist)</c> 两参版本，
/// 0.111 / 0.113 是一参版本，用编译符号 <c>STS2_0_107_1</c> 区分。
/// </para>
/// </summary>
public sealed class PotionFilterPatch : IPatchMethod
{
    public static string PatchId => "rainworld_potion_filter";

    public static string Description => "确保本 mod 的所有药水只会生成给蛞蝓猫角色";

    public static bool IsCritical => false;

    private static readonly Assembly ModAssembly = typeof(PotionFilterPatch).Assembly;

    public static ModPatchTarget[] GetTargets() =>
    [
#if STS2_0_107_1
        new(typeof(PotionFactory), nameof(PotionFactory.GetPotionOptions),
            new[] { typeof(Player), typeof(IEnumerable<PotionModel>) }),
#else
        new(typeof(PotionFactory), nameof(PotionFactory.GetPotionOptions), new[] { typeof(Player) }),
#endif
    ];

    public static void Postfix(Player player, ref IEnumerable<PotionModel> __result)
    {
        // 蛞蝓猫角色照旧；原版角色一律看不到本 mod 的药水。
        if (player.Character is Slugcat)
            return;

        __result = __result.Where(p => p.GetType().Assembly != ModAssembly).ToList();
    }
}
