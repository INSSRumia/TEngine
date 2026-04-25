## Context

当前战斗域 Factory 的主要职责已经包含对象创建、宿主初始化、默认骨架能力挂载、配置能力分发与扩展 creator 调度，但在 `RuntimeData` 和 `Ability` 的实例化环节仍然直接展开大量配置字段。典型问题包括：

- `EquipmentFactory` 直接读取 `config.BowFire.*`、`config.BowAim.*`、`config.Cooldown.*` 等多个字段并拼接成长参数构造；
- `ProjectileFactory` 直接把 `MoveAbility`、`DamageAbility`、`Lifetime` 下的字段拆开传给多个核心能力；
- `MarbleFactory` 在创建 `MarbleRuntimeData` 时直接把等级配置字段一项项映射到运行态数据。

这会让 Factory 持续承担“理解字段语义 + 决定如何转存”的职责，导致工厂类体积膨胀，也使配置字段调整时需要同时修改 schema、Factory、Ability/RuntimeData 三层代码。

本次设计要把“读取 config 并提取自身运行字段”的职责收回到 `RuntimeData` 与 `Ability` 自身，同时为所有可装配 Ability（包括当前无初始化参数的固定骨架能力）建立稳定配置载体，形成统一构造契约。

## Goals / Non-Goals

**Goals:**
- 让 `RuntimeData` 和 `Ability` 构造函数优先接收对应配置对象，而不是一组离散标量参数。
- 降低 Factory 直接访问配置字段的面积，让 Factory 只保留装配流程与少量宿主上下文拼装职责。
- 为所有可装配 Ability 补齐配置类，保证每个能力都有独立配置载体。
- 统一 Creator/Factory 的配置消费模式，使新增配置字段时优先改能力/运行态自身，不再首先膨胀 Factory。
- 为后续继续做装配收敛、自动化 creator 迁移与配置演化提供稳定模式。

**Non-Goals:**
- 不在本次设计里移除现有 Factory 的 creator 注册扩展点。
- 不强制所有运行时上下文都塞进 Luban 配置；例如宿主引用、source marble、slot 等上下文仍可由 Factory 单独传入。
- 不在本次设计里重做所有 Combat 类的继承层次。
- 不要求一步到位重做所有 Combat 类继承结构；但要求所有被装配 Ability 都纳入统一配置契约。

## Decisions

### 1. 使用“配置对象 + 最小上下文参数”的构造模式
- 选择：`RuntimeData` / `Ability` 构造函数采用“一个 config 对象 + 少量非配置上下文参数”的模式。
- 原因：完全只传 config 不现实，因为部分数据来自运行时上下文而不是 Luban 表，例如宿主实例、source marble、目标对象、slot、伤害结算结果等；但配置字段必须由实例自身读取。
- 约束：
  - config 内可表达的字段不得再在 Factory 中展开为多个标量参数；
  - 非配置上下文参数必须保持最小集合，并具有明确来源。
- 备选方案：
  - 强制所有参数都包装进超级 Context：统一性高，但会引入过度泛化的上下文对象。
  - 继续沿用多参数构造：实现最省事，但无法解决工厂膨胀问题。

### 2. 所有可装配 Ability 都必须拥有显式配置类，并统一由 Luban XML 定义生成
- 选择：无论当前是否有初始化参数，只要 Ability 会被 Factory 或 creator 装配，就必须拥有对应 XML 配置定义，并由 Luban 生成配置类。
- 原因：如果保留“部分能力无配置类”的例外，Factory 与 Ability 构造契约仍会长期分裂；为所有可装配 Ability 提供统一配置载体后，即使未来新增字段，也不需要再次做 schema 补齐与构造迁移。
- 约束：
  - 固定骨架能力也必须拥有显式主骨架配置字段或显式子 bean；
  - 当前没有字段的能力允许使用空配置 bean，但不得省略配置类型本身；
  - 不再保留“纯流程能力可无配置类”的例外。
- 备选方案：
  - 仅为有参数能力补配置类：短期改动较小，但会保留双轨体系；
  - 继续让固定骨架能力硬编码创建：无法真正统一构造契约。

### 3. RuntimeData 的 config 消费保持“运行态对象自己拉平字段”
- 选择：`MarbleRuntimeData`、`EquipmentRuntimeData`、`ProjectileRuntimeData` 等运行态对象在构造函数中读取 config 并完成字段初始化，而不是让 Factory 先拉平成临时参数再传入。
- 原因：运行态字段的归属本来就在 RuntimeData 内部，让 RuntimeData 自己决定从哪个 config 字段派生哪些运行态初值更合理。
- 约束：
  - Factory 仍可负责补充 config 之外的上下文字段（如 camp、source marble id、spawn position）；
  - RuntimeData 构造函数应避免反向依赖宿主组件，保持纯数据初始化。
- 备选方案：
  - 引入独立 Mapper 层做 config → runtime：解耦更强，但会新增一层样板代码，当前收益不足。

### 4. Creator 接口签名同步收敛到“传 config，不传字段”
- 选择：各类 `CreatorForConfig`、默认 Factory 分支和固定骨架挂载逻辑统一改为直接传配置对象给目标实例。
- 原因：如果仅修改 Ability 构造函数而 creator 仍在中间拆字段，Factory 膨胀问题只会转移位置，不会消失。
- 约束：
  - Creator 只负责识别配置类型、决定创建哪种对象以及补齐少量上下文；
  - 不负责解释一长串配置字段语义。
- 备选方案：
  - 仅修改 Factory，不修改 creator：会导致扩展点与默认路径风格割裂。

### 5. 分阶段迁移，优先覆盖当前最膨胀的构造点
- 选择：按 `Equipment` → `Projectile` → `MarbleRuntimeData / MarbleAbility` 的顺序推进，优先迁移当前已知长参数构造最密集的位置。
- 原因：这些位置最直接暴露 `config.xxx` 字段细节，也是工厂代码增长最快的来源。
- 备选方案：
  - 一次性全量迁移所有能力：风险高，回归面大；
  - 只改一个工厂做试点：价值不足，不能形成统一契约。

## Risks / Trade-offs

- [Risk] 配置类数量增加，可能带来 schema 与生成代码膨胀  
  → Mitigation：接受“所有可装配 Ability 都有配置类”的一致性成本，但通过清晰命名与分文件组织控制维护复杂度；空配置 bean 仅承载统一契约，不承载额外行为复杂性。

- [Risk] 构造函数签名调整会波及大量调用点  
  → Mitigation：以 Factory / creator 为唯一收口点分批迁移，并在每步迁移后运行生成与编译验证。

- [Risk] 部分能力既依赖 config 又依赖 runtime context，边界容易再次模糊  
  → Mitigation：明确约束“config 字段由实例内部读取，runtime context 由 Factory 单独传入”，避免混传离散 config 字段。

- [Risk] 旧配置路径与新配置路径短期并存，可能造成风格不统一  
  → Mitigation：在任务中要求按模块成片迁移，不保留同一工厂内新旧两种主流写法长期共存。

- [Risk] 固定骨架能力补配置后，可能诱导未来把所有逻辑都配置化  
  → Mitigation：明确本次目标是“所有被装配 Ability 都拥有配置载体”，不是“把所有运行逻辑都外置成可自由编排配置”。
