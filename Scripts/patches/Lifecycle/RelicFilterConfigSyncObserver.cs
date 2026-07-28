using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib;
using STS2RitsuLib.RunData;
using Test.Scripts;

namespace Rainworld.Scripts;

public sealed class RelicFilterConfigSyncObserver : ILifecycleObserver
{
    public void OnEvent(IFrameworkLifecycleEvent evt)
    {
        if (evt is not RunSavedDataLobbyStagingEvent staging)
            return;

        if (staging.Reason != RunSavedDataLobbyStagingReason.Committing)
            return;

        if (!staging.IsMultiplayer)
            return;

        bool isHost = RunManager.Instance?.NetService?.NetId == 1;
        if (!isHost)
            return;

        SlugcatResources.RelicFilterData.Lobby.Modify(staging.Lobby, d =>
        {
            d.EnableMaxHpCantUsedRelicFix = RainworldModConfig.EnableMaxHpCantUsedRelicFix;
            d.EnableHpRelicFix = RainworldModConfig.EnableHpRelicFix;
        });
    }
}