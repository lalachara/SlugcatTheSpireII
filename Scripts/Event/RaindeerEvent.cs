using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
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
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Rainworld.Scripts.Telemetry;
using System.Collections.Generic;

namespace Rainworld.Scripts.Event;

[RegisterSharedEvent]
// [RegisterSharedEvent] // 如果需要自定义生成条件，可以注册成通用再重载isAllowed
public sealed class RaindeerEvent : ModEventTemplate
{
    // —— 雨鹿事件遥测用（实例字段，随事件实例生命周期；AfterEventStarted 时清零）——
    /// <summary>是否喂食雨鹿（删牌）。</summary>
    private bool _fed;
    /// <summary>选择去商店(merchant)还是篝火(rest)；未喂食离开则为 null。</summary>
    private string? _branch;
    /// <summary>是否触发失败扣血。</summary>
    private bool _failedTookDamage;

    public override Task AfterEventStarted()
    {
        _fed = false;
        _branch = null;
        _failedTookDamage = false;
        return Task.CompletedTask;
    }

    // 背景图位置
    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: "res://Resource/Event/Raindeer1.png"
    );
    public override bool IsShared => true;
    // 设置一些数值
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(15m, ValueProp.Unblockable | ValueProp.Unpowered),
    ];

    // 什么时候会遇到。这里的条件是所有玩家的金币都大于等于60
    public override bool IsAllowed(IRunState runState)
    {
        var cfg = SpawnConfigRuntime.GetEffectiveConfig();
        return SpawnConfigRuntime.ModeShouldAppear(cfg.RaindeerEventSpawnMode);
    }

    // 生成事件初始选项。这里是两个选项：失去生命值或者失去金币，然后进入选择奖励阶段
    // 与 CustomEventModel.Option(delegate, pageKey) 一致：textKey = Id.Entry + ".pages." + page + ".options." + Slugify(方法名)
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, Feed, $"{Id.Entry}.pages.INITIAL.options.FEED"),
        new EventOption(this, Run, $"{Id.Entry}.pages.INITIAL.options.RUN"),
    ];
    // 失去生命
    private async Task Feed()
    {
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 1);
        var selected = await CardSelectCmd.FromDeckForRemoval(Owner, prefs);
        await CardPileCmd.RemoveFromDeck(selected.ToList<CardModel>());
        _fed = true;
        ChoosePage();
    }


    // 失去金币
    private async Task Run()
    {
        RunPage();
    }

    private void RunPage()
    {
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.INITIAL.options.RUN.description"));

    }
    private async Task Finish()
    {
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.RUN.options.RUN.description"));
    }


    // 进入事件第二阶段，两个选项：选择药水或者选择卡牌
    private void ChoosePage()
    {
        
        SetEventState(L10NLookup($"{Id.Entry}.pages.FEED.description"), [
            new EventOption(this, GoShop, $"{Id.Entry}.pages.FINISH.options.GO_SHOP"),
            new EventOption(this, GoRest, $"{Id.Entry}.pages.FINISH.options.GO_REST"),
        ]);
    }
    private void DiePage()
    {
        Tools.ChangeEventImg("res://Resource/Event/Raindeer2.png");
        SetEventState(L10NLookup($"{Id.Entry}.pages.DIE.description"), [
            new EventOption(this, Finish, $"{Id.Entry}.pages.RUN.options.RUN"),
        ]);
    }
// GoShop
    private async Task GoShop()
    {
        _branch = "merchant";
        var random = Rng.NextBool();
        if (random)
        {
            if(LocalContext.IsMe(Owner))
                await RunManager.Instance.EnterRoomWithoutExitingCurrentRoom(new MerchantRoom(), true);

        }
        else
        {

            _failedTookDamage = true;
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars.Damage, null, null);
            DiePage();
        }
    }

// GoRest
    private async Task GoRest()
    {
        _branch = "rest";
        var random = Rng.NextBool();
        if (random)
        {
            if(LocalContext.IsMe(Owner))
                await RunManager.Instance.EnterRoomWithoutExitingCurrentRoom(new RestSiteRoom(), true);

        }
        else
        {
            _failedTookDamage = true;
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars.Damage, null, null);
            DiePage();
        }
    }

    protected override void OnEventFinished()
    {
        base.OnEventFinished();
        try
        {
            Entry.Logger.Info($"[遥测] 雨鹿事件结束: fed={_fed}，branch={_branch}，failedTookDamage={_failedTookDamage}");
            TelemetrySetup.CaptureGameplay(
                "event_raindeer",
                new Dictionary<string, object?>
                {
                    ["character"] = Owner?.Character?.Id?.Entry?.ToString() ?? "unknown",
                    ["fed"] = _fed,
                    ["branch"] = _branch,
                    ["failed_took_damage"] = _failedTookDamage,
                });
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"[遥测] 雨鹿事件上报失败: {ex.Message}");
        }
    }
}