# projectile-factory Specification

## Purpose
TBD - created by archiving change projectile-module. Update Purpose after archive.
## Requirements
### Requirement: 发射物创建

系统 SHALL 提供 `ProjectileFactory.Spawn()` 方法，根据配置创建发射物。

#### Scenario: 创建发射物
- **WHEN** 调用 `ProjectileFactory.Spawn(configId, level, source, target, position, rotation)`
- **THEN** 从 ConfigSystem 加载 ProjectileConfig
- **AND** 创建 ProjectileRuntimeData
- **AND** 设置 SourceCamp 为发射者 Marble 的阵营
- **AND** 设置 TargetMarbleInstId 为目标 Marble 的 InstId
- **AND** 实例化 Prefab 并初始化 Projectile
- **AND** 根据配置组合 Ability 挂载到 Projectile

### Requirement: 能力组合

系统 SHALL 按 Projectile 的既有装配骨架挂载能力：固定骨架能力由 Factory 统一挂载，`ProjectileLevelConfig.lst_ability` 中声明的扩展能力则 MUST 通过配置 creator 入口创建并附加到 Projectile。该规则同样适用于新的击退扩展 Ability。

#### Scenario: Config-driven optional ability is attached through creator
- **WHEN** `ProjectileLevelConfig.lst_ability` 中包含一个 `ProjectileKnockbackConfig`
- **THEN** `ProjectileFactory` 必须通过配置 creator 创建 `ProjectileKnockbackAbility`
- **AND** 再按配置优先级将其挂载到 Projectile

#### Scenario: Fixed core abilities remain outside optional knockback config
- **WHEN** 系统创建一个带击退扩展能力的 Projectile
- **THEN** 移动、伤害、生命周期和追踪等固定骨架能力仍必须按既有入口挂载
- **AND** 击退能力不得替代或混入这些固定骨架字段

### Requirement: 发射物销毁

系统 SHALL 提供 `ProjectileFactory.Despawn()` 方法，安全销毁发射物。

#### Scenario: 销毁发射物
- **WHEN** 调用 `ProjectileFactory.Despawn(projectile)`
- **THEN** 如果 projectile 为 null，直接返回
- **AND** 移除所有 Ability
- **AND** 调用 Unity Destroy 移除 GameObject

### Requirement: 实例 ID 管理

系统 SHALL 为每个发射物分配唯一实例 ID。

#### Scenario: ID 递增
- **WHEN** 创建发射物时
- **THEN** 从 `_instIdCounter` 获取下一个 ID
- **AND** `_instIdCounter` 自增
