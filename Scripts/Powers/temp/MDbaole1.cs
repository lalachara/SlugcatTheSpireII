﻿using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using Rainworld.Scripts.Card.Liver.Attack;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

[RegisterPower]

public sealed class MDbaole1 : ModTemporaryAppliedPowerTemplate<Rainworld_Liver_Mdbaole, StrengthPower>
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Resource/Powers/Tired128.png",
        BigIconPath: $"res://Resource/Powers/Tired128.png"
    );
}