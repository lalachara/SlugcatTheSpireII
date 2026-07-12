using STS2RitsuLib;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.RunData;

namespace Rainworld.Scripts;

public static class SlugcatResources
{
    public static PlayerRunSavedData<SlugcatRunData> Data { get; private set; } = null!;
    public static SecondaryResourceDefinition CorruptValue { get; private set; } = null!;
    public static string CorruptValueID { get; private set; } = string.Empty;
    public static void Register()
    {
        using (RitsuLibFramework.BeginModDataRegistration(Entry.ModId))
        {
            var store = RitsuLibFramework.GetRunSavedDataStore(Entry.ModId);
            Data = store.RegisterPerPlayer(
                key: "slugcat_data",
                defaultFactory: () => new SlugcatRunData(),
                options: new RunSavedDataOptions
                {
                    WritePolicy = RunSavedDataWritePolicy.WhenSet,
                    SyncLobbyOnChange = true,
                });
        }
        
        var registry = RitsuLibFramework.GetSecondaryResourceRegistry(Entry.ModId);
        
        CorruptValue = registry.Register("CorruptValue", new SecondaryResourceDefinition(
            defaultAmount: 0,
            baseMaxAmount: 5,
            turnStartPolicy: SecondaryResourceTurnStartPolicy.None,
            persistencePolicy: SecondaryResourcePersistencePolicy.Combat
        ));
        CorruptValueID = CorruptValue.Id;
    }
}