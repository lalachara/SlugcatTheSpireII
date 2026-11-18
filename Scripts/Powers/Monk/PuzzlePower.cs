using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using SlugcatTheSpireII.Scripts;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 炫彩拼图：每回合你首次打出一种颜色的牌时，抽 1 张牌。
/// 颜色口径与七彩石（Rock7）、云翳（Cloud）一致：普通牌按颜色去重，
/// 带彩色标签的牌颜色是"所有蛞蝓猫颜色"，所以它每次打出都算一种新颜色
/// （但不写进已打出的颜色里，免得一张彩虹牌就把本回合剩下的颜色全占了）。
/// </summary>
[RegisterPower]
public sealed class PuzzlePower : ModPowerTemplate
{
    /// <summary>本回合已经打出过的颜色，回合开始时清空。</summary>
    private readonly HashSet<CardPoolModel> _usedColors = new();

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/puzzle128.png",
        BigIconPath: "res://Resource/Powers/Monk/puzzle128.png"
    );

    /// <summary>可叠加：每层让「首次打出某颜色」时的抽牌数 +1（2 层 = 抽 2 张）。</summary>
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>每次触发抽几张：等于当前层数（至少 1）。</summary>
    private int DrawCount => Math.Max(1, (int)Amount);

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card.Owner != Owner.Player)
            return;
        if (card.Keywords.Contains(RainworldKeywords.Colorful))
        {
            // 彩色牌每次都算新颜色、但不占位
            await CardPileCmd.Draw(choiceContext, DrawCount, Owner.Player);
        }
        else
        {
            var pool = card.VisualCardPool;
            if (pool != null && _usedColors.Add(pool))
                await CardPileCmd.Draw(choiceContext, DrawCount, Owner.Player);
        }
    }

    /// <summary>新回合重新数颜色（与七彩石的重置点在同一个钩子上）。</summary>
    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player)
            _usedColors.Clear();
        return Task.CompletedTask;
    }
}
