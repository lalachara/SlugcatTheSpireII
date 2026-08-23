---
name: sts2-reference
description: 引导访问外部 reference 工作区（c:/Project/Sts2 Mod Project/project/reference）中的 STS2 参考资源（教程文件、游戏源代码解包、成品 mod 示例）。当任务涉及 STS2 mod 开发、不理解某个游戏功能/API、需要查看原版实现或参考其它 mod 写法时，先查阅本技能与 INDEX.md 目录索引再动手。
---

# STS2 参考资源导航

## 触发时机

进行 STS2 mod 开发相关任务（写卡牌/遗物/能力/药水/怪物/事件/人物、查 API、改游戏逻辑、参考示例）时，**先查阅本技能**。参考资源主体存放在外部路径 `c:/Project/Sts2 Mod Project/project/reference/`（本工作区外，通过绝对路径访问，不要复制到本工作区）。

## 快速开始

1. **先读目录索引** [INDEX.md](c:/Project/Sts2%20Mod%20Project/project/reference/INDEX.md)（reference 根目录），按"使用流程"三步骤定位所需内容。
2. 根据需求进入对应资源目录（见下，注意使用绝对路径）。
3. 遇到不理解的术语/功能时，先在教程文件里找对应章节，不要直接猜。

## 三大资源及用法

### 1. 教程文件（理解功能用法）

**优先读本工作区根目录的 `教程文件/`**（内容与 reference 完全一致，且额外包含 `ForAIRead/` AI 读取数据与 `model/` 示例）：

- `Basics/`：环境配置、看源码修改、基础库选型、变量与描述、先古对话、调试热重载、启动参数、控制台指令、Patch 基础
- `RitsuLib/`：核心教程，按三块组织
  - `01 - 添加基础内容/`（18 篇）：卡牌、遗物、能力、药水、怪物、事件、人物、充能球、附魔、卡池、单例、时间线等
  - `02 - 玩法基底/`（13 篇）：血条覆盖、手牌上限、手牌泛光、顶栏按钮、通知提示、自定义目标、右键交互、次要资源等
  - `03 - 模组工具/`（10 篇）：数据保存、生命周期事件、补丁系统、内容注册、节点附加、模组联动等
  - `RitsuLib/README.md`：依赖安装与初始化写法
- `Migrations/`：正式版↔测试版、BaseLib→RitsuLib 迁移注意事项
- `ForAIRead/`：给 AI 读取的游戏数据 json（卡牌、遗物、能力、怪物、事件、关键词等）

> 规则：用户问"怎么做 X"且教程里有对应章节时，先读完该章节再回答或写代码。

### 2. 游戏源代码解包（查原版实现）— 外部路径

入口：`c:/Project/Sts2 Mod Project/project/reference/游戏源代码解包/sts2.sln`
源码主体：`c:/Project/Sts2 Mod Project/project/reference/游戏源代码解包/sts2/MegaCrit/sts2/`

| 需求 | 查找位置 |
|------|----------|
| 卡牌/遗物/能力/药水等基类与数据结构 | `Core/Models/`（Cards、Relics、Powers、Potions、Enchantments、Orbs、Monsters、Encounters、Events、Characters…） |
| 动作效果（伤害、抽牌、增益…） | `Core/GameActions/` |
| UI / Godot 节点 / 特效 | `Core/Nodes/`（Cards、Combat、Screens、Vfx…） |
| mod 加载、注册接口 | `Core/Modding/` |
| 战斗逻辑、怪物意图 | `Core/Combat/`、`Core/MonsterMoves/` |
| 事件、存档、地图、日志、本地化 | `Core/Events/`、`Core/Saves/`、`Core/Map/`、`Core/Logging/`、`Core/Localization/` |

> 规则：确认 API 签名/继承关系/机制细节时以解包源码为准，可用 LSP 跳转定义与引用。

### 3. 成品 mod 示例（参考真实写法）— 外部路径

入口：`c:/Project/Sts2 Mod Project/project/reference/OtherMod/YuWanCardCode/`（鱼丸卡牌 mod）

- 写卡牌 → 参考 `Cards/Pig/`、`Cards/Colorless/` 中的同类卡
- 写遗物/能力 → 参考 `Relics/`、`Powers/`
- 写新人物 → 参考 `Characters/`（Pig.cs + 三个 Pool）
- Harmony Patch → 参考 `Patches/`（88 个 patch）与 `Core/Patches/`
- 复杂配置架构 → 参考 `Config/`
- 跨 mod 联动 → 参考 `Integrations/Hextech/`、`Balatro/`

> 规则：需要示例代码时优先复用该 mod 中已验证的写法惯例（命名、属性、注册方式），保持风格一致。

## 本项目（SlugcatTheSpireII）特有机制速查

以下为本项目已验证的关键机制结论，修改相关代码前先读：

- **业力复活机制**：蛞蝓猫死亡时的业力复活由 `Scripts/patches/Singleton/RunSingleton.cs` 中的 hook 触发（`[RegisterSingleton]`，`HookType.Run`）——`ShouldDie(creature)` 在业力>0 时返回 false 阻止死亡，`AfterPreventingDeath(creature)` 调用 `SlugcatData.reLive` 消费业力复活。
- **已弃用的补丁**：`LoseHpInternalPatch`、`ForceKillPatch` 是旧版复活实现，已弃用（`RainworldPatches.cs` 中保持注释状态），不要启用或参考它们。
- **多人模式非战斗死亡防御**：原版 `CreatureCmd.KillWithoutCheckingWinCondition` 在（非战斗 && 多人 && 非强制）时会 Log.Error 并把玩家回血到 1 后 return，导致 `Hook.ShouldDie` 永远不被调用（单人模式无此 guard，所以单人复活正常）。本项目在 `Scripts/patches/EventOptionWorkLevelPatch.cs` 的 `EventOptionMultiplayerGuardPatch` 中处理。**不能把 `force` 置 true 绕过 guard**——原版 `force || Hook.ShouldDie(...)` 短路会直接进入真死分支；**也不要在 Kill/死亡流程里绕**——`Kill` 复数后续的 `runState.Players.All(IsDead)`/game over 检查、`Damage` 复数后续逻辑都会因蛞蝓猫状态异常而抛 NullReferenceException。**正确做法是在 `CreatureCmd.Damage` 复数重载（`(PlayerChoiceContext, IEnumerable<Creature>, decimal, ValueProp, Creature, CardModel, CardPlay)`，所有 damage 的归一入口）的 Prefix 中、伤害造成死亡前拦截**：非战斗多人事件的蛞蝓猫、业力>0 且本次伤害致死时，同步扣 1 业力 + 回满血并从 `targets` 剔除（`ref` 替换集合），蛞蝓猫根本不进入死亡流程，后续逻辑自然正常；业力=0 时不拦截，交给原版 `WillKillPlayer` 禁选致死选项。
- 业力核心逻辑：`Scripts/Slugcat/SlugcatData.cs`（`canbekill`/`reLive`/`addworklevel`/`SetWorklevel` 等）。

## 注意事项

- 教程文件与源码均为**参考材料**，不要修改它们；reference 目录位于本工作区之外，务必通过绝对路径访问，不要复制到本工作区。
- 若索引内容与实际目录不一致（如新增章节），以实际目录为准，可顺手更新 [INDEX.md](c:/Project/Sts2%20Mod%20Project/project/reference/INDEX.md)。
- 三种资源冲突时，优先级：用户需求 > 游戏源码实际行为 > 教程 > mod 示例代码。
