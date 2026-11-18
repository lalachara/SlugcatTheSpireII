using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;


using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
[RegisterPower]
public sealed class IcewaterPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Resource/Powers/River/{GetType().Name}128.png",
        BigIconPath: $"res://Resource/Powers/River/{GetType().Name}128.png"
    );

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 只在真正结算伤害时扣减层数（after hook），避免其它 mod 调用 ModifyHpLostAfterOsty 做预测时误扣。
    private decimal _pendingRemove = 0;

    public override decimal ModifyHpLostAfterOsty(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        // 伙伴替主人挡下攻击时冰晶照样生效：先扣冰晶，剩下的才由伙伴掉血
        if (!FriendGuardPower.IsOwnerOrGuardingPet(Owner, target) || amount <= 0)
            return amount;

        if (amount >= Amount)
        {
            _pendingRemove = Amount;
            return amount - Amount;
        }

        _pendingRemove = amount;
        return 0;
    }

    public override async Task AfterModifyingHpLostAfterOsty()
    {
        // 伤害真正结算后才会走到这里；若在 Modify 阶段被跳过（例如预测），_pendingRemove 不消费。
        if (_pendingRemove <= 0)
        {
            await base.AfterModifyingHpLostAfterOsty();
            return;
        }

        if (_pendingRemove >= Amount)
        {
            await PowerCmd.Remove(this);
        }
        else
        {
            await PowerCmd.ModifyAmount(new BlockingPlayerChoiceContext(), this, -_pendingRemove, Owner, null);
        }
        _pendingRemove = 0;

        await base.AfterModifyingHpLostAfterOsty();
    }
}