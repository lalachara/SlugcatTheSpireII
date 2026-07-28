using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts;

public partial class MushblockOverlay : Control
{
    private const string LogoPath = "res://Resource/Powers/RedCat/MushBlockPower128.png";
    private const float LogoSize = 18f;
    private const float FontSize = 14f;

    private TextureRect _logoRect = null!;
    private Label _amountLabel = null!;
    private NCreature? _creatureNode;
    private Texture2D? _logoTexture;
    private int _lastAmount = -1;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;

        _logoTexture = GD.Load<Texture2D>(LogoPath);

        _logoRect = new TextureRect
        {
            Texture = _logoTexture,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            Size = new Vector2(LogoSize, LogoSize),
            Position = new Vector2(0, 0),
        };
        AddChild(_logoRect);

        _amountLabel = new Label
        {
            Position = new Vector2(LogoSize + 2f, 1f),
            Size = new Vector2(24f, LogoSize),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            AddThemeColorOverride("font_color", Colors.White),
            AddThemeColorOverride("font_outline_color", new Color(0f, 0f, 0f, 0.7f)),
            AddThemeConstantOverride("outline_size", 8),
            AddThemeFontSizeOverride("font_size", (int)FontSize),
        };
        AddChild(_amountLabel);

        _creatureNode = FindCreatureNode();
        Visible = false;
    }

    public override void _Process(double delta)
    {
        if (_creatureNode == null || !GodotObject.IsInstanceValid(_creatureNode))
        {
            _creatureNode = FindCreatureNode();
            if (_creatureNode == null) return;
        }

        var creature = _creatureNode.Entity;
        if (creature == null) return;

        var power = creature.GetPower<MushblockPower>();
        var amount = power?.Amount ?? 0;

        if (amount != _lastAmount)
        {
            _lastAmount = amount;
            Visible = amount > 0;
            if (amount > 0)
            {
                _amountLabel.Text = amount.ToString();
            }
        }
    }

    private NCreature? FindCreatureNode()
    {
        var parent = GetParent();
        while (parent != null)
        {
            if (parent is NCreature creature)
                return creature;
            parent = parent.GetParent();
        }
        return null;
    }
}