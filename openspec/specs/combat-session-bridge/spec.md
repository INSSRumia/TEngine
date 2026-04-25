## ADDED Requirements

### Requirement: 远征与 Combat SHALL 通过桥接数据包交互
系统 SHALL 使用远征侧请求/结果数据包将远征流程与 `Gameplay.Combat` 模块连接起来，而不是让远征流程直接持有 Combat 场景对象引用。

#### Scenario: 发起 Combat 会话
- **WHEN** 远征流程进入一个 `CombatNode`
- **THEN** 系统构造一份 `CombatSessionRequest`
- **AND** 请求中包含本次参战 Marble 快照、当前远征 Buff 和目标 Combat 节点配置

#### Scenario: 接收 Combat 会话结果
- **WHEN** 一场 Combat 结束
- **THEN** Combat 模块返回一份 `CombatSessionResult`
- **AND** 该结果中包含胜负状态、Marble 结果与本场奖励

### Requirement: Combat 结果 SHALL 可被远征流程稳定回写
系统 SHALL 让 `CombatSessionResult` 成为远征侧回写 Marble 快照和节点记录的唯一结果来源。

#### Scenario: 回写 Combat 结果
- **WHEN** 远征流程收到 `CombatSessionResult`
- **THEN** 系统使用该结果更新当前节点记录
- **AND** 系统使用该结果更新本次远征中的 Marble 快照
- **AND** 系统不直接读取 Combat 场景对象作为结算依据

### Requirement: 桥接层 SHALL 统一使用 Combat 术语
系统 SHALL 在远征侧桥接对象与流程状态中统一使用 `Combat` 术语，避免与现有 `Gameplay.Combat` 域产生双重命名体系。

#### Scenario: 新桥接对象命名
- **WHEN** 系统定义远征与战斗域之间的请求与结果对象
- **THEN** 新对象名称使用 `CombatSessionRequest` 与 `CombatSessionResult`
- **AND** 不使用与之并行的 `BattleSession*` 命名

### Requirement: Combat 会话请求 SHALL 携带选定场地
系统 SHALL 在远征发起 Combat 会话前完成场地选择，并通过 `CombatSessionRequest` 或等价桥接对象携带选定场地配置 Id。远征侧 MUST 不直接解析 `CombatBattlefieldConfig`。

#### Scenario: 请求携带显式场地
- **WHEN** Combat 遭遇配置显式指定场地
- **THEN** 远征侧 Combat 会话请求携带该场地信息

#### Scenario: 请求携带环境随机场地
- **WHEN** Combat 遭遇未显式指定场地
- **AND** 系统从当前环境随机选择了场地
- **THEN** 远征侧 Combat 会话请求携带随机选中的场地信息

### Requirement: Combat 层 SHALL 根据桥接场地加载作战空间
Combat 层 SHALL 使用远征侧会话请求提供的场地配置 Id，通过 `BattlefieldFactory` 在 `Assets/AssetRaw/Actor/Prefabs/Battlefield` 目录下加载同名作战空间 prefab，并在该场地中生成参战 Marble。

#### Scenario: Combat 使用请求场地
- **WHEN** Combat 层收到包含场地信息的会话请求
- **THEN** Combat 层按场地配置 Id 加载同名 prefab
- **AND** Combat 层在加载出的场地中生成玩家和敌方 Marble
