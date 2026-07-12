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
    
}