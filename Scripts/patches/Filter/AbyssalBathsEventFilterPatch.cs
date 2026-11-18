using System;
using System.Linq;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches.Filter;

/// <summary>
/// 根据配置在游玩蛞蝓猫时禁用原版事件「深渊浴场」（AbyssalBaths）。
/// 配置开关（EnableDisableAbyssalBaths）打开且队伍中存在蛞蝓猫时，抽到深渊浴场会重新抽取；
/// 关闭或无蛞蝓猫时不生效（事件正常生成）。多人模式以房主配置为准（走 RelicFilterData 同步）。
/// </summary>
public sealed class AbyssalBathsEventFilterPatch : IPatchMethod
{
    public static string PatchId => "rainworld_abyssal_baths_filter";
    public static string Description => "根据配置在游玩蛞蝓猫时禁用原版事件深渊浴场";
    public static bool IsCritical => false;

    [ThreadStatic]
    private static bool _isReplacing;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(ActModel), nameof(ActModel.PullNextEvent),
            new[] { typeof(RunState) }),
    ];

    public static void Postfix(ref EventModel __result, RunState runState)
    {
        if (_isReplacing)
            return;

        if (__result == null)
            return;

        // 只处理本次新抽到的原事件深渊浴场。
        if (__result.GetType() != typeof(AbyssalBaths))
            return;

        bool hasSlugcat = runState.Players.Any(p => p.Character is Slugcat);
        if (!hasSlugcat)
            return;

        bool enabled = ReadConfig(runState);
        if (!enabled)
            return;

        Entry.Logger.Info($"[事件过滤] {__result.GetType().Name} 在过滤列表中，重新抽取");
        _isReplacing = true;
        try
        {
            const int maxRetries = 20;
            for (int i = 0; i < maxRetries; i++)
            {
                var newEvent = runState.Act.PullNextEvent(runState);
                if (newEvent.GetType() != typeof(AbyssalBaths))
                {
                    __result = newEvent;
                    Entry.Logger.Info($"[事件过滤] 重新抽取成功: {newEvent.GetType().Name}");
                    return;
                }
            }

            Entry.Logger.Warn($"[事件过滤] 重试{maxRetries}次后仍无法获取非过滤事件");
        }
        finally
        {
            _isReplacing = false;
        }
    }

    private static bool ReadConfig(RunState runState)
    {
        bool isMultiplayer = RunManager.Instance?.NetService?.Type.IsMultiplayer() ?? false;
        if (isMultiplayer)
        {
            try
            {
                return SlugcatResources.RelicFilterData.Get(runState).EnableDisableAbyssalBaths;
            }
            catch (System.Exception ex)
            {
                Entry.Logger.Warn($"[事件过滤] 读取局内保存数据失败: {ex.Message}");
                return false;
            }
        }

        return RainworldModConfig.EnableDisableAbyssalBaths;
    }
}
