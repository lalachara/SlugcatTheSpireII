using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Events;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Patches;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches;

/// <summary>
/// 允许蛞蝓猫在业力(WorkLevel)大于1时选择会造成致死伤害的事件选项。
/// 原版在多人模式下，若事件选项的伤害 >= 当前生命值(WillKillPlayer)则会阻止选择。
/// 蛞蝓猫可以通过消耗业力复活，因此业力>1时应当允许选择。
/// </summary>
public sealed class EventOptionWorkLevelPatch : IPatchMethod
{
	public static string PatchId => "rainworld_event_option_worklevel";
	public static string Description => "蛞蝓猫业力>0时允许选择致死伤害事件选项";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
		[new(typeof(NEventOptionButton), "WillKillPlayer", Type.EmptyTypes)];

	public static bool Prefix(NEventOptionButton __instance, ref bool __result)
	{
		Player? owner = __instance.Event?.Owner;
		if (owner != null
			&& LocalContext.IsMe(owner)
			&& owner.Character is Slugcat
			&& SlugcatData.GetWorkLevel(owner) > 0)
		{
			__result = false;
			return false; // 跳过原方法，不视为致死
		}
		return true;
	}
}

/// <summary>
/// 蛞蝓猫（多人事件）受到致命伤害时，在伤害造成死亡之前直接化解为「扣1业力 + 回满血」。
/// 原版在 非战斗 && 多人 && 非强制 时 KillWithoutCheckingWinCondition 会回血到1并阻断死亡流程，
/// 导致真正的业力复活无法接管；且只绕过该方法本身，Kill/Damage 后续的死亡逻辑仍会崩溃。
/// 因此改为在 CreatureCmd.Damage 复数重载（所有 damage 的归一入口）应用伤害前拦截：
/// 若目标是非战斗多人事件中的蛞蝓猫、业力>0 且本次伤害致死，则同步扣业力 + 回满血并从 targets 剔除，
/// 使其根本不进入死亡流程——蛞蝓猫不死，Damage/Kill 的后续逻辑自然正常。
/// 业力=0 时不拦截，交给原版 WillKillPlayer 禁止选择致死选项（见 EventOptionWorkLevelPatch）。
/// 注意：不能把 force 置 true 绕过 guard——force=true 会短路 Hook.ShouldDie，直接进入真死分支。
/// </summary>
public sealed class EventOptionMultiplayerGuardPatch : IPatchMethod
{
	public static string PatchId => "rainworld_event_option_multiguard";
	public static string Description => "蛞蝓猫多人事件致命伤害时扣业力复活而非死亡";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
		// 0.113：IEnumerable 目标的 Damage 增加了一个可选参数 ModifyDamageHookType，
		// Harmony 按精确参数列表匹配，必须把它也列出来
		[new(typeof(CreatureCmd), "Damage", new[] { typeof(PlayerChoiceContext), typeof(IEnumerable<Creature>), typeof(decimal), typeof(ValueProp), typeof(Creature), typeof(CardModel), typeof(CardPlay), typeof(MegaCrit.Sts2.Core.Hooks.ModifyDamageHookType) })];

	public static bool Prefix(ref IEnumerable<Creature>? targets, decimal amount)
	{
		if (targets == null)
		{
			return true;
		}

		// 仅在非战斗（事件/房间）场景化解；战斗内蛞蝓猫走原版 RunSingleton hook 复活。
		if (CombatManager.Instance.IsInProgress)
		{
			return true;
		}

		var filtered = new List<Creature>();
		bool revivedAny = false;

		foreach (var c in targets)
		{
			bool isSlugcatPlayer = c.IsPlayer && c.Player?.Character is Slugcat;
			if (!isSlugcatPlayer)
			{
				filtered.Add(c);
				continue;
			}

			Player player = c.Player!;
			bool multi = player.RunState.Players.Count > 1;
			bool lethal = amount > 0m && c.CurrentHp <= amount;
			bool canbeKill = SlugcatData.canbekill(player);

			if (multi && SlugcatData.GetWorkLevel(player) > 0 && lethal && !canbeKill)
			{
				// 命中多人防御 guard 且应复活：从本次伤害中剔除蛞蝓猫，同步扣业力 + 回满血。
				ReLiveSync(player, amount);
				revivedAny = true;
				continue; // 不被伤害，不进入死亡流程
			}

			// 非多人 / 业力0 / 不足以致死：交给原方法处理（业力0时 WillKillPlayer 已禁止选致死选项）。
			filtered.Add(c);
		}

		if (!revivedAny)
		{
			return true;
		}

		targets = filtered;
		return true;
	}

	private static void ReLiveSync(Player player, decimal damageAmount)
	{
		Creature creature = player.Creature;
		// 回满血（同步）。SetCurrentHpInternal 会把 HP clamp 到 MaxHp。
		creature.SetCurrentHpInternal(creature.MaxHp);

		// 播放扣血数字 + 闪红，模拟业力复活时受到的"致命伤"演出。
		var dmgResult = new DamageResult(creature, ValueProp.Unblockable | ValueProp.Unpowered)
		{
			UnblockedDamage = (int)damageAmount,
		};
		var numVfx = NDamageNumVfx.Create(creature, dmgResult);
		if (numVfx != null)
		{
			var vfxContainer = creature.GetVfxContainer();
			if (vfxContainer != null)
			{
				vfxContainer.AddChildSafely(numVfx);
			}
			else
			{
				NRun.Instance?.GlobalUi?.AddChildSafely(numVfx);
			}
		}
		if (LocalContext.IsMe(player))
		{
			PlayerHurtVignetteHelper.Play();
		}

		if (creature.HasPower<WorklockPower>())
		{
			// Worklock 分支：原版 reLive 只回血并减一层锁（PowerCmd.Decrement 为异步，此处仅回血）。
			return;
		}

		// 普通分支：扣 1 点业力（等价 reLive → addworklevel(-1) 的核心，同步处理避免 await）。
		// 这条同步路径绕过了 addworklevel，音效必须自己补，否则战斗外事件扣业力全程无声。
		if (LocalContext.IsMe(player))
			SfxCmd.Play("event:/Rainworld/sfx/workleveldown");
		var data = SlugcatResources.Data.Get(player);
		int newLevel = Math.Clamp(data.WorkLevel - 1, 0, data.MaxWorkLevel);
		SlugcatResources.Data.Modify(player, d => d.WorkLevel = newLevel);
		CombatUiPatch.SetWorklevel(newLevel, player);
		SlugcatData.SavetoRelic(player);
	}
}
