using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using STS2RitsuLib.Settings;
using STS2RitsuLib.Telemetry;

namespace Rainworld.Scripts.Telemetry;

/// <summary>
/// 遥测设置：注册申请方（直连 PostHog us 区）。
/// 仅当玩家在设置页接受授权后，RitsuLib 才会把数据发往 PostHog。
/// </summary>
public static partial class TelemetrySetup
{
    private const string ApplicantId = Entry.ModId;
    private const string PostHogHost = "https://us.i.posthog.com";
    // 公开项目 key（直连时不可隐藏；介意可后续改用 Cloudflare Worker 代理）。
    private const string PostHogProjectKey = "phc_mWHG3Ua7eaQuGAvT9DsnAnjEDuQAAGeQoXMUgfMRUqbD";

    /// <summary>自定义平衡/玩法事件的申请 id（角色选择、遗物、卡牌奖励等）。</summary>
    private const string GameplayRequestId = "gameplay_events";

    private static ITelemetryClient Client = null!;

    public static void Register()
    {
        TelemetryRegistry.RegisterApplicant(new TelemetryApplicant
        {
            ApplicantId = ApplicantId,
            OwnerModId = Entry.ModId,
            DisplayName = "雨世界蛞蝓猫 mod",
            DisplayNameText = ModSettingsText.Literal("雨世界蛞蝓猫 mod"),
            Adapter = new PostHogTelemetryAdapter(PostHogHost, PostHogProjectKey),
            Requests =
            [
                TelemetryRequest.BasicUsage(
                    ModSettingsText.Literal("发送版本、平台、语言和匿名安装 ID，用来估算兼容性问题范围。")),
                TelemetryRequest.RunHistory(
                    ModSettingsText.Literal("发送已结束跑局的原版 run-history，用来分析平衡性。")),
                TelemetryRequest.Diagnostics(
                    ModSettingsText.Literal("发送异常和诊断上下文，用来定位崩溃。")),
                TelemetryRequest.Custom(
                    GameplayRequestId,
                    ModSettingsText.Literal("发送角色选择、遗物获得、卡牌奖励等玩法事件，用来分析平衡性。")),
            ],
        });

        Client = TelemetryApi.GetClient(ApplicantId);

        // 诊断日志：确认授权状态（Godot 输出面板搜索 [遥测] 查看）
        Entry.Logger.Info(
            $"[遥测] 注册完成 applicant={ApplicantId}，play_events 授权={Client.IsEnabled(GameplayRequestId)}");
    }

    /// <summary>上报玩法事件（需玩家授权 <c>gameplay_events</c>）。</summary>
    public static void CaptureGameplay(string eventName, IReadOnlyDictionary<string, object?>? properties = null)
    {
        Entry.Logger.Info(
            $"[遥测] CaptureGameplay: event={eventName}，request={GameplayRequestId}，已授权={Client.IsEnabled(GameplayRequestId)}");
        Client.Capture(eventName, GameplayRequestId, properties);
    }

    /// <summary>上报异常（需玩家授权 <c>diagnostics</c>）。未授权时是 no-op。</summary>
    public static void ReportException(Exception ex, IReadOnlyDictionary<string, object?>? properties = null)
        => Client.CaptureException(ex, properties);

    /// <summary>判断异常是否应当上报（避免把空引用等无害的框架杂音带上）。可按需放宽。</summary>
    public static bool ShouldReport(Exception ex) =>
        ex is not (InvalidOperationException) || ex.Message.Length > 0;

    /// <summary>
    /// 挂接全局未捕获异常钩子，把玩家运行时遇到、但未在 mod 代码内捕获的异常上报到 PostHog（需授权 diagnostics）。
    /// 同时覆盖同步主线程未捕获异常（<see cref="AppDomain.UnhandledException" />）与 <see cref="Task" /> 未观察异常。
    /// </summary>
    public static void AttachGlobalExceptionHooks()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
                ReportException(ex, new Dictionary<string, object?> { ["kind"] = "unhandled" });
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            if (!ShouldReport(e.Exception))
            {
                e.SetObserved();
                return;
            }
            ReportException(e.Exception, new Dictionary<string, object?> { ["kind"] = "unobserved_task" });
            e.SetObserved();
        };
    }
}
