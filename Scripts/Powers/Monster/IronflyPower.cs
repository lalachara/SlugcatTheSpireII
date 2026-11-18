using MegaCrit.Sts2.Core.Commands.Builders;
  using MegaCrit.Sts2.Core.Entities.Cards;
  using MegaCrit.Sts2.Core.Entities.Players;
  using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;
using Rainworld.Scripts.Monster;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;


using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
[RegisterPower]
public sealed class IronflyPower : ModPowerTemplate
{

  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/IronFly128.png",
    BigIconPath: "res://Resource/Powers/IronFly128.png"
  );
  public int basenum;

  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;

  public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
  {
    basenum = this.Amount;

    await base.AfterApplied(applier, cardSource);
  }
  
  public override Decimal ModifyHpLostAfterOstyLate(
    Creature target,
    Decimal amount,
    ValueProp props,
    Creature? dealer,
    CardModel? cardSource)
  {
    // 生命流失（内伤、中毒等）不吃减伤：塔2 的流失伤害带的是 Unblockable|Unpowered 组合标记，
    // 用 == / != 与 ValueProp.Unpowered 做相等比较会漏判，必须按位测试。
    if(target == this.Owner && (props & ValueProp.Unpowered) == 0)
      return amount/2; 
    return amount;
  }
  
  public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
  {
    if(Amount<basenum)
      await PowerCmd.ModifyAmount(choiceContext,this,basenum-Amount,Owner,null);
  }
  public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult _, ValueProp props, Creature? dealer, CardModel? __)
  {
    if (target == base.Owner && dealer != null && (props & ValueProp.Unpowered) == 0 && _.UnblockedDamage>0)
    {
      await PowerCmd.Decrement(this);
    }
  }

  public override async Task AfterRemoved(Creature oldOwner)
  {
    if(Owner.Monster==null || Owner.Monster is not Ironbird ironbird || Owner.IsDead)
      return;
    
    //SfxCmd.Play("event:/sfx/enemy/enemy_attacks/lagavulin_matriarch/lagavulin_matriarch_awaken");
    ironbird.IsDazed = true;
    ironbird.SetMoveImmediate(ironbird.sleepmove);
    await CreatureCmd.TriggerAnim(Owner, "Sleep", 0.6f);


  }
}