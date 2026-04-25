# Projectile Core 发射物核心实体

## Purpose

定义 Projectile 实体的核心结构和行为规范。Projectile 只负责实体生命周期和碰撞事件转发，具体行为由 Ability 实现。

## ADDED Requirements

### Requirement: Projectile 实体创建

系统 SHALL 提供 `Projectile` 实体，继承 `ASC<ProjectileRuntimeData>`，作为发射物的核心载体。

#### Scenario: Projectile 继承 ASC 基类
- **WHEN** Projectile 实例被创建
- **THEN** 它继承 `ASC<ProjectileRuntimeData>`
- **AND** 具备 `Rigidbody2D` 物理组件
- **AND** 具备 `Collider2D` 碰撞组件

### Requirement: ProjectileRuntimeData 运行时数据

系统 SHALL 提供 `ProjectileRuntimeData`，包含发射物的所有运行时属性。

#### Scenario: 运行时数据初始化
- **WHEN** 创建 Projectile 时
- **THEN** `ProjectileRuntimeData` 包含以下属性：
  - `Level`: 等级
  - `ConfigId`: 配置ID
  - `SourceCamp`: 发射者阵营
  - `SourceMarbleInstId`: 发射者 Marble 实例 ID
  - `TargetMarbleInstId`: 追踪目标 Marble 实例 ID
  - `TargetPoint`: 追踪目标坐标（用于 Point 模式）
  - `IsActive`: 是否激活

### Requirement: 生命周期管理

系统 SHALL 提供发射物的生命周期管理，包括激活和销毁。

#### Scenario: 发射物激活
- **WHEN** Projectile 被创建并初始化 RuntimeData
- **THEN** `IsActive` 被设置为 true
- **AND** 开始 Ability 循环

#### Scenario: 发射物销毁
- **WHEN** 调用 `Despawn()` 方法
- **THEN** `IsActive` 被设置为 false
- **AND** Projectile 被从场景中移除

### Requirement: 碰撞事件转发

系统 SHALL 在发射物碰撞时转发事件给 Ability 系统。

#### Scenario: 碰撞触发
- **WHEN** Projectile 的 Collider 与 Marble 的 Collider 发生碰撞
- **THEN** 触发 `OnProjectileHit(Marble target)` 事件
- **AND** 由 Ability 处理具体伤害逻辑

### Requirement: 移动由 Ability 处理

系统 SHALL 让 Ability 负责发射物的移动，Projectile 本身不实现移动逻辑。

#### Scenario: 移动职责分离
- **WHEN** Projectile 执行 FixedUpdate
- **THEN** 调用 Ability 系统的 FixedUpdate
- **AND** 由 `ProjectileMoveAbility` 实现具体移动逻辑
