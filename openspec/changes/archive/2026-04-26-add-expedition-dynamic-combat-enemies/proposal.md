## Why

当前远征的事件奖励已经可以按远征进度动态缩放，但 Combat 敌人的类型、数量和等级仍主要依赖写死配置。这会让同一批 Combat 遭遇在远征前期可用、后期却明显偏弱，也会迫使配置者复制大量只差强度的遭遇模板。

现在需要把“敌人强度动态化”纳入远征配置体系，让遭遇既能保留固定敌人，又能从当前环境中动态生成敌人，并复用和奖励档位相同的分层思路，避免后续内容扩展时配置爆炸。

## What Changes

- 新增远征敌人强度档位配置，用于把“敌人数量档位”和“敌人等级档位”解析成不同远征阶段下的真实数值。
- 为环境配置新增动态敌人候选池。环境负责提供本环境下可能出现的敌人类型及其权重。
- 扩展 Combat 遭遇配置：保留现有固定敌人列表，同时新增动态敌人组配置。每个动态敌人组只声明数量档位和等级档位，不直接写死具体敌人类型。
- 远征在发起 Combat 前，先将 Combat 遭遇解析为最终敌方 roster：固定敌人直接保留，动态敌人从当前环境敌人池按权重抽取类型，再用敌人档位配置解析数量与等级。
- 动态敌人类型抽取首版按放回方式执行，同一场 Combat 中允许多次抽到同一种候选敌人。
- 动态敌人的最终等级由动态敌人组解析结果覆盖候选 `MarbleSpawnConfig` 中的等级字段，避免环境候选池和强度档位双重定义等级来源。
- 若动态敌人组存在，但当前环境没有有效敌人候选池，系统必须给出清晰错误并阻止本场 Combat 静默以错误配置开始。
- 不允许 agent 创建、编辑、填充或修改任何 xlsx 表格。若 schema 变更需要表格调整，agent 必须暂停并通知用户手工修改后再继续。

## Capabilities

### New Capabilities

- `expedition-enemy-profiles`: 定义远征敌人数量档位、等级档位和阶段化强度映射。
- `expedition-dynamic-combat-enemies`: 定义 Combat 遭遇中固定敌人与动态敌人组的混合生成规则，以及环境敌人池到最终敌方 roster 的解析流程。

### Modified Capabilities

- `expedition-environments`: 增加环境敌人候选池，以及当前环境作为动态敌人类型来源的运行规则。
- `expedition-luban-static-config`: 增加敌人档位配置、环境敌人候选池、动态敌人组和远征引用敌人档位配置的 schema 要求。
- `combat-session-bridge`: 调整远征发起 Combat 时的桥接语义，要求远征侧在请求阶段就提供已解析完成的敌方 roster，而不是让 Combat 解析远征动态敌人配置。

## Impact

- 影响 Luban schema 与生成代码：
  - `Configs/GameConfig/Defines/expedition.xml`
  - `UnityProject/Assets/GameScripts/HotFix/GameProto/GameConfig/Gameplay/Expedition/`
- 影响远征运行时代码：
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/`
- 影响 Combat 桥接层：
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/Controller/`
- 影响配置协作流程：
  - schema 改动后需要用户手工修改对应 `xlsx` 并重新生成 Luban 代码，agent 不直接改表。
