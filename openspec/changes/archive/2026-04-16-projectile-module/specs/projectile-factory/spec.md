# Projectile Factory 发射物工厂

## Purpose

定义 ProjectileFactory 工厂类的职责和使用规范。Factory 根据配置组合 Ability 创建发射物。

## ADDED Requirements

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

系统 SHALL 根据配置表的 lst_ability 组合 Ability。

#### Scenario: 组合移动和伤害能力
- **WHEN** 配置中指定了 ProjectileMoveAbility 和 ProjectileDamageAbility
- **THEN** 创建并挂载这些 Ability 到 Projectile
- **AND** Ability 引用对应的配置数据

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
