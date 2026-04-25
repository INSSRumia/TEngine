# projectile-core Specification

## Purpose
TBD - created by archiving change projectile-module. Update Purpose after archive.
## Requirements
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

系统 SHALL 在发射物碰撞时通过统一的命中流水线转发事件给 Ability 系统。该流水线 MUST 在识别有效目标后构建统一的命中上下文，并将其提供给伤害处理与其他实现命中接口的可选扩展 Ability，而不是让每个扩展 Ability 各自重复解析碰撞对象。

#### Scenario: Valid collision is forwarded as hit context
- **WHEN** Projectile 的 Collider 与一个有效敌对目标发生碰撞
- **THEN** 系统必须构建本次命中的统一上下文
- **AND** 将该上下文转发给负责伤害和命中特效的相关 Ability

#### Scenario: Invalid collision is not forwarded as a valid hit
- **WHEN** Projectile 的 Collider 与友军、重复命中目标或无效对象发生碰撞
- **THEN** 系统不得把该碰撞作为有效命中转发给命中特效 Ability

### Requirement: 移动由 Ability 处理

系统 SHALL 让 Ability 负责发射物的移动，Projectile 本身不实现移动逻辑。

#### Scenario: 移动职责分离
- **WHEN** Projectile 执行 FixedUpdate
- **THEN** 调用 Ability 系统的 FixedUpdate
- **AND** 由 `ProjectileMoveAbility` 实现具体移动逻辑
