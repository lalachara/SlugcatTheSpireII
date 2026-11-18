using System.Reflection;
    using Godot;
    using MegaCrit.Sts2.Core.Modding;
    using MegaCrit.Sts2.Core.Models;
    using Rainworld.relics;
using Rainworld.Scripts.Card;
using Rainworld.Scripts.Card.Liver.Attack;
using Rainworld.Scripts.Card.Monk;
using Rainworld.Scripts.Card.RedCat;
using Rainworld.Scripts.Card.River.Skills;
using Rainworld.Scripts.patches;
using Rainworld.Scripts.Monster.Minions;
using Rainworld.Scripts.Telemetry;
using STS2RitsuLib;
using STS2RitsuLib.Audio;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;
using STS2RitsuLib.Scaffolding.Cards.HandOutline;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace Rainworld.Scripts;

[ModInitializer(nameof(Init))]
public class Entry
{
    public const string ModId = "SlugcatTheSpireII";
    public static readonly Logger Logger = RitsuLibFramework.CreateLogger(ModId);

    public static void Init()
    {
        var assembly = Assembly.GetExecutingAssembly();

        // 注册自定义卡牌 tag（必须在模型初始化之前完成，否则会被 RitsuLib 拒绝）
        RainworldCardTags.Register();

        // 使用 RitsuLib 封装的补丁系统
        var patcher = RitsuLibFramework.CreatePatcher(ModId, "core-patches");
        patcher.RegisterPatches<RainworldPatches>();
        if (!patcher.PatchAll())
            throw new InvalidOperationException("Critical patches failed.");

        // 注册模组配置
        RainworldModConfig.Register();

        // 注册遥测（直连 PostHog）
        TelemetrySetup.Register();

        // 注册次级资源（食物、业力等级）
        SlugcatResources.Register();

        // 注册 Godot 脚本
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);

        // 自动注册内容（卡牌、遗物、能力等）
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);


        SlugCatCardTargets.Register();
        
        RitsuLibFramework.RegisterArchaicToothTranscendenceMapping<Rainworld_Liver_Spear, Rainworld_Liver_Spearfire>();
        RitsuLibFramework.RegisterArchaicToothTranscendenceMapping<Rainworld_River_Inertia, Rainworld_River_Playwater>();
        RitsuLibFramework.RegisterArchaicToothTranscendenceMapping<Rainworld_Redcat_Better, Rainworld_Redcat_Starter>();
        RitsuLibFramework.RegisterArchaicToothTranscendenceMapping<Rainworld_Monk_Interact, Rainworld_Monk_Wake>();

        RitsuLibFramework.RegisterTouchOfOrobasRefinementMapping<Liver_Fruit1, Liver_Fruit2>();
        RitsuLibFramework.RegisterTouchOfOrobasRefinementMapping<River_Waterball1, River_Waterball2>();
        RitsuLibFramework.RegisterTouchOfOrobasRefinementMapping<Redcat_Book1, Redcat_Book2>();
        RitsuLibFramework.RegisterTouchOfOrobasRefinementMapping<Monk_Bottlecab, Monk_BigBottlecab>();
        
        ModCardHandOutlineRegistry.Register<CardModel>(ModCardHandOutlineRules.Fixed( // 特定种类卡牌。可以设置为你的卡牌基类，让所有子类发光。
            card => card.Keywords.Contains(RainworldKeywords.Corrupt), // 发光条件
            Colors.Purple // 发光颜色
            // 0, // （可选）优先级。更高的才会展示。
            // false // 不可打出时隐藏边框
        ));
        ModCardHandOutlineRegistry.Register<CardModel>(ModCardHandOutlineRules.Fixed( // 特定种类卡牌。可以设置为你的卡牌基类，让所有子类发光。
            card => card.Keywords.Contains(RainworldKeywords.Treasurespear), // 发光条件
            Colors.Gold // 发光颜色
            // 0, // （可选）优先级。更高的才会展示。
            // false // 不可打出时隐藏边框
        ));
        FmodStudioDeferredBankRegistration.RegisterBank("res://Resource/Sound/SlugcatTheSpireII.bank");
        FmodStudioDeferredBankRegistration.RegisterStudioGuidMappings("res://Resource/Sound/GUIDs.txt");
        
        RitsuLibFramework.SubscribeLifecycle(new CreatureDeathObserver());
        RitsuLibFramework.SubscribeLifecycle(new SpawnConfigSyncObserver());
        RitsuLibFramework.SubscribeLifecycle(new OtherCharPoolFilterConfigSyncObserver());
        RitsuLibFramework.SubscribeLifecycle(new RelicFilterConfigSyncObserver());
        RitsuLibFramework.SubscribeLifecycle(new TelemetryObserver());
        Entry.Logger.Info("[遥测] TelemetryObserver 已订阅生命周期事件");

    }
}