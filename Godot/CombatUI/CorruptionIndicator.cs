using Godot;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using Rainworld.Patches;
using STS2RitsuLib.Combat.SecondaryResources;

namespace Rainworld.Scripts;

public partial class CorruptionIndicator : NButton
{
    [Export] private string BackgroundPath = "res://Godot/CombatUI/resource/RedCatVar_K.png";
    [Export] private string BackgroundBtmPath = "res://Godot/CombatUI/resource/RedCatVar_Kbtm.png";
    [Export] private string FillPath1 = "res://Godot/CombatUI/resource/RedCatVar1.png";
    [Export] private string FillPath2 = "res://Godot/CombatUI/resource/RedCatVar2.png";
    [Export] private string FillPath3 = "res://Godot/CombatUI/resource/RedCatVar3.png";
    [Export] private string FillPath4 = "res://Godot/CombatUI/resource/RedCatVar4.png";
    [Export] private string FillPath5 = "res://Godot/CombatUI/resource/RedCatVar.png";
    [Export] private string FramePath = "res://Godot/CombatUI/resource/RedCatVar_btm.png";
    [Export] private float DisplayScale = 0.6f;
    [Export] private float FlashSpeed = 3f;
    [Export] private float MinAlpha = 0.3f;
    [Export] private float MaxAlpha = 1f;

    private TextureRect _fill, _backgroundbtm;
    private Texture2D[] _fillTextures;
    private float _time;

    private float _barWidth;
    private float _barHeight;
    private const int MaxCorruption = 5;

    private IHoverTip _hoverTip;
    private LocString _title = new LocString("settings_ui", "RAINWORLD-Corruption_title");

    public override void _Ready()
    {
        ConnectSignals();

        var bgTex = GD.Load<Texture2D>(BackgroundPath);
        _fillTextures = new Texture2D[6];
        _fillTextures[1] = GD.Load<Texture2D>(FillPath1);
        _fillTextures[2] = GD.Load<Texture2D>(FillPath2);
        _fillTextures[3] = GD.Load<Texture2D>(FillPath3);
        _fillTextures[4] = GD.Load<Texture2D>(FillPath4);
        _fillTextures[5] = GD.Load<Texture2D>(FillPath5);

        var frameTex = GD.Load<Texture2D>(FramePath);
        var bgBtmTex = GD.Load<Texture2D>(BackgroundBtmPath);

        _barWidth = _fillTextures[1].GetWidth();
        _barHeight = _fillTextures[1].GetHeight();


        var frame = new TextureRect
        {
            Texture = frameTex,
            StretchMode = TextureRect.StretchModeEnum.Keep,
        };
        AddChild(frame);

        _fill = new TextureRect
        {
            StretchMode = TextureRect.StretchModeEnum.Keep,
            Visible = false,
        };
        AddChild(_fill);



        var background = new TextureRect
        {
            Texture = bgBtmTex,
            StretchMode = TextureRect.StretchModeEnum.Keep,
        };
        AddChild(background);


        _backgroundbtm = new TextureRect
        {
            Texture = bgTex,
            StretchMode = TextureRect.StretchModeEnum.Keep,
        };
        AddChild(_backgroundbtm);
        _backgroundbtm.Visible = false;

        Size = new Vector2(_barWidth, _barHeight);
        Scale = new Vector2(DisplayScale, DisplayScale);
    }

    public override void _Process(double delta)
    {
        if (!_backgroundbtm.Visible) return;
        _time += (float)delta;
        float alpha = Mathf.Lerp(MinAlpha, MaxAlpha, (Mathf.Sin(_time * FlashSpeed) + 1f) / 2f);
        _backgroundbtm.Modulate = new Color(1, 1, 1, alpha);
    }

    protected override void OnFocus()
    {
        LocString locString = new LocString("settings_ui", "RAINWORLD-Corruption_description");
        locString.Add("corruption_value", SlugcatData.getCorruptValue(CombatUiPatch.creature.Player!));
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

    protected override void OnRelease()
    {
    }

    public void setRedmush(bool isRed)
    {
        _backgroundbtm.Visible = isRed;
        if (!isRed)
            _backgroundbtm.Modulate = Colors.White;
    }

    public void UpdateCorruption(int value)
    {
        value = Mathf.Clamp(value, 0, MaxCorruption);
        if (value == 0)
        {
            _fill.Visible = false;
        }
        else
        {
            _fill.Texture = _fillTextures[value];
            _fill.Visible = true;
        }
    }
}