## Context

当前 Combat 结构中，`ASC` 负责 Ability 挂载与 `Update/FixedUpdate` 分发，`MarbleMovementAbility` 与 `MarbleRotationAbility` 作为执行器从 `RuntimeData` 的各类 `PriorityValueManager` 读取合成值并施加到刚体。业务 Ability 的职责是按帧向这些 Manager 持续提交意图值，而不是直接修改 Rigidbody。

现有代码已经存在一个简单的冷却范式：`WeaponCooldownAbility` 在 `Update` 中推进冷却剩余时间，并通过显式方法判断是否可消费冷却。但 Marble 侧还没有统一的“持续生效 + 冷却 + 不同触发节奏”的抽象，导致每个定时技能都需要各自实现内部状态机。

这次设计的重点不是改变宿主分发机制，而是在 Marble Ability 层提供一个可复用的定时生命周期基类，并允许注入不同的时序策略来支持固定时长、随机时长、手动触发和自动循环等规则。

## Goals / Non-Goals

**Goals:**
- 提供 `TimedMarbleAbility` 基类，统一定时 Ability 的生命周期接入方式。
- 提供可注入的时序策略接口，封装激活、持续、冷却与再次可触发的状态流转。
- 保持与现有 `ASC`、`MarbleMovementAbility`、`PriorityValueManager` 兼容，不破坏现有职责边界。
- 使冲刺类 Ability 可以在激活期间持续提升 `TargetVelocity` 和 `Acceleration`，结束后自动或手动进入冷却。
- 支持未来扩展为随机持续时间、随机冷却时间、手动触发、自动循环等多种时间规则。

**Non-Goals:**
- 不修改 `ASC` 的注册、排序和分发机制。
- 不改变 `PriorityValueManager` 的消费模型。
- 不在本次设计中引入通用 Buff 系统、动画状态机或网络同步机制。
- 不要求所有现有 Ability 立即迁移到 `TimedMarbleAbility`。

## Decisions

### 决策 1：在 Ability 层引入 `TimedMarbleAbility`，不把时序逻辑放进 `ASC`
- 方案选择：`TimedMarbleAbility` 继承现有 `MarbleAbility`，并实现 `IAbilityUpdate`，由其在逻辑帧中推进时序。
- 原因：`ASC` 当前职责是挂载、索引和分发，不应该感知技能的业务阶段。将定时状态机放在 Ability 侧，能保持宿主简单并符合现有组合式设计。
- 备选方案：将持续/冷却状态放入 `ASC` 的统一表中。放弃原因是会让宿主与具体技能行为耦合，破坏职责边界。

### 决策 2：使用“可查询状态 + 可选事件通知”的时序策略接口，而不是纯事件黑盒
- 方案选择：定义例如 `IAbilityTiming` 的接口，至少暴露 `Update`、`Reset`、`TryActivate`、`IsActive`、`IsCooldown`、`CanActivate` 等成员；可选提供状态切换事件供派生类在需要时监听。
- 原因：Ability 在 `FixedUpdate` 必须直接判断当前是否处于激活期，以决定是否持续提交移动意图；若仅靠事件回调，状态会隐藏在黑盒里，调试和推理都更困难。
- 备选方案：只注入一个事件型 Timer，由 Ability 注册事件并在事件中改状态。放弃原因是状态分散，容易出现注册/反注册遗漏，且不利于在任何时刻查询当前阶段。

### 决策 3：时序推进使用 `Update`，效果提交使用 `FixedUpdate`
- 方案选择：`TimedMarbleAbility.OnAbilityUpdate` 调用 timing 的 `Update(elapseSeconds)` 推进时间；需要影响刚体/移动的派生 Ability 在 `OnAbilityFixedUpdate` 中检查 `IsActive` 后持续写入 Manager。
- 原因：项目已有 `WeaponCooldownAbility` 在逻辑帧推进冷却的先例；而移动执行链是物理帧驱动，冲刺期间每个物理帧重新写入目标速度和加速度才能与 `PriorityValueManager` 的清空机制兼容。
- 备选方案：所有时间与效果都放在 `FixedUpdate`。放弃原因是会将纯时间状态机与物理效果耦合，且不利于复用到非物理类定时技能。

### 决策 4：冲刺等定时移动技能继续走 Manager 提案流，不直接操作 Rigidbody
- 方案选择：派生 Ability 在激活期内向 `TargetDirectionManager`、`TargetVelocityManager`、`AccelerationManager` 提交值，由 `MarbleMovementAbility` 执行最终施力。
- 原因：这样可以复用现有优先级与合成策略，并与其他移动类 Ability 形成一致的覆盖/叠加关系。
- 备选方案：冲刺 Ability 直接写 `Rigidbody.velocity` 或 `AddForce`。放弃原因是会绕开现有合成链路，难以与其他 Ability 协同。

### 决策 5：时序策略负责“何时激活”，Ability 负责“激活时做什么”
- 方案选择：时序策略只处理阶段流转和时间规则，如固定持续、随机冷却、自动循环或手动触发；Ability 负责记录方向、选择目标以及向 RuntimeData 提交具体数值。
- 原因：这样可以把“固定/随机/手动/自动”的变化点集中到时序策略中，而不让每个技能都复制时间计算逻辑。
- 备选方案：每个 Ability 自己写完整状态机。放弃原因是重复代码多，后续难统一扩展。

## Risks / Trade-offs

- [风险] 接口过度抽象，导致首个使用者（如冲刺 Ability）实现复杂度反而上升。→ 缓解：首版只保留最小必要接口（状态查询、更新时间、触发入口），事件回调作为可选增强而非必需能力。
- [风险] 自动循环与手动触发两种模式混在同一个时序实现里，容易产生分支膨胀。→ 缓解：将不同触发模式拆成不同 timing 实现，例如固定自动循环、固定手动触发、随机循环等。
- [风险] Ability 在激活期间依赖当前目标，若目标在持续期被切换，行为可能不符合预期。→ 缓解：允许具体 Ability 在激活开始时缓存方向或目标实例，避免时序层关心业务目标。
- [风险] `PriorityValueManager` 每帧清空，若开发者误以为激活时只需写一次值，会导致效果失效。→ 缓解：在 `TimedMarbleAbility` 的设计约束中明确：激活态仅表示“允许提交效果”，具体效果类必须在每个 `FixedUpdate` 持续提交。
