# Projectile Config 发射物配置表

## Purpose

定义 Luban 配置表结构，用于数据驱动发射物和武器配置。

## ADDED Requirements

### Requirement: 发射物配置表 (projectile.xml)

系统 SHALL 提供 `projectile.xml` Luban 配置表文件。

#### Scenario: 配置表结构
- **WHEN** 定义 projectile.xml
- **THEN** 包含以下类型：
  - `EnumProjectileTrackingMode`: 追踪模式枚举
  - `ProjectileLevelConfig`: 发射物等级配置
  - `ProjectileAbilityConfig`: 发射物能力配置基类
  - `ProjectileMoveConfig`: 移动能力配置
  - `ProjectileDamageConfig`: 伤害能力配置
  - `ProjectileLifetimeConfig`: 生命周期能力配置
  - `ProjectileConfig`: 发射物配置入口
  - `TbProjectile`: 发射物数据表

### Requirement: 发射物等级配置

系统 SHALL 通过 `ProjectileLevelConfig` 定义发射物的基础属性。

#### Scenario: 等级配置字段
- **WHEN** 定义 ProjectileLevelConfig
- **THEN** 包含字段：
  - level: 等级
  - name: 名称
  - prefab_path: Prefab 路径
  - lst_ability: 能力配置列表（可选）

### Requirement: 发射物能力配置

系统 SHALL 通过能力配置列表定义发射物附加的能力组合。

#### Scenario: 能力组合配置
- **WHEN** 定义 lst_ability
- **THEN** 每个能力配置包含：
  - priority: 优先级
  - 对应能力的具体参数

### Requirement: 移动能力配置

系统 SHALL 提供 `ProjectileMoveConfig` 定义移动参数。

#### Scenario: 移动配置字段
- **WHEN** 定义 ProjectileMoveConfig
- **THEN** 包含字段：
  - speed: 移动速度
  - tracking_mode: 追踪模式
  - tracking_rate: 追踪转向速率

### Requirement: 伤害能力配置

系统 SHALL 提供 `ProjectileDamageConfig` 定义伤害参数。

#### Scenario: 伤害配置字段
- **WHEN** 定义 ProjectileDamageConfig
- **THEN** 包含字段：
  - damage: 伤害值
  - piercing_count: 穿透数量

### Requirement: 生命周期能力配置

系统 SHALL 提供 `ProjectileLifetimeConfig` 定义生命周期参数。

#### Scenario: 生命周期配置字段
- **WHEN** 定义 ProjectileLifetimeConfig
- **THEN** 包含字段：
  - max_lifetime: 最大存在时间（秒），0 表示无限

### Requirement: 弓武器配置扩展

系统 SHALL 扩展 BowLevelConfig，关联发射物配置。

#### Scenario: 弓配置关联发射物
- **WHEN** 定义 BowLevelConfig
- **THEN** 新增字段：
  - projectile_config_id: 发射物配置 ID
  - projectile_level: 发射物等级
