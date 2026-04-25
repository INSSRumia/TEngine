## Context

当前 `MarbleRuntimeData` 既承担战斗运行时状态，又直接承载从配置投影出来的基础数据，还混入了各种帧级控制器对象。这个大类在“Ability 可方便读写”的层面是有效的，但随着能力数量增加，它正在逐渐失去语义边界：有些字段更像同步态，有些字段更像配置态，还有些字段根本只是本地帧内计算辅助。用户已经明确希望保留“大 Runtime 黑板”模型，因为未来网络同步时希望尽量围绕 RuntimeData 组织数据；同时也不希望现在就去改 `EquipmentRuntimeData` 和 `ProjectileRuntimeData`，重点只放在 Marble。

本次设计的核心不是把 RuntimeData 拆散，而是在 `MarbleRuntimeData` 内部建立结构化黑板：仍然保持一个统一入口，但在内部明确划分 `State / Config / Frame` 三个区域，让 Ability 继续通过 Runtime 读写，同时为未来网络同步和 AI 修改建立更清楚的边界。

## Goals / Non-Goals

**Goals:**
- 保留 `MarbleRuntimeData` 作为统一黑板根对象，避免大量能力调用方式立即被打散。
- 在内部明确区分运行时状态数据、配置投影数据和帧临时数据。
- 将各类控制器对象与可同步状态字段分离，但仍然放在 Runtime 根对象内便于访问。
- 让未来网络同步能够优先围绕 `State` 区组织，而不是继续面对平铺字段大杂烩。
- 先只改 `MarbleRuntimeData` 及其能力读写路径，不扩散到 Equipment/Projectile。

**Non-Goals:**
- 不在本次变更中重构 `EquipmentRuntimeData` 和 `ProjectileRuntimeData`。
- 不立即实现真正的网络同步协议。
- 不追求将所有配置基础值都从 RuntimeData 中移除。
- 不引入复杂 ECS、独立黑板系统或跨模块数据总线。

## Decisions

### 1. 保留单一 Runtime 根对象，但内部拆成三层区块
- 选择：`MarbleRuntimeData` 继续作为唯一黑板入口，内部增加 `State`、`Config`、`Frame` 三个子块。
- 原因：这兼顾了当前 Ability 访问便利性与未来同步边界清晰性，是最适合现阶段的折中方案。
- 备选方案：
  - 完全平铺字段继续演化：最省事，但问题只会继续变重。
  - 把配置和控制器完全挪出 Runtime：同步边界纯净，但会显著破坏当前 Ability 读写便利性。

### 2. `State` 只放跨帧真实状态，`Config` 放配置投影，`Frame` 放帧临时控制数据
- 选择：
  - `State`：生命、护盾、经验、目标、存活状态等真实运行态
  - `Config`：攻击、防御、质量、缩放、升级阈值等配置投影基础态
  - `Frame`：各类 `PriorityValueManager`、临时缓存和帧内控制结果
- 原因：这样既能让能力继续通过 Runtime 黑板访问数据，也让未来同步与本地重建的职责更清楚。
- 备选方案：
  - 把控制器继续混在顶层：短期方便，但长期会再次失控。

### 3. 配置投影数据暂时继续留在 Runtime 黑板中
- 选择：不把配置投影数据立刻从 Runtime 根中剥离，而是先放进 `Config` 区块统一管理。
- 原因：用户明确希望 Runtime 继续承担统一黑板角色，这样后续能力重构成本最低。
- 备选方案：
  - 彻底从 Runtime 中移除配置数据：理论更纯，但当前并不符合项目实际使用习惯。

### 4. 先改 Marble，不扩散到 Equipment / Projectile
- 选择：只对 `MarbleRuntimeData` 和 Marble abilities 的访问路径做结构化调整。
- 原因：Marble 是当前能力最复杂、字段最混乱、最值得优先治理的对象，其它运行时对象暂时没有同等复杂度。
- 备选方案：
  - 三种 RuntimeData 一起改：一致性更好，但风险和改动面明显过大。

## Risks / Trade-offs

- [Risk] 虽然内部结构化了，但 Runtime 黑板仍然可能继续被滥用  
  → Mitigation：明确约定 Ability 默认修改 `State` 和 `Frame`，尽量不要随意篡改 `Config`。

- [Risk] 旧字段迁移到三层结构后，会影响较多 Marble ability 的读写路径  
  → Mitigation：分阶段迁移，优先保证命名清晰和访问路径稳定，再逐步清理旧写法。

- [Risk] 未来若真的进入网络同步实现阶段，仍然可能需要进一步细分同步态  
  → Mitigation：先通过 `State / Config / Frame` 建立一级边界，后续同步协议可以只围绕 `State` 展开。

- [Risk] 当前项目没有对 Blackboard 使用建立写入规则，开发者可能继续跨层乱写  
  → Mitigation：在任务和实现中补充访问约定，并优先把明显属于 `Frame` 的控制器迁移进去。
