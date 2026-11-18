using System;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Rainworld.Scripts.Monster.Minions;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches;

/// <summary>
/// 伙伴站位补丁：把本 mod 的伙伴摆到塔1 的五个固定站位上。
/// <para>原版对宠物只有一套排版（<c>NCombatRoom.AddCreature</c> / <c>PositionPlayersAndPets</c>：
/// 贴着主人脚印排成一排，而且已经休眠倒地的伙伴也会被一起拖到玩家脚下），
/// 而伙伴要站五个位置、倒地等待复活的还得留在原地，所以在这三个时机之后统一按
/// <see cref="FriendLayout" /> 重新摆一次：</para>
/// <list type="bullet">
/// <item>生物进场（战斗开始逐只进场、中途召唤新伙伴）→ 摆好新伙伴并复位其它伙伴；</item>
/// <item>节点移除（伙伴阵亡 / 拾荒者退场）→ 后面的伙伴补上空出来的站位；</item>
/// <item>战斗开始整体排版（<c>PositionPlayersAndPets</c>）→ 覆盖原版的「一排贴脚」排版。</item>
/// </list>
/// </summary>
public sealed class FriendPetLayoutPatch : IPatchMethod
{
    public static string PatchId => "rainworld_friend_pet_layout";

    public static string Description => "按塔1 五站位摆放伙伴，休眠/倒地的伙伴不再被摆到玩家脚下";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NCombatRoom), nameof(NCombatRoom.AddCreature), new[] { typeof(Creature) }),
        new(typeof(NCombatRoom), nameof(NCombatRoom.RemoveCreatureNode), new[] { typeof(NCreature) }),
        // 战斗开始时的整体排版（静态方法，参数列表必须精确匹配，Harmony 不会自动匹配可选参数）
        new(typeof(NCombatRoom), nameof(NCombatRoom.PositionPlayersAndPets),
            new[] { typeof(List<NCreature>), typeof(float), typeof(bool) })
    ];

    public static void Postfix()
    {
        try
        {
            FriendLayout.Apply();
        }
        catch (Exception e)
        {
            // 站位只是显示问题，不能因为异常打断召唤 / 死亡 / 战斗开始流程
            Entry.Logger.Info($"[伙伴站位] 摆放伙伴位置失败：{e.Message}");
        }
    }
}
