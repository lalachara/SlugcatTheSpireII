---
kind: configuration_system
name: 基于 RitsuLib 的模组运行时配置系统
category: configuration_system
scope:
    - '**'
source_files:
    - Scripts/RainworldModConfig.cs
    - Scripts/Entry.cs
    - mod_manifest.json
    - project.godot
    - export_presets.cfg
---

## 1. 使用的系统与框架

本仓库是一个 STS2（杀戮尖塔2）Godot.NET 模组工程，其**运行时配置**完全依赖第三方库 **STS2RitsuLib** 提供的 `ModSettingsValueBinding`、`ModDataStore` 与 `RitsuLibFramework.RegisterModSettings` 机制。项目自身没有自建配置文件解析器或 INI/JSON 读写逻辑，所有可持久化的用户设置均通过该库完成。

此外，还存在两类与“构建/发布”相关的静态配置：
- Godot 引擎配置：`project.godot`（引擎版本、渲染后端、C# 特性开关等）
- 导出配置：`export_presets.cfg`（Windows Desktop 目标、资源过滤、远程调试脚本等）
- 模组清单：`mod_manifest.json`（模组 ID、版本、依赖 `STS2-RitsuLib ≥ 0.4.41`、是否包含 PCK/DLL 等）

这些文件由 Godot 编辑器生成/维护，不属于运行时用户可调配置范畴，但共同构成模组的装配配置层。

## 2. 关键文件

| 文件 | 作用 |
|---|---|
| `Scripts/RainworldModConfig.cs` | 定义 `RainworldModSettings` 数据类与 `RainworldModConfig` 静态注册入口，集中声明全部布尔型功能开关 |
| `Scripts/Entry.cs` | 模组初始化入口 `[ModInitializer]`，在 `Init()` 中调用 `RainworldModConfig.Register()` 完成配置注册 |
| `mod_manifest.json` | STS2 模组清单，声明模组元信息与对 RitsuLib 的依赖 |
| `project.godot` | Godot 项目引擎配置（engine 4.5 + C# + Mobile 特性） |
| `export_presets.cfg` | Godot 导出预设（Windows Desktop，含远程调试 PowerShell 脚本） |

## 3. 架构与设计约定

### 3.1 运行时配置模型
- 所有用户可切换的功能开关集中在一个密封类 `RainworldModSettings` 中，当前全部为 `bool` 类型，默认值在属性声明处给出（如 `EnableCombatCantLossMaxHp = false`、`EnableModAncientFix = true` 等）。
- 每个字段对应一个 `ModSettingsValueBinding<RainworldModSettings, bool>` 静态只读字段，绑定三要素：`Entry.ModId`（模组标识）、`"mod_settings"`（数据键）、`SaveScope.Global`（全局持久化范围），以及 getter/setter lambda。
- `RainworldModConfig` 同时暴露同名 `static` 属性（如 `EnableMushroomMonster`），内部委托给对应的 Binding 的 `Read()` / `Write()`，并在写时调用 `Save()` 实现即时持久化。
- 通过 `ModDataStore.For(Entry.ModId).Register<RainworldModSettings>(key: "mod_settings", fileName: "mod_settings.json", scope: SaveScope.Global, defaultFactory: () => new RainworldModSettings(), autoCreateIfMissing: true)` 将配置序列化为 JSON 文件存储。
- UI 侧通过 `RitsuLibFramework.RegisterModSettings(...)` 以 builder 风格注册页面，按 section 分组：`general`（HP 修复相关）、`monster`（怪物启用开关）、`event`（事件启用开关），每个 toggle 绑定到对应 Binding，并附带本地化标题和描述。

### 3.2 生命周期集成
- 配置注册发生在模组入口 `Entry.Init()` 中，位于补丁注册之后、内容自动发现之前，确保其他模块可在初始化阶段读取已加载的设置。
- 日志通过 `RitsuLibFramework.CreateLogger(ModId)` 创建，与 ModId 关联，便于区分不同模组的输出。

### 3.3 构建/发布配置
- `mod_manifest.json` 声明 `has_pck: true`、`has_dll: true`，表明模组同时打包 Godot 资源与 .NET 程序集；依赖项强制要求 `STS2-RitsuLib` 最低版本 `0.4.41`。
- `project.godot` 固定使用 Godot 4.5 引擎、C# 语言、Mobile 渲染后端；`dotnet/project/assembly_name` 设为 `demo`（可能与实际项目名不一致，需留意）。`rendering_method` 为 `mobile`。
- `export_presets.cfg` 仅定义了 Windows Desktop 预设，导出路径硬编码到本地 Steam 目录，且包含一段 PowerShell 远程调试脚本用于启动 Godot 远程调试任务。

## 4. 约定与约束

- **单一数据类约定**：所有运行时开关必须添加到 `RainworldModSettings` 中，新增字段后需同步添加对应的 `ModSettingsValueBinding`、静态属性访问器，并在 `Register()` 的 builder 链中添加 UI toggle，否则无法在设置界面编辑。
- **保存范围统一为 Global**：所有 binding 均使用 `SaveScope.Global`，意味着设置跨存档共享，不随角色/关卡隔离。
- **文件名固定**：配置持久化文件名为 `mod_settings.json`，由 `ModDataStore.Register` 的 `fileName` 参数决定。
- **布尔开关模式**：当前所有配置均为 `bool` 开关，未出现数值、字符串枚举等其他类型；若引入新类型需自行扩展 Binding 用法。
- **本地化依赖**：设置项标题与描述通过 `ModSettingsText.LocString(new LocString("settings_ui", key))` 引用，键名遵循 `RAINWORLD-config.*` 命名空间（如 `enableMushroomMonster.desc`），需在本地化资源中提供对应翻译。
- **Godot 配置不可直接修改**：`project.godot` 与 `export_presets.cfg` 明确注释建议通过编辑器 UI 修改而非手写，避免破坏引擎内部结构。
- **无环境变量/命令行配置**：代码中未发现对 `Environment.GetEnvironmentVariable`、`args` 或自定义 CLI 参数的读取，运行时行为完全由模组内 JSON 配置驱动。
- **无热重载配置**：设置写入后立即 `Save()`，但未观察到监听文件变更的热重载机制，修改后需重启游戏生效。
