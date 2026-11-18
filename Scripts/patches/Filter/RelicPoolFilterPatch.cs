using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches;

public sealed class RelicPoolFilterPatch : IPatchMethod
{
    public static string PatchId => "rainworld_relic_pool_filter";
    public static string Description => "根据配置禁用对蛞蝓猫无效的原版遗物（覆盖战斗奖励和商店）";
    public static bool IsCritical => false;

    internal static readonly HashSet<System.Type> MaxHpRelics = new()
    {         
        typeof(Mango),//芒果
        typeof(DragonFruit),//火龙果
        typeof(Pear),//梨子
        typeof(LoomingFruit),//布质果实
        typeof(Strawberry),//草莓
        typeof(FakeMango),//假芒果
        typeof(DarkstonePeriapt),//黑石护符

    };

    internal static readonly HashSet<System.Type> HpRelics = new()
    {
         typeof(BloodVial),//小血瓶
         typeof(BookOfFiveRings),//五环书
         typeof(EternalFeather),//永恒羽毛
         typeof(FakeLeesWaffle),//假华夫饼
         typeof(LeesWaffle),//华夫饼
         typeof(MealTicket),//菜单票
         typeof(MeatOnTheBone),//带骨肉
         typeof(Pantograph),//缩放仪
         typeof(RegalPillow),//枕头
         typeof(Planisphere),//活动星图
    };
    internal static readonly HashSet<System.Type> NoUseRelics = new()
    {
        typeof(DragonFruit),//火龙果
        typeof(FakeMango),//假芒果
    };

    [ThreadStatic]
    private static bool _isReplacing;
    
    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(RelicFactory), nameof(RelicFactory.PullNextRelicFromFront),
            new[] { typeof(Player), typeof(RelicRarity), typeof(System.Func<RelicModel, bool>) }),
        new(typeof(RelicFactory), nameof(RelicFactory.PullNextRelicFromBack),
            new[] { typeof(Player), typeof(RelicRarity), typeof(System.Func<RelicModel, bool>) }),
    ];

    public static void Postfix(ref RelicModel __result, Player player)
    {
        if (_isReplacing)
            return;
        
        if (__result == null)
        {
            return;
        }

        if (player.Character is not Slugcat)
        {
            return;
        }

        bool isMultiplayer = RunManager.Instance?.NetService?.Type.IsMultiplayer() ?? false;

        var toRemove = new HashSet<System.Type>();

        if (isMultiplayer)
        {
            var runState = player.RunState;
            if (runState is RunState rs)
            {
                try
                {
                    var data = SlugcatResources.RelicFilterData.Get(rs);
                    
                    if (data.EnableMaxHpRelicFix)
                    {
                        toRemove.UnionWith(MaxHpRelics);
                    }
                    if (data.EnableHpRelicFix)
                    {
                        toRemove.UnionWith(HpRelics);
                    }
                    if (data.EnableMaxHpCantUsedRelicFix)
                    {
                        toRemove.UnionWith(NoUseRelics);
                    }
                }
                catch (System.Exception ex)
                {
                    Entry.Logger.Warn($"[遗物过滤] 读取局内保存数据失败: {ex.Message}");
                }
            }
        }
        else
        {
            if (RainworldModConfig.EnableMaxHpRelicFix)
            {
                toRemove.UnionWith(MaxHpRelics);
            }
            if (RainworldModConfig.EnableHpRelicFix)
            {
                toRemove.UnionWith(HpRelics);
            }
            // if (RainworldModConfig.EnableMaxHpCantUsedRelicFix)
            // {
            //     toRemove.UnionWith(NoUseRelics);
            // }
        }

        if (toRemove.Count == 0)
            return;
        
        if (toRemove.Contains(__result.GetType()))
        {
            Entry.Logger.Info($"[遗物过滤] {__result.GetType().Name} 在过滤列表中，重新抽取");
            _isReplacing = true;
            try
            {
                const int maxRetries = 20;
                for (int i = 0; i < maxRetries; i++)
                {
                    // 0.113：PullNextRelicFromFront(Player) 重载已移除，奖励用的等价方法是 PullNextRelicForRewardFromFront
                    var newRelic = RelicFactory.PullNextRelicForRewardFromFront(player);
                    if (!toRemove.Contains(newRelic.GetType()))
                    {
                        __result = newRelic;
                        Entry.Logger.Info($"[遗物过滤] 重新抽取成功: {newRelic.GetType().Name}");
                        return;
                    }
                }
                Entry.Logger.Warn($"[遗物过滤] 重试{maxRetries}次后仍无法获取非过滤遗物，使用备用遗物");
                __result = RelicFactory.FallbackRelic;
            }
            finally
            {
                _isReplacing = false;
            }
        }
    }
}