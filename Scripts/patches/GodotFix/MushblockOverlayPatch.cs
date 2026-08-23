using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;
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
        var hpBarContainer = __instance.FindChild("HpBarContainer", recursive: true) as Control;
        if (hpBarContainer == null) return;

        var overlay = new MushblockOverlay
        {
            Name = "MushblockOverlay",
            LayoutMode = 1,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        overlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);

        hpBarContainer.AddChild(overlay);
    }
}

public sealed class BlockColorPatch : IPatchMethod
{
    public static string PatchId => "rainworld_overlay_color";
    public static string Description => "修改血条颜色和边框";
    public static bool IsCritical => false;

    private static readonly Color MushFgColor = new(0.55f, 0.27f, 0.07f);
    private static readonly Color MushOutlineColor = new("ffce77");

    private static readonly Color IceFgColor = new("71f7f5");
    private static readonly Color IceOutlineColor = new("d9feff");

    public static ModPatchTarget[] GetTargets() =>
        [new(typeof(NHealthBar), "RefreshBlockUi")];

    public static void Postfix(NHealthBar __instance)
    {
        bool hasMushblock = false;
        bool hasIce = false;
        var creature = Traverse.Create(__instance).Field("_creature").GetValue<Creature>();
        foreach (var power in creature.Powers)
        {
            if (power is MushblockPower mushblock)
            {
                hasMushblock = true;
                break;
            }

            if (power is IcewaterPower icewater)
            {
                hasIce = true;
                break;
            }
        }


        if (!hasMushblock && !hasIce)
            return;

        if (creature.Block == 0)
        {
            if (hasMushblock)
            {
                var hpForeground = Traverse.Create(__instance).Field("_hpForeground").GetValue<Control>();
                //if (hpForeground != null)
                    //hpForeground.SelfModulate = MushFgColor;
                var blockOutline = Traverse.Create(__instance).Field("_blockOutline").GetValue<Control>();
                if (blockOutline != null)
                {
                    blockOutline.Visible = true;
                    blockOutline.SelfModulate = MushOutlineColor;
                }
            }
            else if (hasIce)
            {
                var hpForeground = Traverse.Create(__instance).Field("_hpForeground").GetValue<Control>();
                //if (hpForeground != null)
                    //hpForeground.SelfModulate = IceFgColor;
                var blockOutline = Traverse.Create(__instance).Field("_blockOutline").GetValue<Control>();
                if (blockOutline != null)
                {
                    blockOutline.Visible = true;
                    blockOutline.SelfModulate = IceOutlineColor;
                }
            }
        }
        else
        {
            var blockOutline = Traverse.Create(__instance).Field("_blockOutline").GetValue<Control>();
            if (blockOutline != null)
                blockOutline.SelfModulate = new Color(1, 1, 1, 1);
        }
    }
}