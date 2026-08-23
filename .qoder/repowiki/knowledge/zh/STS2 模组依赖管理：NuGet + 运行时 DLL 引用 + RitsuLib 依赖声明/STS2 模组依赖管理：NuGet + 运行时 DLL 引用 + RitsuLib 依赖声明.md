---
kind: dependency_management
name: STS2 模组依赖管理：NuGet + 运行时 DLL 引用 + RitsuLib 依赖声明
category: dependency_management
scope:
    - '**'
source_files:
    - SlugcatTheSpireII.csproj
    - mod_manifest.json
    - project.godot
    - bin/spine_godot_extension.gdextension
    - bin/windows/libspine_godot.windows.editor.x86_64.dll
    - export_presets.cfg
---

## 1. 使用的系统与工具

本仓库是一个基于 Godot.NET 4.5 + C# 的《杀戮尖塔2》（Slay the Spire 2）模组工程，依赖管理由三层组成：

- **构建期 .NET 包**：通过 `SlugcatTheSpireII.csproj` 中的 `<PackageReference>` 使用 NuGet 获取 `Lib.Harmony`（版本固定为 `2.4.2`）和 `STS2.RitsuLib`（版本通配符 `*`）。项目 SDK 为 `Godot.NET.Sdk/4.5.1`，目标框架 `net9.0`，语言版本 `12.0`。
- **运行时游戏 DLL 引用**：通过 `<Reference Include="..." HintPath="$(Sts2DataDir)\xxx.dll">` 引用游戏目录下的 `sts2.dll`、`0Harmony.dll`、`Steamworks.NET.dll`，以及第三方模组库 `STS2-RitsuLib.dll`。这些 DLL 均设置 `<Private>false</Private>`，即不随模组打包，而是依赖游戏运行时装载。
- **模组元数据与运行时依赖**：`mod_manifest.json` 声明模组 ID、版本、最低游戏版本，并通过 `dependencies` 字段声明对 `STS2-RitsuLib`（最低版本 `0.4.41`）的运行时依赖，供 STS2 模组加载器校验。

此外，Godot 侧通过 `project.godot` 启用 C# 脚本支持（`config/features=PackedStringArray("4.5", "C#", "Mobile")`），并通过 `bin/spine_godot_extension.gdextension` 及 `bin/windows/libspine_godot.windows.*.dll` 引入 Spine Godot 扩展作为二进制插件。

## 2. 关键文件

- `SlugcatTheSpireII.csproj`：.NET 项目文件，集中声明 NuGet 包、游戏 DLL 引用、内容资源、PostBuild 自动复制逻辑。
- `mod_manifest.json`：STS2 模组清单，声明模组标识、版本、依赖关系。
- `project.godot`：Godot 引擎配置，指定引擎版本、C# 支持、渲染后端。
- `bin/spine_godot_extension.gdextension` 与 `bin/windows/libspine_godot.windows.editor.x86_64.dll`：Spine Godot 扩展的 GDExtension 描述与平台二进制。
- `export_presets.cfg`：Godot 导出配置（用于生成 PCK，配合 `has_pck: true`）。

## 3. 架构与约定

- **双轨依赖来源**：构建期依赖走 NuGet（`Lib.Harmony`、`STS2.RitsuLib`），运行时依赖直接引用游戏安装目录中的 DLL（`sts2.dll`、`0Harmony.dll`、`Steamworks.NET.dll`），避免将游戏核心库打入模组。
- **路径变量化**：`Sts2Dir`、`Sts2DataDir`、`RitsuLibDeployDir`、`FmodBuildDir` 等 MSBuild 属性集中在 `<PropertyGroup>` 中，便于切换本地游戏路径。
- **自动部署**：`Copy Mod` Target 在 PostBuild 阶段将编译产物复制到 `$(Sts2Dir)\mods\$(MSBuildProjectName)\`，并同步 FMOD 音频资源（`.bank` 与 `GUIDs.txt`）到 `Resource\Sound\`。
- **PCK + DLL 混合发布**：`mod_manifest.json` 同时设置 `has_pck: true` 与 `has_dll: true`，表明模组既包含 Godot 导出的 PCK 资源包，也包含编译后的 .NET 程序集。
- **GDExtension 集成**：Spine 骨骼动画通过 Godot 4 的 GDExtension 机制加载，二进制文件随仓库提交（`bin/` 目录），无需额外下载。

## 4. 约定与约束

- **NuGet 包版本策略**：`Lib.Harmony` 锁定具体版本 `2.4.2`；`STS2.RitsuLib` 使用 `Version="*"` 通配符，实际版本由 NuGet 解析最近可用版本，但运行时最低版本由 `mod_manifest.json` 的 `min_version: "0.4.41"` 强制约束。
- **游戏 DLL 不随包分发**：所有 `<Reference Include="sts2|0Harmony|Steamworks.NET|STS2-RitsuLib">` 均设 `<Private>false</Private>`，确保模组仅依赖宿主游戏提供这些程序集。
- **最低游戏版本约束**：`mod_manifest.json` 中 `min_game_version: "0.110.0"` 限制模组只能在满足该版本的 STS2 上运行。
- **Godot 引擎版本锁定**：`project.godot` 中 `config/features` 明确声明引擎 `4.5` 与 C# 特性，确保编辑器与导出环境一致。
- **FMOD 资源路径硬编码**：`FmodBuildDir` 指向本地绝对路径 `C:\Users\1394\Downloads\STS2_FModProject_Minimal-main\...`，属于开发者机器相关配置，需在不同环境修改后重新构建。
- **教程代码排除编译**：`<Compile Remove="教程文件\**\*.cs" />` 将 `教程文件/` 下的源码从编译输出中剔除，避免污染构建产物。

## 5. 缺失与注意事项

- 仓库未包含 `packages.lock.json` 或 `nuget.config`，因此 NuGet 包版本解析完全依赖通配符与网络源，缺乏确定性锁文件。
- 没有 `.gitignore` 中显式忽略 `bin/` 或 `obj/` 的说明（需检查 `.gitignore` 确认），但 `bin/` 下已提交平台特定的 GDExtension 二进制，属于二进制制品入库模式。
- 依赖更新流程未在仓库中自动化（无 CI 脚本），需手动更新 `mod_manifest.json` 中的 `min_version` 与 `csproj` 中的 `PackageReference`。