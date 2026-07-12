using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Powers;

[RegisterPower]
public sealed class BaptismPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/River/wash.png",
        BigIconPath: "res://Resource/Powers/River/wash.png"
    );

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
    
        if (cardPlay.Card.Owner != Owner.Player || cardPlay.Card.Type != CardType.Power || Amount <= 0)
            return;

        await PowerCmd.Decrement(this);
        foreach (var player in CombatState.Players)
        {
            if (player == Owner.Player || !player.Creature.IsAlive)
                continue;

            var copy= cardPlay.Card.CreateCloneForPlayer(player);
            if(cardPlay.Card.IsUpgraded)
                CardCmd.Upgrade(copy);
            await CardCmd.AutoPlay(choiceContext, copy, player.Creature);
        }

    }
}