using System.Collections.Generic;
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
}
