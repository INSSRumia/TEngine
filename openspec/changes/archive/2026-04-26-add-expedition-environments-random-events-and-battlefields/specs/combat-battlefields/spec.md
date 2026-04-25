## ADDED Requirements

### Requirement: Combat SHALL 支持场地配置
系统 SHALL 支持在 `Gameplay.Combat` 命名空间下通过配置定义 Combat 场地。每个场地配置 MUST 提供 `battlefield_config_id`，Combat 层 MUST 通过 `BattlefieldFactory` 在 `Assets/AssetRaw/Actor/Prefabs/Battlefield` 目录下按同名规则加载对应 prefab，并作为 Combat 会话生成场景内容的依据。

#### Scenario: 读取场地配置
- **WHEN** Combat 会话准备加载作战空间
- **THEN** 系统能够根据场地配置获取 `battlefield_config_id`
- **AND** Combat 层通过 `BattlefieldFactory` 使用该 Id 加载同名场地 prefab

### Requirement: Battlefield SHALL 提供双方出生区域
系统 SHALL 提供 Combat 命名空间下的 `Battlefield` 组件。该组件 MUST 允许配置玩家方和敌方 Marble 的出生 Bounds，并 MUST 能根据 CombatSide 将 Marble 放置到对应区域中的随机位置。

#### Scenario: 根据 CombatSide 放置 Marble
- **WHEN** Combat 层生成一个 Marble 并调用 Battlefield 放置方法
- **THEN** Battlefield 根据传入 side 选择玩家方或敌方出生 Bounds
- **AND** Marble 被放置到该 Bounds 内的随机位置

#### Scenario: 编辑器显示出生区域
- **WHEN** 开发者在 Unity 编辑器中选中 Battlefield 所在对象
- **THEN** 系统通过 Gizmos 显示玩家方和敌方出生 Bounds

### Requirement: Combat 遭遇 SHALL 可显式指定场地
系统 SHALL 允许远征 Combat 遭遇配置显式指定一个场地。当遭遇配置提供有效场地时，系统 MUST 使用该场地，而不是从当前环境中随机选择。

#### Scenario: 遭遇显式指定场地
- **WHEN** 远征进入一个 Combat 节点
- **AND** 该 Combat 遭遇配置了有效场地
- **THEN** 系统选择遭遇配置指定的场地作为本场 Combat 场地

### Requirement: Combat 场地 SHALL 可从当前环境按权重随机选择
当 Combat 遭遇未显式指定场地时，系统 MUST 从当前环境配置的场地候选中按权重随机选择一个场地。场地随机选择 MUST 是放回的，不因历史选择而移除候选场地。

#### Scenario: 从当前环境随机选择场地
- **WHEN** 远征进入一个 Combat 节点
- **AND** 该 Combat 遭遇没有显式指定场地
- **THEN** 系统从当前环境的场地候选中按权重选择一个场地
- **AND** 本次选择不会从环境候选列表中移除该场地

#### Scenario: 相同场地可在多场 Combat 中重复出现
- **WHEN** 两场 Combat 都需要从当前环境随机选择场地
- **THEN** 系统允许两场 Combat 选择到同一个场地

### Requirement: Combat 场地缺失 SHALL 明确失败
当 Combat 遭遇未显式指定场地且当前环境没有有效场地候选时，系统 MUST 输出清晰错误并阻止 Combat 以未知场地静默开始。

#### Scenario: 无可用场地时阻止 Combat 开始
- **WHEN** 远征进入一个 Combat 节点
- **AND** 遭遇没有显式场地
- **AND** 当前环境没有有效场地候选
- **THEN** 系统报告缺失场地配置错误
- **AND** 系统不以空场地或未知场地启动 Combat 会话
