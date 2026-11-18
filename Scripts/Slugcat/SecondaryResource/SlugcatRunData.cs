﻿namespace Rainworld.Scripts;

public sealed class SlugcatRunData
{
    public int WorkLevel { get; set; } = 0;
    public int MaxWorkLevel { get; set; } = 0;
    public int Food { get; set; } = 0;
    public int MaxFood { get; set; } = 0;
    public int SleepFood { get; set; } = 0;
    public bool IsInitialized { get; set; } = false;
    
    public bool HaveChaoshengt { get; set; } = false;

    /// <summary>
    /// 本场战斗内是否失去过业力（「轮回之花」据此在战斗奖励里追加瓶装业力花）。
    /// 战斗开始时清零，发放奖励后清零。
    /// </summary>
    public bool KarmaLostInCombat { get; set; } = false;

    /// <summary>
    /// 该玩家在角色选择界面选中的 Spine 皮肤。
    /// 它是按玩家存储的局内数据，联机开局时会随大厅暂存数据同步给所有客户端，
    /// 因此每个客户端都能用队友自己选的皮肤来渲染队友的角色。
    /// </summary>
    public string Skin { get; set; } = string.Empty;
    
}