using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches;

public sealed class RelicGrabBagFilterPatch : IPatchMethod
{
    public static string PatchId => "rainworld_relic_grab_bag_filter";
    public static string Description => "过滤RelicGrabBag中的遗物（宝藏房间）";
    public static bool IsCritical => false;

    [ThreadStatic]
    private static bool _isReplacing;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(RelicGrabBag), nameof(RelicGrabBag.PullFromFront),
            new[] { typeof(RelicRarity), typeof(Func<RelicModel, bool>), typeof(IRunState) }),
        new(typeof(RelicGrabBag), nameof(RelicGrabBag.PullFromBack),
            new[] { typeof(RelicRarity), typeof(Func<RelicModel, bool>), typeof(IRunState) }),
    ];

    public static void Postfix(ref RelicModel __result, RelicGrabBag __instance, RelicRarity rarity, Func<RelicModel, bool> filter, IRunState runState)
    {
        if (_isReplacing)
            return;

        if (__result == null)
            return;

        bool hasSlugcat = runState.Players.Any(p => p.Character is Slugcat);
        if (!hasSlugcat)
            return;

        var toRemove = new HashSet<Type>();
        bool isMultiplayer = RunManager.Instance?.NetService?.Type.IsMultiplayer() ?? false;

        if (isMultiplayer)
        {
            if (runState is RunState rs)
            {
                try
                {
                    var data = SlugcatResources.RelicFilterData.Get(rs);
                    if (data.EnableMaxHpRelicFix)
                        toRemove.UnionWith(RelicPoolFilterPatch.MaxHpRelics);
                    if (data.EnableHpRelicFix)
                        toRemove.UnionWith(RelicPoolFilterPatch.HpRelics);
                    if (data.EnableMaxHpCantUsedRelicFix)
                        toRemove.UnionWith(RelicPoolFilterPatch.NoUseRelics);
                }
                catch (Exception ex)
                {
                    Entry.Logger.Warn($"[遗物过滤GrabBag] 读取局内保存数据失败: {ex.Message}");
                }
            }
        }
        else
        {
            if (RainworldModConfig.EnableMaxHpRelicFix)
                toRemove.UnionWith(RelicPoolFilterPatch.MaxHpRelics);
            if (RainworldModConfig.EnableHpRelicFix)
                toRemove.UnionWith(RelicPoolFilterPatch.HpRelics);
            // if (RainworldModConfig.EnableMaxHpCantUsedRelicFix)
            //     toRemove.UnionWith(RelicPoolFilterPatch.NoUseRelics);
        }

        if (toRemove.Count == 0)
            return;

        if (toRemove.Contains(__result.GetType()))
        {
            Entry.Logger.Info($"[遗物过滤GrabBag] {__result.GetType().Name} 在过滤列表中，重新抽取");
            _isReplacing = true;
            try
            {
                const int maxRetries = 20;
                for (int i = 0; i < maxRetries; i++)
                {
                    var newRelic = __instance.PullFromFront(rarity, filter, runState);
                    if (newRelic != null && !toRemove.Contains(newRelic.GetType()))
                    {
                        __result = newRelic;
                        Entry.Logger.Info($"[遗物过滤GrabBag] 重新抽取成功: {newRelic.GetType().Name}");
                        return;
                    }
                }
                Entry.Logger.Warn($"[遗物过滤GrabBag] 重试{maxRetries}次后仍无法获取非过滤遗物");
            }
            finally
            {
                _isReplacing = false;
            }
        }
    }
}