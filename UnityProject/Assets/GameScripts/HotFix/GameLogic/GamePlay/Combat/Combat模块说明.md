# Combat 模块说明

本文档用于解释 Combat 模块的对象关系、Factory 装配骨架、Runtime 黑板和配置驱动边界，帮助 AI 与开发者在不通读全部源码的情况下建立稳定认知。

## 1. 模块结构总览

Combat 目录由三类战斗实体和一组共享基建组成：

- `Marble/`
  - 战斗主体与主黑板。
- `Equipment/`
  - 装备体系，按护甲/武器拆分能力与 RuntimeData。
- `Projectile/`
  - 发射物体系，负责飞行、碰撞和命中后处理。
- 根目录共享基建
  - `ASC.cs`：Ability 宿主基础类。
  - `Ability.cs`：Ability 基类。
  - `RuntimeData.cs`：运行时数据公共根类型。
  - `Interface/`：战斗事件、组合策略与固定更新接口。

## 2. 三类实体关系

### 2.1 Marble

- Combat 的核心战斗实体。
- 持有 `MarbleRuntimeData`，其中包含状态、配置投影和逐帧临时黑板。
- 能挂载核心能力：
  - 受伤/治疗/护盾/死亡/升级/索敌/移动/旋转等。
- 能附加多个装备，装备又会进一步扩展 Marble 的攻防与攻击方式。

### 2.2 Equipment

- 总是依附于某个 Marble。
- 通过 `EquipmentFactory` 根据 `EquipmentLevelConfig` 创建 RuntimeData，并按装备类型挂载不同核心能力。
- 护甲与武器分工不同：
  - 护甲：处理承伤、减伤、吸收等。
  - 武器：处理冷却、瞄准、伤害计算、发射或近战碰撞。

### 2.3 Projectile

- 由武器能力创建的独立实体。
- 生命周期独立于 Marble，但保留来源阵营、来源 Marble 和目标信息。
- 不注册到 `CombatManager`，主要靠 `SourceCamp` 做敌我判断。
- 典型核心骨架是：移动、伤害、生命周期、追踪能力。

## 3. 配置驱动模型

Combat 主要由 Luban schema 驱动，关键入口如下：

- `marble.xml`
  - 定义 Marble 配置入口、等级配置、固定骨架字段、装备挂载列表和扩展能力列表。
- `equip.xml`
  - 定义装备槽位、装备基类配置、武器/护甲差异配置。
- `projectile.xml`
  - 定义发射物等级配置、核心骨架字段和可选扩展能力入口。
- `*_ability.xml`
  - 定义各能力配置 bean，供 Factory 转换成运行时 Ability 实例。
- `timing.xml`
  - 定义定时类能力的持续、冷却和自动激活规则。

设计约定：

- 显式命名字段通常对应“固定骨架能力参数”。
- `lst_ability` 列表用于“玩法扩展能力”。
- Factory 读取显式字段与扩展列表后，统一转换为运行时 Ability。

## 4. Factory 装配骨架

### 4.1 MarbleFactory

装配顺序：

1. 读取 `TbMarble` 与指定等级配置。
2. 创建 `MarbleRuntimeData`。
3. 加载 Marble 预制体并初始化实体。
4. 挂载固定核心能力：
   - 同步缩放、同步质量、伤害/治疗/护盾管线、受伤、加血、加经验、死亡、升级、索敌、移动、旋转。
5. 读取 `MarbleLevelConfig.lst_ability`，通过 creator 列表转换成扩展 Ability。
6. 读取 `MarbleLevelConfig.lst_equipment`，为 Marble 继续挂装 Equipment。

关键点：

- Factory 既负责实例创建，也负责定义“哪些能力属于骨架、哪些属于扩展”。
- 扩展能力通过 `IMarbleAbilityCreatorForConfig` 注册，方便后续玩法按配置扩展。

### 4.2 EquipmentFactory

装配顺序：

1. 读取装备配置与等级配置。
2. 加载预制体。
3. 由 `IEquipmentCreatorForConfig` 为不同装备类型创建对应 RuntimeData。
4. 初始化装备实体。
5. 按装备类型挂固定核心能力：
   - 全装备公共：挂载、损坏。
   - 武器公共：冷却、伤害计算。
   - 护甲/弓/剑：再挂各自专属核心能力。
6. 读取 `EquipmentLevelConfig.lst_ability` 挂扩展能力。

关键点：

- 装备类型分流发生在 Factory，而不是分散在能力内部。
- 核心骨架能力和配置扩展能力是两个明确层次。

### 4.3 ProjectileFactory

装配顺序：

1. 读取发射物配置与等级配置。
2. 加载预制体并设置出生位置与旋转。
3. 创建 `ProjectileRuntimeData`，写入来源 Marble、目标 Marble、目标点、伤害和方向。
4. 初始化发射物实体。
5. 挂固定核心能力：
   - `ProjectileMoveAbility`
   - `ProjectileDamageAbility`
   - `ProjectileLifetimeAbility`
   - 由 `tracking` 字段转换出的追踪核心能力
6. 读取 `ProjectileLevelConfig.lst_ability`，挂扩展能力。

关键点：

- `tracking` 虽然来源于配置，但仍属于发射物固定骨架的一部分，而不是通用玩法扩展。
- 发射物的“骨架追踪能力”和“扩展能力列表”是两个不同入口。

## 5. Marble Runtime 黑板

`MarbleRuntimeData` 是 Marble 能力系统共享访问的黑板根对象，显式分为三个区域：

- `State`
  - 长生命周期、可被结算改变的状态数据。
  - 例如：`Hp`、`Shield`、`Exp`、`TargetMarbleInstId`、`IsAlive`。
- `Config`
  - 由等级配置投影而来的长期参数与加成参数。
  - 例如：`Attack`、`Defense`、`UpgradeExp`、各类加成和倍率、`Scale`、`Mass`。
- `Frame`
  - 每帧/每个 FixedUpdate 参与组合计算的临时值。
  - 例如：目标方向、目标速度、加速度、角速度及其优先级合成器。

使用原则：

- 会被伤害/治疗/升级等结果持久影响的数据写入 `State`。
- 来自配置或长期 Buff 投影的数据写入 `Config`。
- 仅用于当前帧驱动移动/转向求解的数据写入 `Frame`。

## 6. 关键流程

### 6.1 Marble 受伤流程

1. 外部调用 `IReceiveDamage.ReceiveDamage`。
2. `MarbleReceiveDamageAbility` 将请求转交给 `MarbleDamagePipelineAbility`。
3. DamagePipeline 按阶段处理：
   - `Receive`
   - `Calculate`
   - `Apply`
   - `Completed`
4. 每个阶段允许对应接口能力参与修正。
5. 最终结果写回 `State.Shield / State.Hp`。

这个流程的重点不是单个 Ability 名称，而是：

- DamagePipeline 统一持有上下文并串行处理嵌套伤害。
- 其它能力通过阶段接口插入，不直接复制整条结算链。

### 6.2 弓箭发射流程

1. `BowFireAbility` 判断武器是否可用并消耗冷却。
2. 通过 `WeaponCalculateDamageAbility` 计算本次发射的基础伤害值。
3. 根据 `ShootType` 决定是连续发射还是散射。
4. 每支箭都调用 `ProjectileFactory.CreateProjectile` 创建独立发射物。
5. 发射物再通过自身骨架能力完成飞行、命中、生命周期管理。

## 7. 阅读建议

如果你要修改 Combat：

1. 先看本文档判断改动属于 Marble / Equipment / Projectile 哪一层。
2. 再看对应 Luban schema，判断改的是固定骨架字段还是扩展能力列表。
3. 再看对应 Factory，确认能力挂载入口与装配边界。
4. 最后进入具体 Ability 和 RuntimeData。
