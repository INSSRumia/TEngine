# Projectile Module 发射物模块

## Why

当前战斗系统中，武器（如弓）已具备瞄准能力，但缺少发射机制。发射物（箭矢、火球等）作为独立的战场实体，需要统一的生命周期管理、移动逻辑和碰撞伤害系统，以支持远程攻击场景。

## What Changes

- 新增 `Projectile` 实体类，继承 `ASC<ProjectileRuntimeData>`，通过 Ability 组合实现不同效果
- 新增 `ProjectileRuntimeData` 运行时数据，支持追踪模式、穿透、生命周期管理
- 新增 `ProjectileFactory` 工厂类，根据配置组合 Ability 创建发射物
- 新增发射物 Ability：`ProjectileMoveAbility`（移动）、`ProjectileDamageAbility`（伤害）、`ProjectileLifetimeAbility`（生命周期）
- 新增 `WeaponFireAbility` 基类，封装武器发射的通用逻辑（冷却、射击间隔）
- 新增 `BowFireAbility`，支持弓的连射和散射模式
- 扩展 Luban 配置表：新增 `projectile.xml`（含 Ability 配置），扩展 `equip.xml` 的 BowLevelConfig
- Projectile 不注册到 CombatManager，通过 SourceCamp 进行敌我判断

## Capabilities

### New Capabilities

- `projectile-core`: 发射物核心实体
- `projectile-runtime-data`: 发射物运行时数据
- `projectile-ability`: 发射物能力（移动、伤害、生命周期）
- `projectile-factory`: 发射物工厂，根据配置组合 Ability 创建发射物
- `projectile-weapon-fire`: 武器发射能力基类
- `projectile-bow-fire`: 弓发射能力
- `projectile-config`: Luban 配置表

### Modified Capabilities

- 无

## Impact

- **新增文件**：
  - `Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Projectile/Projectile.cs`
  - `Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Projectile/ProjectileRuntimeData.cs`
  - `Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Projectile/ProjectileFactory.cs`
  - `Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Projectile/Ability/Core/ProjectileMoveAbility.cs`
  - `Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Projectile/Ability/Core/ProjectileDamageAbility.cs`
  - `Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Projectile/Ability/Core/ProjectileLifetimeAbility.cs`
  - `Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Equipment/Ability/Core/WeaponFireAbility.cs`
  - `Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Equipment/Ability/Core/BowFireAbility.cs`

- **修改文件**：
  - `Configs/GameConfig/Defines/projectile.xml`（新增）
  - `Configs/GameConfig/Defines/equip.xml`（扩展 BowLevelConfig）

- **依赖**：
  - 依赖现有 `ASC<T>` 基类
  - 依赖现有 `Ability` 系统
  - 依赖现有 `CombatManager`（仅用于获取目标 Marble）
  - 依赖 Luban 配置系统
