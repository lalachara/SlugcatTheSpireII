using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Random;
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
        if (creature.Player?.Character is Slugcat && creature.Player.GetRelic<Redmushroom>() == null)
        {
            // 跳过原方法，执行自定义逻辑
            __result = CustomGainMaxHpLogic(creature, amount);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 蛞蝓猫的最大生命提升规则（塔1「提升最大生命的效率削减为 1/5」）：
    /// <list type="bullet">
    /// <item>单次 <b>≥ 5 点</b>：老算法，除以 5 **向下取整**，把这么多上限真的加上去；</item>
    /// <item>单次 <b>&lt; 5 点</b>：因为除 5 会变成 0，改成<b>按概率给 1 点上限</b>，
    /// 概率 = 20% × 这次的数值（1 点 → 20%，3 点 → 60%，4 点 → 80%），没中就什么都不发生。</item>
    /// </list>
    /// 判定用的是<b>游戏自己的随机流</b>（<c>RunState.Rng.Niche</c>，随存档序列化、随开局快照同步），
    /// 不能用 <c>new Random()</c> / <c>DateTime</c> 之类的本地随机，否则多人联机会各算各的。
    /// </summary>
    private static async Task CustomGainMaxHpLogic(Creature creature, decimal amount)
    {
        Player? player = creature.Player;
        if (player == null || amount <= 0m)
            return;

        // ———— 5 点及以上：每 5 点换 1 点上限（向下取整）————
        if (amount >= 5m)
        {
            decimal gained = Math.Floor(amount / 5m);
            if (gained <= 0m)
                return;

            await ApplyMaxHpGain(creature, player, gained);
            return;
        }

        // ———— 小于 5 点：按 20% × 数值 的概率给 1 点上限 ————
        int chancePercent = (int)Math.Floor(amount * 20m);
        if (chancePercent <= 0)
            return;
        if (chancePercent > 100)
            chancePercent = 100;

        // 游戏自带的随局随机器：同一局里各端抽到同一个值，联机不会不同步
        Rng rng = player.RunState.Rng.Niche;
        if (rng.NextInt(0, 100) >= chancePercent)
        {
            return;
        }

        await ApplyMaxHpGain(creature, player, 1m);
    }

    /// <summary>真正把上限加上去，并同步「本局获得的最大生命」统计 + 回血（与原版口径一致）。</summary>
    private static async Task ApplyMaxHpGain(Creature creature, Player player, decimal gained)
    {
        decimal actual = await CreatureCmd.SetMaxHp(creature, creature.MaxHp + gained);

        MapPointHistoryEntry? pointHistoryEntry = player.RunState.CurrentMapPointHistoryEntry;
        if (pointHistoryEntry != null)
            pointHistoryEntry.GetEntry(player.NetId).MaxHpGained += (int)actual;

        await CreatureCmd.Heal(creature, actual);
    }
}

public sealed class LoseMaxHpPatch : IPatchMethod
{
    public static string PatchId => "rainworld_lose_max_hp";
    public static string Description => "蛞蝓猫单次失去生命上限固定为1";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
        // 0.113：LoseMaxHp 的最后一个参数由 bool isFromCard 改成 CardModel? cardSource
        [new(typeof(CreatureCmd), nameof(CreatureCmd.LoseMaxHp), new[] { typeof(PlayerChoiceContext), typeof(Creature), typeof(Decimal), typeof(MegaCrit.Sts2.Core.Models.CardModel) })];

    public static bool Prefix(
        PlayerChoiceContext choiceContext,
        Creature creature,
        ref Decimal amount,
        MegaCrit.Sts2.Core.Models.CardModel? cardSource
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
