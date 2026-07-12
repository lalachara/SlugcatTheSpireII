﻿﻿using Rainworld.Patches;
  using SlugcatTheSpireII.Scripts.patches;
using STS2RitsuLib.Patching.Core;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches;

public sealed class RainworldPatches : IModPatches
{
    public static void AddTo(ModPatcher patcher)
	{
		//patcher.RegisterPatch<CardPlayTreasureSpearPatch>();
		//patcher.RegisterPatch<CreatureAfterTurnStartPatch>();
		patcher.RegisterPatch<ForceKillPatch>();
		patcher.RegisterPatch<MaxhpFixPatch>();
		// patcher.RegisterPatch<TouchOfOrobasPatch>();
		// patcher.RegisterPatch<ArchaicToothTranscendencePatch>();
		patcher.RegisterPatch<LoseHpInternalPatch>();
		//patcher.RegisterPatch<RunManagerNewSinglePatch>();
		//patcher.RegisterPatch<RunManagerNewMultiPatch>();
		//patcher.RegisterPatch<SaveManagerSaveRunPatch>();
		//patcher.RegisterPatch<SaveManagerLoadRunPatch>();
		patcher.RegisterPatch<CombatUiReadyPatch>();
		patcher.RegisterPatch<CombatUiActivatePatch>();
		patcher.RegisterPatch<CombatUiDeactivatePatch>();
		patcher.RegisterPatch<LoseMaxHpPatch>();
		patcher.RegisterPatch<StrangeBomb_ModifyHpLostBeforeOstyPatch>();
	}
}