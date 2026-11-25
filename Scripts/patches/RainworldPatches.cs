using MegaCrit.Sts2.Core.Models.Cards;
 using Rainworld.Patches;
  using SlugcatTheSpireII.Scripts.patches;
using Rainworld.Scripts.patches.Filter;
using STS2RitsuLib.Patching.Core;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches;

public sealed class RainworldPatches : IModPatches
{
    public static void AddTo(ModPatcher patcher)
	{
		//patcher.RegisterPatch<CardPlayTreasureSpearPatch>();
		//patcher.RegisterPatch<CreatureAfterTurnStartPatch>();
		//  patcher.RegisterPatch<ForceKillPatch>();
		patcher.RegisterPatch<MaxhpFixPatch>();
		// patcher.RegisterPatch<TouchOfOrobasPatch>();
		// patcher.RegisterPatch<ArchaicToothTranscendencePatch>();
		//  patcher.RegisterPatch<LoseHpInternalPatch>();
		//patcher.RegisterPatch<RunManagerNewSinglePatch>();
		patcher.RegisterPatch<PotionFilterPatch>();
		patcher.RegisterPatch<SplashCardPoolFilterPatch>();
		patcher.RegisterPatch<PrismaticGemCardPoolFilterPatch>();
		patcher.RegisterPatch<BlockColorPatch>();
		patcher.RegisterPatch<RelicPoolFilterPatch>();
		patcher.RegisterPatch<RelicGrabBagFilterPatch>();
		// 仅蛞蝓猫可获得的遗物（典藏书签 / 轮回之花）：原版角色抽到就重新抽
		patcher.RegisterPatch<SlugcatOnlyRelicFilterPatch>();
		patcher.RegisterPatch<SlugcatOnlyRelicGrabBagFilterPatch>();
		patcher.RegisterPatch<AbyssalBathsEventFilterPatch>();
		patcher.RegisterPatch<MushblockOverlayPatch>();
		patcher.RegisterPatch<CombatUiReadyPatch>();
		patcher.RegisterPatch<CombatUiActivatePatch>();
		patcher.RegisterPatch<CombatUiDeactivatePatch>();
		patcher.RegisterPatch<LoseMaxHpPatch>();
		patcher.RegisterPatch<StrangeBomb_ModifyHpLostBeforeOstyPatch>();
		
		patcher.RegisterPatch<EncounterGettersFilterPatch>();
		patcher.RegisterPatch<EventOptionWorkLevelPatch>();
		patcher.RegisterPatch<EventOptionMultiplayerGuardPatch>();
		// 真理石板：蛞蝓猫生命上限不足时改为扣业力，避免多人事件里被原版 Kill 打断（不升级/不掉业力）
		patcher.RegisterPatch<TabletOfTruthSlugcatPatch>();
		patcher.RegisterPatch<FpEventTitlePatch>();
		patcher.RegisterPatch<CharacterSelectSkinPatch>();
		// 悬停玩家时伙伴淡出并隐藏伙伴的血条/能力 UI
		patcher.RegisterPatch<MinionHoverFadePatch>();
		// 伙伴五站位：进场 / 节点移除 / 战斗开始整体排版之后，把本 mod 的伙伴摆回塔1 站位
		patcher.RegisterPatch<FriendPetLayoutPatch>();
		// 多人模式主人战死后，把仍留在场上的伙伴一起带离战场
		patcher.RegisterPatch<FriendPetOwnerDeathPatch>();
		// 点击队友角色隐藏 / 恢复该队友的伙伴（本地玩家仍用护甲面板）
		patcher.RegisterPatch<TeammateClickHidePatch>();
		// 伙伴守护：攻击按顺序在多只伙伴间依次分摊，全部倒下才打玩家
		// 蛞蝓猫的火堆「休息」描述改为「回复 N 级业力」（文本在本地化里）
		patcher.RegisterPatch<RestSiteHealTextPatch>();
		// 左上角生命值悬停提示：蛞蝓猫追加当前业力等级与上限，其它角色保持原版
		patcher.RegisterPatch<TopBarHpHoverTipPatch>();
	}
}