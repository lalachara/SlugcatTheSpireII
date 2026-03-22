using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves;
using Rainworld.Scripts;

namespace demo.Scripts.patches;
[HarmonyPatch]
public class SaveDataPatch
{
    [HarmonyTargetMethod]
    public static MethodBase TargetMethod()
    {
        // 补丁 SaveManager 的 SaveRun 方法（所有保存场景都会调用）
        // 注意：参数要和 dnSpy 里看到的一致，这里是最常见的重载
        return AccessTools.Method(
            typeof(SaveManager),
            nameof(SaveManager.SaveRun),
            new[] { typeof(AbstractRoom),typeof(bool) } // 入参是 AbstractRoom?（预完成的房间）
        );
    }

    public static bool Prefix(SaveManager __instance, AbstractRoom? preFinishedRoom)
    {
        try
        {
            // ====================== 你的自定义逻辑 ======================
            // 示例1：同步 SlugcatData 到存档缓存（之前写的逻辑）
            GD.PrintErr("执行保存数据。");

            SlugcatField.SaveAllData();
            
        }
        catch (Exception e)
        {
            // 捕获异常，避免你的逻辑出错导致游戏无法保存
        }

        // 返回 true：继续执行原版的保存逻辑（必须返回 true，否则游戏不会保存）
        return true;
    }
}