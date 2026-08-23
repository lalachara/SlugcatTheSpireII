using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using Rainworld.Scripts;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;

namespace Rainworld.Scripts.patches;
//用于在顶部玩家血条横幅旁显示当前蛞蝓猫玩家（含自己与队友）的业力（WorkLevel）
[RegisterNodeAttachment(
    typeof(NGlobalUi),
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
        var container = NRun.Instance?.GlobalUi?.MultiplayerPlayerContainer;
        if (container == null || container.GetChildCount() == 0) return false;

        _baseTexture = GD.Load<Texture2D>(BaseTexturePath);
        _maxLevel = (int)(_baseTexture.GetHeight() / SOURCE_ICON_SIZE) - 1;

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

        float scale = (float)ICON_SIZE / SOURCE_ICON_SIZE;

        // displayIndex 按容器子节点顺序：0=自己（容器先加自己），1=队友...，刚好对应血条横幅排列
        for (int di = 0; di < container.GetChildCount(); di++)
        {
            if (container.GetChild(di) is not NMultiplayerPlayerState state) continue;
            var player = state.Player;
            if (player.Character is not Slugcat) continue;

            var sprite = new Sprite2D
            {
                Texture = _baseTexture,
                RegionEnabled = true,
                RegionRect = new Rect2(0, 0, SOURCE_ICON_SIZE, SOURCE_ICON_SIZE),
                Scale = new Vector2(scale, scale),
            };

            AddChild(sprite);
            _karmaIcons.Add((player, sprite, di));
        }

        return _karmaIcons.Count > 0;
    }

    private float CalculateIconY(int level)
    {
        return (_maxLevel - level) * SOURCE_ICON_SIZE;
    }
}
