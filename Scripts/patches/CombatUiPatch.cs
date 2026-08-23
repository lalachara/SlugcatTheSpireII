﻿using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Rooms;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.RunData;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Patches;

public static class CombatUiPatch
{
	public static bool clearflag = false;
	public static Creature creature;
	public static int SleepCD = 0;
	internal static NAutoButton? _SleepButton;
	internal static WorkLevelIndicator? _workLevelIndicator;
	internal static CorruptionIndicator? _corruptionIndicator;
	public static bool wawawa = false;
	internal static bool IsInstanceValid(GodotObject? obj) => obj != null && GodotObject.IsInstanceValid(obj);

	public static void UpdateWorkLevel(int newWorkLevel)
	{
		if (!IsInstanceValid(_workLevelIndicator)) return;
		try
		{
			_workLevelIndicator.UpdateWorkLevel(newWorkLevel);
		}
		catch
		{
		}
	}

	public static void callTurnStart()
	{
		if(SleepCD > 0)
			SleepCD--;
		if(SleepCD == 0)
			SetSleepButton(SlugcatData.cansleep(creature.Player));
		if(SleepCD == 1)
			SetSleepButtonDown1();
		SlugcatData.callTurnStart(creature.Player);
	}
	public static void UpdateCorruption(int value,Player player)
	{
		if (IsInstanceValid(_corruptionIndicator)&&player==creature.Player)
			_corruptionIndicator!.UpdateCorruption(value);
	}
	
	public static async Task callVictory(Player player)
	{
		wawawa = false;
		SleepCD = 0;
		await SlugcatData.callVictory(player);
	}

	public static void callSleep()
	{
		if (SlugcatData.cansleep(creature.Player))
		{
			SlugcatData.trysleep(creature.Player);
            			SleepCD = 3;
            			if (IsInstanceValid(_SleepButton))
            				_SleepButton.SetState(NAutoButton.ButtonState.Down2);
		}
		else
		{
			SetSleepButton(false);
		}
		
	}

	public static void callDead()
	{
	}

	// public static void update(Player player)
	// {
	// 	Setfood(SecondaryResourceCmd.Get(player, SlugcatResources.FoodId));
	// 	SetSleepfood(SecondaryResourceCmd.Get(player, SlugcatResources.SleepFoodId));
	// 	UpdateWorkLevel(SecondaryResourceCmd.Get(player, SlugcatResources.WorkLevelId));
	// }

	public static void SetSleepButton(bool enabled)
	{
		if (IsInstanceValid(_SleepButton))
			_SleepButton.SetState(enabled ? NAutoButton.ButtonState.Active : NAutoButton.ButtonState.Down);
	}

	public static void Setfood(int food,Player player)
	{
		if(player.Creature!=creature)
			return;
		if (IsInstanceValid(_workLevelIndicator))
			_workLevelIndicator.SetCurrentFood(food);
		if(SleepCD ==0)
			SetSleepButton(SlugcatData.cansleep(creature.Player));
	}

	public static void SetWorklevel(int level, Player player)
	{
		if (CombatUiPatch._workLevelIndicator != null && player.RunState.CurrentRoom is CombatRoom&&player==creature.Player)
		{
			CombatUiPatch._workLevelIndicator.UpdateWorkLevel(level);
			//player.Creature.SetCurrentHpInternal(player.Creature.MaxHp);
		}
	}
		public static void SetCorruptValue(int value, Player player)
		{
			if (CombatUiPatch._corruptionIndicator != null && player.RunState.CurrentRoom is CombatRoom&&player==creature.Player)
			{
				CombatUiPatch._corruptionIndicator.UpdateCorruption(value);
				if(player.Creature.HasPower<RedmushroomPower>())
					CombatUiPatch._corruptionIndicator.setRedmush(true);
				else
					CombatUiPatch._corruptionIndicator.setRedmush(false);
			}
		}

	public static void SetSleepfood(int food)
	{
		if (IsInstanceValid(_workLevelIndicator))
			_workLevelIndicator.SetSleepFood(food);
	}

	public static void SetSleepButtonDown1()
	{
		if (IsInstanceValid(_SleepButton))
			_SleepButton.SetState(NAutoButton.ButtonState.Down1);
	}
}

public sealed class CombatUiReadyPatch : IPatchMethod
{
	public static string PatchId => "rainworld_combat_ui_ready";
	public static string Description => "战斗 UI 就绪时加载模组按钮和业力指示器";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
		[new(typeof(NCombatUi), "_Ready")];

	public static void Postfix(NCombatUi __instance)
	{
		try
		{
			CombatUiPatch.SleepCD = 0;
			if (CombatUiPatch._SleepButton == null || !CombatUiPatch.IsInstanceValid(CombatUiPatch._SleepButton))
			{
				var buttonScenePath = "res://Godot/CombatUI/Test.tscn";
				var buttonPackedScene = GD.Load<PackedScene>(buttonScenePath);
				CombatUiPatch._SleepButton = buttonPackedScene.Instantiate<NAutoButton>();
				CombatUiPatch._SleepButton.Name = "MyModButton";
				CombatUiPatch._SleepButton.LayoutMode = 1;

				__instance.AddChild(CombatUiPatch._SleepButton);
				CombatUiPatch._SleepButton.SetAnchorsPreset(Control.LayoutPreset.BottomLeft);
				CombatUiPatch._SleepButton.OffsetLeft = 60;
				CombatUiPatch._SleepButton.OffsetTop = -550;
				CombatUiPatch._SleepButton.OffsetRight = 160;
				CombatUiPatch._SleepButton.OffsetBottom = -450;
				CombatUiPatch._SleepButton.TreeExiting += () => CombatUiPatch._SleepButton = null;
			}

			if (CombatUiPatch._workLevelIndicator == null || !CombatUiPatch.IsInstanceValid(CombatUiPatch._workLevelIndicator))
			{
				var indicatorScenePath = "res://Godot/CombatUI/WorkLevel.tscn";
				var indicatorPackedScene = GD.Load<PackedScene>(indicatorScenePath);
				CombatUiPatch._workLevelIndicator = indicatorPackedScene.Instantiate<WorkLevelIndicator>();
				CombatUiPatch._workLevelIndicator.Name = "WorkLevelIndicator";

				CombatUiPatch._workLevelIndicator.LayoutMode = 1;
				CombatUiPatch._workLevelIndicator.SetAnchorsPreset(Control.LayoutPreset.BottomLeft);
				CombatUiPatch._workLevelIndicator.OffsetLeft = 40;
				CombatUiPatch._workLevelIndicator.OffsetTop = -450;
				CombatUiPatch._workLevelIndicator.OffsetRight = 160;
				CombatUiPatch._workLevelIndicator.OffsetBottom = -330;

				__instance.AddChild(CombatUiPatch._workLevelIndicator);
				CombatUiPatch._workLevelIndicator.TreeExiting += () => CombatUiPatch._workLevelIndicator = null;
			}
				if (CombatUiPatch._corruptionIndicator == null || !CombatUiPatch.IsInstanceValid(CombatUiPatch._corruptionIndicator))
			{
				CombatUiPatch._corruptionIndicator = new CorruptionIndicator();
				CombatUiPatch._corruptionIndicator.Name = "CorruptionIndicator";
				CombatUiPatch._corruptionIndicator.LayoutMode = 1;
				CombatUiPatch._corruptionIndicator.SetAnchorsPreset(Control.LayoutPreset.BottomLeft);
				CombatUiPatch._corruptionIndicator.OffsetLeft = 150;
				CombatUiPatch._corruptionIndicator.OffsetTop = -600;
				CombatUiPatch._corruptionIndicator.OffsetRight = 450;
				CombatUiPatch._corruptionIndicator.OffsetBottom = -300;

				__instance.AddChild(CombatUiPatch._corruptionIndicator);
				CombatUiPatch._corruptionIndicator.TreeExiting += () => CombatUiPatch._corruptionIndicator = null;
			}
		}
		catch (System.Exception ex)
		{
		}
	}
}

public sealed class CombatUiActivatePatch : IPatchMethod
{
	public static string PatchId => "rainworld_combat_ui_activate";
	public static string Description => "战斗 UI 激活时初始化蛞蝓猫 UI 元素";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
		[new(typeof(NCombatUi), "Activate")];

	public static void Postfix(NCombatUi __instance, CombatState state)
	{
		CombatUiPatch.creature = LocalContext.GetMe(state).Creature;
		if (CombatUiPatch.creature.Player.Character is Slugcat)
		{
			if (CombatUiPatch.IsInstanceValid(CombatUiPatch._SleepButton)) 
				CombatUiPatch._SleepButton.Enable();
			if (CombatUiPatch.IsInstanceValid(CombatUiPatch._workLevelIndicator))
			{
				CombatUiPatch._workLevelIndicator.Visible = true;

				var player = CombatUiPatch.creature.Player;
				
				//SlugcatData.Initialize(player);
				var data = SlugcatResources.Data.Get(player);
				int workLevel = data.WorkLevel;
				int maxFood = data.MaxFood;
				int sleepFood = data.SleepFood;
				int food = data.Food;

				CombatUiPatch._workLevelIndicator.UpdateWorkLevel(workLevel);
				CombatUiPatch._workLevelIndicator.SetFoodData(maxFood, sleepFood, food);
			} 
			if (CombatUiPatch.creature.Player.Character is Rainworld_Redcat)
			{
				if (CombatUiPatch.IsInstanceValid(CombatUiPatch._corruptionIndicator))
				{
					CombatUiPatch._corruptionIndicator.Visible = true;
					//var corruption = SlugcatData.GetCorruption(player);
					//CombatUiPatch._corruptionIndicator.UpdateCorruption(corruption);
				}
			}
			else
			{
				if (CombatUiPatch.IsInstanceValid(CombatUiPatch._corruptionIndicator))
					CombatUiPatch._corruptionIndicator.Visible = false;
			}
		}
		else
		{
			if (CombatUiPatch.IsInstanceValid(CombatUiPatch._SleepButton)) CombatUiPatch._SleepButton.Visible = false;
			if (CombatUiPatch.IsInstanceValid(CombatUiPatch._workLevelIndicator)) CombatUiPatch._workLevelIndicator.Visible = false;
			if (CombatUiPatch.IsInstanceValid(CombatUiPatch._corruptionIndicator)) CombatUiPatch._corruptionIndicator.Visible = false;
		}
	}
}

public sealed class CombatUiDeactivatePatch : IPatchMethod
{
	public static string PatchId => "rainworld_combat_ui_deactivate";
	public static string Description => "战斗 UI 停用时隐藏模组元素";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
		[new(typeof(NCombatUi), "Deactivate")];

	public static void Postfix()
	{
		if (CombatUiPatch.IsInstanceValid(CombatUiPatch._SleepButton)) CombatUiPatch._SleepButton.Disable();
		if (CombatUiPatch.IsInstanceValid(CombatUiPatch._workLevelIndicator)) CombatUiPatch._workLevelIndicator.Visible = false;
	}
}

// public sealed class CombatUiOnCombatWonPatch : IPatchMethod
// {
// 	public static string PatchId => "rainworld_combat_ui_on_combat_won";
// 	public static string Description => "战斗胜利时触发蛞蝓猫逻辑";
// 	public static bool IsCritical => false;
//
// 	public static ModPatchTarget[] GetTargets() =>
// 		[new(typeof(NCombatUi), "OnCombatWon")];
//
// 	public static void Postfix()
// 	{
// 			CombatUiPatch.callVictory();
//
// 	}
//
// 	
// }