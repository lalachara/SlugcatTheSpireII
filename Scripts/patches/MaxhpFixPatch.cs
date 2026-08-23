﻿﻿using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using Rainworld.relics;
using Rainworld.Scripts;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches;

public sealed class MaxhpFixPatch : IPatchMethod
{
	public static string PatchId => "rainworld_maxhp_fix";
	public static string Description => "蛞蝓猫 GainMaxHp 转为增加食物";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
		[new(typeof(CreatureCmd), nameof(CreatureCmd.GainMaxHp), new[] { typeof(Creature), typeof(decimal) })];

	public static bool Prefix(
		Creature creature,
		decimal amount,
		ref Task __result
	)
    {
        
        // ========== 第二步：根据条件执行逻辑 ==========
        if (creature.Player?.Character is Slugcat&&creature.Player.GetRelic<Redmushroom>()==null)
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
        if (amount>=5)
        {
            amount /= 5;
            Decimal amount1 = await CreatureCmd.SetMaxHp(creature, (Decimal) creature.MaxHp + amount);
            MapPointHistoryEntry pointHistoryEntry = creature.Player?.RunState.CurrentMapPointHistoryEntry;
            if (pointHistoryEntry != null)
                pointHistoryEntry.GetEntry(creature.Player.NetId).MaxHpGained += (int) amount1;
            await CreatureCmd.Heal(creature, amount1);
        }
        else
        {
            await SlugcatData.addfood(creature.Player, (int)amount);
            
        }

       
    }
    
    

}

public sealed class LoseMaxHpPatch : IPatchMethod
{
    public static string PatchId => "rainworld_lose_max_hp";
    public static string Description => "蛞蝓猫单次失去生命上限固定为1";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
        [new(typeof(CreatureCmd), nameof(CreatureCmd.LoseMaxHp), new[] { typeof(PlayerChoiceContext), typeof(Creature), typeof(Decimal), typeof(bool) })];

    public static bool Prefix(
        PlayerChoiceContext choiceContext,
        Creature creature,
        ref Decimal amount,
        bool isFromCard
    )
    {
        if (creature.Player?.Character is Slugcat)
        {
            if (IsCombatCantLossMaxHp(creature.Player) && creature.CombatState != null)
                amount = 0;
            else
            {
                amount = 1;
            }
        }

        return true;
    }

    private static bool IsCombatCantLossMaxHp(Player player)
    {
        bool isMultiplayer = RunManager.Instance?.NetService?.Type.IsMultiplayer() ?? false;
        if (isMultiplayer && player.RunState is RunState rs)
        {
            try
            {
                return SlugcatResources.RelicFilterData.Get(rs).EnableCombatCantLossMaxHp;
            }
            catch (System.Exception ex)
            {
                Entry.Logger.Warn($"[生命值修复] 读取局内保存数据失败: {ex.Message}");
            }
        }
        return RainworldModConfig.EnableCombatCantLossMaxHp;
    }
}