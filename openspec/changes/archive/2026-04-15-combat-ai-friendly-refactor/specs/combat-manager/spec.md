## MODIFIED Requirements

### Requirement: ASC.CombatManager 属性注入
系统 SHALL 在 `ASC` 基类中暴露 `CombatManager` 属性，并在 ASC 创建时为战斗对象注入可用的 CombatManager 引用。所有依赖 CombatManager 的战斗对象创建流程 ALSO MUST 使用稳定、一致的工厂装配结构，避免因为不同工厂实现差异导致目标获取、注册和战斗行为扩展点难以推断。

#### Scenario: ASC 获取 CombatManager
- **WHEN** ASC 实例被创建
- **THEN** `ASC.CombatManager` 属性被赋值为可用的 CombatManager 引用

#### Scenario: Factory-created combat objects expose consistent manager-dependent behavior
- **WHEN** Marble、Equipment 或 Projectile 由各自的战斗工厂创建
- **THEN** 它们依赖 CombatManager 的能力装配路径必须保持一致和可预测
- **AND** 工厂扩展点不得因为结构漂移而破坏目标查询、注册或后续能力扩展
