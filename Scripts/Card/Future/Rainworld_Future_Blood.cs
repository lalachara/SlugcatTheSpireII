﻿using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Resource.Card;
using Rainworld.Scripts.Powers;

namespace Rainworld.Scripts.Card.Future;

public class Rainworld_Future_Blood : FutureCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;

    
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        new[] {         HoverTipFactory.FromPower<StrengthPower>()
        };
    


    public Rainworld_Future_Blood() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
       await PowerCmd.Apply<BloodPower>(choiceContext, Owner.Creature,1,Owner.Creature,this);
    }

    protected override void OnUpgrade()
    {
       EnergyCost.UpgradeBy(-1);
    }
}