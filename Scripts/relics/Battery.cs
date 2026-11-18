using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Godot.Character;
using Rainworld.Scripts;
using Rainworld.Scripts.Skins;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.relics;

using MegaCrit.Sts2.Core.Models.RelicPools;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class Battery : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;
    public override bool ShowCounter => true;
    
    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://Resource/Relics/battery.png",
        IconOutlinePath: "res://Resource/Relics/outline/bettery.png",
        BigIconPath: "res://Resource/Relics/battery.png"
    );
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1)
    ];
    private bool _wasUsed;
    private int _hitCount;
    public override int DisplayAmount => _hitCount;

    public override bool IsUsedUp => _wasUsed;

    [SavedProperty]
    public bool WasUsed
    {
        get => _wasUsed;
        set
        {
            AssertMutable();
            _wasUsed = value;
            if (IsUsedUp)
            {
                base.Status = RelicStatus.Disabled;
            }
        }
    }

    public override async Task BeforeRoomEntered(AbstractRoom room)
    {
        
        await base.BeforeRoomEntered(room);
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {

        if (WasUsed || !participants.Contains(Owner.Creature))
            return;
        await PlayerCmd.GainEnergy(1, Owner);
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (WasUsed || target != Owner.Creature||result.UnblockedDamage==0 ||(props & ValueProp.Unpowered) != 0)
            return;

        _hitCount++;
        this.InvokeDisplayAmountChanged();
        Flash();

        if (_hitCount >= 8)
        {
            SfxCmd.Play("event:/Rainworld/sfx/pointbomb");

            await CreatureCmd.Damage(choiceContext, Owner.Creature.CombatState!.HittableEnemies, 40, ValueProp.Unpowered, Owner.Creature);
            await CreatureCmd.Damage(choiceContext, Owner.Creature, 40, ValueProp.Unpowered, Owner.Creature);
            WasUsed = true;

            var visuals = NCombatRoom.Instance?.GetCreatureNode(Owner.Creature)?.Visuals;
            if (visuals is NRiver river)
            {
                river.SetItemSkin(SlugcatSkins.RiverBatteryEmptyItemSkin);
            }
        }
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        _hitCount = 0;
        return Task.CompletedTask;
    }
}