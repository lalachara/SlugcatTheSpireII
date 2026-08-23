using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib;
using STS2RitsuLib.RunData;

namespace Rainworld.Scripts;

/// <summary>
/// 宿主机在对局提交（Committing）阶段把当前的「非蛞蝓猫角色检索其它角色卡排除蛞蝓猫卡」配置写入大厅暂存数据，
/// 使全体客户端在对局内读到同一份配置，以宿主机为准，避免多人 de-sync。
/// </summary>
public sealed class OtherCharPoolFilterConfigSyncObserver : ILifecycleObserver
{
	public void OnEvent(IFrameworkLifecycleEvent evt)
	{
		if (evt is not RunSavedDataLobbyStagingEvent staging)
			return;

		if (staging.Reason != RunSavedDataLobbyStagingReason.Committing)
			return;

		if (!staging.IsMultiplayer || !staging.IsHost)
			return;

		SlugcatResources.OtherCharPoolFilterData.Lobby.Modify(staging.Lobby, d =>
		{
			d.EnableExcludeSlugcatFromOtherCharPool = RainworldModConfig.EnableExcludeSlugcatFromOtherCharPool;
		});
	}
}
