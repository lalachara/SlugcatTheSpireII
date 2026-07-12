﻿using Godot;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Cards.Mocks;
using Rainworld.Scripts;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

#nullable enable
namespace Rainworld.Resource.Card;

[RegisterSharedCardPool]
public class Rainworld_Future_CardPool : TypeListCardPoolModel
{
    
    
    private static readonly Material? _poolFrameMaterial = MaterialUtils.CreateUnmodulatedHsvShaderMaterial(); // 如果你是自定义卡框，使用这个
    public override Material? PoolFrameMaterial => _poolFrameMaterial;
    public override string Title => "RAINWORLD_FUTURE_CARD_POOL";
    public override string EnergyColorName => "RAINWORLD_FUTURE_CARD_POOL";

    public override Color DeckEntryCardColor => new("000000");
    public override string BigEnergyIconPath => "res://Resource/SlugCat/card_orb.png";
    public override string TextEnergyIconPath => "res://Resource/SlugCat/small_orb.png";

    public override Color EnergyOutlineColor => new("000000");
    public override bool IsColorless => false;
}