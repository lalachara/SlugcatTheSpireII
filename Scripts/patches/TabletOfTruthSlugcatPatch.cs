using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using Rainworld.Scripts;
using STS2RitsuLib.Patching.Models;

namespace Rainworld.Scripts.patches;

/// <summary>
/// 原版「真理石板」(TabletOfTruth) 的解读选项：
/// 当解读费用 &gt;= 当前生命上限时，原版会先 <c>LoseMaxHp(MaxHp-1)</c> 再 <c>CreatureCmd.Kill</c>。
/// 多人模式下在战斗外击杀玩家会打出
/// 「Player X has been killed outside of combat in multiplayer! This should not occur」并中断事件流程，
/// 表现为：<b>不触发升级牌，也不掉业力</b>（<c>ForceKillPatch</c> 目前并未启用，所以连业力归零都没有）。
/// <para>蛞蝓猫规则（见关键词「业力」）：单次失去最大生命最多 1；受到致命伤害时若业力不为 1，
/// 失去 1 业力并回满血。因此这里给蛞蝓猫换成：<b>生命上限不够支付时改为扣 1 业力，然后照常走升级流程</b>；
/// 够支付时维持原版扣上限（<c>LoseMaxHpPatch</c> 会把单次数值压到 1）。</para>
/// </summary>
public sealed class TabletOfTruthSlugcatPatch : IPatchMethod
{
    public static string PatchId => "rainworld_tablet_of_truth_slugcat";

    public static string Description => "真理石板：蛞蝓猫生命上限不足时改为扣业力，不死亡且照常升级";

    public static bool IsCritical => false;

    private static readonly FieldInfo? DecipherCountField =
        typeof(TabletOfTruth).GetField("_decipherCount", BindingFlags.Instance | BindingFlags.NonPublic);

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(TabletOfTruth), "LoseMaxHpAndUpgrade", new[] { typeof(decimal) })
    ];

    public static bool Prefix(TabletOfTruth __instance, decimal hp, ref Task __result)
    {
        Player? owner = __instance.Owner;
        if (owner?.Character is not Slugcat)
            return true;

        __result = SlugcatLoseMaxHpAndUpgrade(__instance, owner, hp);
        return false;
    }

    private static async Task SlugcatLoseMaxHpAndUpgrade(TabletOfTruth evt, Player owner, decimal hp)
    {
        Creature creature = owner.Creature;

        // 统一规则：每点击一次「解读」固定扣 1 点生命上限（上限最低到 1），
        // 不再额外扣当前血量、也不扣业力；上限被压下来时引擎只会把超出的血量夹到新上限。
        if (creature.MaxHp > 1)
        {
#if STS2_0_113_0 || STS2_0_114_0
            await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), creature, 1m, null);   // 0.113: CardModel? cardSource
#else
            await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), creature, 1m, false);  // 0.111: bool isFromCard
#endif
        }

        // ———— 以下照抄原版升级流程，保证「解读」一定给到升级 ————
        int decipherCount = DecipherCountField?.GetValue(evt) is int count ? count : 0;
        List<CardModel> upgradable = PileType.Deck.GetPile(owner).Cards
            .Where(c => c.IsUpgradable)
            .ToList();

        if (decipherCount == 4)
        {
            foreach (CardModel card in upgradable)
            {
                CardCmd.Upgrade(card, CardPreviewStyle.MessyLayout);
                await Cmd.CustomScaledWait(0.1f, 0.2f);
            }

            await Cmd.CustomScaledWait(0.6f, 1.2f);
        }
        else if (upgradable.Count != 0)
        {
            CardModel card = evt.Rng.NextItem(upgradable);
            CardCmd.Upgrade(card, CardPreviewStyle.EventLayout);
            await Cmd.CustomScaledWait(0.3f, 0.6f);
        }
        else
        {
            await Cmd.CustomScaledWait(0.5f, 1f);
        }
    }
}
