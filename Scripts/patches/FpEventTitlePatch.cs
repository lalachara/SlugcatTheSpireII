using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Rainworld.Scripts;
using Rainworld.Scripts.Event;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches;

/// <summary>
/// 高墙之上事件标题角色差分。
/// <see cref="EventModel.Title" /> 是非 virtual 属性，无法在 <see cref="FpEvent" /> 内 override，
/// 故用 Harmony 在 getter 上拦截：river 角色时显示「腐化之地」（title.RIVER），否则保持默认「高墙之上」。
/// </summary>
public sealed class FpEventTitlePatch : IPatchMethod
{
	public static string PatchId => "rainworld_fp_event_title";
	public static string Description => "高墙之上事件标题角色差分(river)";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[new(typeof(EventModel), "Title", MethodType.Getter)];

	public static bool Prefix(EventModel __instance, ref LocString __result)
	{
		if (__instance is not FpEvent fp || fp.Owner?.Character is not Rainworld_River)
			return true;

		__result = new LocString(fp.LocTable, $"{fp.Id.Entry}.title.RIVER");
		return false;
	}
}
