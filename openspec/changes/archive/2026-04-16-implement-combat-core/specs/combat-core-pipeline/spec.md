## ADDED Requirements

### Requirement: Combat resolution SHALL use an explicit per-action context
战斗核心结算 MUST 为每一次伤害、治疗或护盾处理创建或复用一个显式上下文对象，用于承载本次结算所需的原始输入值、最终结果、来源、目标与阶段状态。运行时常驻属性 MUST 继续保存在 RuntimeData 或后续属性层中，不得将临时结算中间值长期写回常驻数据结构。

#### Scenario: Damage uses transient context
- **WHEN** Marble 或 Equipment 接收到一次新的伤害请求
- **THEN** 系统必须通过本次伤害的结算上下文完成原始输入到最终值的计算，并在流程结束后清理或复用该上下文，而不是把全部中间值散落存放在 RuntimeData 顶层字段中

#### Scenario: Runtime data keeps persistent state only
- **WHEN** 一次伤害、治疗或护盾结算完成
- **THEN** RuntimeData 仅保留生命、护盾、防御、长期有效的 Addition/Multiplier 等常驻状态，本次结算的阶段标记与临时结果不得继续作为持久状态残留

### Requirement: Persistent modifiers SHALL remain outside per-action context
由 Buff、装备、被动或其他持续效果提供的加成值与倍率值 MUST 保存在宿主的常驻属性层中，并在结算阶段被只读使用；单次结算上下文 MUST NOT 成为这些长期修正的唯一存储位置。

#### Scenario: Buff-added damage multiplier persists across multiple hits
- **WHEN** 某个 Buff 在 OnAdd 生命周期中提升宿主的伤害倍率
- **THEN** 该倍率修正必须在后续多次伤害结算中持续生效，直到 Buff OnRemove 时被移除，而不是仅在某一个 CombatContext 生命周期内存在

#### Scenario: Calculate stage reads persistent modifiers from runtime state
- **WHEN** 宿主进入伤害、治疗或护盾的 Calculate 阶段
- **THEN** 系统必须从 RuntimeData 或等价的常驻属性层读取 Addition/Multiplier 等长期修正，并将结果应用到当前上下文的最终值计算中

### Requirement: Combat context SHALL be host-owned transient state
CombatContext MUST 由宿主 ASC 在单次结算期间短暂持有，并在流程结束后清理、复用或回收到宿主内部缓存。CombatContext MUST NOT 作为 RuntimeData 的长期字段存在。

#### Scenario: Host stores current context during resolution
- **WHEN** 宿主开始处理一次新的伤害、治疗或护盾请求
- **THEN** 宿主必须在自身调度状态中持有当前 CombatContext，并用它驱动本次阶段化结算

#### Scenario: Host releases context after completion
- **WHEN** 当前结算链完成且无待处理后续请求
- **THEN** 宿主必须清空或回收当前 CombatContext，使其不再作为该实体的常驻状态长期保留

### Requirement: Combat pipeline SHALL separate receive, calculate, and apply stages
战斗核心 MUST 将伤害、治疗和护盾结算拆分为显式的接收、计算和应用阶段。每个阶段 MUST 允许宿主仅分发订阅该阶段的 Ability，并以确定顺序完成本次结算。

#### Scenario: Damage pipeline dispatches stage abilities in order
- **WHEN** 宿主处理一次伤害结算
- **THEN** 系统必须先执行受击阶段 Ability，再执行计算阶段 Ability，最后执行应用阶段 Ability，且后续阶段只能读取前一阶段已确认的结算结果

#### Scenario: Heal pipeline remains explicit instead of frame-polled
- **WHEN** 宿主处理一次治疗请求
- **THEN** 治疗链路必须通过显式阶段调度在该次请求内完成，而不是依赖未来某一帧的统一轮询被动触发

### Requirement: Shield SHALL block one incoming resolved hit when shield is present
当目标在本次伤害应用阶段开始时拥有大于零的护盾值，系统 MUST 使护盾至少抵挡这一次已结算伤害命中，且本次命中不得再穿透到生命值。护盾数值 MUST 依据本次命中的最终伤害进行扣减并保持不小于零。

#### Scenario: Shield blocks oversized hit once
- **WHEN** 目标拥有护盾且本次最终伤害大于当前护盾值
- **THEN** 本次命中只会扣减护盾而不会扣减生命值，且护盾值被扣减到不小于零的结果

#### Scenario: No shield allows hp damage
- **WHEN** 目标在本次伤害应用阶段开始时护盾值为零
- **THEN** 本次最终伤害必须直接作用于生命值，并继续参与后续死亡判定

### Requirement: Combat stage dispatch SHALL prevent recursive re-entry loops
战斗核心 MUST 对同一宿主上的结算阶段分发提供重入保护。任何 IAfterReceiveDamage、IAfterCalculateDamage、IAfterApplyDamage 或对应治疗/护盾阶段 Ability 在当前阶段执行期间，不得导致同阶段无限递归重入；若产生新的结算请求，系统 MUST 以受控方式拒绝、延后或排队处理。

#### Scenario: After-receive ability cannot recursively reopen same stage
- **WHEN** 某个 AfterReceiveDamage Ability 在处理当前伤害时再次触发同一宿主的 ReceiveDamage
- **THEN** 系统必须阻止同阶段立即递归展开，避免形成无限嵌套调用链

#### Scenario: Deferred follow-up action runs after current pipeline completes
- **WHEN** 某个阶段 Ability 产生新的后续结算请求且策略允许延后执行
- **THEN** 新请求只能在当前结算链完成后的安全时机被处理，不得破坏当前阶段的顺序与一致性
