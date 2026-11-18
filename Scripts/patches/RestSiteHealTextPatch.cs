using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches;

/// <summary>
/// 火堆「休息」的描述替换：蛞蝓猫不再显示原版的“回复百分比生命”，改为“回复 N 级业力”。
/// 文本本身放在本地化 rest_site_ui 表的 <c>OPTION_HEAL_SLUGCAT.description</c> 里，
/// 代码只负责按角色决定数字 N（Monk 2 级，其余蛞蝓猫 1 级），通过 <c>{Karma}</c> 占位符拼进文本。
/// </summary>
public sealed class RestSiteHealTextPatch : IPatchMethod
{
	public static string PatchId => "rainworld_rest_site_heal_text";
	public static string Description => "蛞蝓猫的火堆休息描述改为恢复业力";
	public static bool IsCritical => false;

	/// <summary>本地化键（zhs / eng / kor 三个语言文件里都要有同一条目）。</summary>
	private const string LocKey = "OPTION_HEAL_SLUGCAT.description";

	/// <summary><see cref="RestSiteOption.Owner" /> 是 protected，补丁里只能反射读取。</summary>
	private static readonly PropertyInfo? OwnerProperty =
		typeof(RestSiteOption).GetProperty("Owner", BindingFlags.Instance | BindingFlags.NonPublic);

	public static ModPatchTarget[] GetTargets() =>
		[new(typeof(HealRestSiteOption), "Description", MethodType.Getter)];

	public static bool Prefix(RestSiteOption __instance, ref LocString __result)
	{
		if (OwnerProperty?.GetValue(__instance) is not Player player)
			return true;
		// 只有蛞蝓猫角色才把休息文案换成业力
		if (player.Character is not Slugcat)
			return true;
		// 缺对应语言的本地化条目时退回原版文本，避免界面上直接显示键名
		if (!LocString.Exists("rest_site_ui", LocKey))
			return true;

		// 回复等级与实际结算共用 SlugcatData 的同一处规则（Monk 2 级，其余 1 级）
		int karma = SlugcatData.GetRestKarmaLevel(player);

		LocString description = new("rest_site_ui", LocKey);
		description.Add("Karma", karma.ToString());
		// 与原版一致：把其它遗物/mod 通过 ModifyExtraRestSiteHealText 追加的说明也带上
		IReadOnlyList<LocString> extra = Hook.ModifyExtraRestSiteHealText(player.RunState, player,
			Array.Empty<LocString>());
		description.Add("ExtraText", extra.Count > 0
			? "\n" + string.Join("\n", extra.Select(s => s.GetFormattedText()))
			: string.Empty);

		__result = description;
		return false;
	}
}
