using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot;
using MegaCrit.Sts2.Core.Context; // LocalContext.GetMe（拿本机玩家）
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs; // RunManager.Instance.DebugOnlyGetState()（拿运行时 Player 以读业力）
using MegaCrit.Sts2.Core.Saves; // SerializableRun（RunEndedEvent.Run）
using MegaCrit.Sts2.Core.Saves.Runs; // SerializablePlayer / SerializableCard
using Rainworld.Scripts.Telemetry;
using STS2RitsuLib;

namespace Rainworld.Scripts;

/// <summary>
/// 遥测观察者：订阅 RitsuLib 生命周期事件，将玩法行为上报到 PostHog。
/// 对应 TelemetrySetup 中注册的 <c>gameplay_events</c> 申请项，需玩家授权才会上报。
/// </summary>
public sealed class TelemetryObserver : ILifecycleObserver
{
    public void OnEvent(IFrameworkLifecycleEvent evt)
    {
        switch (evt)
        {
            case RunEndedEvent runEnded:
                Entry.Logger.Info("[遥测][observer] 收到事件: " + nameof(RunEndedEvent));
                OnRunEnded(runEnded);
                break;
            case RunStartedEvent runStarted:
                Entry.Logger.Info("[遥测][observer] 收到事件: " + nameof(RunStartedEvent));
                OnRunStarted(runStarted);
                break;
        }
    }



    private static void OnRunEnded(RunEndedEvent evt)
    {
        try
        {
            Entry.Logger.Info($"[遥测] 触发 run_ended_meta: victory={evt.IsVictory}，abandoned={evt.IsAbandoned}");

            // 结束事件本身没有 IsMultiplayer，用玩家数>1 近似判断（多人合作模式至少 2 名玩家）。
            var isMultiplayer = (evt.Run?.Players?.Count ?? 0) > 1;
            // 进阶等级（0 表示普通难度），用于按进阶统计胜率。
            var ascension = evt.Run?.Ascension ?? 0;
            // 对局花费时长（精确到分钟，不足 1 分钟记为 0）。
            var runTimeMinutes = (evt.Run?.RunTime ?? 0) / 60;

            // 完局事件由 RitsuLib 的 run_history 自动收集原版数据，这里额外补充角色与胜负的轻量标记。
            TelemetrySetup.CaptureGameplay(
                "run_ended_meta",
                new Dictionary<string, object?>
                {
                    ["language"] = LocManager.Instance?.Language ?? "unknown",
                    // 只上报自己（本机玩家）的角色，多人合作模式下不再拼接整个队伍。
                    ["character"] = LocalPlayerCharacter(evt.Run),
                    ["is_multiplayer"] = isMultiplayer,
                    ["ascension"] = ascension,
                    ["run_time_minutes"] = runTimeMinutes,
                    // 蛞蝓猫业力等级（0-9 转 1-10），用于统计胜局时的业力；非蛞蝓猫/无蛞蝓猫角色输出 0。
                    ["karma_level"] = KarmaLevel(),
                    // 失败时处于第几幕（0-based 索引 +1）；胜利/弃权/取不到时输出 0。
                    ["failed_act"] = FailedActNumber(evt.IsVictory),
#if STS2_0_113_0
                    // 是否恐惧模式（Dread）对局：0.113 才有，0.111 不统计
                    ["is_dread"] = IsDreadRun(evt.Run),
#endif
                    ["victory"] = evt.IsVictory,
                    ["abandoned"] = evt.IsAbandoned,
                });

            // 卡牌胜率：为每一张本局牌组中的 mod 卡，单独发一条“该卡在这局的胜负”事件。
            // 这样在 PostHog 里按 card 分组、对 victory 求占比，即可得到“选了这张卡时的胜局/总局数”。
            foreach (var card in CollectRunModCards(evt.Run))
            {
                var model = card.Id != null ? ModelDb.GetByIdOrNull<CardModel>(card.Id) : null;
                TelemetrySetup.CaptureGameplay(
                    "card_run_victory",
                    new Dictionary<string, object?>
                    {
                        ["card"] = card.CardId,
                        ["card_name"] = CardName(card.CardId),
                        ["upgraded"] = card.MaxUpgrade > 0,
                        ["rarity"] = model?.Rarity.ToString() ?? "unknown",
                        ["cost"] = model?.EnergyCost.Canonical ?? -1,
                        ["has_duplicate"] = card.Count > 1,
                        ["character"] = card.CharacterId,
                        ["language"] = LocManager.Instance?.Language ?? "unknown",
                        ["is_multiplayer"] = isMultiplayer,
                        ["ascension"] = ascension,
                        ["run_time_minutes"] = runTimeMinutes,
                        ["victory"] = evt.IsVictory,
                        ["abandoned"] = evt.IsAbandoned,
                    });
            }

            // 遗物统计：与卡牌同理，为本局每件 mod 遗物发一条"该遗物随这局胜利/失败"的事件。
            foreach (var relic in CollectRunModRelics(evt.Run))
            {
                var relicModel = relic.Id != null ? ModelDb.GetByIdOrNull<RelicModel>(relic.Id) : null;
                TelemetrySetup.CaptureGameplay(
                    "relic_run_victory",
                    new Dictionary<string, object?>
                    {
                        ["relic"] = relic.RelicId,
                        ["relic_name"] = RelicName(relic.RelicId),
                        ["rarity"] = relicModel?.Rarity.ToString() ?? "unknown",
                        ["character"] = relic.CharacterId,
                        ["language"] = LocManager.Instance?.Language ?? "unknown",
                        ["is_multiplayer"] = isMultiplayer,
                        ["ascension"] = ascension,
                        ["run_time_minutes"] = runTimeMinutes,
                        ["victory"] = evt.IsVictory,
                        ["abandoned"] = evt.IsAbandoned,
                    });
            }
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"[遥测] run_ended_meta 上报失败: {ex.Message}");
        }
    }

    private static void OnRunStarted(RunStartedEvent evt)
    {
        try
        {
            var language = LocManager.Instance?.Language;
            // 只上报自己（本机玩家）的角色，多人合作模式下不再拼接整个队伍。
            var characters = LocalPlayerCharacter(evt.RunState);
            // 生效的 spawn 配置（多人用宿主机下发值，否则用本机快照）。
            var spawn = SpawnConfigRuntime.GetEffectiveConfig();
            Entry.Logger.Info($"[遥测] 触发 run_started: language={language}，characters={characters}，isMultiplayer={evt.IsMultiplayer}，isDaily={evt.IsDaily}");
            TelemetrySetup.CaptureGameplay(
                "run_started",
                new Dictionary<string, object?>
                {
                    ["language"] = language ?? "unknown",
                    ["character"] = characters,
                    ["is_multiplayer"] = evt.IsMultiplayer,
                    ["is_daily"] = evt.IsDaily,
                    // 怪物/事件三态配置：Always=始终开启、OnlySlugcat=仅蛞蝓猫、Disabled=禁用。
                    ["monster_master"] = spawn.MonsterMasterSpawnMode.ToString(),
                    ["monster_mushroom"] = spawn.MushroomMonsterSpawnMode.ToString(),
                    ["monster_ironbird"] = spawn.IronbirdMonsterSpawnMode.ToString(),
                    ["monster_firecab"] = spawn.FirecabMonsterSpawnMode.ToString(),
                    ["monster_firecent"] = spawn.FirecentMonsterSpawnMode.ToString(),
                    ["monster_watercent"] = spawn.WatercentMonsterSpawnMode.ToString(),
                    ["monster_greencab"] = spawn.GreencabMonsterSpawnMode.ToString(),
                    ["event_master"] = spawn.EventMasterSpawnMode.ToString(),
                    ["event_raindeer"] = spawn.RaindeerEventSpawnMode.ToString(),
                    ["event_watcher"] = spawn.WatcherEventSpawnMode.ToString(),
                });
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"[遥测] run_started 上报失败: {ex.Message}");
        }
    }

    /// <summary>mod 卡 ID 的 Entry 前缀（snake_case 大写形式，非驼峰的 Entry.ModId）。</summary>
    private const string ModCardIdPrefix = "SLUGCAT_THE_SPIRE_II";

    /// <summary>mod 英文本地化表缓存（文件名 → 字典型字典），终身只加载一次。</summary>
    private static readonly Dictionary<string, Dictionary<string, string>?> _engTables = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>mod 遗物 ID 的 Entry 前缀（snake_case 大写形式）。</summary>
    private const string ModRelicIdPrefix = "SLUGCAT_THE_SPIRE_II_RELIC";

    /// <summary>收集结果：每张 mod 卡在本局的 Id、Entry、最高升级等级、在本局牌组出现的总次数（判断同名牌），以及所属角色 ID。</summary>
    private readonly record struct CardRunInfo(ModelId? Id, string CardId, int MaxUpgrade, int Count, string CharacterId);

    /// <summary>
    /// 从结束跑局的可序列化快照中，收集本机玩家（自己）牌组里出现过的本 mod 卡（去重），并记录每种卡在本局的最高升级等级与出现次数。
    /// 只统计本机玩家的牌组：多人合作下每个客户端各自上报自己的数据，避免各客户端遍历全队导致同一份数据被重复上报。
    /// 仅统计本 mod 的卡，用 mod 卡 Entry 前缀（如 SLUGCAT_THE_SPIRE_II_CARD_...）判断。
    /// </summary>
    private static IEnumerable<CardRunInfo> CollectRunModCards(SerializableRun run)
    {
        if (run == null)
            yield break;

        // 只取本机玩家，多人时不再遍历队友牌组。
        var me = LocalPlayerOf(run);
        var deck = me?.Deck;
        if (deck == null)
            yield break;

        var characterId = me?.CharacterId?.Entry?.ToString() ?? "unknown";
        var stats = new Dictionary<string, (ModelId? Id, int MaxUpgrade, int Count, string CharacterId)>(StringComparer.OrdinalIgnoreCase);

        var matchedInPlayer = 0;
        foreach (var serializableCard in deck)
        {
            var entry = serializableCard?.Id?.Entry?.ToString();
            if (string.IsNullOrEmpty(entry))
                continue;
            if (!entry.StartsWith(ModCardIdPrefix, StringComparison.OrdinalIgnoreCase))
                continue;
            matchedInPlayer++;
            var upgrade = serializableCard?.CurrentUpgradeLevel ?? 0;
            if (stats.TryGetValue(entry, out var existing))
                stats[entry] = (existing.Id, Math.Max(existing.MaxUpgrade, upgrade), existing.Count + 1, existing.CharacterId);
            else
                stats[entry] = (serializableCard?.Id, upgrade, 1, characterId);
        }

        Entry.Logger.Info($"[遥测] 采集本机牌组 可选卡数={deck.Count}，匹配到的 mod 卡={matchedInPlayer}");

        foreach (var pair in stats)
            yield return new CardRunInfo(pair.Value.Id, pair.Key, pair.Value.MaxUpgrade, pair.Value.Count, pair.Value.CharacterId);
    }

    /// <summary>收集结果：每件 mod 遗物在本局的 Id、Entry 与所属角色 ID。</summary>
    private readonly record struct RelicRunInfo(ModelId? Id, string RelicId, string CharacterId);

    /// <summary>
    /// 从结束跑局的可序列化快照中，收集本机玩家（自己）拥有的本 mod 遗物（去重），并记录所属角色。
    /// 只统计本机玩家的遗物：多人合作下每个客户端各自上报自己的数据，避免各客户端遍历全队导致同一份数据被重复上报。
    /// 仅统计本 mod 的遗物，用 mod 遗物 Entry 前缀（如 SLUGCAT_THE_SPIRE_II_RELIC_...）判断。
    /// </summary>
    private static IEnumerable<RelicRunInfo> CollectRunModRelics(SerializableRun run)
    {
        if (run == null)
            yield break;

        // 只取本机玩家，多人时不再遍历队友遗物。
        var me = LocalPlayerOf(run);
        var relics = me?.Relics;
        if (relics == null)
            yield break;

        var characterId = me?.CharacterId?.Entry?.ToString() ?? "unknown";
        var seen = new Dictionary<string, (ModelId? Id, string CharacterId)>(StringComparer.OrdinalIgnoreCase);

        var matchedInPlayer = 0;
        foreach (var serializableRelic in relics)
        {
            var entry = serializableRelic?.Id?.Entry?.ToString();
            if (string.IsNullOrEmpty(entry))
                continue;
            if (!entry.StartsWith(ModRelicIdPrefix, StringComparison.OrdinalIgnoreCase))
                continue;
            matchedInPlayer++;
            if (!seen.ContainsKey(entry))
                seen[entry] = (serializableRelic?.Id, characterId);
        }

        Entry.Logger.Info($"[遥测] 采集本机遗物 数量={relics.Count}，匹配到的 mod 遗物={matchedInPlayer}");

        foreach (var pair in seen)
            yield return new RelicRunInfo(pair.Value.Id, pair.Key, pair.Value.CharacterId);
    }

    /// <summary>
    /// 恒定返回卡牌的英文名（不跟随当前游戏语言）。
    /// 这样同一张卡在中英文环境下上报的都是同一个英文名，在 PostHog 里按 <c>card_name</c> 分组即可把
    /// 不同语言产生的历史记录合并。英文名取自 mod 自带的 <c>eng/cards.json</c>；取不到时回退用原始 ID（保证分组稳定）。
    /// </summary>
    private static string CardName(string entry)
    {
        try
        {
            if (string.IsNullOrEmpty(entry))
                return entry;

            var cards = LoadEngTable("cards.json");
            if (cards != null && cards.TryGetValue(entry + ".title", out var name) && !string.IsNullOrWhiteSpace(name))
                return name;
            return entry;
        }
        catch
        {
            return entry;
        }
    }

    /// <summary>
    /// 恒定返回遗物的英文名（不跟随当前游戏语言）。
    /// 与 <see cref="CardName"/> 同理，英文名取自 mod 自带的 <c>eng/relics.json</c>；取不到时回退用原始 ID。
    /// </summary>
    private static string RelicName(string entry)
    {
        try
        {
            if (string.IsNullOrEmpty(entry))
                return entry;

            var relics = LoadEngTable("relics.json");
            if (relics != null && relics.TryGetValue(entry + ".title", out var name) && !string.IsNullOrWhiteSpace(name))
                return name;
            return entry;
        }
        catch
        {
            return entry;
        }
    }

    /// <summary>
    /// 加载所有已加载 mod 提供的英文本地化表 <c>eng/{fileName}</c>（如 cards.json/relics.json），合并为静态缓存。
    /// 用 <c>ModManager.GetModdedLocTables</c> 获取路径，与游戏加载 mod 本地化的方式保持一致。
    /// </summary>
    private static Dictionary<string, string>? LoadEngTable(string fileName)
    {
        if (_engTables.TryGetValue(fileName, out var cached))
            return cached;

        try
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var path in ModManager.GetModdedLocTables("eng", fileName))
            {
                using var file = global::Godot.FileAccess.Open(path, global::Godot.FileAccess.ModeFlags.Read);
                if (file == null)
                    continue;
                var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(file.GetAsText(false));
                if (parsed == null)
                    continue;
                foreach (var kv in parsed)
                    dict[kv.Key] = kv.Value;
            }
            _engTables[fileName] = dict;
            Entry.Logger.Info($"[遥测] 已加载英文表 eng/{fileName}，共 {dict.Count} 条");
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"[遥测] 英文表 {fileName} 加载失败: " + ex.Message);
            _engTables[fileName] = null;
        }

        return _engTables.TryGetValue(fileName, out var result) ? result : null;
    }


    /// <summary>
    /// 本机玩家（自己）的业力等级（代码内 0-9，实际 1-10，故 +1）。
    /// 只统计自己、不统计队友（多人合作模式下队友的业力不纳入）。
    /// 本机玩家非蛞蝓猫角色（Liver/Redcat/River 等）或取不到时返回 0。
    /// 说明：业力是运行时数据（key 为运行时 Player），只能从活着的 RunManager 拿；
    /// RunEndedEvent 只带 SerializableRun，故这里用 DebugOnlyGetState() 读运行时玩家。
    /// </summary>
#if STS2_0_113_0
    /// <summary>
    /// 本局是否为恐惧模式（Dread）。
    /// 优先读运行时 <see cref="RunState" /> 的 GameMode / RunModels（结束时状态通常还在），
    /// 取不到时退回结束事件里的 <c>SerializableRun.GameMode</c>。
    /// </summary>
    private static bool IsDreadRun(SerializableRun? run)
    {
        try
        {
            var state = RunManager.Instance?.DebugOnlyGetState();
            if (state != null)
            {
                if (state.GameMode == GameMode.Dread)
                    return true;

                if (state.RunModels != null &&
                    state.RunModels.Any(m => m is MegaCrit.Sts2.Core.Models.Dread.DreadRunModel))
                    return true;
            }

            return run?.GameMode == GameMode.Dread;
        }
        catch
        {
            return false;
        }
    }
#endif

    private static int KarmaLevel()
    {
        try
        {
            var state = RunManager.Instance?.DebugOnlyGetState();
            if (state == null)
                return 0;

            var me = LocalContext.GetMe(state);
            if (me?.Character is Slugcat)
                return SlugcatData.GetWorkLevel(me) + 1;
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// 失败时处于第几幕（<see cref="MegaCrit.Sts2.Core.Runs.RunState.CurrentActIndex"/> 为 0-based，故 +1）。
    /// 胜利或弃权时返回 0（非“失败”场景不统计幕数）。取不到运行时状态时也返回 0。
    /// </summary>
    private static int FailedActNumber(bool isVictory)
    {
        if (isVictory)
            return 0;

        try
        {
            return (RunManager.Instance?.DebugOnlyGetState()?.CurrentActIndex ?? 0) + 1;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// 本机玩家（自己）的角色 ID。多人合作模式下只返回自己的角色，不再拼接整个队伍。
    /// 单机（自己即唯一玩家）也返回自身角色。取不到时回退 unknown。
    /// </summary>
    private static string LocalPlayerCharacter(RunState? runState)
    {
        try
        {
            if (runState == null)
                return "unknown";
            var me = LocalContext.GetMe(runState);
            return me?.Character?.Id?.Entry?.ToString() ?? "unknown";
        }
        catch
        {
            return "unknown";
        }
    }

    /// <summary>
    /// 本机玩家（自己）的角色 ID（基于结束跑局快照）。多人合作模式下只返回自己的角色。
    /// </summary>
    private static string LocalPlayerCharacter(SerializableRun? run)
    {
        try
        {
            if (run == null)
                return "unknown";
            var me = LocalContext.GetMe(run);
            return me?.CharacterId?.Entry?.ToString() ?? "unknown";
        }
        catch
        {
            return "unknown";
        }
    }

    /// <summary>
    /// 结束跑局快照中的本机玩家（自己）状态。多人合作模式下只取自己，避免各客户端遍历全队重复上报。
    /// <c>LocalContext.GetMe(run)</c> 在本机 NetId 为空时返回 null、快照内找不到本机座位时抛异常，故这里统一兜底为 null。
    /// </summary>
    private static SerializablePlayer? LocalPlayerOf(SerializableRun run)
    {
        try
        {
            return run == null ? null : LocalContext.GetMe(run);
        }
        catch
        {
            return null;
        }
    }

}
