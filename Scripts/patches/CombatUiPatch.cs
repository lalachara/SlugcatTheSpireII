using BaseLib;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using Rainworld.Scripts;

namespace Rainworld.Patches;

[HarmonyPatch(typeof(NCombatUi))]
public static class CombatUiPatch
{
    public static bool clearflag = false;
    public static Creature creature;
    
    private static NAutoButton? _myButton;
    private static WorkLevelIndicator? _workLevelIndicator;

    [HarmonyPatch("_Ready")]
    [HarmonyPostfix]
    public static void Postfix_Ready(NCombatUi __instance) {
        MainFile.Logger.Info("Injecting UI elements into CombatUi...");

        try {
            // 1. 加载测试按钮（不变）
            if (_myButton == null || !IsInstanceValid(_myButton))
            {
                var buttonScenePath = "res://Godot/CombatUI/Test.tscn";
                var buttonPackedScene = GD.Load<PackedScene>(buttonScenePath);
                _myButton = buttonPackedScene.Instantiate<NAutoButton>();
                _myButton.Name = "MyModButton";
                _myButton.LayoutMode = 1;

                __instance.AddChild(_myButton);
                _myButton.SetAnchorsPreset(Control.LayoutPreset.BottomLeft);
                _myButton.OffsetLeft = 60;
                _myButton.OffsetTop = -550;
                _myButton.OffsetRight = 160;
                _myButton.OffsetBottom = -450;
                _myButton.TreeExiting += () => _myButton = null;
                MainFile.Logger.Info("Test button injected successfully!");
            }

            // 2. 加载业力指示器（✅ 修正实例化类型，匹配NButton场景）
            if (_workLevelIndicator == null || !IsInstanceValid(_workLevelIndicator))
            {
                var indicatorScenePath = "res://Godot/CombatUI/WorkLevel.tscn";
                var indicatorPackedScene = GD.Load<PackedScene>(indicatorScenePath);
                // 直接强转WorkLevelIndicator，无需转Control
                _workLevelIndicator = indicatorPackedScene.Instantiate<WorkLevelIndicator>();
                _workLevelIndicator.Name = "WorkLevelIndicator";
                
                _workLevelIndicator.LayoutMode = 1;
                _workLevelIndicator.SetAnchorsPreset(Control.LayoutPreset.BottomLeft);
                _workLevelIndicator.OffsetLeft = 40;
                _workLevelIndicator.OffsetTop = -450;
                _workLevelIndicator.OffsetRight = 160;
                _workLevelIndicator.OffsetBottom = -330;

                __instance.AddChild(_workLevelIndicator);
                _workLevelIndicator.TreeExiting += () => _workLevelIndicator = null;
                MainFile.Logger.Info("WorkLevelIndicator injected successfully!");
            }
        }
        catch (System.Exception ex) {
            MainFile.Logger.Error($"Failed to inject UI elements: {ex}");
        }
    }

    [HarmonyPatch("Activate")]
    [HarmonyPostfix]
    public static void Postfix_Activate(NCombatUi __instance, CombatState state) {
        creature = LocalContext.GetMe(state).Creature;
        if (creature.Player.Character is Slugcat)
        {
            if (IsInstanceValid(_myButton)) _myButton.Enable();
            if (IsInstanceValid(_workLevelIndicator))
            {
                _workLevelIndicator.Visible = true;
                var data = SlugcatField.GetSlugCatDataByCreature(creature);
                data.workLevelIndicator = _workLevelIndicator;
                _workLevelIndicator.UpdateWorkLevel(data.workLevel); 
                _workLevelIndicator.SetFoodData(data.maxFood, data.sleepFood, data.food);
            }
        }
        else
        {
            if (IsInstanceValid(_myButton)) _myButton.Visible = false;
            if (IsInstanceValid(_workLevelIndicator)) _workLevelIndicator.Visible = false;
        }
    }

    [HarmonyPatch("Deactivate")]
    [HarmonyPostfix]
    public static void Postfix_Deactivate() {
        if (IsInstanceValid(_myButton)) _myButton.Disable();
        if (IsInstanceValid(_workLevelIndicator)) _workLevelIndicator.Visible = false;
    }
    
    [HarmonyPatch("OnCombatWon")]
    [HarmonyPostfix]
    public static void Postfix_OnCombatWon() => callVictory();

    private static bool IsInstanceValid(GodotObject? obj) => obj != null && GodotObject.IsInstanceValid(obj);

    public static void UpdateWorkLevel(int newWorkLevel)
    {
        if (!IsInstanceValid(_workLevelIndicator)) return;
        try { _workLevelIndicator.UpdateWorkLevel(newWorkLevel); }
        catch { MainFile.Logger.Error("Failed to update WorkLevel"); }
    }

    // 其余方法完全不变
    public static void callTurnStart() => SlugcatField.GetSlugCatDataByCreature(creature).callTurnStart();
    public static void callVictory() => SlugcatField.GetSlugCatDataByCreature(creature).callVictory();
    public static void callSleep() => SlugcatField.GetSlugCatDataByCreature(creature).trysleep();
    public static void callDead() { }
    public static void callCombatStart() => SlugcatField.GetSlugCatDataByCreature(creature).callCombatStart();

    public static void SetSleepButton(bool enabled)
    {
        if (IsInstanceValid(_myButton))
            _myButton.SetState(enabled ? NAutoButton.ButtonState.Active : NAutoButton.ButtonState.Down);
    }

    public static void Setfood(int food)
    {
        if (IsInstanceValid(_workLevelIndicator))
            _workLevelIndicator.SetCurrentFood(food);
    }

    public static void SetSleepfood(int food)
    {
        if (IsInstanceValid(_workLevelIndicator))
            _workLevelIndicator.SetSleepFood(food);
    }

    public static void SetSleepButtonDown1()
    {
        if (IsInstanceValid(_myButton))
            _myButton.SetState(NAutoButton.ButtonState.Down1);
    }
}