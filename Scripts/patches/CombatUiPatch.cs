// using BaseLib;
// using Godot;
//
// namespace demo.Scripts.patches;
//
// using Godot;
// using HarmonyLib;
// using MegaCrit.Sts2.Core.Nodes.Combat;
//
// [HarmonyPatch(typeof(NCombatUi))]
// public static class CombatUiPatch
// {
//     // 存储我们添加的按钮引用
//     private static NAutoButton? _myButton;
//
//     [HarmonyPatch("_Ready")]
//     [HarmonyPostfix]
//     public static void Postfix_Ready(NCombatUi __instance)
//     {
//         MainFile.Logger.Info("Injecting MyButton into CombatUi...");
//
//         try {
//             if (_myButton != null) return;
//             // 加载按钮场景
//             var scenePath = "res://VakuuAuto/scenes/my_button.tscn";
//             var packedScene = GD.Load<PackedScene>(scenePath);
//
//             if (packedScene == null)
//             {
//                 MainFile.Logger.Error($"Failed to load scene: {scenePath}");
//                 return;
//             }
//
//             // 实例化按钮
//             _myButton = packedScene.Instantiate<NAutoButton>();
//             _myButton.Name = "MyModButton";
//
//             // 设置布局模式为锚点模式
//             _myButton.LayoutMode = 1;
//
//             // 将按钮添加到CombatUi
//             __instance.AddChild(_myButton);
//
//             // 设置按钮位置（在EndTurnButton附近）
//             // EndTurnButton位置: offset_left = -316, offset_top = -234
//             // 我们在其左侧放置
//             _myButton.SetAnchorsPreset(Control.LayoutPreset.BottomRight);
//             _myButton.OffsetLeft = -480;
//             _myButton.OffsetTop = -234;
//             _myButton.OffsetRight = -330;
//             _myButton.OffsetBottom = -144;
//
//             MainFile.Logger.Info("MyButton injected successfully!");
//         }
//         catch (System.Exception ex)
//         {
//             MainFile.Logger.Error($"Failed to inject MyButton: {ex}");
//         }
//     }
//
//     [HarmonyPatch("Activate")]
//     [HarmonyPostfix]
//     public static void Postfix_Activate()
//     {
//         // 战斗开始时启用按钮
//         _myButton?.Enable();
//     }
//
//     [HarmonyPatch("Deactivate")]
//     [HarmonyPostfix]
//     public static void Postfix_Deactivate()
//     {
//         // 战斗结束时禁用按钮
//         _myButton?.Disable();
//     }
// }