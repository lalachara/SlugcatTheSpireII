using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rainworld.Scripts.Powers;

[RegisterPower]
public sealed class ThinkPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/life128.png",
        BigIconPath: "res://Resource/Powers/Monk/life128.png"
    );

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 与 SeethroughPower 同一套判定：只认「你作为来源造成」的生命流失。
    /// 生命流失同时带「不可格挡」与「无能力来源」两个标记；普通伤害不算，中毒这类没有来源的损失也不算。
    /// </summary>
    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer,
        DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != Owner)
            return;
        if ((props & (ValueProp.Unblockable | ValueProp.Unpowered)) !=
            (ValueProp.Unblockable | ValueProp.Unpowered))
            return;
        if (result.UnblockedDamage <= 0)
            return;

        // 每失去 1 点生命，就按当前层数获得格挡（2 层 = 每点流失 2 格挡），不再是固定「等量」。
        decimal layers = Math.Max(Amount, 1m);
        await CreatureCmd.GainBlock(Owner, result.UnblockedDamage * layers, ValueProp.Unpowered, null);
    }
}