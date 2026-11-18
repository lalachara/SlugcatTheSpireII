using System;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using Rainworld.Scripts;

namespace Rainworld.Scripts.Skins;

/// <summary>
/// 角色选择界面里的 Spine 皮肤选择（参考 Goldenglow 的 SkinSelect）：
/// 只显示当前选中皮肤的单张预览 + 左右箭头，选择结果写入 ModDataStore，
/// 战斗开始时由各角色的 NCreatureVisuals 读取。
///
/// 本项目是「同一套骨架内切 skin」，所以预览直接复用角色的战斗外观场景，
/// 从中取出 SpineSprite 后丢弃其余节点，避免把角色脚本 / 点击交互带进选择界面。
/// </summary>
public sealed partial class SlugcatSkinSelect : Control
{
	/// <summary>
	/// 面板尺寸：宽高比 1.2，和四只蛞蝓猫的 selectskinbg.png（701x585 ≈ 1.198）一致，
	/// 底框铺满面板时几乎不裁切。
	/// </summary>
	private const float PanelWidth = 420f;
	private const float PanelHeight = 350f;

	/// <summary>整个面板的显示缩放：内部布局仍按 420x350 排，缩放后按钮 / 文字 / 命中区等比缩小。</summary>
	private const float PanelScale = 0.7f;

	/// <summary>面板整体上移的像素（约等于未缩放时的面板高度）。想调高低调这个常量。</summary>
	private const float PanelLift = 350f;
	private const float PanelMargin = 40f;

	private const float SlotSizeX = 250f;
	private const float SlotSizeY = 270f;
	private const float SlotLeft = (PanelWidth - SlotSizeX) / 2f;
	private const float SlotTop = 34f;

	/// <summary>
	/// 预览的统一缩放倍率：把角色「游戏内尺寸」整体缩到预览框高度，四只蛞蝓猫用同一个倍率。
	/// 数字调大 = 预览里的 spine 更大（底框尺寸不受影响）。
	/// 不能按各自的 Bounds 高度归一化——Bounds 是战斗里的点击判定框，Monk 的只有其它角色的一半，
	/// 按它算会把 Monk 的预览放大一倍（战斗渲染不使用 Bounds，所以游戏内看着正常）。
	/// </summary>
	private const float PreviewFit = (SlotSizeY - 6f) / 300f * 1.3f;   // 1.3 倍放大

	private static readonly Color GoldColor = new(0.937255f, 0.784314f, 0.317647f);

	private readonly Control _previewHost;
	private readonly Label _titleLabel;
	private readonly Label _skinLabel;
	private readonly TextureButton _leftButton;
	private readonly TextureButton _rightButton;
	private readonly NinePatchRect _backgroundFallback;
	private readonly TextureRect _backgroundImage;

	private Type? _characterType;
	private StartRunLobby? _lobby;
	private string[] _skins = Array.Empty<string>();
	private MegaSprite? _previewSprite;
	private int _index;
	private bool _built;

	public SlugcatSkinSelect()
	{
		Name = "SlugcatSkinSelect";
		MouseFilter = MouseFilterEnum.Ignore;
		SetAnchorsPreset(LayoutPreset.CenterRight);
		OffsetLeft = -PanelWidth - PanelMargin;
		OffsetTop = -PanelHeight / 2f - PanelLift;
		OffsetRight = -PanelMargin;
		OffsetBottom = PanelHeight / 2f - PanelLift;

		// 整体缩放：内部布局/子节点坐标保持 420x350，视觉与命中区按 0.7 等比缩小；
		// 轴心放在右上角，缩放后贴着屏幕右侧收，不会往中间飘。
		Scale = new Vector2(PanelScale, PanelScale);
		PivotOffset = new Vector2(PanelWidth, 0f);

		// 底框（fallback）：原版九宫格深色底，只在角色专属底图还没进 pck 时露出来
		_backgroundFallback = new NinePatchRect
		{
			Name = "Background",
			MouseFilter = MouseFilterEnum.Ignore,
			Modulate = new Color(0f, 0f, 0f, 0.5f),
			Texture = GD.Load<Texture2D>("res://images/ui/fuzzy_nine_patch_char_select.png"),
			PatchMarginLeft = 42,
			PatchMarginTop = 42,
			PatchMarginRight = 42,
			PatchMarginBottom = 42,
		};
		_backgroundFallback.SetAnchorsPreset(LayoutPreset.FullRect);
		AddChild(_backgroundFallback);

		// 底框（正式）：各角色自己的 res://Godot/Character/<角色>/selectskinbg.png，
		// 由 ApplyBackground 在重建面板时按当前角色换上；填充方式为等比铺满（几乎不裁切）。
		_backgroundImage = new TextureRect
		{
			Name = "BackgroundImage",
			MouseFilter = MouseFilterEnum.Ignore,
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
			Visible = false,
		};
		_backgroundImage.SetAnchorsPreset(LayoutPreset.FullRect);
		AddChild(_backgroundImage);

		_titleLabel = new Label
		{
			Name = "Title",
			MouseFilter = MouseFilterEnum.Ignore,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			Text = new LocString("settings_ui", "RAINWORLD-skin.title").GetFormattedText(),
		};
		// 标题居中显示在选择框正上方，字号加大
		_titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_titleLabel.Position = new Vector2(SlotLeft, 2f);
		_titleLabel.Size = new Vector2(SlotSizeX, 38f);
		ApplyGoldTextStyle(_titleLabel, 30);
		AddChild(_titleLabel);

		_previewHost = new Control
		{
			Name = "Preview",
			MouseFilter = MouseFilterEnum.Ignore,
			ClipContents = true,
			Position = new Vector2(SlotLeft, SlotTop),
			Size = new Vector2(SlotSizeX, SlotSizeY),
		};
		AddChild(_previewHost);

		_leftButton = CreateArrow("res://images/atlases/ui_atlas.sprites/settings_tiny_left_arrow.tres", "LeftButton");
		_leftButton.Position = new Vector2(6f, SlotTop + SlotSizeY / 2f - 30f);
		_leftButton.Pressed += () => Rotate(-1);
		AddChild(_leftButton);

		_rightButton = CreateArrow("res://images/atlases/ui_atlas.sprites/settings_tiny_right_arrow.tres", "RightButton");
		_rightButton.Position = new Vector2(PanelWidth - 46f, SlotTop + SlotSizeY / 2f - 30f);
		_rightButton.Pressed += () => Rotate(1);
		AddChild(_rightButton);

		_skinLabel = new Label
		{
			Name = "SkinName",
			MouseFilter = MouseFilterEnum.Ignore,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
		};
		// 皮肤名贴底边，字号略小，让中间的 spine 占得更满
		_skinLabel.Position = new Vector2(PanelMargin, 304f);
		_skinLabel.Size = new Vector2(PanelWidth - PanelMargin * 2f, 40f);
		ApplyGoldTextStyle(_skinLabel, 26);
		AddChild(_skinLabel);

		Visible = false;
	}

	/// <summary>由 NCharacterSelectScreen.SelectCharacter 的 Postfix 调用。</summary>
	public static void Attach(NCharacterSelectScreen screen, CharacterModel? character, bool locked)
	{
		var panel = screen.GetNodeOrNull<SlugcatSkinSelect>("SlugcatSkinSelect");
		if (panel == null)
		{
			if (character == null)
				return;

			panel = new SlugcatSkinSelect();
			screen.AddChild(panel);
		}

		panel.Configure(screen.Lobby, character, locked);
	}

	private void Configure(StartRunLobby? lobby, CharacterModel? character, bool locked)
	{
		_lobby = lobby;

		if (character == null || locked || !SlugcatSkins.HasSkins(character.GetType()))
		{
			ClearPreviews();
			Visible = false;
			return;
		}

		var type = character.GetType();
		var options = SlugcatSkins.GetOptions(type);
		if (options.Count == 0)
		{
			ClearPreviews();
			Visible = false;
			return;
		}

		Visible = true;

		if (!_built || _characterType != type)
			Rebuild(type, options);

		var selected = SlugcatSkins.GetSelectedSkin(character);
		_index = IndexOf(selected);
		ApplyCurrent();
		UpdateLabel();
		StageSelectedSkin();
	}

	private void Rebuild(Type type, System.Collections.Generic.IReadOnlyList<string> options)
	{
		_characterType = type;
		_skins = new string[options.Count];
		for (var i = 0; i < options.Count; i++)
			_skins[i] = options[i];

		ClearPreviews();
		ApplyBackground(type);

		var visualsPath = SlugcatSkins.GetVisualsPath(type);
		if (visualsPath == null)
		{
			Visible = false;
			return;
		}

		var preview = CreatePreview(type, visualsPath);
		if (preview == null)
		{
			Visible = false;
			return;
		}

		_previewHost.AddChild(preview);
		_previewSprite = new MegaSprite(preview);
		_built = true;
	}

	private static Node2D? CreatePreview(Type characterType, string visualsPath)
	{
		var scene = GD.Load<PackedScene>(visualsPath);
		var root = scene?.Instantiate<Node2D>();
		if (root == null)
			return null;

		// 只保留 SpineSprite：角色脚本（点击叫声、电池皮肤等）不会跟着进入选择界面。
		var visuals = root.FindChild("Visuals", recursive: true, owned: false) as Node2D;
		if (visuals == null)
		{
			root.Free();
			return null;
		}

		// Bounds 只用来定位（脚底与水平中心），缩放统一走 PreviewFit：它的高度在 Monk 场景里并不代表实际画面大小。
		var bounds = root.FindChild("Bounds", recursive: true, owned: false) as Control;
		var boundsRect = bounds?.GetRect() ?? new Rect2();

		root.RemoveChild(visuals);
		visuals.Owner = null;
		root.Free();

		var fit = PreviewFit;
		visuals.Scale *= fit;
		visuals.Name = "Visuals";

		// 有些角色的骨架比 Bounds 偏左（溪流最明显），按 Bounds 居中会被预览框裁掉左边一小条，
		// 这里按角色给一个向右的补偿量（单位是未缩放前的槽位像素）。
		var shiftRight = SlugcatSkins.GetPreviewShiftRight(characterType);
		var extraOffset = SlugcatSkins.GetPreviewOffset(characterType);

		visuals.Position = new Vector2(
			SlotSizeX / 2f - boundsRect.GetCenter().X * fit + shiftRight + extraOffset.X,
			SlotSizeY - 6f - boundsRect.End.Y * fit + extraOffset.Y);
		return visuals;
	}

	private void ClearPreviews()
	{
		_previewSprite = null;
		foreach (var child in _previewHost.GetChildren())
		{
			_previewHost.RemoveChild(child);
			child.QueueFree();
		}

		_built = false;
	}

	/// <summary>
	/// 换上当前角色自己的皮肤面板底框（<c>res://Godot/Character/&lt;角色&gt;/selectskinbg.png</c>）。
	/// 新加的 png 若还没在 Godot 编辑器里导入（或 pck 没重新导出），<c>GD.Load</c> 返回 null，
	/// 这时保留原来的九宫格深色底，面板照常可用，不会变成空白。
	/// </summary>
	private void ApplyBackground(Type type)
	{
		var path = SlugcatSkins.GetSelectBackgroundPath(type);
		var texture = path == null ? null : GD.Load<Texture2D>(path);

		if (texture != null)
		{
			_backgroundImage.Texture = texture;
			_backgroundImage.Visible = true;
			_backgroundFallback.Visible = false;
			return;
		}

		_backgroundImage.Texture = null;
		_backgroundImage.Visible = false;
		_backgroundFallback.Visible = true;
	}

	private void ApplyCurrent()
	{
		if (_previewSprite == null || _skins.Length == 0 || _characterType == null)
			return;

		SlugcatSkins.ApplyPreview(this, _previewSprite,
			SlugcatSkins.GetPreviewSkins(_characterType, _skins[_index]));
	}

	private void Rotate(int direction)
	{
		if (!_built || _skins.Length <= 1)
			return;

		_index = (_index + direction + _skins.Length) % _skins.Length;
		ApplyCurrent();
		UpdateLabel();

		if (_characterType != null)
			SlugcatSkins.SetSelectedSkin(_characterType, _skins[_index]);

		StageSelectedSkin();
	}

	/// <summary>
	/// 把当前皮肤写入大厅的按玩家暂存数据。开局时 RitsuLib 会把这些数据提交到局内，
	/// 并随开局快照同步给所有客户端，队友的战斗外观就能读到各自选择的皮肤。
	/// </summary>
	private void StageSelectedSkin()
	{
		if (_lobby == null || _characterType == null || _skins.Length == 0)
			return;

		var skinName = _skins[_index];
		var netId = _lobby.NetService.NetId;

		// 每次切换角色都会重新 Configure，值没变时不重复推送大厅消息。
		if (SlugcatResources.Data.Lobby.TryGet(_lobby, netId, out var staged) && staged.Skin == skinName)
			return;

		SlugcatResources.Data.Lobby.Modify(_lobby, netId, data => data.Skin = skinName);
	}

	private void UpdateLabel()
	{
		_skinLabel.Text = _skins.Length == 0
			? string.Empty
			: SlugcatSkins.GetSkinDisplayName(_skins[_index]);
		_leftButton.Visible = _skins.Length > 1;
		_rightButton.Visible = _skins.Length > 1;
	}

	private int IndexOf(string skinName)
	{
		for (var i = 0; i < _skins.Length; i++)
		{
			if (_skins[i] == skinName)
				return i;
		}

		return 0;
	}

	private static TextureButton CreateArrow(string texturePath, string name)
	{
		return new TextureButton
		{
			Name = name,
			MouseFilter = MouseFilterEnum.Stop,
			FocusMode = FocusModeEnum.None,
			IgnoreTextureSize = true,
			TextureNormal = GD.Load<Texture2D>(texturePath),
			CustomMinimumSize = new Vector2(40f, 60f),
			Size = new Vector2(40f, 60f),
			StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered,
		};
	}

	private static void ApplyGoldTextStyle(Label label, int fontSize)
	{
		var font = GD.Load<Font>("res://themes/kreon_bold_shared.tres");
		if (font != null)
			label.AddThemeFontOverride("font", font);
		label.AddThemeFontSizeOverride("font_size", fontSize);
		label.AddThemeColorOverride("font_color", GoldColor);
		label.AddThemeColorOverride("font_outline_color", new Color(0f, 0f, 0f, 0.5f));
		label.AddThemeConstantOverride("outline_size", 8);
	}
}
