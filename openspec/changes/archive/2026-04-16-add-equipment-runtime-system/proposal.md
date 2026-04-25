## Why

当前 `Marble` 已经具备运行时数据、宿主和 Ability 的基础战斗结构，但装备仍停留在配置层，无法承接护具吸收伤害、武器碰撞伤害以及弓类装备自动瞄准和射击这类独立行为。为了做出最小可玩的战斗 Demo，需要把 `marble.xml` 中定义的装备配置真正落成运行时系统，并保持数据层尽量纯净、不存对象引用。

## What Changes

- 新增装备运行时系统，使 `MarbleLevelConfig.lst_equipment_id` 能在创建 Marble 时生成实际装备对象。
- 新增 `Equipment` 宿主、`EquipmentRuntimeData` 及其子类型，约束 `Owner` 放在宿主类中而不是数据类中。
- 为防具、普通武器和弓类武器建立分层 Ability 结构，支持静态挂载、减伤/吸伤、碰撞伤害、自动瞄准与射击。
- 为装备系统补充工厂与槽位挂载规则，使装备能依附 Marble 生命周期和位置同步。
- 扩展 Marble 战斗链路，使装备可以参与伤害结算和行为驱动。

## Capabilities

### New Capabilities
- `equipment-runtime-system`: 定义装备从配置到运行时对象、宿主关系、槽位挂载、分类行为与生命周期的完整规则。

### Modified Capabilities
- `marble-ability-execution-model`: 扩展 Marble 侧能力模型，使 Marble 可装配并驱动附属 Equipment 宿主与装备 Ability。

## Impact

- 影响代码：`Assets/GameScripts/HotFix/GameLogic/GamePlay/Marble/` 及新增的装备相关目录、运行时数据、工厂和 Ability 类。
- 影响配置：依赖 `Configs/GameConfig/Defines/marble.xml` 中的 `EquipmentConfig / ArmorConfig / WeaponConfig / BowConfig` 结构。
- 影响系统：Marble 创建流程、伤害结算链、战斗驱动、未来的投射物与索敌逻辑。
