using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Linq;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 铸剑为犁：持有者失去力量时，令所有敌人失去等量的力量。
/// </summary>
[RegisterPower]
public sealed class PloughsharePower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/mindconnect128.png",
        BigIconPath: "res://Resource/Powers/Monk/mindconnect128.png"
    );

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        // amount 为本次变化量，负数代表力量下降；只响应持有者自身的力量变化。
        if (power is not StrengthPower || power.Owner != Owner || amount >= 0)
            return;

        var enemies = CombatState?.HittableEnemies;
        if (enemies == null || enemies.Count == 0)
            return;

        Flash();
        foreach (var enemy in enemies.ToList())
            await PowerCmd.Apply<StrengthPower>(choiceContext, enemy, amount, Owner, null);
    }
}
