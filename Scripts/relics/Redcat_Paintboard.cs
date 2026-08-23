using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.relics;

[RegisterRelic(typeof(Rainworld_Redcat_RelicPool))]
public sealed class Redcat_Paintboard : ModRelicTemplate
{
    private const int VigorThreshold = 10;
    private const int StrengthGain = 1;

    public override RelicRarity Rarity => RelicRarity.Uncommon;
    public override bool ShowCounter => true;
    public override async Task AfterCombatVictory(CombatRoom room)
    {
        _vigorAccumulated = 0;
        InvokeDisplayAmountChanged();

        await base.AfterCombatVictory(room);
    }

    public override int DisplayAmount => _vigorAccumulated;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Relics/board.png",
        IconOutlinePath: "res://Resource/Relics/outline/board.png",
        BigIconPath: "res://Resource/Relics/board.png"
    );

    private int _vigorAccumulated;
    

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier,
        CardModel? cardSource)
    {
        if (amount > 0 && power is VigorPower && power.Owner == Owner.Creature)
        {
            _vigorAccumulated += (int)amount;
            while (_vigorAccumulated >= VigorThreshold)
            {
                _vigorAccumulated -= VigorThreshold;
                await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, StrengthGain, Owner.Creature, null);
                Flash();
            }
            InvokeDisplayAmountChanged();
        }

        await base.AfterPowerAmountChanged(choiceContext, power, amount, applier, cardSource);
    }
}