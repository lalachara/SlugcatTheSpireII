namespace Rainworld.Scripts;

/// <summary>
/// 对局内共享的「非蛞蝓猫角色检索其它角色卡排除蛞蝓猫卡」配置快照。
/// 由宿主机在对局提交（Committing）阶段写入大厅暂存数据，全体客户端通过对局内 <c>Get(RunState)</c> 读到同一份，
/// 从而避免各自读本机配置导致的 de-sync。
/// </summary>
public sealed class OtherCharPoolFilterRunData
{
	/// <summary>非蛞蝓猫角色使用原版「检索其它角色卡」机制时，排除蛞蝓猫角色的卡。</summary>
	public bool EnableExcludeSlugcatFromOtherCharPool { get; set; }
}
