## Context

当前战斗系统已经形成了 `MarbleFactory`、`EquipmentFactory`、`ProjectileFactory` 三套工厂，但三者在“默认能力挂载”“配置驱动能力挂载”“creator 扩展点”“Timing 构建逻辑”上存在不同写法。随着战斗能力数量增加，这些差异已经开始导致两个问题：一是人类维护时需要反复切换心智模型，二是 AI 在生成或修改能力装配代码时容易把默认能力、配置能力和扩展能力混淆。

本次设计不打算引入新的复杂运行时层，而是围绕现有 Factory 模型做结构收敛：保留现在的 CreatorForConfig 扩展方向，但统一三个 Factory 的装配骨架；同时把 Marble 中与定时能力相关的 Timing 构建逻辑抽离成轻量工厂，避免配置解析分散在具体 ability creator 中。

## Goals / Non-Goals

**Goals:**
- 让三个 Combat Factory 使用一致的装配骨架，降低扩展和理解成本。
- 明确区分固定骨架能力与配置驱动能力，避免“默认能力”和“可选能力”语义混乱。
- 保留现有的 `CreatorForConfig` 扩展点，使未来新增配置能力仍然可以通过注册扩展。
- 将 Timing 配置解析收敛为独立的轻量创建入口，供 Marble 的定时能力共享。
- 修复当前工厂层已经暴露出的映射错误、作用域错误和职责混乱问题。

**Non-Goals:**
- 不重写整个战斗系统的数据模型或 Ability 生命周期模型。
- 不在本次变更中引入新的 DI 框架、ScriptableObject 注册中心或反射式自动注册机制。
- 不要求把所有当前默认能力立即配置化。
- 不修改 CombatManager 的职责边界，只关注其所依赖的战斗对象工厂装配稳定性。

## Decisions

### 1. 保留 Factory + CreatorForConfig 模型，但统一三套骨架
- 选择：保留 `RegisterAbilityCreatorForConfig` / `CreateAbilityFromConfig` 这一扩展路径，不另起一套抽象工厂体系。
- 原因：当前项目已经基于这个模型运行，多数问题来自结构不一致而不是模型本身错误。继续沿用并统一，成本最低且最利于渐进式重构。
- 备选方案：
  - 引入通用泛型工厂基类：理论上更统一，但会显著增加抽象层数，当前收益不足。
  - 每个系统独立演化：短期省事，但会继续扩大风格漂移。

### 2. 以“固定骨架能力 / 配置驱动能力”替代“默认 / 可选”心智模型
- 选择：将装配语义明确为两层：
  - 固定骨架能力：系统运行所必须的核心能力
  - 配置驱动能力：由 Luban 配置决定的能力
- 原因：`Optional` 这个词在当前代码中容易让人误解为“非关键、随便有无”，但实际上很多配置能力是玩法关键能力。用“配置驱动能力”更贴近事实，也更利于 AI 判断修改入口。
- 备选方案：
  - 继续沿用 `OptionalAbilities` 命名：心智负担更大，后续还会重复解释。

### 3. EquipmentFactory 维持外层统一挂配置能力，类型 creator 只管默认骨架
- 选择：`DefaultEquipmentCreatorForConfig.AttachDefaultAbilities(...)` 只负责装备类型的固定骨架和类型主能力；配置驱动的扩展能力仍由 `EquipmentFactory` 外层统一挂载。
- 原因：Equipment 目前有两层 creator（装备实例创建 + ability config 创建），若把配置扩展再下沉到 creator 内部，会让作用域和职责再次混乱。由外层统一挂载更稳定。
- 备选方案：
  - 让每个装备类型在 creator 内自行调用配置扩展：更贴近单类封装，但实际会引出私有访问、重复挂载和装配顺序分裂问题。

### 4. Timing 创建独立为轻量 `AbilityTimingFactory`
- 选择：把 `AbilityTimingConfig -> IAbilityTiming` 的映射独立成无状态、轻量的 `AbilityTimingFactory`。
- 原因：Timing 是跨 Marble 定时能力的通用配置构建逻辑，不应寄生在某个具体 ability creator 里；单独收敛后，未来新增 timing 类型只需补工厂即可。
- 备选方案：
  - 保留在 `DefaultMarbleAbilityCreatorForConfig` 内部：耦合过强，未来复用不自然。
  - 为 Timing 再设计 creator/registry：目前过度设计，没有必要。

### 5. 中间态接口如果没有实际调用价值则直接删除
- 选择：移除仅为“以后可能统一”而引入、但当前无消费方的接口，例如之前误加的 timing 或 factory 占位接口。
- 原因：当前阶段最重要的是把真实的扩展点收敛清楚，而不是增加额外抽象。对 AI 来说，无用接口会显著增加误判概率。
- 备选方案：
  - 预留更多接口做未来扩展：会让当前结构更难读，不符合这次“快速收口”的目标。

## Risks / Trade-offs

- [Risk] 只统一骨架、不彻底配置化，后续仍可能有人继续把逻辑塞回默认挂载分支  
  → Mitigation：通过 spec 和 tasks 明确后续新增能力时的装配规则，优先走配置驱动入口。

- [Risk] EquipmentFactory 仍然是三者中职责最重的工厂  
  → Mitigation：本次先统一边界，后续可再拆 `Armor/Bow/Sword` 的配置到能力映射函数，逐步减轻体积。

- [Risk] OpenSpec 中部分现有 capability 名称与本次实现边界不完全贴合  
  → Mitigation：在 delta spec 中只修改真正发生规范变化的部分，其余实现细节保留在 design 和 tasks。

- [Risk] 新增文件需要 Unity 重新生成 csproj，外部 `dotnet build` 无法稳定代表 Unity 真实编译状态  
  → Mitigation：明确将最终编译验证交给 Unity Editor 侧完成。
