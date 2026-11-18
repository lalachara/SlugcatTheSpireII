using System;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using Rainworld.Scripts.Monster.Minions;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches;

/// <summary>
/// 多人模式主人彻底战死后，把仍留在场上的伙伴一起带离战场。
/// 原版 <c>CombatManager.HandlePlayerDeath</c> 只清空死者的能量、星辰与卡牌
/// （手牌/抽牌堆/弃牌堆/消耗堆/出牌堆），不处理宠物；单人 / 全灭时战斗直接结束、
/// 宠物随场景清理，所以只有「队友还在打」的多人战斗会看到伙伴继续留在场上。
/// 这里在死亡处理跑完后统一调用 <see cref="FriendPositions.LoseFriend" />，
/// 让伙伴跳过死亡与休眠复活流程直接退场
/// （不能用 <c>CreatureCmd.Escape</c>：会被记入逃离者名单、按比例扣减战斗金币奖励）。
/// </summary>
public sealed class FriendPetOwnerDeathPatch : IPatchMethod
{
    public static string PatchId => "rainworld_friend_pet_owner_death";
    public static string Description => "多人模式主人战死后移除其伙伴";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
        [new(typeof(CombatManager), nameof(CombatManager.HandlePlayerDeath))];

    public static void Postfix(ref Task __result, Player player)
    {
        __result = AwaitThenRemoveFriends(__result, player);
    }

    /// <summary>等死亡处理跑完再退场，避免在击杀流程中间移除生物。</summary>
    private static async Task AwaitThenRemoveFriends(Task original, Player player)
    {
        try
        {
            await original;
        }
        finally
        {
            RemoveFriends(player);
        }
    }

    private static void RemoveFriends(Player player)
    {
        try
        {
            ICombatState? combatState = player.Creature.CombatState;
            if (combatState == null || !combatState.IsLiveCombat())
                return;

            // 队伍全灭时战斗会整体结束，宠物随场景一起清理，不必逐个移除
            if (combatState.Players.All(p => p.Creature.IsDead))
                return;

            foreach (Creature friend in FriendPositions.All(player).ToList())
            {
                if (friend.Monster is FriendPetBase)
                    FriendPositions.LoseFriend(player, friend);
            }
        }
        catch (Exception e)
        {
            // 退场只是收尾工作，不能因为异常打断死亡流程
            Entry.Logger.Info($"[伙伴退场] 主人死亡后移除伙伴失败：{e.Message}");
        }
    }
}
