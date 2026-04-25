## Why

当前最小远征流程虽然已经跑通，但玩家开局资金与初始 Marble 仍然写死在运行时代码里，敌方编队也仍保留远征域专用的 Marble 配置结构，无法支撑“不同开局阵营”“不同初始队伍”“敌方混合多个配置阵营 Marble”这类后续需求。同时，战斗内的敌我归属继续使用 `Camp` 语义，会与 `camp_config_id` 表示的配置阵营产生持续混淆。

## What Changes

- 新增开局配置链路：通过 `InitialConfig -> CampConfig` 决定玩家以哪个阵营包开始游戏，并由阵营包提供初始资金、初始 Marble 列表与初始可用远征列表。
- 新增通用 Marble 生成配置：以 `Gameplay.Combat.MarbleSpawnConfig` 作为通用静态条目，统一承载玩家初始 Marble 与远征敌方 Marble。
- 修改远征 Combat 遭遇配置：移除远征域专用敌方 Marble 结构的依赖，改为直接复用 `MarbleSpawnConfig`。
- 修改运行时初始化流程：去掉 `ExpeditionPersistentDataStore` 中写死的默认 Marble 和默认资金，改为按开局配置初始化持久化数据。
- 修改 Combat 命名：将战斗内敌我归属统一重命名为 `CombatSide`，与 `camp_config_id` 表示的配置阵营语义彻底区分。
- **BREAKING**：远征 Combat 遭遇配置中的敌方 Marble 条目类型发生变化；Combat 域内使用 `Camp` 表示战斗敌我的代码命名将统一迁移到 `CombatSide`。

## Capabilities

### New Capabilities
- `camp-initial-state`: 定义游戏开局配置链路，包括默认开局阵营、阵营包初始资源、初始 Marble 与初始可用远征。

### Modified Capabilities
- `expedition-luban-static-config`: 远征静态配置需要改为复用 `MarbleSpawnConfig` 作为敌方编队条目，并接入新的开局配置链路。
- `expedition-run-loop`: 远征入口和局外持久化初始化不再写死默认队伍，而是从 `InitialConfig` 和 `CampConfig` 构建初始状态。
- `combat-session-bridge`: 远征与 Combat 的桥接对象和流程需要接受通用 Marble 生成配置，并统一使用 `CombatSide` 术语。
- `marble-persistent-data`: Marble 持久化数据初始化来源需要从硬编码默认值改为配置化种子数据，并保留配置阵营信息。

## Impact

- 受影响的配置定义：`Configs/GameConfig/Defines/marble.xml`、`Configs/GameConfig/Defines/expedition.xml`、`Configs/GameConfig/Defines/Camp.xml`、`Configs/GameConfig/Defines/initial.xml`
- 受影响的生成代码：`UnityProject/Assets/GameScripts/HotFix/GameProto/GameConfig/Gameplay/Combat/*`、`.../Gameplay/Expedition/*`、新增 `Gameplay/Camp/*`、`Gameplay/Initial/*`
- 受影响的运行时代码：`ExpeditionPersistentDataStore`、`ExpeditionConfigBridge`、`CombatSessionRequest`、`ExpeditionCombatSessionController`、`ExpeditionFlowController.*` 以及 `Gameplay.Combat` 域内当前使用战斗 `Camp` 语义的代码
- 依赖与流程影响：需要重新生成 Luban 代码与数据；不允许 agent 创建或修改任何 xlsx，表格变更由用户手工维护
