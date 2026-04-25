# Spec: combat-manager

## ADDED Requirements

### Requirement: CombatManager 模块注册

系统 SHALL 在 `GameApp.Awake()` 中创建 `CombatManager` 实例并注册到 `GameModule` 中，使得通过 `GameModule.GetModule<ICombatManager>()` 可获取该模块引用。

#### Scenario: 模块注册成功
- **WHEN** 游戏启动并进入热更域
- **THEN** `CombatManager` 实例被创建并注册到 `GameModule`
- **AND** `GameModule.GetModule<ICombatManager>()` 返回非空引用

### Requirement: 获取最近的敌对单位

系统 SHALL 提供 `GetNearestEnemy(Marble marble)` 方法，根据给定单位的阵营，返回最近的一个敌对 Marble；若不存在敌对单位则返回 null。

#### Scenario: 存在敌对单位
- **WHEN** 调用 `GetNearestEnemy(campA_Marble)`
- **THEN** 系统遍历所有存活的 Marble
- **AND** 筛选阵营与 `campA_Marble.RuntimeData.Camp` 不同的单位
- **AND** 按欧几里得距离排序
- **AND** 返回距离最近的敌方 Marble

#### Scenario: 不存在敌对单位
- **WHEN** 调用 `GetNearestEnemy(marble)` 且场景中没有敌对单位
- **THEN** 方法返回 null

### Requirement: 根据实例 ID 获取单位

系统 SHALL 提供 `GetTarget(int instId)` 方法，根据唯一实例 ID 返回对应的 Marble；若不存在则返回 null。

#### Scenario: 单位存在
- **WHEN** 调用 `GetTarget(validInstId)`
- **THEN** 方法返回 `InstId == validInstId` 的 Marble

#### Scenario: 单位不存在
- **WHEN** 调用 `GetTarget(invalidInstId)`
- **THEN** 方法返回 null

### Requirement: 阵营敌对判断

系统 SHALL 提供 `IsEnemy(Marble a, Marble b)` 方法，判断两个 Marble 是否互为敌对。阵营 ID 不同即为敌对，相同即为友军。

#### Scenario: 同一阵营
- **WHEN** 调用 `IsEnemy(camp1_Marble, camp1_OtherMarble)`
- **THEN** 返回 false

#### Scenario: 不同阵营
- **WHEN** 调用 `IsEnemy(camp1_Marble, camp2_Marble)`
- **THEN** 返回 true

### Requirement: ASC.CombatManager 属性注入

系统 SHALL 在 `ASC` 基类中暴露 `CombatManager` 属性，并在 ASC 创建时从 GameModule 获取注入的 CombatManager 引用。

#### Scenario: ASC 获取 CombatManager
- **WHEN** ASC 实例被创建
- **THEN** `ASC.CombatManager` 属性被赋值为 `GameModule.GetModule<ICombatManager>()`

### Requirement: 活跃单位注册与注销

系统 SHALL 提供 `Register(Marble marble)` 和 `Unregister(Marble marble)` 方法，管理所有活跃的 Marble 列表，确保搜索时只考虑存活单位。

#### Scenario: 单位注册
- **WHEN** `Register(marble)` 被调用
- **THEN** marble 被添加到活跃单位列表

#### Scenario: 单位注销
- **WHEN** `Unregister(marble)` 被调用
- **AND** marble 存在于活跃列表
- **THEN** marble 从活跃单位列表移除

#### Scenario: 已注销单位不参与搜索
- **WHEN** 调用 `GetNearestEnemy()`
- **THEN** 未注册或已注销的单位不在搜索范围内
