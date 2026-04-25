## Why

当前 `Marble` 战斗框架将大部分能力统一纳入逐帧 `OnUpdate` / `OnFixedUpdate` 轮询，原型阶段实现简单，但随着弹珠数量、Buff 类型和战斗状态增长，会让大量本应事件触发的能力持续空跑，抬高热更层战斗开销。
现在需要在保持现有 Ability 扩展性的前提下，明确轮询型与事件型能力边界，为后续将军技能、Buff、撤退、升级与死亡链路扩展提供更稳定的性能基础。

## What Changes

- 为 Marble Ability 框架增加能力执行分类，区分逐帧更新、物理帧更新与纯事件驱动能力。
- 调整宿主 `ASC<T>` 的能力管理方式，仅遍历声明需要 `Update` / `FixedUpdate` 的能力集合。
- 将明显不适合常驻轮询的战斗能力（如升级检测、死亡判定、伤害结算）转为由状态变更或统一结算入口触发。
- 为 Marble 战斗层补充统一的状态变更/结算触发约定，减少空跑并为后续 Buff 与技能系统扩展预留接入点。
- 保持现有 Marble 业务语义不变，不引入面向玩家的规则变更。

## Capabilities

### New Capabilities
- `marble-ability-execution-model`: 规范 Marble Ability 的执行模型，定义轮询型、物理型、事件型能力的职责边界与触发方式。

### Modified Capabilities
- `entity-lifecycle-module`: 扩展实体生命周期模块在战斗实体上的能力调度要求，使实体宿主支持按执行类型分流能力更新。

## Impact

- 影响代码：`Assets/GameScripts/HotFix/GameLogic/GamePlay/Marble/` 下的 `ASC`、`Ability`、`Marble` 相关能力类与工厂初始化逻辑。
- 影响系统：热更层战斗循环、伤害/死亡/升级结算链路、后续 Buff 与将军技能接入方式。
- 影响风险：需要确保重构后不改变现有 Marble 行为结果，并注意事件驱动能力的调用时序一致性。
