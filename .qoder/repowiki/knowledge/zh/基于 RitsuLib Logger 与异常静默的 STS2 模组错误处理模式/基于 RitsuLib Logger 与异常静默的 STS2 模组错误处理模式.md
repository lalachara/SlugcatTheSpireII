---
kind: error_handling
name: 基于 RitsuLib Logger 与异常静默的 STS2 模组错误处理模式
category: error_handling
scope:
    - '**'
source_files:
    - Scripts/Entry.cs
    - Scripts/RainworldModConfig.cs
    - Scripts/Slugcat/SlugcatData.cs
    - Scripts/patches/CombatUiPatch.cs
    - Scripts/patches/Filter/RelicGrabBagFilterPatch.cs
    - Scripts/patches/Filter/RelicPoolFilterPatch.cs
    - Scripts/Card/Liver/Skills/Rainworld_Liver_Sirthisway.cs
    - Scripts/Card/River/Skill/Rainworld_River_Sirthisway.cs
---

## 1. 使用的系统与框架

本仓库是一个基于 Godot.NET + C# 的杀戮尖塔2（STS2）模组工程，错误处理完全依赖 STS2RitsuLib 提供的运行时基础设施：
- 日志系统：通过 `Entry.cs` 中 `RitsuLibFramework.CreateLogger(ModId)` 创建的 `Logger` 单例，统一以 `Entry.Logger.Info/Warn/Error/Debug/Fatal` 输出。
- 补丁框架：所有对 STS2 核心代码的侵入式修改均通过 `IPatchMethod` 形式的 Patch 完成，由 `RitsuLibFramework.CreatePatcher(...).PatchAll()` 驱动；Patch 方法本身不抛出业务异常，而是用 try/catch 包裹 UI 初始化等易错逻辑。
- 配置开关：`RainworldModConfig` 暴露一组布尔开关（如 `EnableMaxHpRelicFix`、`EnableHpRelicFix` 等），用于在运行期切换某些“修复”行为，本质上是把“可能出错的路径”降级为可关闭的功能。

## 2. 关键文件与位置

| 文件 | 作用 |
|---|---|
| `Scripts/Entry.cs` | 模组入口，创建全局 `Logger`，调用 `patcher.PatchAll()` 并在失败时抛 `InvalidOperationException` |
| `Scripts/RainworldModConfig.cs` | 定义 `RainworldModSettings` 并通过 `ModSettingsValueBinding` 注册到 RitsuLib 设置系统 |
| `Scripts/Slugcat/SlugcatData.cs` | 加载次级资源数据时 catch 空块吞掉异常（第 101–103 行） |
| `Scripts/patches/CombatUiPatch.cs` | 战斗 UI 注入逻辑被整个 try/catch(System.Exception) 包裹，异常被静默忽略 |
| `Scripts/patches/Filter/RelicGrabBagFilterPatch.cs` | 读取局内保存数据失败时记录 Warn 并继续 |
| `Scripts/patches/Filter/RelicPoolFilterFilterPatch.cs` | 同上，且重试 20 次后回退到 `RelicFactory.FallbackRelic` |
| `Scripts/Card/*/Skills/Rainworld_*_Sirthisway.cs` | 在 VFX 未加入场景树时主动抛出 `InvalidOperationException` |

## 3. 架构与约定

### 3.1 启动阶段：致命错误直接抛异常
`Entry.Init()` 中 `if (!patcher.PatchAll()) throw new InvalidOperationException("Critical patches failed.");` 是唯一的“启动即崩溃”路径——当核心 Patch 无法应用时，模组直接终止。这符合 STS2 模组生命周期：启动失败应让游戏拒绝加载该模组。

### 3.2 运行时：优先 try/catch + 日志，而非自定义异常类型
仓库**没有**定义任何自定义 Exception 类或统一的错误码枚举。运行时错误主要采用以下两种模式：
- **静默吞异常**：`CombatUiPatch.cs` 中整个 UI 初始化块被 `catch (System.Exception ex) { }` 包裹；`SlugcatData.cs` 中读取持久化数据的 try/catch 体也为空。这类做法保证 UI 渲染失败不会中断战斗流程。
- **降级+日志**：`RelicGrabBagFilterPatch.cs` 和 `RelicPoolFilterPatch.cs` 在读取局内保存数据失败时记录 `Entry.Logger.Warn(...)`，然后回退到使用全局配置开关（`RainworldModConfig.Enable*`）决定是否过滤遗物；若重试 20 次仍失败，则使用 `FallbackRelic` 作为兜底。

### 3.3 参数校验：使用 `ArgumentNullException.ThrowIfNull`
大量卡牌 `OnPlay` 入口使用 `ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target")` 进行前置参数校验，属于 .NET 标准库的防御性编程方式，而非自定义 sentinel error。

### 3.4 明确失败点：少数位置主动抛出 `InvalidOperationException`
四个 `*Sirthisway.cs` 技能文件在 VFX 节点未成功加入战斗场景树时抛出 `InvalidOperationException("VFX is not inside tree after adding it to combat room!")`，表示这是不可恢复的内部状态不一致，不应被吞掉。

### 3.5 无全局中间件/错误处理器
仓库不存在类似 ASP.NET Core 的 middleware 或全局异常捕获器。每个 Patch 方法独立负责自身的健壮性，错误传播边界就是单个 Patch 方法的 try/catch 范围。

## 4. 约定与约束

- **Patch 方法默认非致命**：大多数 `IPatchMethod.Postfix/Prefix` 内部都自带 try/catch 或条件守卫，确保单个 Patch 失败不影响其他 Patch 执行。
- **UI 相关逻辑必须容错**：`CombatUiPatch` 将整段 UI 注入代码包在 try/catch 中，体现“UI 渲染失败不应影响战斗逻辑”的约定。
- **配置项即错误规避手段**：`RainworldModConfig` 中的 `Enable*Fix` 开关允许玩家/调试者关闭可能导致异常的兼容逻辑，是一种“可配置的降级策略”。
- **日志是唯一对外可见的错误通道**：除启动阶段的 `throw` 外，运行时异常仅通过 `Entry.Logger.Warn/Info` 输出，没有弹窗、没有错误码返回给上层。
- **未观察到 panic/recover**：C# 无 panic/recover 概念，仓库也未使用 `try/finally` 做资源清理之外的恢复逻辑。

综上，该仓库的错误处理风格是：**启动阶段严格（失败即抛）、运行时宽容（try/catch + Logger.Warn + 降级回退）、无自定义错误类型体系、以配置开关控制兼容性路径**。