using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.sts2.Core.Nodes.TopBar;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches;

/// <summary>
/// 左上角生命值指示器的悬停提示：
/// - 蛞蝓猫：换成 mod 版说明，并在末尾追加「当前业力 / 业力上限」一行（与业力指示器口径一致）；
/// - 其它角色：走原版提示（mod 不再覆盖 static_hover_tips 表里的 HIT_POINTS.description）。
/// 原版 <see cref="NTopBarHp"/>._player 是 private，只能用反射读。
/// </summary>
public sealed class TopBarHpHoverTipPatch : IPatchMethod
{
	public static string PatchId => "rainworld_top_bar_hp_hover_tip";
	public static string Description => "生命值悬停提示：蛞蝓猫附加当前业力等级与上限";
	public static bool IsCritical => false;

	/// <summary>本地化键（zhs / eng / kor 三个语言文件里都要有同一条目）。</summary>
	private const string LocKey = "HIT_POINTS_SLUGCAT.description";

	private static readonly FieldInfo? PlayerField =
		typeof(NTopBarHp).GetField("_player", BindingFlags.Instance | BindingFlags.NonPublic);

	public static ModPatchTarget[] GetTargets() =>
		[new(typeof(NTopBarHp), "OnFocus")];

	public static bool Prefix(NTopBarHp __instance)
	{
		if (PlayerField?.GetValue(__instance) is not Player player)
			return true;
		// 只有蛞蝓猫才换成带业力的说明，其它角色保持原版
		if (player.Character is not Slugcat)
			return true;
		// 缺对应语言的本地化条目时退回原版，避免界面上直接显示键名
		if (!LocString.Exists("static_hover_tips", LocKey))
			return true;

		LocString description = new("static_hover_tips", LocKey);
		// 与业力指示器同一口径：内部从 0 起算，界面显示 +1
		description.Add("work_level", SlugcatData.GetWorkLevel(player) + 1);
		description.Add("max_work_level", SlugcatData.GetMaxWorkLevel(player) + 1);

		HoverTip hoverTip = new(new LocString("static_hover_tips", "HIT_POINTS.title"), description);
		NHoverTipSet? tipSet = NHoverTipSet.CreateAndShow(__instance, hoverTip, HoverTipAlignment.None);
		// 位置与原版一致：血条下方 20 像素
		tipSet?.SetGlobalPosition(__instance.GlobalPosition + new Vector2(0f, __instance.Size.Y + 20f), false);
		return false;
	}
}
