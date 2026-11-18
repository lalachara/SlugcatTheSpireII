using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using Rainworld.Scripts.Skins;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches;

/// <summary>
/// 在原版角色选择界面上挂载蛞蝓猫皮肤选择面板（单预览 + 左右切换）。
/// SelectCharacter 在界面打开和每次切换角色时都会调用，因此这里统一负责创建 / 刷新 / 隐藏面板。
/// <para>
/// 0.113：<c>NCharacterSelectScreen.SelectCharacter(NCharacterSelectButton, CharacterModel)</c>
/// 改成了 <c>SelectCharacter(NCharacterSelectButton)</c>（角色从按钮自己的
/// <see cref="NCharacterSelectButton.Character" /> 取）。按旧签名写的 Postfix 在 0.113 上匹配不到
/// 目标方法，Harmony 会直接跳过整个补丁，表现就是「选角色界面没有皮肤选择」，
/// 所以这里的目标与方法签名一起跟着改。
/// </para>
/// </summary>
public sealed class CharacterSelectSkinPatch : IPatchMethod
{
	public static string PatchId => "rainworld_character_select_skin";
	public static string Description => "角色选择界面添加蛞蝓猫 Spine 皮肤选择";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
		[new(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.SelectCharacter), new[] { typeof(NCharacterSelectButton) })];

	public static void Postfix(NCharacterSelectScreen __instance, NCharacterSelectButton charSelectButton)
	{
		SlugcatSkinSelect.Attach(__instance, charSelectButton.Character, charSelectButton.IsLocked);
	}
}
