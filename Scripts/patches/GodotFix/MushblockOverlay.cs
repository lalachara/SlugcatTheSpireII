using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts;

public partial class MushblockOverlay : Control
{
    private const string BasePath = "res://Resource/other/powerfix/";

    private TextureRect _mushTex = null!;
    private Label _mushLabel = null!;
    private TextureRect _iceTex = null!;
    private Label _iceLabel = null!;
    private NCreature? _creatureNode;
    private int _lastMushAmount = -1;
    private int _lastIceAmount = -1;
    private int _fontSize = 20;
    private const float iconSize = 35f;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;

        var ph = GetParent() is Control pp ? pp.Size.Y : 16f;
        var font = GD.Load<FontVariation>("res://themes/kreon_bold_shared.tres");

        var texMush = GD.Load<Texture2D>(BasePath + "mushblock.png");
        var texIce = GD.Load<Texture2D>(BasePath + "iceblock.png");
        var iconOffsetY = (ph - iconSize) / 2f - 10f;

        _mushTex = new TextureRect
        {
            Texture = texMush,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            Size = new Vector2(iconSize, iconSize),
            AnchorRight = 1f,
            AnchorLeft = 1f,
            OffsetLeft = -iconSize,
            OffsetTop = iconOffsetY,
        };
        AddChild(_mushTex);

        _mushLabel = new Label
        {
            Size = new Vector2(iconSize, iconSize),
            AnchorRight = 1f,
            AnchorLeft = 1f,
            OffsetLeft = -iconSize,
            OffsetTop = iconOffsetY,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        _mushLabel.AddThemeColorOverride("font_color", Colors.White);
        _mushLabel.AddThemeColorOverride("font_outline_color", new Color("af0000"));
        _mushLabel.AddThemeConstantOverride("outline_size", 14);
        _mushLabel.AddThemeFontOverride("font", font);
        _mushLabel.AddThemeFontSizeOverride("font_size", _fontSize);
        AddChild(_mushLabel);

        _iceTex = new TextureRect
        {
            Texture = texIce,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            Size = new Vector2(iconSize, iconSize),
            AnchorRight = 1f,
            AnchorLeft = 1f,
            OffsetLeft = -iconSize,
            OffsetTop = iconOffsetY,
        };
        AddChild(_iceTex);

        _iceLabel = new Label
        {
            Size = new Vector2(iconSize, iconSize),
            AnchorRight = 1f,
            AnchorLeft = 1f,
            OffsetLeft = -iconSize,
            OffsetTop = iconOffsetY,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        _iceLabel.AddThemeColorOverride("font_color", Colors.White);
        _iceLabel.AddThemeColorOverride("font_outline_color", new Color(0f, 0f, 0f, 0.7f));
        _iceLabel.AddThemeConstantOverride("outline_size", 14);
        _iceLabel.AddThemeFontOverride("font", font);
        _iceLabel.AddThemeFontSizeOverride("font_size", _fontSize);
        AddChild(_iceLabel);

        _creatureNode = FindCreatureNode();
        UpdateDisplay();
    }

    public override void _Process(double delta)
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (_creatureNode == null || !GodotObject.IsInstanceValid(_creatureNode))
        {
            _creatureNode = FindCreatureNode();
        }

        var mushAmount = 0;
        var iceAmount = 0;
        if (_creatureNode != null)
        {
            var creature = _creatureNode.Entity;
            foreach (var power in creature.Powers)
            {
                if (power is MushblockPower mushblock)
                {
                    mushAmount = mushblock.Amount;
                    break;
                }
                if (power is IcewaterPower icewater)
                {
                    iceAmount = icewater.Amount;
                    break;
                }
                
            }
            
        }

        
        
        var hasMush = mushAmount > 0;
        var hasIce = iceAmount > 0;

        if (hasMush)
        {
            _mushTex.OffsetLeft = -iconSize;
            _mushLabel.OffsetLeft = -iconSize;
        }
        else if (hasIce)
        {
            _iceTex.OffsetLeft = -iconSize;
            _iceLabel.OffsetLeft = -iconSize;
        }

        _mushTex.Visible = hasMush;
        _mushLabel.Visible = hasMush;
        _iceTex.Visible = hasIce;
        _iceLabel.Visible = hasIce;

        if (hasMush && mushAmount != _lastMushAmount)
        {
            _lastMushAmount = mushAmount;
            _mushLabel.Text = mushAmount.ToString();
        }

        if (hasIce && iceAmount != _lastIceAmount)
        {
            _lastIceAmount = iceAmount;
            _iceLabel.Text = iceAmount.ToString();
        }

        Visible = hasMush || hasIce;
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