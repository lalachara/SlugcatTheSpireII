using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Rainworld.Scripts.Monster;
using Rainworld.Scripts.Monster.MonkeyToll;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches;

internal static class EncounterFilterHelpers
{
	private static readonly AccessTools.FieldRef<ActModel, RoomSet> RoomsField =
		AccessTools.FieldRefAccess<ActModel, RoomSet>("_rooms");

	private static readonly AccessTools.FieldRef<RoomSet, EncounterModel?> BossField =
		AccessTools.FieldRefAccess<RoomSet, EncounterModel?>("_boss");

	/// <summary>判断某遭遇类型是否保留。数据取自 host 优先的生效配置。</summary>
	public static IEnumerable<EncounterModel> FilterDisabledEncounters(IEnumerable<EncounterModel>? encounters)
	{
		if (encounters == null)
			return Enumerable.Empty<EncounterModel>();

		var cfg = SpawnConfigRuntime.GetEffectiveConfig();

		return encounters.Where(e =>
		{
			var t = e.GetType();

			// 模组怪物：只读各怪物明细的三态（总控已在界面一键覆写到明细）。
			if (t == typeof(MushroomBoss))
				return SpawnConfigRuntime.ModeShouldAppear(cfg.MushroomMonsterSpawnMode);
			if (t == typeof(IronbirdElite))
				return SpawnConfigRuntime.ModeShouldAppear(cfg.IronbirdMonsterSpawnMode);
			if (t == typeof(FirecabElite))
				return SpawnConfigRuntime.ModeShouldAppear(cfg.FirecabMonsterSpawnMode);
			if (t == typeof(FirecentElite))
				return SpawnConfigRuntime.ModeShouldAppear(cfg.FirecentMonsterSpawnMode);
			if (t == typeof(WatercentMonster))
				return SpawnConfigRuntime.ModeShouldAppear(cfg.WatercentMonsterSpawnMode);
			if (t == typeof(GreencabMonster))
				return SpawnConfigRuntime.ModeShouldAppear(cfg.GreencabMonsterSpawnMode);

			if (t == typeof(ThreeChickenEvent))
				return false;
			// 拾荒者收费站：事件专用遭遇，同样不进随机池
			if (t == typeof(MonkeyTollEncounter))
				return false;
			return true;
		});
	}
}

public sealed class EncounterGettersFilterPatch : IPatchMethod
{
	public static string PatchId => "rainworld_encounter_getters_filter";
	public static string Description => "根据配置从遭遇池中移除被禁用的模组怪物";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(ActModel), "get_AllEncounters"),
		new(typeof(ActModel), "get_AllBossEncounters"),
		new(typeof(ModelDb), "get_AllEncounters"),
	];

	public static void Postfix(ref IEnumerable<EncounterModel> __result)
	{
		__result = EncounterFilterHelpers.FilterDisabledEncounters(__result).ToList();
	}
}
