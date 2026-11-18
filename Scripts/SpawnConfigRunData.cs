namespace Rainworld.Scripts;

/// <summary>
/// 对局内共享的 spawn 配置快照。由宿主机在对局提交（Committing）阶段写入大厅暂存数据，
/// 全体客户端通过对局内 <c>Get(RunState)</c> 读到同一份，从而避免各自读本机配置导致的 de-sync。
/// 字段与 <see cref="RainworldModSettings" /> 的 spawn 部分一一对应。
/// </summary>
public sealed class SpawnConfigRunData
{
	/// <summary>怪物区三态总控：始终 / 仅蛞蝓猫 / 禁用。为 Always 时跟随各怪物明细。</summary>
	public MonsterSpawnMode MonsterMasterSpawnMode { get; set; } = MonsterSpawnMode.Always;

	public MonsterSpawnMode MushroomMonsterSpawnMode { get; set; } = MonsterSpawnMode.Always;
	public MonsterSpawnMode IronbirdMonsterSpawnMode { get; set; } = MonsterSpawnMode.Always;
	public MonsterSpawnMode FirecabMonsterSpawnMode { get; set; } = MonsterSpawnMode.Always;
	public MonsterSpawnMode FirecentMonsterSpawnMode { get; set; } = MonsterSpawnMode.Always;
	public MonsterSpawnMode WatercentMonsterSpawnMode { get; set; } = MonsterSpawnMode.Always;
	public MonsterSpawnMode GreencabMonsterSpawnMode { get; set; } = MonsterSpawnMode.Always;

	/// <summary>事件区三态总控：始终 / 仅蛞蝓猫 / 禁用。为 Always 时跟随各事件明细。</summary>
	public MonsterSpawnMode EventMasterSpawnMode { get; set; } = MonsterSpawnMode.Always;

	public MonsterSpawnMode RaindeerEventSpawnMode { get; set; } = MonsterSpawnMode.Always;
	public MonsterSpawnMode WatcherEventSpawnMode { get; set; } = MonsterSpawnMode.Always;
	public MonsterSpawnMode ScavengerEventSpawnMode { get; set; } = MonsterSpawnMode.Always;
}
