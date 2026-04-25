# Weapon Fire Ability 武器发射能力基类

## Purpose

定义 WeaponFireAbility 基类，封装武器发射的通用逻辑。

## ADDED Requirements

### Requirement: WeaponFireAbility 基类

系统 SHALL 提供 `WeaponFireAbility`，继承 `EquipmentAbility` 并实现 `IAbilityUpdate`。

#### Scenario: 基类继承
- **WHEN** WeaponFireAbility 被实例化
- **THEN** 它继承 `EquipmentAbility`
- **AND** 实现 `IAbilityUpdate` 接口

### Requirement: 冷却管理

系统 SHALL 在基类中管理武器冷却。

#### Scenario: 冷却倒计时
- **WHEN** 武器冷却剩余时间大于 0
- **THEN** 每帧减少 CooldownRemaining
- **AND** 冷却期间不执行发射

#### Scenario: 冷却完成
- **WHEN** CooldownRemaining 减少到 0 或以下
- **THEN** 可以继续检查发射条件

### Requirement: 射击间隔管理

系统 SHALL 管理连射武器的射击间隔。

#### Scenario: 射击间隔倒计时
- **WHEN** _fireInterval 大于 0 且 _fireCountdown 大于 0
- **THEN** 每帧减少 _fireCountdown
- **AND** 间隔期间不执行发射

### Requirement: 发射条件检查

系统 SHALL 提供 `CanFire()` 抽象方法，由子类实现具体发射条件。

#### Scenario: 检查发射条件
- **WHEN** 冷却和间隔都完成
- **THEN** 调用 `CanFire()` 检查是否可以发射
- **AND** 如果返回 true，执行发射

### Requirement: 发射执行

系统 SHALL 提供 `DoFire()` 抽象方法，由子类实现具体发射逻辑。

#### Scenario: 执行发射
- **WHEN** `CanFire()` 返回 true
- **THEN** 调用 `DoFire()` 执行具体发射
- **AND** 重置射击间隔计时器
