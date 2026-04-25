## Context

当前最小远征闭环已经具备基本可玩性，但开局状态仍由运行时代码硬编码：`ExpeditionPersistentDataStore.EnsureInitialized()` 直接写入默认资金和 3 个默认 Marble。与此同时，远征 Combat 遭遇仍保留远征域专用的敌方 Marble 配置概念，而用户已经将表结构推进为以下方向：

- `Gameplay.Combat.MarbleSpawnConfig` 作为通用 Marble 静态生成条目
- `Gameplay.Camp.CampConfig` 作为开局阵营包，提供初始资金、初始 Marble 与初始可用远征
- `Gameplay.Initial.InitialConfig` 作为单例启动配置，指定默认开局阵营
- `camp_config_id` 表示 Marble 使用哪套阵营配置，会影响外观、颜色等表现语义

在这个前提下，继续使用 `Camp` 作为战斗内敌我归属命名会产生持续歧义，因为运行时“敌我关系”与配置侧“阵营表现配置”已经不再是同一个概念。当前 `Gameplay.Combat` 域大量使用 `RuntimeData.Camp`、`PlayerCamp`、`EnemyCamp` 等命名，属于本次变更的主要跨模块影响面。

## Goals / Non-Goals

**Goals:**
- 让玩家开局状态通过 `InitialConfig -> CampConfig` 完整配置化，不再写死默认资金和默认 Marble。
- 统一使用 `MarbleSpawnConfig` 表达玩家初始 Marble 与远征敌方 Marble，移除远征域专用敌方 Marble 结构依赖。
- 在 Marble 持久化数据中保留 `camp_config_id`，使配置阵营信息可用于后续表现层与衍生逻辑。
- 将战斗内敌我归属统一重命名为 `CombatSide`，与 `camp_config_id` 的配置语义彻底分离。
- 保持当前最小远征流程可继续运行，并让远征入口改为依赖当前开局阵营包提供的可用远征。
- 明确人工协作边界：agent 不创建、不修改 xlsx，只消费用户已维护并重新生成的配置结果。

**Non-Goals:**
- 不在本次变更中实现“开局选择多个阵营”的完整 UI 流程，当前只消费 `InitialConfig` 指定的默认阵营。
- 不在本次变更中实现基于 `camp_config_id` 的 Marble 外观换肤逻辑，只保证运行时数据链路完整保留该字段。
- 不在本次变更中重构全部 Combat 测试工具的玩法逻辑，只统一敌我归属命名和必要的桥接输入。
- 不在本次变更中扩展开局资源种类，仍以现有 `money` 为唯一局外资源。

## Decisions

### 1. 开局状态以 `InitialConfig -> CampConfig` 作为唯一配置来源
- 选择：当局外持久化数据为空时，系统先读取 `InitialConfig`，再解析对应 `CampConfig`，并据此初始化 `Money`、`LstMarbles`、可用远征列表。
- 原因：`InitialConfig` 负责“本局从哪一个阵营包开始”，`CampConfig` 负责“这个阵营包给什么起始资源”，职责清晰且便于后续扩展为多阵营选择。
- 备选方案：
  - 直接把初始 Marble 和资金挂在远征配置上：会把“开局状态”错误地绑定到单个远征。
  - 继续写死在 `EnsureInitialized()`：无法支持后续通用配置化。

### 2. `MarbleSpawnConfig` 保留在 `Gameplay.Combat` 模块下，并作为跨域可复用条目
- 选择：`MarbleSpawnConfig` 定义在 `Gameplay.Combat` 下，由 `CampConfig` 和 `ExpeditionCombatEncounterConfig` 共同复用。
- 原因：这个 bean 描述的是“生成一个怎样的 Combat Marble”，本质上是 Combat 域的静态生成输入，而不是远征专属结构。
- 备选方案：
  - 在远征域和开局域各自定义一套 Marble 条目：会造成重复 schema 和重复运行时映射。

### 3. `camp_config_id` 保留在 `MarbleSpawnConfig` 和持久化 Marble 数据中
- 选择：每个 Marble 静态条目和持久化条目都保留 `camp_config_id`。
- 原因：`camp_config_id` 不再只是“属于哪个开局阵营包”的冗余信息，而是 Marble 自身配置阵营语义的一部分，后续会影响表现层和颜色，也允许一场遭遇中混合多个配置阵营 Marble。
- 备选方案：
  - 仅在外层 `CampConfig` 上保留阵营：无法表达敌方混合阵营，也丢失单 Marble 的配置来源。

### 4. 战斗内敌我归属统一命名为 `CombatSide`
- 选择：把当前 Combat 域中用于敌我判定的 `Camp` 概念统一迁移到 `CombatSide`。
- 原因：`Camp` 已经被配置层稳定占用为 `camp_config_id` 语义，继续复用会导致“配置阵营”和“战斗敌我归属”混淆。
- 备选方案：
  - 保留 `Camp`：短期省改名成本，但长期会不断制造歧义。
  - 改名为 `Side`：表达足够，但在代码搜索和跨模块阅读时不如 `CombatSide` 明确。

### 5. 局外初始化采用“只种子一次”的策略
- 选择：`ExpeditionPersistentDataStore.EnsureInitialized()` 仅在当前持久化数据为空时，按配置生成初始数据；已有持久化数据时不重复覆盖。
- 原因：这是最小循环当前的安全语义，避免玩家已经积累的局外进度被再次初始化覆盖。
- 备选方案：
  - 每次打开入口都按 `InitialConfig` 重建：会破坏持久化目标。

### 6. 可用远征列表由当前 `CampConfig` 提供
- 选择：远征入口层和默认启动逻辑读取当前阵营包的 `LstExpedition`，不再硬依赖单个写死的 `MinimalExpeditionId` 作为唯一远征来源。
- 原因：一旦开局阵营配置化，远征入口也应当与当前阵营包绑定。
- 备选方案：
  - 保留常量 `MinimalExpeditionId`：会让 `CampConfig.LstExpedition` 失去意义。

## Risks / Trade-offs

- [Risk] `camp_config_id` 与 `CombatSide` 混用，导致敌我判定或表现逻辑出错  
  → Mitigation：在设计和任务中明确区分“配置阵营”和“战斗归属”；所有敌我判定只读取 `CombatSide`。

- [Risk] Combat 域中 `Camp -> CombatSide` 的改名范围较大，容易漏改  
  → Mitigation：任务中单列 Combat 命名迁移步骤，优先覆盖 `MarbleRuntimeData`、`MarbleFactory`、`CombatManager`、Projectile 与 Equipment 命中判定链路，再做全局检索收口。

- [Risk] 生成代码和运行时代码阶段性不一致，导致编译失败  
  → Mitigation：明确执行顺序为“用户改表并重新生成 -> 代码消费新生成结果 -> 编译验证”；agent 不在表格未就绪时提前修改依赖新生成字段的代码。

- [Risk] `CampConfig.LstExpedition` 为空时入口无法启动远征  
  → Mitigation：运行时增加配置校验和清晰日志；默认启动逻辑在列表为空时返回失败而不是静默回退。

- [Risk] 持久化初始化改为配置驱动后，旧测试数据可能与新 schema 不兼容  
  → Mitigation：保持“只在空持久化数据时初始化”的策略，减少对已有运行态测试存档的干扰。

## Migration Plan

1. 确认用户已完成 `Camp / Initial / MarbleSpawnConfig / expedition enemy_marbles` 对应 xlsx 和生成代码更新。
2. 更新生成代码消费链路：远征 Combat 遭遇、桥接请求、战斗会话生成统一使用 `MarbleSpawnConfig`。
3. 更新持久化初始化链路：从 `InitialConfig -> CampConfig` 初始化资金、Marble 和可用远征。
4. 在 Marble 持久化数据中补入 `camp_config_id`。
5. 将 Combat 域战斗敌我归属命名统一迁移为 `CombatSide`。
6. 编译验证，并用最小远征流程回归“开局 -> 远征 -> Combat -> 结算 -> 返回入口”。

## Open Questions

- 当前无阻塞性开放问题。后续如果要支持“玩家在开局界面手动选择多个 Camp 之一”，可以在本次 `InitialConfig -> CampConfig` 链路之上扩展，而不需要推翻 `MarbleSpawnConfig + CombatSide` 的基础模型。
