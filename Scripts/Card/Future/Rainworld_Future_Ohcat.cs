﻿﻿using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Card;
using Rainworld.Scripts.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Card.Future;

public class Rainworld_Future_Ohcat : FutureCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Ancient;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(2),
        new CardsVar(2)
    ];
    public override CardAssetProfile AssetProfile => new(
        $"res://Resource/Card/other/rainworld_ohcat.png"
    );
    
    public Rainworld_Future_Ohcat() : base(energyCost, type, rarity, SlugCatCardTargets.SlugcatAlly, shouldShowInCardLibrary)
    {
    }


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        var targetPlayer = CombatState.Players.First(p => p.Creature == cardPlay.Target);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, targetPlayer);

        var text = new LocString("cards", "SLUGCAT_THE_SPIRE_II_CARD_RAINWORLD_FUTURE_OHCAT.speech").GetRawText();
        var vfx = NThoughtBubbleVfx.Create(text, Owner.Creature, 2);
        NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
        var vfx2 = NThoughtBubbleVfx.Create("Meow~",cardPlay.Target, 2);
        NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx2);
        if (targetPlayer.Creature.HasPower<RedmushroomPower>() && targetPlayer.Creature.HasPower<DonttouchPower>())
        {
            await PowerCmd.Apply<CatConstrictPower>(choiceContext,Owner.Creature,2,targetPlayer.Creature,null);
        }
        
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
        DynamicVars.Cards.UpgradeValueBy(1m);

    }
}