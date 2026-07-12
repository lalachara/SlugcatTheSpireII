
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts.Powers;
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
public sealed class NimblePower : ModPowerTemplate
{
  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/NimbleBuff128.png",
    BigIconPath: "res://Resource/Powers/NimbleBuff128.png"
  );
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;
  public bool fromtwohand = false;
  

  public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext,PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
  {
    if (!(amount <= 0m) && applier == base.Owner && power is NimblePower)
    {
      Flash();
      if (Amount >= 10m||(Amount >=8m&&Owner.HasPower<SlugfeelPower>()))
      {
        await PowerCmd.ModifyAmount(choiceContext,power,Owner.HasPower<SlugfeelPower>()?-8m:-10m , applier, cardSource);
        await PowerCmd.Apply<BufferPower>(choiceContext,Owner, 1, base.Owner, null);
      }
    }
  }
}
