---
kind: build_system
name: Godot.NET + STS2 模组构建与发布体系
category: build_system
scope:
    - '**'
source_files:
    - project.godot
    - SlugcatTheSpireII.csproj
    - SlugcatTheSpireII.sln
    - export_presets.cfg
    - mod_manifest.json
    - bin/spine_godot_extension.gdextension
---

## 1. 构建系统概览

本项目是一个基于 **Godot 4.5 + C# (.NET 9)** 的《杀戮尖塔2》（STS2）模组工程，采用 Godot.NET SDK 作为核心构建工具，配合 Visual Studio Solution、`mod_manifest.json` 和 `export_presets.cfg` 完成从源码编译到模组发布的完整流水线。

- **引擎/SDK**：`Godot.NET.Sdk/4.5.1`（见 `.csproj`），目标框架 `net9.0`，语言版本 `C# 12`，启用 `Nullable` 与 `AllowUnsafeBlocks`。
- **IDE/工作区**：`.sln` 定义三个配置：`Debug`、`ExportDebug`、`ExportRelease`，均对应 `Any CPU`。
- **运行时依赖**：通过 `HintPath` 引用游戏目录下的 `sts2.dll`、`0Harmony.dll`、`Steamworks.NET.dll`，以及外部模组 `STS2-RitsuLib`；同时以 NuGet 包形式引入 `STS2.RitsuLib` 与 `Lib.Harmony 2.4.2`。
- **模组清单**：根目录 `mod_manifest.json` 声明模组 ID、名称、作者、版本号 `0.1.4`、最低游戏版本 `0.110.0`，并声明依赖 `STS2-RitsuLib >= 0.4.41`，同时标记 `has_pck=true`、`has_dll=true`、`affects_gameplay=true`。

## 2. 关键文件与职责

| 文件 | 作用 |
|---|---|
| `project.godot` | Godot 项目元数据：引擎版本 `4.5`、特性 `C#`、`Mobile`，渲染后端 `mobile`，dotnet 程序集名 `demo` |
| `SlugcatTheSpireII.csproj` | MSBuild 项目文件：引用 STS2 运行时 DLL、NuGet 包、资源内容项，并在 `PostBuildEvent` 中执行自定义复制任务 |
| `SlugcatTheSpireII.sln` | VS 解决方案，定义 Debug / ExportDebug / ExportRelease 三种构建配置 |
| `export_presets.cfg` | Godot 导出预设：Windows Desktop 平台，输出路径指向本地 Steam 安装目录，包含音频 `.bank` 与 `GUIDs.txt` |
| `mod_manifest.json` | STS2 模组加载器识别的清单，声明版本、依赖、是否含 PCK/DLL |
| `bin/spine_godot_extension.gdextension` | Spine 骨骼动画 Godot 扩展，随模组一起分发 |
| `bin/windows/*.dll` | 预编译的 Spine Godot 原生扩展（editor/template_debug/template_release 各架构） |

## 3. 构建与发布流程

### 3.1 编译阶段（MSBuild / dotnet build）
1. `Godot.NET.Sdk` 根据 `project.godot` 中的 `[dotnet] project/assembly_name="demo"` 生成名为 `demo.dll` 的程序集。
2. 通过 `<Reference HintPath>` 解析 STS2 运行时与 Harmony、Steamworks 依赖。
3. `<Compile Remove="教程文件\**\*.cs">` 排除教程源码参与编译。
4. `<Content Include ... />` 将 Godot 场景、纹理、Sound 资源、多语言 JSON 等标记为内容项，随打包一并输出。

### 3.2 后构建事件（Copy Mod Target）
在 `AfterTargets="PostBuildEvent"` 中执行：
- 创建 `$(Sts2Dir)\mods\$(MSBuildProjectName)\` 目录；
- 将生成的 `$(TargetPath)`（即 `demo.dll`）复制到该目录；
- 从 `FmodBuildDir` 复制 FMOD 音频资源 `SlugcatTheSpireII.bank` 与 `GUIDs.txt` 到 `Resource\Sound\`。

该目标硬编码了本机路径（`D:\Game\Steam\...` 与 `C:\Users\...\STS2_FModProject_Minimal-main\...`），因此当前仅适用于开发机本地构建。

### 3.3 资源导出（Godot 编辑器）
`export_presets.cfg` 定义了 Windows Desktop 导出预设：
- 平台：`Windows Desktop`，架构 `x86_64`；
- 导出过滤器：`all_resources`，额外包含 `*.bank,*/GUIDs.txt`；
- 脚本导出模式 `script_export_mode=2`（嵌入 C# 脚本）；
- 调试符号 `dotnet/include_debug_symbols=true`，不嵌入构建产物 `dotnet/embed_build_outputs=false`；
- 输出路径硬编码为 `D:/Game/Steam/steamapps/common/Slay the Spire 2/mods/demo/demo.exe`。

### 3.4 模组发布
最终产物由 STS2 模组加载器读取 `mod_manifest.json` 进行识别与加载。发布物包括：
- 编译出的 `demo.dll`（位于 `mods/SlugcatTheSpireII/`）；
- 资源目录（Godot 场景、图片、Spine 资源、FMOD bank）；
- `spine_godot_extension.gdextension` 及对应平台原生 DLL；
- `mod_manifest.json`。

## 4. 约定与约束

- **引擎/SDK 锁定**：`project.godot` 固定 Godot 4.5 与 C# 特性；`.csproj` 锁定 `Godot.NET.Sdk/4.5.1`、`net9.0`、`LangVersion=12.0`。升级需同步调整。
- **STS2 路径约定**：`Sts2Dir` 与 `Sts2DataDir` 在 `.csproj` 中以绝对路径声明，构建时直接引用游戏目录下的 `sts2.dll`、`0Harmony.dll`、`Steamworks.NET.dll`。不同机器需修改这些属性。
- **依赖声明**：模组依赖 `STS2-RitsuLib >= 0.4.41`，由 `mod_manifest.json` 强制校验；运行时还依赖 Harmony 与 Steamworks.NET。
- **资源组织**：Godot 场景与资源放在 `Godot/`，C# 脚本放在 `Scripts/`，静态资源放在 `Resource/`，多语言 JSON 放在 `SlugcatTheSpireII/localization/{eng,kor,zhs}/`，并通过 `<Content Include>` 显式纳入构建。
- **导出产物命名**：由于 `project.godot` 中 `assembly_name="demo"`，生成的 DLL 名为 `demo.dll`，而非项目名；发布时需确保文件名与模组目录一致。
- **无 CI/自动化脚本**：仓库未包含 Makefile、GitHub Actions、Dockerfile 等 CI 配置；构建完全依赖本地 Godot 编辑器与 Visual Studio。
- **FMOD 集成**：FMOD 构建产物路径硬编码在 `.csproj` 的 `FmodBuildDir` 中，需手动维护该目录以更新音频资源。

## 5. 适用性说明

本仓库是典型的 Godot.NET 模组工程，构建系统围绕 Godot 编辑器导出与 MSBuild 后构建事件展开，没有独立的 CI 流水线或容器化部署，发布过程高度依赖本地 STS2 安装路径。