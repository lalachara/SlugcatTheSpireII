using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 休眠倒计时标签：伙伴倒地休眠时，直接在其身上（命中框中心）显示一个圆底数字，
/// 表示还需几回合复活（对应塔1眩晕时头上的休眠图标 + 计数）。
/// 节点挂在 NCreature.Hitbox 下，随角色一起移动，不需要额外布局逻辑。
/// </summary>
internal static class FriendDormantLabel
{
    private const string NodeName = "FriendDormantLabelNode";

    /// <summary>圆底直径（像素）。</summary>
    private const float Size = 40f;

    /// <summary>显示倒计时（已存在则更新；不存在则创建）。</summary>
    public static void Show(Creature creature, int turns) => Show(creature, turns.ToString());

    /// <summary>显示文本（永久眩晕不再显示倒计时，只用于普通蜥蜴的数字）。</summary>
    public static void Show(Creature creature, string text)
    {
        Panel? panel = GetOrCreate(creature);
        if (panel == null)
            return;
        SetText(panel, text);
        panel.Show();
    }

    /// <summary>倒计时变化时更新数字。</summary>
    public static void Update(Creature creature, int turns) => Update(creature, turns.ToString());

    /// <summary>倒计时变化时更新文本。</summary>
    public static void Update(Creature creature, string text)
    {
        Panel? panel = Find(creature);
        if (panel != null)
            SetText(panel, text);
    }

    /// <summary>复活/退场时隐藏（保留节点复用）。</summary>
    public static void Hide(Creature creature)
    {
        Find(creature)?.Hide();
    }

    private static Panel? Find(Creature creature)
    {
        Control? hitbox = creature.GetCreatureNode()?.Hitbox;
        return hitbox?.GetNodeOrNull<Panel>(NodeName);
    }

    private static Panel? GetOrCreate(Creature creature)
    {
        Control? hitbox = creature.GetCreatureNode()?.Hitbox;
        if (hitbox == null)
            return null;

        if (hitbox.GetNodeOrNull<Panel>(NodeName) is { } existing)
            return existing;

        Panel panel = new()
        {
            Name = NodeName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        // 锚在命中框中心，整体呈圆形底
        panel.AnchorLeft = 0.5f;
        panel.AnchorTop = 0.5f;
        panel.AnchorRight = 0.5f;
        panel.AnchorBottom = 0.5f;
        panel.OffsetLeft = -Size / 2f;
        panel.OffsetTop = -Size / 2f;
        panel.OffsetRight = Size / 2f;
        panel.OffsetBottom = Size / 2f;

        StyleBoxFlat style = new()
        {
            BgColor = new Color(0.06f, 0.07f, 0.12f, 0.85f),
            BorderColor = new Color(0.35f, 0.85f, 0.95f, 0.9f),
            CornerRadiusTopLeft = (int)(Size / 2f),
            CornerRadiusTopRight = (int)(Size / 2f),
            CornerRadiusBottomLeft = (int)(Size / 2f),
            CornerRadiusBottomRight = (int)(Size / 2f),
            BorderWidthTop = 2,
            BorderWidthBottom = 2,
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
        };
        panel.AddThemeStyleboxOverride("panel", style);

        Label label = new()
        {
            Name = "Count",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        label.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        label.AddThemeFontSizeOverride("font_size", 20);
        label.AddThemeColorOverride("font_color", Colors.White);
        label.AddThemeColorOverride("font_outline_color", Colors.Black);
        label.AddThemeConstantOverride("outline_size", 4);
        panel.AddChild(label);

        hitbox.AddChild(panel);
        return panel;
    }

    private static void SetText(Panel panel, string text)
    {
        if (panel.GetNodeOrNull<Label>("Count") is { } label)
            label.Text = text;
    }
}
