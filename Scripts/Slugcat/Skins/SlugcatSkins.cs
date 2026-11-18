using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;

namespace Rainworld.Scripts.Skins;

/// <summary>
/// 蛞蝓猫皮肤注册表与运行时应用。
///
/// 本项目的皮肤是「同一套 Spine 骨架内的多个 skin」，不需要像整包替换那样加载新的 .tres。
/// 各角色的骨架都按同一套规律分层：
///   default      —— 身体等公共部件
///   normal/0、1  —— 只覆盖帽子 / 耳朵 / 睡觉姿势等可选部件（Liver 的命名为 normal/normal0、normal/normal1）
///   dianchi 等  —— 只覆盖手中道具所在的槽位
/// 所以切换皮肤时必须把各层合并成一个 skin 再 SetSkin，单独 SetSkinByName 会把其它层隐藏。
/// </summary>
public static class SlugcatSkins
{
	/// <summary>表示「角色没有可选皮肤」。正常角色不会返回该值。</summary>
	public const string DefaultChoice = "";

	/// <summary>打底皮肤名：提供身体、尾巴等所有角色共用的部件。</summary>
	private const string BaseSkinName = "default";

	/// <summary>溪流（水猫）没有电池遗物时，手里默认拿的水球皮肤。</summary>
	public const string RiverDefaultItemSkin = "shuiguo";

	/// <summary>溪流（水猫）持有电池遗物、尚未耗尽时的电池皮肤（只含手中道具槽位）。</summary>
	public const string RiverBatteryItemSkin = "dianchi";

	/// <summary>溪流（水猫）电池遗物耗尽后的电池皮肤（只含手中道具槽位）。</summary>
	public const string RiverBatteryEmptyItemSkin = "dianchi2";

	private static readonly Dictionary<Type, string[]> OptionsByCharacter = new()
	{
		// 猎手（红猫）：normal/1 是 RedCat.tscn 里配置的默认皮肤（带帽子），保留为首选。
		[typeof(Rainworld_Redcat)] = ["normal/1", "normal/0"],

		// 溪流（水猫）：这里只列帽子皮肤；手中道具由遗物状态单独叠加（见 NRiver）。
		[typeof(Rainworld_River)] = ["normal/0", "normal/1"],

		// 求生者（白猫）
		[typeof(Rainworld_Liver)] = ["normal/normal0", "normal/normal1"],

		// 僧侣（黄猫）
		[typeof(Rainworld_Monk)] = ["normal/0", "normal/1"],
	};

	/// <summary>角色选择界面预览所用的战斗外观场景（Spine 骨架 + 缩放）。</summary>
	private static readonly Dictionary<Type, string> VisualsPathByCharacter = new()
	{
		[typeof(Rainworld_Redcat)] = "res://Godot/Character/RedCat/RedCat.tscn",
		[typeof(Rainworld_River)] = "res://Godot/Character/River/River.tscn",
		[typeof(Rainworld_Liver)] = "res://Godot/Character/Liver/Liver.tscn",
		[typeof(Rainworld_Monk)] = "res://Godot/Character/Monk/Monk.tscn",
	};

	/// <summary>
	/// 角色选择界面皮肤面板的底框：每只蛞蝓猫一张（701x585，与面板 420x350 同比例）。
	/// 图片放在 pck 里，需要先在 Godot 编辑器里导入并重新导出 pck 才会生效；
	/// 取不到时 <see cref="SlugcatSkinSelect" /> 会退回原来的九宫格深色底。
	/// </summary>
	private static readonly Dictionary<Type, string> SelectBackgroundByCharacter = new()
	{
		[typeof(Rainworld_Redcat)] = "res://Godot/Character/RedCat/selectskinbg.png",
		[typeof(Rainworld_River)] = "res://Godot/Character/River/selectskinbg.png",
		[typeof(Rainworld_Liver)] = "res://Godot/Character/Liver/selectskinbg.png",
		[typeof(Rainworld_Monk)] = "res://Godot/Character/Monk/selectskinbg.png",
	};

	/// <summary>返回某角色可选的 Spine skin 名列表。</summary>
	public static IReadOnlyList<string> GetOptions(Type characterType)
	{
		return OptionsByCharacter.TryGetValue(characterType, out var options)
			? options
			: Array.Empty<string>();
	}

	public static IReadOnlyList<string> GetOptions(CharacterModel? character)
	{
		return character == null ? Array.Empty<string>() : GetOptions(character.GetType());
	}

	/// <summary>
	/// 返回皮肤在角色选择界面显示的本地化名称；缺少对应条目时回退到内部 skin 名。
	/// 键名把 Spine 的分层路径（normal/1、normal/normal0）转换成下划线形式。
	/// </summary>
	public static string GetSkinDisplayName(string skinName)
	{
		var key = "RAINWORLD-skin." + skinName.Replace('/', '_');
		var locString = LocString.GetIfExists("settings_ui", key);
		return locString?.GetFormattedText() ?? skinName;
	}

	public static bool HasSkins(Type characterType) => OptionsByCharacter.ContainsKey(characterType);

	/// <summary>返回该角色的默认皮肤（列表首项）；没有可选皮肤时返回空字符串。</summary>
	public static string GetDefaultSkin(Type characterType)
	{
		var options = GetOptions(characterType);
		return options.Count > 0 ? options[0] : DefaultChoice;
	}



		/// <summary>
	/// 角色选择界面预览时额外向右挪的像素（预览槽位坐标系，槽宽 170）。
	/// 溪流（水猫）的骨架身子比 Bounds 偏左，按 Bounds 居中后最左边会被预览框裁掉一小条，
	/// 所以单独往右让小半个身位。想再微调只改这里的数字。
	/// </summary>
	private static readonly Dictionary<Type, float> PreviewShiftRightByCharacter = new()
	{
		[typeof(Rainworld_River)] = 42f,
	};

	/// <summary>返回该角色预览时的水平微调量（像素，未缩放前的槽位坐标）；没有配置时为 0。</summary>
	public static float GetPreviewShiftRight(Type characterType)
	{
		return PreviewShiftRightByCharacter.TryGetValue(characterType, out var shift) ? shift : 0f;
	}

	/// <summary>
	/// 个别角色的预览额外偏移（像素，预览槽位坐标；X 向右为正、Y 向下为正）。
	/// 白猫（Liver）向下 40 像素，水猫（River）向左 28 像素，红猫（Redcat）右 12 / 下 4 像素，僧侣（Monk）向右 10 像素。
	/// </summary>
	private static readonly Dictionary<Type, Vector2> PreviewOffsetByCharacter = new()
	{
		[typeof(Rainworld_Liver)] = new Vector2(0f, 40f),
		[typeof(Rainworld_River)] = new Vector2(-28f, 0f),
		[typeof(Rainworld_Redcat)] = new Vector2(12f, 4f),
		[typeof(Rainworld_Monk)] = new Vector2(10f, 0f),
	};

	/// <summary>返回该角色预览的额外偏移（未配置时为 <see cref="Vector2.Zero" />）。</summary>
	public static Vector2 GetPreviewOffset(Type characterType)
	{
		return PreviewOffsetByCharacter.TryGetValue(characterType, out var offset) ? offset : Vector2.Zero;
	}
/// <summary>返回该角色预览用的战斗外观场景路径。</summary>
	public static string? GetVisualsPath(Type characterType)
	{
		return VisualsPathByCharacter.TryGetValue(characterType, out var path) ? path : null;
	}

	/// <summary>返回该角色皮肤面板的底框贴图路径；没有配置时返回 null。</summary>
	public static string? GetSelectBackgroundPath(Type characterType)
	{
		return SelectBackgroundByCharacter.TryGetValue(characterType, out var path) ? path : null;
	}

	/// <summary>
	/// 角色选择界面预览时要叠加的皮肤层（不含 default 打底）。
	/// 水猫的手里道具和帽子分属不同槽位，预览时也要把默认水球一起叠上，才和战斗里的外观一致。
	/// </summary>
	public static string[] GetPreviewSkins(Type characterType, string selectedSkin)
	{
		return characterType == typeof(Rainworld_River)
			? [selectedSkin, RiverDefaultItemSkin]
			: [selectedSkin];
	}

	/// <summary>读取本地玩家为该角色选择的皮肤；无效或未选择时回退到列表首个（即角色默认皮肤）。</summary>
	public static string GetSelectedSkin(CharacterModel? character)
	{
		if (character == null)
			return DefaultChoice;

		var options = GetOptions(character.GetType());
		if (options.Count == 0)
			return DefaultChoice;

		var stored = character switch
		{
			Rainworld_Redcat => RainworldModConfig.RedcatSkin,
			Rainworld_River => RainworldModConfig.RiverSkin,
			Rainworld_Liver => RainworldModConfig.LiverSkin,
			Rainworld_Monk => RainworldModConfig.MonkSkin,
			_ => DefaultChoice,
		};

		return IsValidSkin(options, stored) ? stored : options[0];
	}

	/// <summary>
	/// 读取某玩家应显示的外观皮肤。
	/// 联机时皮肤随每名玩家的局内数据同步，所以队友会显示队友自己选的皮肤；
	/// 只有本机玩家缺少同步数据（旧存档）时才回退到本地设置，远程玩家则回退到角色默认皮肤。
	/// </summary>
	public static string GetSelectedSkin(Player? player)
	{
		if (player == null)
			return DefaultChoice;

		var character = player.Character;
		var options = GetOptions(character.GetType());
		if (options.Count == 0)
			return DefaultChoice;

		var synced = GetSyncedSkin(player);
		if (IsValidSkin(options, synced))
			return synced;

		return IsLocalPlayer(player)
			? GetSelectedSkin(character)
			: options[0];
	}

	/// <summary>读取该玩家随局内数据同步过来的皮肤；没有数据时返回空。</summary>
	private static string GetSyncedSkin(Player player)
	{
		if (player.RunState is not RunState runState)
			return DefaultChoice;

		return SlugcatResources.Data.TryGet(runState, player.NetId, out var data)
			? data.Skin
			: DefaultChoice;
	}

	/// <summary>该玩家是否就是本机玩家；只有本机玩家才能在缺少同步数据时沿用本地皮肤设置。</summary>
	public static bool IsLocalPlayer(Player player)
	{
		return RunManager.Instance?.NetService?.NetId == player.NetId;
	}

	/// <summary>皮肤名是否属于该角色的可选列表。</summary>
	private static bool IsValidSkin(IReadOnlyList<string> options, string? skinName)
	{
		if (string.IsNullOrEmpty(skinName))
			return false;

		for (var i = 0; i < options.Count; i++)
		{
			if (options[i] == skinName)
				return true;
		}

		return false;
	}

	/// <summary>记录该角色选择的皮肤（角色选择界面切换时调用）。</summary>
	public static void SetSelectedSkin(Type characterType, string skinName)
	{
		if (characterType == typeof(Rainworld_Redcat))
			RainworldModConfig.RedcatSkin = skinName;
		else if (characterType == typeof(Rainworld_River))
			RainworldModConfig.RiverSkin = skinName;
		else if (characterType == typeof(Rainworld_Liver))
			RainworldModConfig.LiverSkin = skinName;
		else if (characterType == typeof(Rainworld_Monk))
			RainworldModConfig.MonkSkin = skinName;
	}

	/// <summary>
	/// 等 Spine 骨架就绪后应用玩家选择的皮肤。未选择皮肤、骨架加载失败或皮肤名不存在时保持原样。
	/// </summary>
	public static void ApplyWhenReady(Node host, NCreatureVisuals? visuals, CharacterModel? character)
	{
		var skinName = GetSelectedSkin(character);
		if (string.IsNullOrEmpty(skinName))
			return;

		ApplyWhenReady(host, visuals, skinName);
	}

	/// <summary>等 Spine 骨架就绪后应用该玩家同步过来的皮肤（联机时队友各用自己的选择）。</summary>
	public static void ApplyWhenReady(Node host, NCreatureVisuals? visuals, Player? player)
	{
		var skinName = GetSelectedSkin(player);
		if (string.IsNullOrEmpty(skinName))
			return;

		ApplyWhenReady(host, visuals, skinName);
	}

	/// <summary>
	/// 把本机玩家选的皮肤补写进「随局同步」的玩家数据。
	/// 角色选择界面已经把选择暂存到大厅数据里，这里只是兜底：旧存档、中途加入、
	/// 或大厅暂存因为退出重进而丢失时，进战斗补写一次，队友就能读到本机的选择。
	/// 只写本机自己那一条（远程玩家由他们自己的客户端写），值没变时不推送。
	/// </summary>
	public static void SyncLocalSkin(Player? player)
	{
		if (player?.RunState is not RunState runState)
			return;
		if (!IsLocalPlayer(player))
			return;

		var skinName = GetSelectedSkin(player.Character);
		if (string.IsNullOrEmpty(skinName))
			return;
		if (GetSyncedSkin(player) == skinName)
			return;

		SlugcatResources.Data.Modify(runState, player.NetId, data => data.Skin = skinName);
	}

	/// <summary>
	/// 远程玩家的皮肤要等同步数据到达才能确定，可能比场景节点晚：先按当前数据套一次，
	/// 之后每 0.5 秒复查一次「解析出的皮肤组合」，一旦变化就重新套用（最多 20 次 ≈ 10 秒）。
	/// 本机玩家直接用本地设置，不需要等待。
	/// </summary>
	public static void ApplyWithRetry(Node host, NCreatureVisuals? visuals, Player? player, Func<string[]> resolve)
	{
		ApplyWhenReady(host, visuals, resolve());

		if (player == null || IsLocalPlayer(player))
			return;

		_ = RetryApplyAsync(host, visuals, ApplyKey(resolve()), resolve);
	}

	private static string ApplyKey(string[] skins) => string.Join("|", skins);

	private static async Task RetryApplyAsync(Node host, NCreatureVisuals? visuals, string applied, Func<string[]> resolve)
	{
		var tree = host.GetTree();
		if (tree == null)
			return;

		for (var i = 0; i < 20; i++)
		{
			await host.ToSignal(tree.CreateTimer(0.5), SceneTreeTimer.SignalName.Timeout);

			if (!GodotObject.IsInstanceValid(host) || visuals == null)
				return;

			var next = ApplyKey(resolve());
			if (next == applied)
				continue;

			applied = next;
			ApplyWhenReady(host, visuals, resolve());
		}
	}
	/// <summary>等 Spine 骨架就绪后按顺序叠加多套皮肤（default 会自动打底）。</summary>
	public static void ApplyWhenReady(Node host, NCreatureVisuals? visuals, params string[] skinNames)
	{
		if (visuals?.SpineBody == null)
			return;

		var sprite = visuals.SpineBody;
		host.RunWhenSpineReady(sprite, _ => Apply(sprite, skinNames));
	}

	/// <summary>
	/// 角色选择界面的预览：等骨架就绪后切换皮肤并循环播放待机动画。
	/// 战斗中的动画由 NCreatureAnimator 驱动，不能走这里。
	/// </summary>
	public static void ApplyPreview(Node host, MegaSprite sprite, params string[] skinNames)
	{
		host.RunWhenSpineReady(sprite, state =>
		{
			Apply(sprite, skinNames);
			var idle = FindIdleAnimation(sprite);
			if (idle != null)
				state.SetAnimation(idle, true, 0);
		});
	}

	private static string? FindIdleAnimation(MegaSprite sprite)
	{
		if (sprite.HasAnimation("idle_loop"))
			return "idle_loop";
		if (sprite.HasAnimation("idle"))
			return "idle";
		return null;
	}

	/// <summary>
	/// 把 default 打底与指定部件皮肤合并后应用到骨架。返回是否至少套上了一层。
	/// 每套皮肤只包含自己负责的槽位（帽子、耳朵、手中道具），合并后互不覆盖、不会隐藏其它部件。
	/// </summary>
	public static bool Apply(MegaSprite sprite, params string[] skinNames)
	{
		var skeleton = sprite.GetSkeleton();
		if (skeleton == null)
			return false;

		var data = skeleton.GetData();

		var layers = new List<string> { BaseSkinName };
		foreach (var skinName in skinNames)
		{
			if (!string.IsNullOrEmpty(skinName))
				layers.Add(skinName);
		}

		// 以层组合为名缓存合并结果：同一套组合只建一次，来回切换皮肤不会反复往骨架数据里塞新 skin。
		var mergedName = "RainWorld:Merged:" + string.Join("+", layers);
		var merged = data.FindSkin(mergedName);
		if (merged == null)
		{
			foreach (var layer in layers)
			{
				var skin = data.FindSkin(layer);
				if (skin == null)
				{
					if (layer != BaseSkinName)
						GD.PushWarning($"[SlugcatTheSpireII] Spine 骨架中找不到皮肤「{layer}」，已跳过该部件。");
					continue;
				}

				if (merged == null)
				{
					merged = sprite.NewSkin(mergedName);
				}

				merged.AddSkin(skin);
			}
		}

		if (merged == null)
			return false;

		skeleton.SetSkin(merged);
		skeleton.SetSlotsToSetupPose();
		return true;
	}
}
