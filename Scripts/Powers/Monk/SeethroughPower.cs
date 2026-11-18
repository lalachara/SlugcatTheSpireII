using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 识破：你每造成一次生命流失，就抽 Amount 张牌。挂在玩家身上，战斗结束随 powers 一起清掉。
/// 重复打出会叠层，也就是每次抽更多张。
/// </summary>
[RegisterPower]
public sealed class SeethroughPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/line128.png",
        BigIconPath: "res://Resource/Powers/Monk/line128.png"
    );

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 与 ThinkPower 同一套判定：只有生命流失（无视格挡且不吃加成的伤害）才算，普通伤害不算。
    /// 完全没掉血（失血为 0）也不抽牌。
    /// </summary>
    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer,
        DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != Owner)
            return;
        if ((props & ValueProp.Unblockable) == 0 || (props & ValueProp.Unpowered) == 0)
            return;
        if (result.UnblockedDamage <= 0)
            return;
        await CardPileCmd.Draw(choiceContext, Amount, Owner.Player);
    }
}
