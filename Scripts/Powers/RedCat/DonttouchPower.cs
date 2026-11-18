using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;
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
public sealed class DonttouchPower : ModPowerTemplate
{
  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/RedCat/DonttouchPower128.png",
    BigIconPath: "res://Resource/Powers/RedCat/DonttouchPower128.png"
  );
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Single;

  public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props,
    Creature target, CardModel? cardSource)
  {
    if (dealer == base.Owner && (props & ValueProp.Unpowered) == 0&&Owner.HasPower<RedmushroomPower>()&&target!=Owner)
    {
      await PowerCmd.Apply<CatConstrictPower>(choiceContext, target, Amount, base.Owner, null);
    }
    await base.AfterDamageGiven(choiceContext, dealer, result, props, target, cardSource);
  }

  public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props,
    Creature? dealer, CardModel? cardSource)
  {
    if (target == base.Owner && dealer != null && (props & ValueProp.Unpowered) == 0&&Owner.HasPower<RedmushroomPower>()&&dealer!=Owner)
    {
      await PowerCmd.Apply<CatConstrictPower>(choiceContext, dealer, Amount, base.Owner, null);
    }
    await base.AfterDamageReceived(choiceContext, target, result, props, dealer, cardSource);
  }
}