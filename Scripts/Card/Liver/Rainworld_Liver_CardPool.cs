using Godot;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;
using System;
using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using Rainworld.Scripts.Card.Liver.Attack;

#nullable enable
namespace demo.Resource.Card;

public class Rainworld_Liver_CardPool : CustomCardPoolModel
{
    //public const string energyColorName = "RainworldLiverEnergyColor";
    //public const string energyColorName = "ironclad";

    public override string Title => "RAINWORLD_LIVER_CARD_POOL";

    //public override string EnergyColorName => "RainworldLiverEnergyColor";
    //public override string EnergyColorName => "ironclad";

    //public override string CardFrameMaterialPath => "card_frame_rainworld_liver";

    public override Color DeckEntryCardColor => new Color("000000");
    public override String BigEnergyIconPath => "res://Resource/SlugCat/card_orb.png";
    public override String TextEnergyIconPath => "res://Resource/SlugCat/small_orb.png";

    public override bool IsColorless => false;

    protected override CardModel[] GenerateAllCards()
  {
    return new CardModel[ /*0x40*/]
    {
      (CardModel) ModelDb.Card<Rainworld_Liver_Defend>(),
      
    };
  }
}