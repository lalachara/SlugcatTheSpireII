using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.CardTags;

namespace Rainworld.Scripts;

/// <summary>
/// 本 mod 自用的卡牌标签（<see cref="CardTag" />）。
/// 与 keyword 不同：tag 是纯数据标记，不产生任何文本提示，玩家在卡面上看不到，
/// 只供代码查询（引擎原话：behavior-less metadata）。
/// </summary>
public static class RainworldCardTags
{
    /// <summary>
    /// 「不可生成」：带有此标签的牌不会被随机生成 / 三选一效果抽到。
    /// 用于神经元（加最大生命）、珍珠（不能打出）以及红猫的腐蚀值机制牌等
    /// 只有待在牌库里才有意义、被战斗生成出来要么破坏节奏要么完全无用的牌。
    /// </summary>
    public static readonly CardTag CantGenerate =
        ModCardTagRegistry.For(Entry.ModId).RegisterOwned("cantgenerate").CardTagValue;

    /// <summary>
    /// 提前注册标签。RitsuLib 要求卡牌标签在模型初始化之前注册完毕，
    /// 所以这里在 <see cref="Entry.Init" /> 的最前面调用一次。
    /// </summary>
    public static void Register()
    {
        _ = CantGenerate;
    }

    /// <summary>这张牌是否禁止被生成类效果抽到。</summary>
    public static bool IsGenerateBlocked(CardModel card) => card.Tags.Contains(CantGenerate);
}
