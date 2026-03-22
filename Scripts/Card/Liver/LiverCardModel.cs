using BaseLib.Abstracts;
using BaseLib.Utils;
using demo.Resource.Card;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Rainworld.Scripts.Card.Liver.Attack;

namespace Rainworld.Scripts.Card;

[Pool(typeof(Rainworld_Liver_CardPool))]
public abstract class LiverCardModel : CustomCardModel 
{
    public override string PortraitPath => $"res://Resource/Card/Liver/{Id.Entry.ToLowerInvariant()}.png";
    
    public override Texture2D? CustomFrame => GD.Load<Texture2D>("res://Resource/Card/CardSkill1024.png");
    public LiverCardModel(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary) : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        
    }

    
}

