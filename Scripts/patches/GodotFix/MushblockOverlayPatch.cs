using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Rainworld.Scripts;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Patches;

public sealed class MushblockOverlayPatch : IPatchMethod
{
    public static string PatchId => "rainworld_mushblock_overlay";
    public static string Description => "为拥有菌壁能力的生物在血条上添加覆盖层";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
        [new(typeof(NCreature), "_Ready")];

    public static void Postfix(NCreature __instance)
    {
        var bounds = __instance.FindChild("Bounds", recursive: true) as Control;
        if (bounds == null) return;

        var overlay = new MushblockOverlay
        {
            Name = "MushblockOverlay",
            LayoutMode = 1,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        overlay.SetAnchorsPreset(Control.LayoutPreset.Center);
        overlay.Size = new Vector2(50f, 20f);
        overlay.Position = new Vector2(-25f, -10f);

        bounds.AddChild(overlay);
    }
}