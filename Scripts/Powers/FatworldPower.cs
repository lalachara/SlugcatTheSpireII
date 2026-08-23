﻿﻿using Godot;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
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
public sealed class FatworldPower : ModPowerTemplate
{
  public override PowerAssetProfile AssetProfile => new(
    IconPath: "res://Resource/Powers/FatWorld128.png",
    BigIconPath: "res://Resource/Powers/FatWorld128.png"
  );
  
  public override PowerType Type => PowerType.Buff;

  public override PowerStackType StackType => PowerStackType.Counter;
  

 public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
 {
     
     if (cardPlay.Card.Owner == base.Owner.Player&&Owner.Player.Character is Slugcat )
     {
         if (CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry e) =>
                 e.CardPlay.Card.Id == cardPlay.Card.Id&&e.CardPlay.Card.Owner == Owner.Player)==1)
         {
             await SlugcatData.addfood(Owner.Player, Amount);
         }
         
     }

 }
 
}