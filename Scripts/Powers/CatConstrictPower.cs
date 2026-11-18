// Decompiled with JetBrains decompiler
// Type: MegaCrit.Sts2.Core.Models.Powers.CatConstrictPower
// Assembly: sts2, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F7D2A9E0-F1AE-4213-B874-1504473AAEDB
// Assembly location: D:\Game\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll
// XML documentation location: D:\Game\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.xml

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

#nullable enable
namespace MegaCrit.Sts2.Core.Models.Powers;
[RegisterPower]

public sealed class CatConstrictPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/chanrao.png",
        BigIconPath: "res://Resource/Powers/chanrao.png"
    );

    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        CatConstrictPower CatConstrictPower = this;
        if (!participants.Contains<Creature>(CatConstrictPower.Owner))
            return;
        IEnumerable<DamageResult> damageResults = await CreatureCmd.Damage(choiceContext, CatConstrictPower.Owner, (Decimal) CatConstrictPower.Amount, ValueProp.Unpowered, CatConstrictPower.Owner);
    }

    // 0.113：AbstractModel.AfterDeath 新增了 CardModel? source 参数（致命伤害来源卡牌）
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        CardModel? source,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        CatConstrictPower power = this;
        if (wasRemovalPrevented || creature != power.Applier)
            return;
        await PowerCmd.Remove((PowerModel) power);
    }
}