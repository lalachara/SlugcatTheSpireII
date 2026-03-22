using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;

namespace demo.Scripts.patches;

[HarmonyPatch]
public class CatBagPatch
{
    [HarmonyTargetMethod]
    public static MethodBase TargetMethod()
    {
        // 获取 CharacterModel 类的 AfterCardPlayed 方法
        return AccessTools.Method(
            typeof(CharacterModel), 
            nameof(CharacterModel.AfterCardPlayed),
            new[] { typeof(PlayerChoiceContext), typeof(CardPlay) }
        );
    }
    
    public static async void Postfix(
        Task __result, // 原方法的返回值（async Task 的返回值）
        PlayerChoiceContext context, // 原方法的第一个参数
        CardPlay cardPlay // 原方法的第二个参数
    )
    {
        // 等待原方法的异步逻辑执行完成
        await __result;

        // --------------------------
        // 这里编写你的自定义逻辑
        // --------------------------
        try
        {
            if (cardPlay.Card.Keywords.Contains(RainworldKeywords.Treasurespear))
            {
                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(Tools.getrandomSpear(cardPlay.Card.IsUpgraded,cardPlay.Card.Owner) , PileType.Hand, addedByPlayer: true));
            }
        }
        catch (Exception ex)
        {
        }
    }
}