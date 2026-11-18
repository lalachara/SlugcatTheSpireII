using Godot;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using Rainworld.Patches;
using Rainworld.Scripts.Monster.Minions;

namespace Rainworld.Scripts;

/// <summary>
/// 伙伴护甲指示器（模仿塔1 CabBlockUI）：图标正中显示「在场伙伴这一轮总共能替你吸收多少伤害」——
/// 蜥蜴拿身体顶的当前生命（休眠倒计时只剩 1 回合的按满血）+ 意图顺带给的格挡 / 临时生命；
/// 悬停显示伙伴数量、护甲总量与所有伙伴的预计总输出。
/// 显示条件只看角色（Monk），场上有没有伙伴都显示（没有伙伴就是 0）。
/// </summary>
public partial class CabBlockIndicator : NButton
{
    /// <summary>塔1 的图标（162x139）。贴图未导入时退回程序化圆底，数字仍然可读。</summary>
    [Export] private string IconPath = "res://Godot/CombatUI/resource/cabblock.png";

    /// <summary>图标显示高度（塔1 ICON_H = 70）；宽度按原图比例换算，避免拉伸。</summary>
    [Export] private float IconHeight = 70f;

    [Export] private int NumberFontSize = 30;
    [Export] private int NumberOutlineSize = 6;

    private TextureRect? _icon;
    private Panel? _fallback;
    private Label _label = null!;

    /// <summary>面板上的三个数：伙伴数量 / 护甲总量 / 预计总输出（塔1 recalc 的结果）。</summary>
    private int _companionCount;
    private int _blockTotal;
    private int _damageTotal;

    private IHoverTip? _hoverTip;
    private readonly LocString _title = new("settings_ui", "RAINWORLD-CabBlock_title");

    public override void _Ready()
    {
        ConnectSignals();
        BuildVisuals();

        // 点击隐藏 / 显示伙伴：图标淡下去表示「伙伴已被收起」，再点一次恢复
        FriendVisibility.Changed += RefreshToggleVisual;
        TreeExiting += () => FriendVisibility.Changed -= RefreshToggleVisual;
        RefreshToggleVisual();
    }

    /// <summary>
    /// 图标亮度反映伙伴显示开关：收起状态下图标整体变淡（伙伴不在场上的样子），
    /// 面板数字仍然照常计算，因为承伤与行动完全没受影响。
    /// </summary>
    private void RefreshToggleVisual()
    {
        var tint = FriendVisibility.Hidden
            ? new Color(1f, 1f, 1f, 0.4f)
            : Colors.White;

        if (_icon != null)
            _icon.Modulate = tint;
        if (_fallback != null)
            _fallback.Modulate = tint;
    }

    /// <summary>构建图标与中间数字；图标加载失败时用一个圆底代替。</summary>
    private void BuildVisuals()
    {
        Texture2D? texture = GD.Load<Texture2D>(IconPath);
        if (texture != null)
        {
            float iconWidth = IconHeight * texture.GetWidth() / texture.GetHeight();
            _icon = new TextureRect
            {
                Texture = texture,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                MouseFilter = Control.MouseFilterEnum.Ignore,
            };
            AddChild(_icon);
            // 锚在按钮中心、固定尺寸：按钮区域怎么变都保持居中
            _icon.SetAnchorsPreset(Control.LayoutPreset.Center);
            _icon.OffsetLeft = -iconWidth / 2f;
            _icon.OffsetTop = -IconHeight / 2f;
            _icon.OffsetRight = iconWidth / 2f;
            _icon.OffsetBottom = IconHeight / 2f;
        }
        else
        {
            // 新加的 png 还没在 Godot 编辑器里导入时 GD.Load 返回 null；
            // 先用圆底顶上，导入并重新导出 pck 后自动换成塔1 的图标
            Panel fallback = new() { MouseFilter = Control.MouseFilterEnum.Ignore };
            _fallback = fallback;
            StyleBoxFlat style = new()
            {
                BgColor = new Color(0.06f, 0.07f, 0.12f, 0.85f),
                BorderColor = new Color(0.85f, 0.75f, 0.35f, 0.9f),
                CornerRadiusTopLeft = 12,
                CornerRadiusTopRight = 12,
                CornerRadiusBottomLeft = 12,
                CornerRadiusBottomRight = 12,
                BorderWidthTop = 2,
                BorderWidthBottom = 2,
                BorderWidthLeft = 2,
                BorderWidthRight = 2,
            };
            fallback.AddThemeStyleboxOverride("panel", style);
            // 必须用 SetAnchorsAndOffsetsPreset：只改 anchors 会保持加入树时的 0 尺寸矩形
            fallback.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            AddChild(fallback);
        }

        _label = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        // 铺满整个按钮区域，靠 HorizontalAlignment / VerticalAlignment 居中：
        // 不能用 SetAnchorsPreset（它会把 Label 固定成加入树时的 0 尺寸小矩形，文字跑左上角）
        _label.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(_label);
        _label.AddThemeFontSizeOverride("font_size", NumberFontSize);
        _label.AddThemeColorOverride("font_color", Colors.White);
        _label.AddThemeColorOverride("font_outline_color", Colors.Black);
        _label.AddThemeConstantOverride("outline_size", NumberOutlineSize);
        _label.Text = "0";
    }

    /// <summary>每帧重算三个数（塔1 同样每帧 recalc，生命 / 力量 / 协调变化立刻反映到数字上）。</summary>
    public override void _Process(double delta)
    {
        if (!Visible)
            return;

        Player? player = CombatUiPatch.creature?.Player;
        if (player == null)
            return;

        int count = 0;
        decimal block = 0m;
        decimal damage = 0m;

        foreach (Creature friend in FriendPositions.All(player))
        {
            if (friend.Monster is not FriendPetBase pet)
                continue;

            bool dormant = pet.IsDormant;
            // 已经死亡又不在休眠倒计时里的伙伴本场不会回来，不计入（塔1 isDeadOrEscaped）
            if (!friend.IsAlive && !dormant)
                continue;

            count++;
            // 用身体顶伤害：蜥蜴当前生命；休眠倒计时只剩 1 回合的按满血算
            block += pet.ProtectCapacity;
            // 意图顺带的护甲；休眠的伙伴这一轮不行动，拿不到这份护甲
            if (!dormant)
                block += pet.IntentArmor;
            damage += pet.IntentOutput;
        }

        _companionCount = count;
        _blockTotal = (int)decimal.Ceiling(block);
        _damageTotal = (int)decimal.Ceiling(damage);

        string text = _blockTotal.ToString();
        if (_label.Text != text)
            _label.Text = text;
    }

    protected override void OnFocus()
    {
        LocString locString = new("settings_ui", "RAINWORLD-CabBlock_description");
        locString.Add("companion_count", _companionCount);
        locString.Add("block_total", _blockTotal);
        locString.Add("damage_total", _damageTotal);
        _hoverTip = new HoverTip(_title, locString);

        IEnumerable<IHoverTip> hoverTips = new[] { _hoverTip };
        NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, hoverTips);
        nHoverTipSet.GlobalPosition = GlobalPosition + new Vector2(150f, 50f);
    }

    protected override void OnUnfocus()
    {
        NHoverTipSet.Remove(this);
    }

    protected override void OnPress()
    {
        base.OnPress();
    }

    /// <summary>
    /// 点击面板：隐藏 / 显示场上所有伙伴（只改显示，承伤与自动行动照旧）。
    /// </summary>
    protected override void OnRelease()
    {
        FriendVisibility.Toggle();
    }
}
