# Bow Fire Ability 弓发射能力

## Purpose

定义 BowFireAbility，继承 WeaponFireAbility，实现弓的发射逻辑。

## ADDED Requirements

### Requirement: BowFireAbility 弓发射能力

系统 SHALL 提供 `BowFireAbility`，继承 `WeaponFireAbility`。

#### Scenario: 实例化弓发射能力
- **WHEN** BowFireAbility 被创建并添加到 BowEquipment
- **THEN** 它继承 `WeaponFireAbility`
- **AND** _fireInterval 从 BowRuntimeData.ArrowInterval 获取

### Requirement: 发射条件检查

系统 SHALL 检查弓是否瞄准完成。

#### Scenario: 瞄准完成可发射
- **WHEN** BowRuntimeData.CanFire 为 true
- **THEN** CanFire() 返回 true

#### Scenario: 瞄准未完成不可发射
- **WHEN** BowRuntimeData.CanFire 为 false
- **THEN** CanFire() 返回 false

### Requirement: 发射箭矢

系统 SHALL 根据射击类型发射对应数量的箭矢。

#### Scenario: 连射模式 (ShootType = 0)
- **WHEN** ShootType 为 0
- **AND** ArrowCount 为 N
- **THEN** 发射 N 根箭矢
- **AND** 每根箭矢方向相同（弓的朝向）

#### Scenario: 散射模式 (ShootType = 1)
- **WHEN** ShootType 为 1
- **AND** ArrowCount 为 N
- **AND** ArrowAngleStep 为 A
- **THEN** 发射 N 根箭矢
- **AND** 角度间隔为 A 度
- **AND** 以弓朝向为中心向两侧分布

#### Scenario: 奇数箭矢居中
- **WHEN** 散射模式下 ArrowCount 为奇数
- **THEN** 中间一根箭矢方向与弓朝向相同

#### Scenario: 偶数箭矢对称
- **WHEN** 散射模式下 ArrowCount 为偶数
- **THEN** 左右两侧箭矢数量相等
- **AND** 总角度跨度为 (N-1) * ArrowAngleStep

### Requirement: 箭矢创建

系统 SHALL 调用 ProjectileFactory 创建箭矢。

#### Scenario: 创建箭矢
- **WHEN** 执行发射
- **THEN** 调用 `ProjectileFactory.SpawnArrow()`
- **AND** 传入弓的引用、目标、生成位置、旋转角度
