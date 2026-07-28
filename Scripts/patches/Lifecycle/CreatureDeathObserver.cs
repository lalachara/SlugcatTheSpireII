using STS2RitsuLib;

namespace Rainworld.Scripts;

public sealed class CreatureDeathObserver : ILifecycleObserver
{
    public void OnEvent(IFrameworkLifecycleEvent evt)
    {
        if (evt is CreatureDyingEvent dying)
        {
            Entry.Logger.Info($"生物即将死亡：{dying.Creature.Name}");
            // 濒死逻辑
        }
        else if (evt is CreatureDiedEvent died)
        {
            if (!died.WasRemovalPrevented)
            {
                Entry.Logger.Info($"生物已死亡：{died.Creature.Name}");
                // 死亡逻辑
            }
        }
    }
}