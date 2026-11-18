using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Rainworld.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainworld.Scripts.Event;

/// <summary>
/// 高墙之上（FP_EVENT）。
/// 出现范围：<b>仅第 1 幕</b>——密林（Overgrowth）与暗巷（Underdocks）两个区域都会出现，其它幕不出现。
/// 触发条件为「写死仅蛞蝓猫」：队伍中所有玩家都必须是蛞蝓猫才可能遇到，
/// 不读取 SpawnConfigRuntime、不额外新增玩家可见配置项（参考雨鹿/神经元事件但固定为蛞蝓猫）。
/// 流程：INITIAL（攀爬/离开）→ 攀爬掉血 → CLIMB（继续）→ 造物主交流（接受）→ 业力上限+1 → 离开。
/// </summary>
// 仅第 1 幕：密林（Overgrowth）+ 暗巷（Underdocks）；Hive/Glory 等其它幕不注册
[RegisterActEvent(typeof(Overgrowth))]
[RegisterActEvent(typeof(Underdocks))]
public sealed class FpEvent : ModEventTemplate
{
    private static string Img(string name) => $"res://Resource/Event/{name}.png";

    // 背景图位置：进入事件显示阶段0图（river 用 Rock0River；redcat 2/3 阶段用 RockNRed；monk 2/3 阶段用 RockNMonk）。
    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: StageImage(0)
    );
    public override bool IsShared => true;

    // 初始页描述：按当前角色差分（river 用 INITIAL.description.RIVER，红猫/默认用默认文案）。
    public override LocString InitialDescription =>
        DiffLoc($"{Id.Entry}.pages.INITIAL.description");
    // 设置一些数值：攀爬会固定失去15点生命。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(15m, ValueProp.Unblockable | ValueProp.Unpowered),
    ];

    // 触发条件：写死仅蛞蝓猫。队伍里任意一个玩家不是蛞蝓猫就无法遇到。
    public override bool IsAllowed(IRunState runState) =>
        runState.Players.All(p => p.Character is Slugcat);

    // —— 角色差分 helper ——
    // 角色后缀：river→".RIVER"、redcat→".REDCAT"、monk→与 liver 同文本（空串）、默认 liver→空串（回退默认 key）。
    private string CharSuffix => Owner?.Character switch
    {
        Rainworld_River => ".RIVER",
        Rainworld_Redcat => ".REDCAT",
        // monk 的文本和 liver 完全一样（不建 .MONK 键），只换第 2/3 阶段的图
        Rainworld_Monk => string.Empty,
        _ => string.Empty,
    };

    /// <summary>按当前角色取本地化 key：存在「角色后缀 key」则用之，否则回退默认 key。</summary>
    private LocString DiffLoc(string key) =>
        LocString.GetIfExists(LocTable, key + CharSuffix) ?? L10NLookup(key);

    /// <summary>按当前角色返回指定阶段的图片路径。</summary>
    private string StageImage(int stage)
    {
        if (Owner?.Character is Rainworld_River)
            return Img($"Rock{stage}River");
        if (Owner?.Character is Rainworld_Redcat && stage >= 2)
            return Img($"Rock{stage}Red");
        // monk 只有第 2、3 阶段的差分图（Rock2Monk / Rock3Monk），0/1 阶段沿用默认图
        if (Owner?.Character is Rainworld_Monk && stage >= 2)
            return Img($"Rock{stage}Monk");
        return Img($"Rock{stage}");
    }

    // 生成事件初始选项：攀爬（掉15血）/ 离开。
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(
            this,
            Climb,
            DiffLoc($"{Id.Entry}.pages.INITIAL.options.CLIMB.title"),
            DiffLoc($"{Id.Entry}.pages.INITIAL.options.CLIMB.description"),
            $"{Id.Entry}.pages.INITIAL.options.CLIMB",
            Array.Empty<IHoverTip>()).ThatDoesDamage(15m),
        new EventOption(
            this,
            Leave,
            DiffLoc($"{Id.Entry}.pages.INITIAL.options.RUN.title"),
            DiffLoc($"{Id.Entry}.pages.INITIAL.options.RUN.description"),
            $"{Id.Entry}.pages.INITIAL.options.RUN",
            Array.Empty<IHoverTip>()),
    ];

    // 攀爬：固定掉15血，进入 CLIMB 阶段。
    private async Task Climb()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars.Damage, null, null);
        Tools.ChangeEventImg(StageImage(1));
        SetEventState(DiffLoc($"{Id.Entry}.pages.CLIMB.description"), [
            new EventOption(
                this,
                Continue,
                DiffLoc($"{Id.Entry}.pages.CLIMB.options.CONTIUE.title"),
                DiffLoc(""),
                $"{Id.Entry}.pages.CLIMB.options.CONTIUE",
                Array.Empty<IHoverTip>()),
        ]);
    }

    // 离开（INITIAL 页）：直接跳转到结束。
    private Task Leave()
    {
        SetEventFinished(DiffLoc($"{Id.Entry}.pages.RUN.description"));
        return Task.CompletedTask;
    }

    // 造物主交流阶段：继续。
    private Task Continue()
    {
        Tools.ChangeEventImg(StageImage(2));
        SetEventState(DiffLoc($"{Id.Entry}.pages.CLIMB.options.CONTIUE.description"), [
            new EventOption(
                this,
                Accept,
                DiffLoc($"{Id.Entry}.pages.CONTIUE.options.ACCEPT.title"),
                DiffLoc($"{Id.Entry}.pages.CONTIUE.options.ACCEPT.description"),
                $"{Id.Entry}.pages.CONTIUE.options.ACCEPT",
                Array.Empty<IHoverTip>()),
        ]);
        return Task.CompletedTask;
    }

    // 接受：业力上限+1，进入离开阶段。
    private async Task Accept()
    {
        await SlugcatData.addMaxWorkLevel(Owner!, 1);
        Tools.ChangeEventImg(StageImage(3));
        SetEventState(DiffLoc($"{Id.Entry}.pages.ACCEPT.description"), [
            new EventOption(
                this,
                Finish,
                DiffLoc($"{Id.Entry}.pages.ACCEPT.options.RUN.title"),
                DiffLoc($"{Id.Entry}.pages.ACCEPT.options.RUN.description"),
                $"{Id.Entry}.pages.ACCEPT.options.RUN",
                Array.Empty<IHoverTip>()),
        ]);
    }

    // 离开（ACCEPT 页）：不显示 RUN 文本，保留当前页（造物主交流）的文本，直接离开事件。
    private Task Finish()
    {
        SetEventFinished(Description ?? DiffLoc($"{Id.Entry}.pages.ACCEPT.description"));
        Tools.LeaveEvent();
        return Task.CompletedTask;
    }
}
