﻿﻿﻿using Godot;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.relics;
using Rainworld.Scripts;
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
public sealed class HuntsignPower : ModPowerTemplate
{
  public static event Action<Creature>? OnMarkedCreatureKilled;
  
  public override PowerType Type => PowerType.Debuff;
  private Boolean active = false;
  
  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/Huntbuff128.png",
    BigIconPath: "res://Resource/Powers/Huntbuff128.png"
  );

  public override PowerStackType StackType => PowerStackType.Counter;
  
  

  public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props,
    Creature? dealer, CardModel? cardSource)
  {
    if (target == base.Owner && dealer != null && (props!=ValueProp.Unpowered || cardSource is Omnislice))
    {
      if (dealer.Player?.Character is Slugcat)
      {
        await SlugcatData.addfood(dealer.Player, 1);
        if(dealer.Player.GetRelic<Redcat_Book2>()==null)
          await PowerCmd.Decrement(this);
      }
    }
  }


}