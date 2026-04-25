## Why

当前 Marble Ability 已具备按帧提交移动/旋转意图的执行模型，但缺少一个统一的“持续生效 + 冷却 + 触发策略”抽象，导致冲刺、周期性加速、随机触发增益等能力只能各自重复实现状态机。现在补齐该抽象，可以在不破坏现有 ASC 与 Movement 执行职责的前提下，支持更多定时类 Ability 的复用开发。

## What Changes

- 新增 `TimedMarbleAbility` 抽象基类，用于承载 Marble Ability 的定时生命周期。
- 新增可注入的能力时序策略接口，用于描述固定时长、固定冷却、随机时长、随机冷却、手动触发、自动循环等不同时间逻辑。
- 约定 `TimedMarbleAbility` 在 `Update` 推进时序策略，在 `FixedUpdate` 中依据当前激活状态持续向 RuntimeData 的各类 Manager 提交意图值。
- 为冲刺类 Ability 提供标准接入方式：在激活期内提高 `TargetVelocity` 与 `Acceleration`，结束后进入冷却。
- 明确时序策略与业务行为的职责边界，避免将技能状态机塞入 `ASC` 或核心移动执行 Ability。

## Capabilities

### New Capabilities
- `timed-marble-ability`: 为 Marble Ability 提供统一的定时生命周期与可插拔时序策略，支持持续生效、冷却、自动循环、手动触发以及随机时间规则。

### Modified Capabilities
- `marble-ability-execution-model`: 扩展现有 Marble Ability 执行模型的要求，允许 Ability 通过定时生命周期在激活期间持续提交意图值，并与现有 Movement/Rotation 执行链协同工作。

## Impact

- 影响代码目录：`Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Marble/Ability/`
- 影响现有 Marble Ability 扩展方式，后续定时类技能将基于统一基类与时序接口开发。
- 与现有 `MarbleMovementAbility`、`PriorityValueManager`、`WeaponCooldownAbility` 的模式保持兼容，不需要修改 `ASC` 核心分发职责。
- 可能新增测试用例，覆盖激活、持续、冷却与不同触发策略的行为。
