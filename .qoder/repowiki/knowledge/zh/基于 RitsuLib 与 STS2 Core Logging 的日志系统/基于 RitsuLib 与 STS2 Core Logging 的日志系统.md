---
kind: logging_system
name: 基于 RitsuLib 与 STS2 Core Logging 的日志系统
category: logging_system
scope:
    - '**'
source_files:
    - Scripts/Entry.cs
    - Scripts/Slugcat/SlugcatData.cs
    - Scripts/patches/LoseHpInternalPatch.cs
    - Scripts/patches/Filter/RelicGrabBagFilterPatch.cs
    - Scripts/patches/Filter/RelicPoolFilterPatch.cs
---

## 1. 使用的系统与框架

本模组工程没有自建日志库，而是复用 STS2（杀戮尖塔2）运行时提供的日志能力：
- 核心日志类型来自 `MegaCrit.Sts2.Core.Logging.Logger`，在入口类中通过别名引入：`using Logger = MegaCrit.Sts2.Core.Logging.Logger;`
- 同时直接引用 `MegaCrit.Sts2.Core.Logging` 命名空间以使用 `Log.Debug/Info/Warn/Error` 等静态方法。
- 日志实例由 RitsuLib 创建：`Entry.Logger = RitsuLibFramework.CreateLogger(ModId)`，其中 `ModId = "SlugcatTheSpireII"`，因此所有日志均带有模组标识前缀，便于在多模组环境下区分来源。
- Godot 侧还混用 `Godot.GD.Print(...)` 输出调试信息（例如 `relics/Worklevelrepair.cs` 中的初始化参数打印），属于开发期辅助输出。

## 2. 关键文件

| 文件 | 作用 |
|---|---|
| `Scripts/Entry.cs` | 定义 `Entry.Logger` 单例，作为全局日志入口；注册补丁、配置、Godot 脚本等 |
| `Scripts/Slugcat/SlugcatData.cs` | 使用 `Log.Debug` 记录腐化值变化等运行时状态 |
| `Scripts/patches/LoseHpInternalPatch.cs` | 使用 `Log.Info` 记录强制击杀流程 |
| `Scripts/patches/Filter/RelicGrabBagFilterPatch.cs` | 使用 `Entry.Logger.Info/Warn` 记录遗物过滤与重试过程 |
| `Scripts/patches/Filter/RelicPoolFilterPatch.cs` | 同上，记录遗物池过滤逻辑 |
| `log.txt` | 版本更新日志（changelog），非运行期日志输出 |

## 3. 架构与约定

- **集中式 Logger 实例**：`Entry.cs` 在模组初始化阶段通过 `RitsuLibFramework.CreateLogger(ModId)` 创建唯一 `Logger` 实例并暴露为 `Entry.Logger`。调用方统一通过该静态属性访问，避免各模块自行构造 logger。
- **双通道输出**：
  - 业务/补丁逻辑使用 `MegaCrit.Sts2.Core.Logging` 提供的 `Log.*` 或 `Entry.Logger.*`，这些调用会进入 STS2 运行时日志管线。
  - Godot 资源/场景相关调试使用 `GD.Print`，走 Godot 控制台输出。
- **结构化字段**：未实现自定义结构化日志字段；日志内容以字符串插值为主，并通过中文方括号标签 `[遗物过滤]`、`[遗物过滤GrabBag]` 等方式做上下文标记，便于人工阅读。
- **日志级别使用**：
  - `Debug`：用于内部状态变更追踪（如腐化值增减）。
  - `Info`：用于可预期的业务流程节点（如重新抽取遗物成功、强制击杀）。
  - `Warn`：用于异常恢复路径（如读取局内保存数据失败、重试耗尽后仍无法获取目标遗物）。
- **无 sink 配置**：项目内未见对日志输出目标（文件、网络、分级开关）的配置代码；输出策略由 STS2 运行时与 RitsuLib 决定。

## 4. 约定与约束

- **必须通过 `Entry.Logger` 或 `Log.*` 输出**：当前所有日志点均通过这两个入口，未见第三方日志库或自定义 `ILogger` 实现。
- **日志消息使用中文 + 方括号模块标签**：例如 `[遗物过滤]`、`[遗物过滤GrabBag]`，这是当前代码中统一的上下文标注约定，便于在大量输出中快速定位来源。
- **Godot 调试输出仅限 `GD.Print`**：仅在少数初始化/调试位置使用，不替代业务日志。
- **无运行时日志级别开关**：未发现根据配置动态开启/关闭日志的代码，日志行为由底层运行时控制。
- **版本日志独立存放**：`log.txt` 是发布 changelog，不是运行期日志文件，不应与程序日志混淆。

综上，该仓库的日志系统是一个“薄封装”方案：以 RitsuLib 创建的 `Logger` 和 STS2 内置 `Log` API 为核心，配合少量 `GD.Print` 调试输出，采用统一的 `Entry.Logger` 入口和中文方括号标签约定组织日志上下文。