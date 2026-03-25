using System.Reflection;
using System.Text.Json;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;
using MegaCrit.Sts2.Core.Saves.Runs;
using Rainworld.Scripts;

namespace Rainworld.Scripts.patches;

[Serializable]
public class RainworldPlayerData
{
    public int WorkLevel { get; set; }
    public int MaxWorkLevel { get; set; }
    public int Food { get; set; }
    public int SleepFood { get; set; }
    public int MaxFood { get; set; }
    public bool isinit { get; set; } = false;
    
}
public static class RainworldData
{
    // 运行时缓存
    public static RainworldPlayerData Current { get; set; } = new RainworldPlayerData();

    // 文件名
    private const string FileName = "rainworld_run_data.json";

    // 获取存档文件路径
    private static string GetSaveFilePath()
    {
        // 1. 先拿到 Godot 的用户目录根路径（真实物理路径）
        string userRoot = ProjectSettings.GlobalizePath("user://");

        // 2. 从 SaveManager 获取当前 Profile ID，不要硬编码
        int profileId = SaveManager.Instance.CurrentProfileId;

        // 3. 正确拼接路径（Path.Combine 会自动处理分隔符）
        string saveDir = Path.Combine(userRoot, "steam\\76561198867610076\\modded", "profile"+profileId, "saves");

        // 4. 关键：如果目录不存在，先创建它！
        if (!Directory.Exists(saveDir))
        {
            Directory.CreateDirectory(saveDir);
        }

        // 5. 返回完整文件路径
        string fullPath = Path.Combine(saveDir, FileName);
        return fullPath;
    }

    // 新局：清空数据 + 删除旧文件
    public static void OnNewRun()
    {
        Current = new RainworldPlayerData();
        try
        {
            string path = GetSaveFilePath();
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception ex)
        {
        }
    }

    // 存档：写入文件
    public static void SaveToDisk()
    {
        try
        {
            string path = GetSaveFilePath();
            string json = JsonSerializer.Serialize(Current);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
        }
    }

    // 读档：从文件读取
    public static void LoadFromDisk()
    {
        
            string path = GetSaveFilePath();
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                Current = JsonSerializer.Deserialize<RainworldPlayerData>(json) ?? new RainworldPlayerData();
                if (SlugcatField.playerdata?.Player != null)
                {
                    SlugcatField.playerdata.Initialize();
                }
            }
            else
            {
                Current = new RainworldPlayerData();
            }
        
        
    }
}

[HarmonyPatch(typeof(RunManager), "SetUpNewSinglePlayer")]
public static class RunManager_NewSingle_Patch
{
    static void Postfix()
    {
        RainworldData.OnNewRun();
        SlugcatField.playerdata = null;

    }
}

[HarmonyPatch(typeof(RunManager), "SetUpNewMultiPlayer")]
public static class RunManager_NewMulti_Patch
{
    static void Postfix()
    {
        SlugcatField.playerdata = null;
        RainworldData.OnNewRun();
    }
}



// ==========================
// 2. 游戏存档时：我们也存
// ==========================
[HarmonyPatch(typeof(SaveManager), "SaveRun")]
public static class SaveManager_SaveRun_Patch
{
    static void Prefix()
    {
        // 先把 SlugcatData 里的数据同步到 RainworldData
        if (SlugcatField.playerdata != null)
        {
            SlugcatField.playerdata.SaveDataToCharacter();
        }
        // 然后存盘
        RainworldData.SaveToDisk();
    }
}

// ==========================
// 3. 游戏读档时：我们也读（时序最早）
// ==========================
[HarmonyPatch(typeof(SaveManager), "LoadRunSave")]
public static class SaveManager_LoadRun_Patch
{
    static void Postfix()
    {
        RainworldData.LoadFromDisk();
    }
}

