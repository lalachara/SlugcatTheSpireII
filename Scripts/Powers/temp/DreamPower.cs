﻿using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Linq;
using System.Threading.Tasks;

#nullable enable
[RegisterPower]
public sealed class DreamPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/Monk/Dream128.png",
        BigIconPath: "res://Resource/Powers/Monk/Dream128.png"
    );

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;


    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
            return;

        var discardPile = PileType.Discard.GetPile(player);
        if (discardPile.Cards.Count == 0)
            return;

        var cards = discardPile.Cards.ToList();
        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1, (int)Amount);
        var selected = await CardSelectCmd.FromSimpleGrid(choiceContext, cards, player, prefs);


        await CardPileCmd.Add(selected, PileType.Hand, CardPilePosition.Random, this);
        
    }
}