# Spec: combat-test-bridge

## ADDED Requirements

### Requirement: 热更域测试桥接组件注册

`CombatTestAuthoringBridge` SHALL 在热更域启动时（`GameApp.Awake()`）创建并初始化，管理战斗测试的士兵创建和清理流程。

#### Scenario: 桥接组件初始化
- **WHEN** 游戏进入热更域
- **THEN** `CombatTestAuthoringBridge` 实例被创建
- **AND** 组件处于待命状态，等待主域发送配置

### Requirement: 接收测试配置并创建士兵

组件 SHALL 提供 `CreateSoldiers(int camp, int configId, int count)` 方法，根据传入的阵营、配置ID和数量，使用 `MarbleFactory.CreateMarble()` 创建对应数量的士兵，并自动注册到 `CombatManager`。

#### Scenario: 创建同阵营士兵
- **WHEN** 调用 `CreateSoldiers(camp: 1, configId: 1001, count: 5)`
- **THEN** 系统创建 5 个配置ID为 1001 的 Marble
- **AND** 所有士兵的 `RuntimeData.Camp` 设置为 1
- **AND** 每个士兵调用 `CombatManager.Register(marble)` 注册

#### Scenario: 创建多阵营士兵
- **WHEN** 调用 `CreateSoldiers(camp: 1, ...)` 和 `CreateSoldiers(camp: 2, ...)`
- **THEN** 阵营1和阵营2的士兵可互相攻击

### Requirement: 清理测试士兵

组件 SHALL 提供 `ClearAllSoldiers()` 方法，销毁所有通过测试工具创建的士兵并从 CombatManager 注销。

#### Scenario: 清理所有士兵
- **WHEN** 调用 `ClearAllSoldiers()`
- **THEN** 所有测试士兵被销毁
- **AND** 所有士兵从 `CombatManager.Unregister()`
- **AND** 内部列表被清空

### Requirement: 获取测试士兵列表

组件 SHALL 提供只读属性 `IReadOnlyList<Marble> TestSoldiers`，暴露当前所有测试士兵的引用，供调试使用。

#### Scenario: 访问士兵列表
- **WHEN** 调用 `bridge.TestSoldiers`
- **THEN** 返回当前所有测试士兵的只读列表

### Requirement: Main 域通信接口

组件 SHALL 实现与 `CombatTestAuthoring`（Main 域）的通信机制，通过 Unity `SendMessage` 或接口回调接收配置数据。

#### Scenario: 接收主域配置
- **WHEN** `CombatTestAuthoring` 调用 `OnReceiveConfig(config)`
- **THEN** 桥接组件解析配置并调用 `CreateSoldiers()`

### Requirement: 士兵初始朝向

创建的士兵 SHALL 自动面向最近的敌方单位（调用 `GetNearestEnemy()`），若不存在敌人则保持默认朝向。

#### Scenario: 有敌人在场
- **WHEN** 士兵被创建且存在敌对单位
- **THEN** 士兵初始朝向最近敌人

#### Scenario: 无敌人在场
- **WHEN** 士兵被创建且不存在敌对单位
- **THEN** 士兵保持场景默认朝向
