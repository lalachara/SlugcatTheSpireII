using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 柔道：持有者失去力量时，先照常扣除，再返还（层数 + 1）倍的等量力量，净效果为获得「层数 × 失去量」。
/// 可叠加：每多一层，同样失去 1 点力量时返还的更多（净得 = 层数 × 失去量）。
/// 负力量必须真实落账（塔1 SoftFistPatch 的做法），否则「情感链接」这类以失去力量为触发点的能力收不到事件。
/// </summary>
[RegisterPower]
public sealed class SoftPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/Soft128.png",
        BigIconPath: "res://Resource/Powers/Monk/Soft128.png");

    public override PowerType Type => PowerType.Buff;

    // 可叠加：每次施加 +1 层，净获得的力量 =「层数 × 失去量」（下面的 Amount + 1 就是按层数算的）
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        // amount 为本次变化量：只响应持有者自己的力量下降。
        // 这里刻意不去拦截变化本身——力量得真的掉下来，依赖「失去力量」的效果（情感链接等）才收得到回调。
        if (power is not StrengthPower || power.Owner != Owner || amount >= 0m)
            return;

        // 负力量已经扣了一份，再补（层数 + 1）份，净效果就是获得「层数 × 失去量」
        Flash();
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, (-amount) * (Amount + 1m), Owner, null);
    }
}
