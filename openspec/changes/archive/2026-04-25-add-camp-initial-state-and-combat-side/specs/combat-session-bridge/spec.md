## MODIFIED Requirements

### Requirement: 远征与 Combat SHALL 通过桥接数据包交互
系统 SHALL 使用远征侧请求/结果数据包将远征流程与 `Gameplay.Combat` 模块连接起来，而不是让远征流程直接持有 Combat 场景对象引用。桥接请求中的敌方 Marble 列表 MUST 使用通用 `MarbleSpawnConfig` 条目表达，并由桥接层在运行时赋予对应的 `CombatSide`。

#### Scenario: 发起 Combat 会话
- **WHEN** 远征流程进入一个 `CombatNode`
- **THEN** 系统构造一份 `CombatSessionRequest`
- **AND** 请求中包含本次参战 Marble 快照、当前远征 Buff 和目标 Combat 节点配置
- **AND** 请求中的敌方 Marble 条目使用 `MarbleSpawnConfig`

#### Scenario: 接收 Combat 会话结果
- **WHEN** 一场 Combat 结束
- **THEN** Combat 模块返回一份 `CombatSessionResult`
- **AND** 该结果中包含胜负状态、Marble 结果与本场奖励

### Requirement: 桥接层 SHALL 统一使用 Combat 术语
系统 SHALL 在远征侧桥接对象与流程状态中统一使用 `Combat` 术语，避免与现有 `Gameplay.Combat` 域产生双重命名体系。对于战斗内敌我归属，系统 MUST 使用 `CombatSide` 表达，而不是继续使用会与 `camp_config_id` 混淆的 `Camp` 命名。

#### Scenario: 新桥接对象命名
- **WHEN** 系统定义远征与战斗域之间的请求与结果对象
- **THEN** 新对象名称使用 `CombatSessionRequest` 与 `CombatSessionResult`
- **AND** 不使用与之并行的 `BattleSession*` 命名

#### Scenario: 运行时敌我归属命名
- **WHEN** 系统在 Combat 域中表达单位的战斗敌我归属
- **THEN** 系统使用 `CombatSide` 语义进行命名和判定
- **AND** 不将 `camp_config_id` 作为战斗内敌我归属字段直接使用
