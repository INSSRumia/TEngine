## Why

当前 GamePlay 目录已经具备 ASC、RuntimeData、Ability 和部分受伤/治疗/移动实现，但战斗核心结算仍停留在分散的即时处理阶段，缺少统一的结算上下文、阶段化触发约束和可扩展的能力分发骨架。现在补齐战斗核心框架，可以在不推翻现有 Marble 与 Equipment 代码的前提下，为后续索敌、行为、投射物与 Buff 系统提供稳定基础，并提前厘清常驻属性与单次结算数据的边界。

## What Changes

- 为战斗系统新增统一的核心结算框架，覆盖伤害、治疗、护盾三类离散结算流程。
- 引入轻量级战斗结算上下文，用于承载单次结算过程中的基础值、最终值、来源与目标信息，不承载 Buff 提供的常驻属性修正。
- 明确常驻属性修正（如 Addition/Multiplier）保存在 RuntimeData 或后续属性层，由 Buff 在 OnAdd/OnRemove 生命周期中维护。
- 扩展 Ability 宿主的分类缓存与事件型分发能力，使宿主只遍历订阅指定阶段的 Ability 集合。
- 为战斗结算链增加阶段重入保护，避免 IAfterReceiveDamage 等回调触发递归型死循环。
- 保持现有“护盾至少抵挡一次伤害”的规则，并将其显式收敛为规范化行为。
- 修改 Marble 能力执行模型，使其同时覆盖事件型结算阶段的显式调度约束。

## Capabilities

### New Capabilities
- `combat-core-pipeline`: 定义伤害、治疗、护盾的统一结算上下文、常驻属性与临时数据边界、阶段分发与重入保护行为。

### Modified Capabilities
- `marble-ability-execution-model`: 扩展宿主能力分发模型，支持按事件阶段缓存与调度 Ability，而不仅限于 Update/FixedUpdate。

## Impact

- 影响代码范围：`Assets/GameScripts/HotFix/GameLogic/GamePlay/Common/`、`Marble/Ability/`、`MarbleRuntimeData` 及后续战斗相关 Ability。
- 影响能力接口：需要新增若干结算阶段接口与上下文传递约定。
- 对现有业务影响：Marble 的受伤/治疗/护盾逻辑将从直接字段结算迁移到统一流程，但 Buff 或常驻加成的存储位置将保持在 RuntimeData/属性层而不是 Context 中。
- 后续可承接系统：索敌、行为逻辑、远程投射物、Buff/Tag。
