using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Rainworld.Scripts;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;

namespace Rainworld.Scripts.patches;

[RegisterNodeAttachment(
    typeof(NCombatUi),
    "teammate_karma",
    NodeName = "TeammateKarmaDisplay",
    DuplicatePolicy = NodeAttachmentDuplicatePolicy.ReuseExistingByName)]
public partial class TeammateKarmaDisplay : Control, INodeAttachmentSetup
{
    private const string BaseTexturePath = "res://Godot/CombatUI/resource/WorkLevel.png";
    private const int ICON_SIZE = 40;
    private const int SOURCE_ICON_SIZE = 130;

    private const float OFFSET_X = 15f;
    private const float OFFSET_Y = 20f;

    private readonly List<(Player player, Sprite2D sprite, int displayIndex)> _karmaIcons = new();
    private Texture2D _baseTexture = null!;
    private int _maxLevel;
    private bool _initialized = false;
    private float _baseY;
    private float _baseX;
    private float _spacing;

    public void Setup(Node parent, Node node)
    {
        if (parent is Control control)
        {
            Position = Vector2.Zero;
            Size = control.Size;
        }
        MouseFilter = MouseFilterEnum.Ignore;
    }

    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
        if (!_initialized)
        {
            if (!TryInit())
                return;
            _initialized = true;
        }

        if (_karmaIcons.Count == 0) return;

        for (int i = 0; i < _karmaIcons.Count; i++)
        {
            var (player, sprite, displayIndex) = _karmaIcons[i];

            var workLevel = SlugcatData.GetWorkLevel(player);
            var clampedLevel = Mathf.Clamp(workLevel, 0, _maxLevel);
            sprite.RegionRect = new Rect2(0, CalculateIconY(clampedLevel), SOURCE_ICON_SIZE, SOURCE_ICON_SIZE);

            var globalPos = new Vector2(_baseX + OFFSET_X, _baseY + displayIndex * _spacing + OFFSET_Y);
            var localPos = GetGlobalTransform().AffineInverse() * globalPos;
            sprite.Position = localPos;
        }
    }

    private bool TryInit()
    {
        var state = CombatManager.Instance.DebugOnlyGetState();
        if (state == null) return false;

        var me = LocalContext.GetMe(state);
        if (me == null) return false;

        _baseTexture = GD.Load<Texture2D>(BaseTexturePath);
        _maxLevel = (int)(_baseTexture.GetHeight() / SOURCE_ICON_SIZE) - 1;

        var container = NRun.Instance?.GlobalUi?.MultiplayerPlayerContainer;
        if (container == null || container.GetChildCount() == 0) return false;

        GD.Print($"[TeammateKarma] === TryInit ===");
        GD.Print($"[TeammateKarma] 我是: {me.Character?.GetType().Name}");
        for (int pi = 0; pi < state.Players.Count; pi++)
            GD.Print($"[TeammateKarma]   state.Players[{pi}]: {state.Players[pi].Character?.GetType().Name}");
        for (int ci = 0; ci < container.GetChildCount(); ci++)
            GD.Print($"[TeammateKarma]   容器[{ci}]: Name={container.GetChild(ci).Name}");

        // 计算基准Y和间距——用容器子节点里最小的Y作为基准
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        float xVal = 0f;
        for (int ci = 0; ci < container.GetChildCount(); ci++)
        {
            if (container.GetChild(ci) is CanvasItem canvas)
            {
                var pos = canvas.GetGlobalTransform().Origin;
                if (pos.Y < minY) minY = pos.Y;
                if (pos.Y > maxY) maxY = pos.Y;
                xVal = pos.X;
            }
        }
        _baseY = minY;
        _baseX = xVal;
        _spacing = container.GetChildCount() > 1
            ? (maxY - minY) / (container.GetChildCount() - 1)
            : 0f;

        GD.Print($"[TeammateKarma] baseY={_baseY}, baseX={_baseX}, spacing={_spacing}");

        float scale = (float)ICON_SIZE / SOURCE_ICON_SIZE;

        // 显示顺序: [自己, state.Players排除自己按顺序]
        // displayIndex 0=自己, 1=state.Players[0](如果非自己), 2=state.Players[1](如果非自己)...
        var displayList = new List<Player>();
        displayList.Add(me);
        foreach (var p in state.Players)
        {
            if (p != me)
                displayList.Add(p);
        }

        GD.Print($"[TeammateKarma] 显示顺序:");
        for (int di = 0; di < displayList.Count; di++)
            GD.Print($"[TeammateKarma]   [{di}] {displayList[di].Character?.GetType().Name}");

        for (int di = 0; di < displayList.Count; di++)
        {
            var player = displayList[di];
            if (player.Character is not Slugcat) continue;

            var sprite = new Sprite2D
            {
                Texture = _baseTexture,
                RegionEnabled = true,
                RegionRect = new Rect2(0, 0, SOURCE_ICON_SIZE, SOURCE_ICON_SIZE),
                Scale = new Vector2(scale, scale),
            };

            AddChild(sprite);
            GD.Print($"[TeammateKarma] 创建图标: displayIndex={di}, {player.Character.GetType().Name}");
            _karmaIcons.Add((player, sprite, di));
        }

        return true;
    }

    private float CalculateIconY(int level)
    {
        return (_maxLevel - level) * SOURCE_ICON_SIZE;
    }
}