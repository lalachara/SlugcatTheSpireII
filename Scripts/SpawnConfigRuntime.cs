using System.Linq;
using System.Reflection;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using Rainworld.Scripts.Monster;

namespace Rainworld.Scripts;

/// <summary>
/// spawn 配置的运行时解析。多人模式优先使用宿主机下发（写入 <see cref="SpawnConfigRunData" /> 并对局内共享）的
/// 配置，离线/单人则回退到本机 mod_settings 快照，从而始终以宿主机为准，避免 de-sync。
/// </summary>
internal static class SpawnConfigRuntime
{
	private static RunState? CurrentRunState()
	{
		var manager = RunManager.Instance;
		if (manager == null)
			return null;
		return manager.GetType()
			.GetProperty("State", BindingFlags.Instance | BindingFlags.NonPublic)
			?.GetValue(manager) as RunState;
	}

	private static bool IsMultiplayer() =>
		RunManager.Instance?.NetService?.Type.IsMultiplayer() ?? false;

	/// <summary>当前对局是否存在蛞蝓猫玩家；无对局（如主菜单）时视为否。</summary>
	internal static bool IsRunUsingSlugcat()
	{
		var state = CurrentRunState();
		return state != null && state.Players.Any(p => p.Character is Slugcat);
	}

	/// <summary>
	/// 获取当前生效的 spawn 配置。多人且已有宿主机数据时用宿主机值，否则用本机配置快照。
	/// </summary>
	internal static SpawnConfigRunData GetEffectiveConfig()
	{
		var state = CurrentRunState();
		if (state != null && IsMultiplayer())
		{
			if (SlugcatResources.SpawnConfigData.TryGet(state, out var host))
				return host;
		}

		return RainworldModConfig.CreateSpawnSnapshot();
	}

	/// <summary>
	/// 根据明细模式判断最终是否出现。总控已在设置界面「一键覆写」到各明细，因此这里只需读取明细即可，无需再判断总控。
	/// Always 出现；OnlySlugcat 按当前对局是否含蛞蝓猫决定；Disabled 不出现。
	/// </summary>
	internal static bool ModeShouldAppear(MonsterSpawnMode detailMode) => detailMode switch
	{
		MonsterSpawnMode.Always => true,
		MonsterSpawnMode.OnlySlugcat => IsRunUsingSlugcat(),
		MonsterSpawnMode.Disabled => false,
		_ => true,
	};
}
