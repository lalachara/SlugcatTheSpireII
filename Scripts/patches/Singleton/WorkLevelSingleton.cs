using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Patches;
using Rainworld.Scripts;
using Rainworld.Scripts.Powers;
using STS2RitsuLib;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace SlugcatTheSpireII.Scripts.patches.Singleton;

[RegisterSingleton]
public class WorkLevelSingleton: HookedSingletonModel
{
    public static HashSet<(Creature creature, Player player)> DamagingPairs { get; } = new();

    public WorkLevelSingleton() : base(HookType.Combat)
    {
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (result.UnblockedDamage > 0 && target.IsPlayer && dealer != null)
            DamagingPairs.Add((dealer, target.Player!));
        await base.AfterDamageReceived(choiceContext, target, result, props, dealer, cardSource);
    }




    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (combatState.RoundNumber == 1)
        {
            
            DamagingPairs.Clear();
            foreach (var creature in participants)
            {
                if (creature.Player?.Character is Slugcat slugcat)
                {
                    slugcat.SuppressHurtAnim = false;
                    slugcat.wawawa = false;
                }
            }

      
        }

        if (side == CombatSide.Player)
        {
            CombatUiPatch.callTurnStart();
            
        }
        await base.AfterSideTurnStart(side, participants, combatState);
    }

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        foreach (Creature creature in room.Allies)
        {
            if (creature.IsPlayer && creature.Player!.Character is Slugcat slugcat)
            {
                await CombatUiPatch.callVictory(creature.Player);
                RunSingleton.needmeow = false;
            }
        }

        await base.AfterCombatVictory(room);
    }
}