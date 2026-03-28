using Rainworld.relics;
using Rainworld.Scripts.Card.Liver.Attack;

namespace SlugcatTheSpireII.Scripts.patches;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using System.Collections.Generic;
using System.Reflection;


    [HarmonyPatch]
    public static class TouchOfOrobasPatch
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(TouchOfOrobas),
                nameof(TouchOfOrobas.GetUpgradedStarterRelic),
                new Type[]
                {
                    typeof(RelicModel)
                }
            );
        }

        [HarmonyPrefix]
        public static bool Prefix(
            ref RelicModel __result,
            RelicModel starterRelic
            )
        {
            if (starterRelic.Id == ModelDb.Relic<Liver_Fruit1>().Id)
            {
                __result = ModelDb.Relic<Liver_Fruit2>();
                return false;
            }

            return true;
        }
    }
    
    public static class ArchaicToothReflectionHelper
    {
        public static void AddCustomTranscendenceUpgrades()
        {
            // 目标类
            Type targetType = typeof(ArchaicTooth);

            // 获取私有静态字典字段 (精确匹配：private static Dictionary<ModelId, CardModel> TranscendenceUpgrades)
            FieldInfo fieldInfo = targetType.GetField(
                "TranscendenceUpgrades",
                BindingFlags.NonPublic | BindingFlags.Static
            );

            // 获取原版字典实例
            Dictionary<ModelId, CardModel> originalDict = 
                fieldInfo.GetValue(null) as Dictionary<ModelId, CardModel>;

            // 安全校验
            if (originalDict == null) return;

            // ==============================================
            // 在原版基础上 添加你的自定义升级映射
            // ==============================================
            originalDict.Add(ModelDb.Card<你的卡牌1>().Id, ModelDb.Card<你的升级卡牌1>());
            originalDict.Add(ModelDb.Card<你的卡牌2>().Id, ModelDb.Card<你的升级卡牌2>());

            // 想加多少就加多少，原版内容 100% 保留
        }
    }