## Context

当前项目已经在 `GamePlay` 下形成 `ASC + Ability + RuntimeData + Factory` 的战斗雏形，并且 Marble 已具备受伤、治疗、死亡、移动、旋转等初步能力。但现有实现仍以直接字段写入与即时结算为主，缺少统一的单次结算上下文、阶段化能力分发和递归保护机制。用户已明确七项约束：一是希望控制类型数量，因此可接受在 RuntimeData 之外只增加少量轻量 Context；二是护盾规则定义为“只要有盾，本次命中至少被盾挡一次，不穿透到生命”；三是战斗主链中的 After 回调主要关注避免递归死循环，并倾向通过对 Ability 进行直接遍历完成阶段回调；四是 `ValueAddition`、`ValueMultiplier` 一类长期存在的属性修正未来可能由 Buff 在 `OnAdd/OnRemove` 生命周期中维护，因此这些值不能仅存于 Context 中；五是 Marble 本身不应承载伤害、治疗、护盾等 Ability 业务逻辑，Marble 只负责组件引用与宿主承载，所有战斗流程应下沉到 Ability 和数据层；六是轮询型 Ability 不再使用 `AbilityExecutionMode`，而是通过 `IAbilityUpdate`、`IAbilityFixedUpdate` 接口声明自己是否参与对应阶段；七是 Context 与延后队列属于各自的 PipelineAbility，而不是宿主 ASC。

当前仓库已有 `marble-ability-execution-model` 能力规范，要求宿主仅调度订阅对应阶段的 Ability；本次变更需要在此基础上把显式结算阶段纳入宿主分发模型。该设计属于跨 `Common` 基础设施与 Marble 战斗能力链的横切变更，且对未来索敌、行为、投射物、Buff/Tag 有承接作用，因此需要先固化技术设计再编码。

## Goals / Non-Goals

**Goals:**
- 在不推翻现有 ASC 主体结构的前提下，为伤害、治疗、护盾建立统一的核心结算管线。
- 引入轻量级结算上下文，承载单次结算的临时数据与阶段状态，同时保持 RuntimeData 只存常驻状态。
- 明确区分“常驻属性修正”和“单次结算临时值”：Buff、装备、被动提供的 Addition/Multiplier 持续存放在 RuntimeData 或后续属性层，由结算阶段只读使用。
- 让宿主通过接口缓存分发 Update、FixedUpdate 及显式战斗阶段，只遍历真正订阅对应阶段的 Ability。
- 让 Marble 只承担宿主与组件引用职责，把伤害、治疗、护盾的阶段化业务逻辑下沉到 Ability。
- 让 Damage、Heal、Shield 各自拥有独立的 PipelineAbility、Context 与延后队列，避免不必要的总管线耦合。
- 为链路增加重入保护，避免单 Pipeline 内部递归型死循环。
- 明确保留“护盾至少抵挡一次命中”的战斗规则，并在实现中统一应用。

**Non-Goals:**
- 不在本次变更中引入完整 Buff/Tag 系统，也不依赖第三方树状 Tag 库完成主链实现。
- 不在本次变更中完成索敌、行为逻辑、投射物或弓箭系统闭环。
- 不重做现有 EntityModule 或更大范围的战斗管理器架构。
- 不把所有战斗临时对象拆成大量独立类型，以免偏离当前性能与复杂度目标。

## Decisions

### 1. 采用“常驻 RuntimeData + Pipeline 私有 Context”的双层数据模型
- 决策：常驻属性继续保存在 RuntimeData；单次伤害、治疗、护盾流程分别由各自 PipelineAbility 的嵌套 Context 承载临时值。
- 原因：这满足用户对类型数量的控制，同时避免把 `FinalDamage`、阶段标记等一次性数据长期塞进 RuntimeData 顶层，并且让 Context 的所有权明确归属于具体能力。
- 备选方案：
  - 方案 A：全部继续写回 RuntimeData。缺点是临时中间值污染常驻状态，后续扩展难维护。
  - 方案 B：在 Common 层定义统一 `CombatContext`。会让 ASC/Common 再次持有具体战斗业务状态，不符合当前分层目标。
- 结论：优先实现 Damage/Heal/Shield 三个私有 Context，并作为各自 PipelineAbility 的嵌套类维护。

### 2. Addition/Multiplier 等常驻修正留在 RuntimeData 或属性层，不放入 Context 持久化
- 决策：`DamageAddition`、`DamageMultiplier`、`HealAddition`、`HealMultiplier`、`ShieldAddition`、`ShieldMultiplier` 一类长期生效的修正值继续保存在 RuntimeData 或后续专门属性层中，由 Buff/装备/被动通过 `OnAdd/OnRemove` 生命周期维护；Pipeline 私有 Context 只保存本次结算的原始输入、最终结果和阶段状态。
- 原因：用户已明确这些值未来可能由 Buff 提供，若只存在于 Context 中会在单次结算结束后丢失，无法表达长期属性效果。
- 结论：本次先采用 RuntimeData 承载常驻修正、各 Pipeline 私有 Context 承载单次事务值的边界；以后若引入属性模块，可平滑替换 RuntimeData 中对应字段的来源。

### 3. Context 与延后队列由各自 PipelineAbility 自行持有，并复用 TEngine MemoryPool
- 决策：`DamageContext`、`HealContext`、`ShieldContext` 不存入 RuntimeData，也不挂到 ASC；它们由对应的 PipelineAbility 短暂持有，并各自维护 `_isProcessing` 与 `Queue<TContext>` 实现方案 C 的重入保护。上下文实例统一复用 `TEngine.MemoryPool`。
- 原因：用户明确要求这些数据属于 Ability，而不是宿主；方案 C 也要求每种流程单独布尔锁与队列，逻辑更直白。
- 结论：每个 PipelineAbility 自己实现“正在处理时入队，处理完成后 drain 队列”的模式。

### 4. 宿主按接口维护轮询能力缓存与事件能力缓存
- 决策：ASC 保留总 Ability 容器用于生命周期管理，并在注册/移除时基于接口实现维护分类缓存。轮询阶段使用 `IAbilityUpdate`、`IAbilityFixedUpdate` 接口声明能力是否参与 Update/FixedUpdate。
- 原因：用户要求将轮询能力声明方式改为接口继承，这能让分发模型与其他战斗接口保持一致，也避免与项目其他系统的 `IUpdate/IFixedUpdate` 冲突。
- 结论：采用“总列表 + 接口分类缓存”的模式，轮询与事件分发统一走接口订阅模型。

### 5. After 接口不直接传 Context，而是传入对应 PipelineAbility
- 决策：`IAfterReceiveDamage / IAfterCalculateDamage / IAfterApplyDamage` 等接口方法统一传入 `IAbility`，调用方约定传入当前对应的 PipelineAbility；后续 Ability 自行转型为具体 PipelineAbility，并从其 `CurrentContext` 读取数据。
- 原因：用户明确要求 After 接口不要直接暴露 Context，而是通过 PipelineAbility 间接访问，以保持接口层更稳定。
- 结论：PipelineAbility 在执行阶段回调时传 `this`，处理方再转型取 `CurrentContext`。

### 6. Damage / Heal / Shield 各自独立 PipelineAbility
- 决策：不再保留总的 `MarbleCombatPipelineAbility`，改为 `MarbleDamagePipelineAbility`、`MarbleHealPipelineAbility`、`MarbleShieldPipelineAbility` 三个独立能力。
- 原因：用户明确不希望把三类业务揉在一个 Ability 里；拆开后职责更清晰，Context/Queue/重入逻辑也能完全独立。
- 结论：入口 Ability 分别转发到对应 Pipeline，应用型 Ability 只关注各自阶段效果。

### 7. 护盾规则按“至少挡一次命中”实现，并视为规则而非传统数值穿透盾
- 决策：当应用伤害阶段开始时护盾大于零，则该次命中只扣减护盾，不穿透生命值，即使最终伤害大于当前护盾值。
- 原因：用户已明确该行为是设计意图，不是 bug。
- 结论：在实现中继续保留该规则。

## Risks / Trade-offs

- [Risk] 三套 Pipeline 各自维护 Context 与队列，存在重复代码 → Mitigation：先优先保证职责清晰，后续如确有必要再抽取局部公共模板。
- [Risk] 常驻修正继续放在 RuntimeData，后续属性系统接入时需要迁移 → Mitigation：先把字段语义与读取入口固定好，未来只替换字段来源而不改结算阶段契约。
- [Risk] Ability 分类缓存与总列表排序不一致 → Mitigation：统一由宿主在注册/移除后按相同优先级规则刷新各类缓存。
- [Risk] 护盾语义与常见游戏认知不一致 → Mitigation：在 spec 和代码命名中明确这是规则定义，不按传统穿透吸收逻辑处理。
- [Risk] 轮询接口改为 `IAbilityUpdate` / `IAbilityFixedUpdate` 后，需要同步调整所有相关 Ability → Mitigation：已全局替换 `GamePlay` 范围内相关实现并通过编译验证。

## Migration Plan

- 第一步：新增或重构 Common 层中的接口缓存与轮询基础设施，去掉 ASC 对具体战斗 Context/队列的了解。
- 第二步：在 RuntimeData 中补齐战斗常驻修正字段或统一读取入口，使 Buff/装备提供的 Addition/Multiplier 能被各 Pipeline 的 Calculate 阶段稳定读取。
- 第三步：分别实现 Damage/Heal/Shield 三个 PipelineAbility，并把 Context、重入锁、延后队列下沉到各自能力内部。
- 第四步：将 After 接口切换为传入 PipelineAbility，再由处理方从 `CurrentContext` 读取当前上下文。
- 第五步：验证 Marble 本体只保留组件与挂点职责，非结算类 Ability 不受影响。
- 回滚策略：若拆分后的三 Pipeline 方案不稳定，可先回退到旧的直接结算实现，但保留接口缓存与轮询接口基础设施。

## Open Questions

- 是否在本次变更中就为 RuntimeData 补齐全部 Addition/Multiplier 字段，还是先只实现伤害链相关字段并预留治疗/护盾扩展位？
- 后续 Buff 系统接入时，是否需要再为三条 Pipeline 提供统一的修正器接口，而不是直接读取 RuntimeData 字段？
