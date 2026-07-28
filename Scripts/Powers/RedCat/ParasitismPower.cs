using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Threading.Tasks;

namespace Rainworld.Scripts.Powers;

#nullable enable
[RegisterPower]
public sealed class ParasitismPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Powers/RedCat/brave.png",
        BigIconPath: "res://Resource/Powers/RedCat/brave.png"
    );

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (card.Owner.Creature.IsPlayer && card.Owner.Creature != Owner)
        {
            Flash();
            await PowerCmd.Apply<MushblockPower>(choiceContext, card.Owner.Creature, 1, card.Owner.Creature, null);
            await PowerCmd.Apply<MushblockPower>(choiceContext, Owner, 1, Owner, null);
        }
    }
}