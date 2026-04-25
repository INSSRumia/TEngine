## ADDED Requirements

### Requirement: Marble equipment SHALL be instantiated from level configuration
系统 MUST 在创建 Marble 时读取当前 `MarbleLevelConfig.lst_equipment_id`，并为其中每个装备配置生成对应的运行时装备对象。每个装备 MUST 挂载到配置指定的槽位，并跟随所属 Marble 生命周期存在。

#### Scenario: Create equipment from level config
- **WHEN** 一个 Marble 依据某个等级配置完成创建
- **THEN** 系统必须为该等级配置中的每个装备条目创建并挂载对应装备对象

#### Scenario: Equipment follows owner lifecycle
- **WHEN** 所属 Marble 被销毁或失效
- **THEN** 其挂载的装备对象也必须随之失效或移除

### Requirement: Equipment runtime data SHALL not store owner object references
装备运行时数据 MUST 仅保存纯数据状态，不得直接保存 `Marble`、`GameObject`、`Transform` 等对象引用。装备与宿主 Marble 的归属关系 MUST 存在于装备宿主对象层。

#### Scenario: Owner stored on host instead of runtime data
- **WHEN** 系统初始化一个装备对象
- **THEN** 装备宿主必须持有所属 Marble 引用，而运行时数据中不得保存该引用

### Requirement: Armor SHALL support both reduction and absorb behaviors
系统 MUST 支持两类防具语义：当 `ArmorConfig.hp <= 0` 时，装备作为不可破坏防具提供减伤；当 `ArmorConfig.hp > 0` 时，装备作为可破坏防具优先承受伤害，直到自身耐久耗尽后失效。

#### Scenario: Indestructible armor reduces damage
- **WHEN** 一个 `hp <= 0` 的防具参与伤害链
- **THEN** 它必须通过 `defense` 对宿主伤害进行减免且不会进入破坏状态

#### Scenario: Destructible armor absorbs damage first
- **WHEN** 一个 `hp > 0` 的防具参与伤害链并受到伤害
- **THEN** 它必须优先消耗自身耐久，且在耐久耗尽前不得把该部分伤害转移到宿主生命

### Requirement: Weapon SHALL support collision-based damage and cooldown
系统 MUST 支持普通武器运行时数据和行为。武器 MUST 具备基础攻击值、冷却和伤害计算模式，并能在允许攻击时对目标产生伤害。

#### Scenario: Fixed damage weapon deals base attack
- **WHEN** 一个 `is_damage_by_velocity = false` 的武器成功命中目标
- **THEN** 它必须按基础 `attack` 作为伤害值参与结算

#### Scenario: Velocity-scaled weapon uses relative speed
- **WHEN** 一个 `is_damage_by_velocity = true` 的武器成功命中目标
- **THEN** 它必须按相对速度与攻击系数计算伤害，而不是直接使用固定攻击值

### Requirement: Bow SHALL support autonomous aim and fire behavior
系统 MUST 支持 BowConfig 对应的主动武器行为，包括自动目标选择、按旋转速度转向、命中瞄准角后发射，以及按连射/散射规则生成箭矢发射参数。

#### Scenario: Bow rotates toward target before firing
- **WHEN** 弓装备已选择目标且尚未满足发射夹角
- **THEN** 它必须按 `rotate_speed` 持续朝目标方向转动，并在满足 `aim_angle` 前不得发射

#### Scenario: Bow fires according to shoot type
- **WHEN** 弓满足冷却和瞄准条件
- **THEN** 它必须按 `shoot_type`、`arrow_count`、`arrow_interval` 和 `arrow_angle_step` 生成对应的发射行为参数
