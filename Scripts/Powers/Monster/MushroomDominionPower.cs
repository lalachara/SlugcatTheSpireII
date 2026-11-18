#if STS2_0_113_0
using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

/// <summary>
/// 变异长腿菌的统御（恐惧模式）：道中的普通 / 精英敌人最大生命值 +20%。
/// 由 <see cref="MushroomDominion" /> 在敌人进入战斗时施加；图标沿用长腿菌「自我增殖」那张。
/// </summary>
[RegisterPower]
public sealed class MushroomDominionPower : ModPowerTemplate
{
    /// <summary>最大生命提升比例（20%）。</summary>
    public const decimal MaxHpBonusRatio = 0.2m;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Mushroomsleep128.png",
        BigIconPath: "res://Resource/Powers/Mushroomsleep128.png"
    );

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>施加时把最大生命提高 20%（向上取整），并把增加的这部分补满。</summary>
    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        decimal bonus = Math.Ceiling(Owner.MaxHp * MaxHpBonusRatio);
        if (bonus <= 0m)
            return;

        await CreatureCmd.SetMaxHp(Owner, Owner.MaxHp + bonus);
        await CreatureCmd.Heal(Owner, bonus);
    }
}
#endif
