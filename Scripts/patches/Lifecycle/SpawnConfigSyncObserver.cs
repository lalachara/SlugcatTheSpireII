using STS2RitsuLib;
using STS2RitsuLib.RunData;

namespace Rainworld.Scripts;

/// <summary>
/// 宿主机在对局提交（Committing）阶段把当前的 spawn 配置写入大厅暂存数据，
/// 使全体客户端在对局内读到同一份配置，以宿主机为准，避免多人 de-sync。
/// </summary>
public sealed class SpawnConfigSyncObserver : ILifecycleObserver
{
	public void OnEvent(IFrameworkLifecycleEvent evt)
	{
		if (evt is not RunSavedDataLobbyStagingEvent staging)
			return;

		if (staging.Reason != RunSavedDataLobbyStagingReason.Committing)
			return;

		if (!staging.IsMultiplayer || !staging.IsHost)
			return;

		var snapshot = RainworldModConfig.CreateSpawnSnapshot();

		SlugcatResources.SpawnConfigData.Lobby.Modify(staging.Lobby, d =>
		{
			d.MonsterMasterSpawnMode = snapshot.MonsterMasterSpawnMode;
			d.MushroomMonsterSpawnMode = snapshot.MushroomMonsterSpawnMode;
			d.IronbirdMonsterSpawnMode = snapshot.IronbirdMonsterSpawnMode;
			d.FirecabMonsterSpawnMode = snapshot.FirecabMonsterSpawnMode;
			d.FirecentMonsterSpawnMode = snapshot.FirecentMonsterSpawnMode;
			d.WatercentMonsterSpawnMode = snapshot.WatercentMonsterSpawnMode;
			d.GreencabMonsterSpawnMode = snapshot.GreencabMonsterSpawnMode;
			d.EventMasterSpawnMode = snapshot.EventMasterSpawnMode;
			d.RaindeerEventSpawnMode = snapshot.RaindeerEventSpawnMode;
			d.WatcherEventSpawnMode = snapshot.WatcherEventSpawnMode;
		});
	}
}
