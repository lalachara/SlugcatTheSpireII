﻿using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts.Card.Liver.Attack;

namespace Rainworld.Scripts.Powers;
public sealed class MDbaole1 : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<Rainworld_Liver_Mdbaole>();

    protected override bool IsPositive => true;
}