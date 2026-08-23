using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Rainworld.Scripts.Monster;
using Rainworld.Scripts.Telemetry; // TelemetrySetup.CaptureGameplay
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Rainworld.Scripts.Event;

[RegisterActEvent(typeof(Glory))] 
// [RegisterSharedEvent] // 如果需要自定义生成条件，可以注册成通用再重载isAllowed
public sealed class WatcherEvent : ModEventTemplate
{
    public static int times = 1;

    // —— 神经元自助餐遥测用（实例字段，避免污染 static times：times 被 Threechicken 复用为攻击连击次数）——
    /// <summary>事件进入前玩家的最大生命值。</summary>
    private int _maxHpBefore;
    /// <summary>本次事件最终选择吃掉神经元的次数。</summary>
    private int _eatCount;
    /// <summary>本次事件是否遭遇了战斗（吃到最后触发 Fight）。</summary>
    private bool _encounteredCombat;

    // 背景图位置
    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: "res://Resource/Event/Watcher1.png"
    );
    public override bool IsShared => true;
    // 设置一些数值
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];
    //public override EventLayoutType LayoutType => EventLayoutType.Combat; // 使用战斗场景

   // public override EncounterModel CanonicalEncounter => ModelDb.Encounter<ThreeChickenEvent>(); // 即将进行的遭遇
    
    // 什么时候会遇到。这里的条件是所有玩家的金币都大于等于60
    public override bool IsAllowed(IRunState runState)
    {
        var cfg = SpawnConfigRuntime.GetEffectiveConfig();
        return SpawnConfigRuntime.ModeShouldAppear(cfg.WatcherEventSpawnMode);
    }

    public override async Task AfterEventStarted()
    {
        // 记录进入事件前的玩家最大生命值，作为“进入前”基线。
        _maxHpBefore = Owner?.Creature?.MaxHp ?? 0;
        _eatCount = 0;
        _encounteredCombat = false;
        await base.AfterEventStarted();
    }

    protected override void OnEventFinished()
    {
        base.OnEventFinished();
        try
        {
            // 离开时玩家最大生命值（吃完、或战后返回结算时）。
            var maxHpAfter = Owner?.Creature?.MaxHp ?? 0;
            Entry.Logger.Info($"[遥测] 神经元自助餐结束: eatCount={_eatCount}，fought={_encounteredCombat}，maxHpBefore={_maxHpBefore}，maxHpAfter={maxHpAfter}");
            TelemetrySetup.CaptureGameplay(
                "event_neuron_feast",
                new Dictionary<string, object?>
                {
                    ["character"] = Owner?.Character?.Id?.Entry?.ToString() ?? "unknown",
                    ["encountered_combat"] = _encounteredCombat,
                    ["neuron_eat_count"] = _eatCount,
                    ["max_hp_before"] = _maxHpBefore,
                    ["max_hp_after"] = maxHpAfter,
                });
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"[遥测] 神经元自助餐上报失败: {ex.Message}");
        }
    }

    // 生成事件初始选项。这里是两个选项：失去生命值或者失去金币，然后进入选择奖励阶段
    // 与 CustomEventModel.Option(delegate, pageKey) 一致：textKey = Id.Entry + ".pages." + page + ".options." + Slugify(方法名)
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, Eat, $"{Id.Entry}.pages.INITIAL.options.EAT"),
        new EventOption(this, Run, $"{Id.Entry}.pages.INITIAL.options.RUN"),
    ];
    // 失去生命
    private async Task Eat()
    {
        times = 1;
        _eatCount = 1;
        await CreatureCmd.GainMaxHp(Owner!.Creature, 5);
        SetEventState(L10NLookup($"{Id.Entry}.pages.EAT.description"), [
            new EventOption(this, EatAgain, $"{Id.Entry}.pages.INITIAL.options.EAT"),
            new EventOption(this, Run, $"{Id.Entry}.pages.INITIAL.options.RUN"),
        ]);
        
    }
    
    private async Task EatAgain()
    { 
        times ++;
        _eatCount ++;
        await CreatureCmd.GainMaxHp(Owner!.Creature, 5);
        
        int random = Rng.NextInt(0,6);
        if (random != 0)
        {
            SetEventState(L10NLookup($"{Id.Entry}.pages.EAT.description"), [
                new EventOption(this, EatAgain, $"{Id.Entry}.pages.INITIAL.options.EAT"),
                new EventOption(this, Run, $"{Id.Entry}.pages.INITIAL.options.RUN"),
            ]);
        }
        else
        {
            Tools.ChangeEventImg("res://Resource/Event/Watcher2.png");
            SetEventState(L10NLookup($"{Id.Entry}.pages.FIGHT.description"), [
                new EventOption(this, Fight, $"{Id.Entry}.pages.FIGHT.options.FIGHT"),
                new EventOption(this, null, $"{Id.Entry}.pages.FIGHT.options.RUN",true),
            ]);
        }

    }

    // 失去金币
    private async Task Run()
    {
        SetEventFinished(L10NLookup($"{Id.Entry}_EVENT_RAINDEER_EVENT.pages.RUN.description"));
    }

    private Task Fight()
    {
        _encounteredCombat = true;
        EnterCombatWithoutExitingEvent<ThreeChickenEvent>(
            Array.Empty<Reward>(),
            shouldResumeAfterCombat: false
        );
        return Task.CompletedTask;
    }



}