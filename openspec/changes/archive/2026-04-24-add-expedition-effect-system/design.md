## Context

远征配置化已经完成了主流程、事件和 Combat 遭遇的 Luban 接入，但当前事件选项和 Combat 奖励仍然停留在固定字段模式：事件选项依赖 `crystal_delta`、`exp_delta`、`hp_delta`，Combat 遭遇依赖固定胜利奖励字段。这种结构虽然适合最小版本，但一旦远征需要支持更多类型的结果变化，就必须不断往同一个配置 bean 上追加字段，既影响可读性，也会让运行时代码持续堆积固定分支。

这次变更的目标是把远征结果应用升级为配置驱动的 Expedition Effect 体系，让事件选项和 Combat 胜负都可以通过 `LstEffect` 组合多个效果。同时，用户已明确要求代码中的资源命名统一使用 `money`，玩家可见文案仍可继续叫“晶体”。这意味着本次设计既涉及 Luban schema 调整，也涉及远征运行时代码的结果应用路径与命名收敛。

## Goals / Non-Goals

**Goals:**
- 引入 `IExpeditionEffect` 接口与统一的 Expedition Effect 执行上下文。
- 让 Event Option 与 Combat 胜负奖励都通过 `LstEffect` 配置表达，而不是固定数值字段。
- 首批支持添加金钱、为玩家全队添加经验、为玩家全队修改血量这 3 种 Effect。
- 将远征内部资源命名从 `crystal` 统一为 `money`，提高代码可读性。
- 让远征运行时代码通过 Effect 工厂和执行路径应用结果，而不是在控制器中手写固定字段处理逻辑。

**Non-Goals:**
- 不在这次变更中设计全游戏通用的 `IGameEffect` 体系。
- 不在这次变更中扩展家园、战斗主流程或局外成长系统的效果架构。
- 不在这次变更中引入复杂的目标筛选语言、条件表达式或脚本化效果。
- 不在这次变更中改变 Luban 的 JSON/bytes 生成策略。

## Decisions

### 1. Effect 体系限定在远征域，不提前泛化到全游戏
- 选择：使用 `IExpeditionEffect`，而不是 `IGameEffect`。
- 原因：当前问题只发生在远征事件与 Combat 奖励结果应用上，直接做成全游戏接口会导致上下文过度泛化，反而更难维护。
- 备选方案：
  - `IGameEffect`：理论上更通用，但会很快演变成携带大量无关字段的大上下文。

### 2. Execute 统一接收一个 ExecutionContext
- 选择：所有 Expedition Effect 的执行入口统一为 `Execute(ExpeditionEffectExecutionContext context)`。
- 原因：这与项目中 `ProjectileHitContext` 的模式一致，能够避免为每一种 Effect 设计一套不同参数签名，同时也比把各种 manager 直接传进 `Execute` 更稳定。
- 备选方案：
  - 为每种 Effect 设计不同参数：接口无法统一。
  - 在 Effect 中直接访问全局 manager：耦合度高，测试和推理都困难。

### 3. Effect 直接操作远征领域状态，而不是场景对象或 UI 系统
- 选择：Effect 的主要读写对象是 `ExpeditionRunState`、`ExpeditionPersistentDataStore`、当前节点记录以及玩家 Marble 快照。
- 原因：远征事件和 Combat 奖励的本质是改变远征与局外状态，而不是直接操作战斗场景里的实体对象或展示层。
- 备选方案：
  - 直接操作 manager、UI 或场景 Marble：职责边界会变得混乱。

### 4. Event Option 与 Combat Encounter 都改用 Effect 列表
- 选择：
  - Event Option 使用 `LstEffect`
  - Combat Encounter 使用 `LstVictoryEffect` 与 `LstDefeatEffect`
- 原因：事件选项和 Combat 结果都属于“节点结算触发一组结果”的模式，统一为 Effect 列表后，运行时处理链会更一致。
- 备选方案：
  - 保留固定字段并少量追加：短期简单，但会继续扩散字段爆炸问题。

### 5. 首批 Effect 保持最小集合
- 选择：第一批仅实现 `AddMoneyEffect`、`AddPlayerMarbleExpEffect`、`AddPlayerMarbleHpEffect`。
- 原因：这三个效果刚好覆盖当前已有固定字段的语义，是最小可替换集合，能够先把架构跑顺。
- 备选方案：
  - 一次性做大量效果：会让 schema 和工厂复杂度过快膨胀。

### 6. 代码内部统一使用 money 命名，展示文案仍允许使用晶体
- 选择：远征运行时与配置字段在代码内部统一使用 `money` 命名，例如 `MoneyDelta`、`AddMoneyEffect`。
- 原因：用户明确说明“晶体”本质上就是钱，内部命名使用 `money` 更有利于阅读和长期维护。
- 备选方案：
  - 继续混用 `crystal` 和 `money`：会持续制造理解成本。

## Risks / Trade-offs

- [Risk] 从固定字段迁到 Effect 列表后，配置 authoring 的理解成本会上升  
  → Mitigation：首批 Effect 数量严格控制在 3 个，并在 schema 命名上保持自解释。

- [Risk] `crystal -> money` 的命名收敛会影响现有代码与 UI 摘要文本  
  → Mitigation：明确区分“内部命名”和“玩家可见文案”，运行时变量统一为 `money`，显示层文案可继续使用“晶体”。

- [Risk] 如果 ExecutionContext 承担过多职责，后续仍可能膨胀  
  → Mitigation：只放远征领域需要的状态和少量必要服务，不向其中堆砌所有系统 manager。

- [Risk] Event 与 Combat 同时切换到 Effect 列表会涉及多处运行时代码调整  
  → Mitigation：保持事件与 Combat 的处理链结构一致，通过工厂和统一执行入口减少分支扩散。

## Migration Plan

1. 为远征新增 Expedition Effect 相关 Luban schema 和运行时接口。
2. 将 Event Option 的固定效果字段替换为 `LstEffect`。
3. 将 Combat Encounter 的固定奖励字段替换为 `LstVictoryEffect` 与 `LstDefeatEffect`。
4. 在运行时代码中加入 Effect 配置到运行时 Effect 的工厂映射与执行上下文。
5. 用首批 3 个基础 Effect 替换当前固定字段逻辑。
6. 将远征相关内部资源命名统一收敛为 `money`，并验证最小远征流程仍可正确结算。

## Open Questions

- 当前没有额外必须先决的问题。未来如果远征需要支持更复杂的目标筛选、条件触发或跨系统效果，再考虑在 `IExpeditionEffect` 之上继续分层，而不是在本次变更中提前泛化。
