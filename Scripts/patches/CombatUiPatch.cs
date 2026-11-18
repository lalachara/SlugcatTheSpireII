using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Rooms;
using Rainworld.Scripts;
using Rainworld.Scripts.Skins;
using Rainworld.Scripts.Monster.Minions;
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
	internal static CabBlockIndicator? _cabBlockIndicator;
	public static bool wawawa = false;
	internal static bool IsInstanceValid(GodotObject? obj) => obj != null && GodotObject.IsInstanceValid(obj);

	/// <summary>当前是否处于玩家回合。</summary>
	public static bool IsPlayerTurn()
	{
		if (creature == null || creature.CombatState == null)
			return false;
		return creature.CombatState.CurrentSide == CombatSide.Player;
	}

	/// <summary>玩家此刻是否还能行动（按下结束回合后，直到下个玩家回合开始前为 false）。</summary>
	public static bool CanAct()
	{
		CombatManager manager = CombatManager.Instance;
		return manager == null || !manager.PlayerActionsDisabled;
	}

	/// <summary>休眠按钮此刻是否允许点击。</summary>
	public static bool IsSleepAvailable()
	{
		return IsInstanceValid(_SleepButton) && IsPlayerTurn() && CanAct();
	}

	/// <summary>玩家结束回合后把休眠按钮置为不可用（不会覆盖休眠冷却中的贴图）。</summary>
	public static void SetSleepUnavailable()
	{
		if (!IsInstanceValid(_SleepButton))
			return;
		if (_SleepButton!.GetState() == NAutoButton.ButtonState.Active)
			_SleepButton.SetState(NAutoButton.ButtonState.Down);
	}

	/// <summary>玩家撤销结束回合时恢复休眠按钮。</summary>
	public static void RestoreSleepButton()
	{
		if (creature == null || !IsInstanceValid(_SleepButton))
			return;
		if (SleepCD != 0)
			return;
		SetSleepButton(SlugcatData.cansleep(creature.Player));
	}

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
		//只有玩家回合内、且还没结束回合时才刷新按钮，避免敌方回合被重新点亮
		if(SleepCD ==0 && IsPlayerTurn() && CanAct())
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

			if (CombatUiPatch._cabBlockIndicator == null || !CombatUiPatch.IsInstanceValid(CombatUiPatch._cabBlockIndicator))
			{
				CombatUiPatch._cabBlockIndicator = new CabBlockIndicator();
				CombatUiPatch._cabBlockIndicator.Name = "CabBlockIndicator";
				CombatUiPatch._cabBlockIndicator.LayoutMode = 1;
				CombatUiPatch._cabBlockIndicator.SetAnchorsPreset(Control.LayoutPreset.BottomLeft);
				// 贴着业力指示器（40~160）右边缘，垂直方向与它同心（塔1 CabBlockUI 的位置）
				CombatUiPatch._cabBlockIndicator.OffsetLeft = 140;
				CombatUiPatch._cabBlockIndicator.OffsetTop = -440;
				CombatUiPatch._cabBlockIndicator.OffsetRight = 260;
				CombatUiPatch._cabBlockIndicator.OffsetBottom = -320;

				__instance.AddChild(CombatUiPatch._cabBlockIndicator);
				CombatUiPatch._cabBlockIndicator.TreeExiting += () => CombatUiPatch._cabBlockIndicator = null;
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

		// 新一场战斗从「伙伴可见」开始：上一场如果点了收起，这里恢复显示
		FriendVisibility.Reset();

		if (CombatUiPatch.creature.Player.Character is Slugcat)
		{
			// 联机：把本机选的皮肤补写进随局同步数据（大厅暂存丢失时由这里兜底）
			SlugcatSkins.SyncLocalSkin(CombatUiPatch.creature.Player);
			//休眠按钮的可点击性完全由 SetState（贴图状态）驱动，不再在这里无条件启用
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

			// 伙伴护甲面板：Monk 专属机制（蜥蜴拿身体替主人挡伤），与场上有没有伙伴无关
			if (CombatUiPatch.IsInstanceValid(CombatUiPatch._cabBlockIndicator))
				CombatUiPatch._cabBlockIndicator.Visible = CombatUiPatch.creature.Player.Character is Rainworld_Monk;
		}
		else
		{
			if (CombatUiPatch.IsInstanceValid(CombatUiPatch._SleepButton)) CombatUiPatch._SleepButton.Visible = false;
			if (CombatUiPatch.IsInstanceValid(CombatUiPatch._workLevelIndicator)) CombatUiPatch._workLevelIndicator.Visible = false;
			if (CombatUiPatch.IsInstanceValid(CombatUiPatch._corruptionIndicator)) CombatUiPatch._corruptionIndicator.Visible = false;
			if (CombatUiPatch.IsInstanceValid(CombatUiPatch._cabBlockIndicator)) CombatUiPatch._cabBlockIndicator.Visible = false;
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
		// 战斗结束：伙伴显示开关复位，别带到下一场
		FriendVisibility.Reset();

		if (CombatUiPatch.IsInstanceValid(CombatUiPatch._SleepButton)) CombatUiPatch._SleepButton.Disable();
		if (CombatUiPatch.IsInstanceValid(CombatUiPatch._workLevelIndicator)) CombatUiPatch._workLevelIndicator.Visible = false;
		if (CombatUiPatch.IsInstanceValid(CombatUiPatch._cabBlockIndicator)) CombatUiPatch._cabBlockIndicator.Visible = false;
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