using Rainworld.Scripts.Card.Liver.Attack;

namespace SlugcatTheSpireII.Scripts.patches;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using System.Collections.Generic;
using System.Reflection;

    // 在 Mod 初始化时调用一次，比如在 _Ready() 里
    public static class ArchaicToothReflectionPatch
    {
        public static void AddMyCards()
        {
            var type = typeof(ArchaicTooth);
            var prop = type.GetProperty("TranscendenceUpgrades", BindingFlags.NonPublic | BindingFlags.Static);
            if (prop == null) return;

            var dict = (Dictionary<ModelId, CardModel>)prop.GetValue(null);

            // 在这里添加你的卡牌
            dict.Add(ModelDb.Card<Rainworld_Liver_Spear>().Id, ModelDb.Card<Rainworld_Liver_Spearfire>());
        }
    }
