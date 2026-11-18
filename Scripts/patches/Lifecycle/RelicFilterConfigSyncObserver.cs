﻿using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib;
using STS2RitsuLib.RunData;

namespace Rainworld.Scripts;

public sealed class RelicFilterConfigSyncObserver : ILifecycleObserver
{
    public void OnEvent(IFrameworkLifecycleEvent evt)
    {
        if (evt is not RunSavedDataLobbyStagingEvent staging)
            return;

        if (staging.Reason != RunSavedDataLobbyStagingReason.Committing)
            return;

        if (!staging.IsMultiplayer || !staging.IsHost)
            return;

        SlugcatResources.RelicFilterData.Lobby.Modify(staging.Lobby, d =>
        {
            //d.EnableMaxHpCantUsedRelicFix = RainworldModConfig.EnableMaxHpCantUsedRelicFix;
            d.EnableHpRelicFix = RainworldModConfig.EnableHpRelicFix;
            d.EnableMaxHpRelicFix = RainworldModConfig.EnableMaxHpRelicFix;
            d.EnableCombatCantLossMaxHp = RainworldModConfig.EnableCombatCantLossMaxHp;
            d.EnableDisableAbyssalBaths = RainworldModConfig.EnableDisableAbyssalBaths;
        });
    }
}