using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using Rainworld.relics;
using Rainworld.Scripts.Powers;
using STS2RitsuLib;
using STS2RitsuLib.RunData;

namespace Rainworld.Scripts;

public sealed class CreatureDeathObserver : ILifecycleObserver
{
    public void OnEvent(IFrameworkLifecycleEvent evt)
    {
        if (evt is CreatureDyingEvent dying)
        {
            if (dying.Creature.IsPlayer && dying.Creature.Player!.Character is Slugcat && dying.Creature.HasPower<RedmushroomPower>() &&SlugcatData.canbekill(dying.Creature.Player))
            {
                SlugcatData.setworklevel(dying.Creature.Player,0);
            }
            
        }

    }
    
    
}