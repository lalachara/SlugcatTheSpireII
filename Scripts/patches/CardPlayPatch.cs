using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Rainworld.Scripts;

namespace SlugcatTheSpireII.Scripts.patches;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
public class CardPlayPatch
{

    // 后置：同步void + 异步逻辑用Task.Run（Harmony唯一支持的写法）
    [HarmonyPostfix]
    public static void Postfix_OnAnyCardPlayed(
        CardModel __instance,
        PlayerChoiceContext choiceContext,
        Creature target,
        bool isAutoPlay,
        ResourceInfo resources,
        bool skipCardPileVisuals)
    {
        // 空判断
        if (__instance == null || __instance.Owner == null) return;

        // 触发你的逻辑
        if (__instance.Keywords.Contains(RainworldKeywords.Treasurespear))
        {
            // 异步逻辑包裹（Harmony不支持Postfix直接返回Task）
            Tools.getrandomSpear(__instance.IsUpgraded, __instance.Owner);
            
        }
    }
}