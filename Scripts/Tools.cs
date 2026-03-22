using System.Collections;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using Rainworld.Scripts;
using Rainworld.Scripts.Card.Liver.Attack;

namespace Rainworld.Scripts;

public static class Tools
{
    public const CardTag Treasure_Tag = (CardTag)4527;
    public const CardTag TreasureSpear_Tag = (CardTag)4528;
    public const CardTag Corrupt_Tag = (CardTag)4529;
    public const CardTag Immunity_Tag = (CardTag)4530;
    public const CardTag CantCorrupt_Tag = (CardTag)4531;
    public const CardTag CatStrike_Tag = (CardTag)4532;
    
    public static CardModel getrandomSpear(Boolean upgraded,Player player)
    {

        CardModel res = spearCards[player.PlayerRng.Transformations.NextInt(spearCards.Count)].CreateClone();
        if (upgraded)
            CardCmd.Upgrade(res);
        res.AddKeyword(CardKeyword.Retain);
        res.AddKeyword(CardKeyword.Exhaust);
        res.AddKeyword(RainworldKeywords.Treasurespear);
        return res;
    }
    public static List<CardModel> ChiefCards = new List<CardModel>
    {
        new Rainworld_Liver_Bomb(),
        new Rainworld_Liver_Spearboom(),
        new Rainworld_Liver_Spearelec(),
        
    };
    public static List<CardModel> spearCards = new List<CardModel>
    {
        new Rainworld_Liver_Spear(),
        new Rainworld_Liver_Spearboom(),
        new Rainworld_Liver_Speardouble(),
        new Rainworld_Liver_Spearelec(),
        new Rainworld_Liver_Spearskip(),
        new Rainworld_Liver_Spearstab(),
        new Rainworld_Liver_Spearstabhead(),
        new Rainworld_Liver_Spearthroat(),
        new Rainworld_Liver_Spearslug()
    };

}