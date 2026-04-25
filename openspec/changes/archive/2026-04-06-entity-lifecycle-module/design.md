## Context

当前 HotFix 侧实体逻辑分散在多个 `MonoBehaviour` 的 `Awake/Start/Update` 中，跨对象依赖通过执行时机隐式建立，导致初始化竞态和偶发空引用问题。TEngine 已提供 `Procedure` 与 `Module` 的可控生命周期主链，适合承载统一调度。

本次变更在不破坏 Unity 对象模型的前提下，引入 `EntityModule + Entity` 组合：`Entity` 继续作为场景组件存在，但业务生命周期由 `EntityModule` 统一驱动，实现顺序可控、行为可测、退出可清理。

## Goals / Non-Goals

**Goals:**
- 建立实体统一调度机制，消除对 `Awake/Start` 执行顺序的业务依赖。
- 支持基于 `Priority` 的稳定执行顺序，并保证同优先级下顺序可预测。
- 定义实体生命周期阶段（初始化、启动、逐帧更新、销毁）并由 `EntityModule` 集中调用。
- 明确 Unity 生命周期与自定义生命周期边界：Unity 仅桥接注册/解绑。
- 提供安全遍历策略（遍历期间增删、失效对象过滤、模块关闭统一回收）。

**Non-Goals:**
- 不替代 TEngine 的 `Procedure` 初始化主流程。
- 不引入 ECS/DOTS 或大规模数据驱动改造。
- 不强制一次性迁移全部现有 `MonoBehaviour` 逻辑。
- 不修改 Unity 全局 Script Execution Order 配置。

## Decisions

1. **新增 `EntityModule` 作为实体生命周期调度中心**
   - 决策：由模块维护托管实体集合，统一执行 `OnEntityInit/OnEntityStart/OnEntityUpdate/OnEntityLateUpdate/OnEntityShutdown`。
   - 原因：模块生命周期由框架控制，天然具备可控初始化与关闭时机。
   - 备选方案：依赖 Unity Script Execution Order。放弃原因：只能粗粒度排序，无法表达运行时动态实体与跨系统依赖。

2. **`Entity` 继承 `MonoBehaviour`，但 Unity 生命周期仅做桥接**
   - 决策：`Awake` 仅注册、`OnDestroy` 仅反注册；业务逻辑仅写在自定义实体生命周期函数中。
   - 原因：避免“双生命周期”并发执行业务造成重复初始化和状态竞争。
   - 备选方案：在 `Start/Update` 同步触发自定义生命周期。放弃原因：仍受 Unity 时序影响，目标无法达成。

3. **执行顺序采用 `Priority` 升序 + 稳定次序**
   - 决策：先按 `Priority` 排序，同优先级按注册先后稳定执行。
   - 原因：满足关键实体前置需求，并降低帧间顺序抖动。
   - 备选方案：仅按注册顺序。放弃原因：缺少系统级依赖表达能力。

4. **采用“延迟并入/延迟移除”避免遍历期集合修改**
   - 决策：更新循环中新增实体进入待并入队列，移除进入待清理队列，在阶段末统一处理。
   - 原因：避免迭代器失效和漏执行。
   - 备选方案：遍历时直接修改集合。放弃原因：易产生异常与未定义行为。

5. **通过 `GameModule.Entity` 暴露访问门面**
   - 决策：业务侧通过门面访问实体模块能力，不直接依赖内部容器结构。
   - 原因：符合项目模块访问规范，便于后续替换实现。

## Risks / Trade-offs

- [风险] 实体同时使用 Unity `Update` 与 `OnEntityUpdate` 造成重复逻辑 → **缓解**：规范禁止在托管 `Entity` 内编写 Unity `Update` 业务代码，评审时检查。
- [风险] 动态启用/禁用对象导致状态错位（已注册但未参与调度）→ **缓解**：模块调度前检查对象有效性与激活状态，并在失效时自动清理。
- [风险] 迁移过程中旧脚本仍依赖 `Start` 顺序 → **缓解**：增量迁移，先迁核心依赖链对象，并提供迁移清单。
- [权衡] 引入模块调度增加一层抽象与少量运行时开销 → **收益**：换取确定性执行顺序和更清晰的生命周期边界。

## Migration Plan

1. 在 HotFix 模块层新增 `EntityModule` 与 `Entity` 基类，接入 `GameModule` 门面。
2. 先选择 1~2 条存在时序问题的实体依赖链进行试点迁移。
3. 将试点实体中的 `Awake/Start/Update` 业务迁入自定义生命周期函数。
4. 验证初始化稳定性、帧更新顺序、对象销毁与模块关闭清理。
5. 逐批迁移其余实体，保留回退路径：移除托管注册即可回到原 MonoBehaviour 流程。

## Open Questions

- `OnEntityInit` 与 `OnEntityStart` 是否都需要，还是合并为单一启动阶段更简洁？
- 当实体被临时禁用后再次启用，是否需要重新触发 `OnEntityStart`？
- 是否需要提供“只在固定帧率系统更新”的实体子类型（如物理/AI 分离更新频率）？
