// using System.Collections.Generic;
// using MegaCrit.Sts2.Core.HoverTips;
// using MegaCrit.Sts2.Core.Models;
// using MegaCrit.Sts2.Core.Runs;
// using STS2RitsuLib.Patching.Models;
//
// namespace Rainworld.Scripts.patches;
//
// public sealed class HpEventFilterPatch : IPatchMethod
// {
//     public static string PatchId => "rainworld_hp_event_filter";
//     public static string Description => "根据配置从事件池中移除对蛞蝓猫无效的原版回血事件";
//     public static bool IsCritical => false;
//
//     internal static readonly HashSet<System.Type> HpEvents = new()
//     {
//         // TODO: 用ILSpy反编译确认所有回血事件后填入
//     };
//
//     [ThreadStatic]
//     private static bool _isReplacing;
//
//     public static ModPatchTarget[] GetTargets() =>
//     [
//         new(typeof(ActModel), nameof(ActModel.PullNextEvent),
//             new[] { typeof(RunState) }),
//     ];
//
//     public static void Postfix(ref EventModel __result, RunState runState)
//     {
//         if (_isReplacing)
//             return;
//
//         if (__result == null)
//             return;
//
//         if (HpEvents.Count == 0)
//             return;
//
//         bool hasSlugcat = false;
//         foreach (var player in runState.Players)
//         {
//             if (player.Character is Slugcat)
//             {
//                 hasSlugcat = true;
//                 break;
//             }
//         }
//
//         if (!hasSlugcat)
//             return;
//
//         if (!RainworldModConfig.HpEventFix)
//             return;
//
//         if (HpEvents.Contains(__result.GetType()))
//         {
//             Entry.Logger.Info($"[事件过滤] {__result.GetType().Name} 在过滤列表中，重新抽取");
//             _isReplacing = true;
//             try
//             {
//                 const int maxRetries = 20;
//                 for (int i = 0; i < maxRetries; i++)
//                 {
//                     var newEvent = runState.Act.PullNextEvent(runState);
//                     if (!HpEvents.Contains(newEvent.GetType()))
//                     {
//                         __result = newEvent;
//                         Entry.Logger.Info($"[事件过滤] 重新抽取成功: {newEvent.GetType().Name}");
//                         return;
//                     }
//                 }
//
//                 Entry.Logger.Warn($"[事件过滤] 重试{maxRetries}次后仍无法获取非过滤事件");
//             }
//             finally
//             {
//                 _isReplacing = false;
//             }
//         }
//     }
// }