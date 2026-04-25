## Context

当前项目已经具备 `Gameplay.Combat` 域的最小可运行能力，包括 Marble/Equipment/Projectile 运行时结构、`CombatManager`、死亡事件和基础战斗 UI 骨架，但游戏主流程仍停留在直接进入测试场景的阶段，缺少“入口 -> 远征 -> 事件/Combat -> 结算 -> 返回入口”的最小闭环。

本次变更的目标不是先做完整家园系统，而是先建立一条最小远征主流程，让现有 Combat 能力成为远征中的一个执行节点。该流程需要同时跨越以下几个边界：

- 玩法主流程编排边界：一次远征从发起到结束的状态迁移
- 运行时数据边界：远征状态、节点执行记录、Marble 持久化数据与 Combat Runtime 的分离
- 模块协作边界：远征流程与 `Gameplay.Combat` 的桥接方式
- UI 串联边界：入口界面、事件卡界面、结算界面与流程状态的映射

当前用户已明确约束：

- 家园阶段只保留“进入远征”的入口，不在本次变更中承担建造、养成或商店功能
- `ExpeditionFlowController` 采用 TEngine FSM，而不是继续扩展顶层 `Procedure`
- 统一使用 `Combat` 作为战斗域英文术语，不在新设计中混用 `Battle`
- 远征中的 `Node` 对应 GDD 里的路线节点；最小版本只包含 `EventNode` 与 `CombatNode`
- UI 开发必须遵循 TEngine 的 `UIWindow/UIModule` 开发流程，而不是临时自建一套 UI 控制模式
- 当前阶段的 UI 不要求美术资源完整，只要求功能可运行、节点可绑定、流程可验证
- 如果实现过程中需要创建或修改 Unity 场景、Prefab、Canvas 节点或 UI 资源，必须优先通过已配置的 Unity MCP 完成

## Goals / Non-Goals

**Goals:**

- 建立一次最小远征的完整运行时模型，包括远征状态、节点记录、Marble 持久化快照与远征结算结果
- 基于 TEngine FSM 实现 `ExpeditionFlowController`，明确远征阶段状态与状态迁移触发条件
- 定义远征与 `Gameplay.Combat` 之间的请求/结果桥接契约，避免流程层直接依赖 Combat 场景对象
- 让远征入口、事件卡、结算界面能通过统一流程状态被串联起来
- 让远征相关 UI 在不依赖最终美术资源的前提下，按照 TEngine UI 规范稳定接入
- 支持最小路线 `EventNode -> CombatNode -> Settlement -> Entry`

**Non-Goals:**

- 不实现完整家园玩法、建筑系统、兵营、商店或局外养成界面
- 不改写现有 Combat 核心骨架，不把远征状态塞入 `MarbleRuntimeData`
- 不实现完整多分支路线、商店节点、Boss 节点或复杂事件池
- 不实现表现化撤退边界与撤离动画；若纳入首版，仅按即时结算处理
- 不在本次设计中引入多场景远征流转，首版维持单场景 + UI 状态切换
- 不在本次设计中追求 UI 美术完成度、动效、最终排版与视觉包装
- 不在本次设计中允许绕开 TEngine UI 流程直接拼接临时脚本式界面

## Decisions

### Decision 1: 远征主流程采用独立 FSM，而不是 Procedure

**选择方案**：`ExpeditionFlowController` 作为 TEngine FSM 的 owner，远征内阶段使用 `FsmState<ExpeditionFlowController>` 实现。

建议状态集合：

- `ExpeditionFlowStatePrepare`
- `ExpeditionFlowStateEnterNode`
- `ExpeditionFlowStateEvent`
- `ExpeditionFlowStateCombat`
- `ExpeditionFlowStateApplyNodeResult`
- `ExpeditionFlowStateSettlement`
- `ExpeditionFlowStateFinished`

**备选方案：**

- 继续复用顶层 `ProcedureBase` 承担远征阶段切换
- 使用普通控制器 + `switch` / `if` 手工维护状态

**理由：**

- `Procedure` 更适合应用启动与大生命周期，不适合承载“游戏内一次远征”的局部流程
- TEngine 的 FSM 已具备 `CreateFsm<T>`、`OnEnter/OnUpdate/OnLeave` 等标准能力，能自然映射远征阶段
- FSM 方案能让 UI、节点推进、Combat 回调都有清晰落点，避免控制器膨胀

---

### Decision 2: 用 `ExpeditionRunState + ExpeditionNodeRecord` 建模远征运行时数据

**选择方案**：

- `ExpeditionRunState` 记录一次远征的总状态
- `ExpeditionNodeRecord` 记录某个节点本次运行后的结果

其中：

- `ExpeditionNodeConfig` 定义“节点原本是什么”
- `ExpeditionNodeRecord` 记录“节点这次实际发生了什么”

**备选方案：**

- 只在 `ExpeditionRunState` 中堆平所有字段，不单独抽节点记录
- 使用 `Progress` / `State` 命名节点结果对象

**理由：**

- 远征总状态与单节点执行结果属于两个层级，分开后更便于结算、回放和调试
- `Record` 比 `Progress` 更准确地表达“节点执行后留下的记录”，避免和 FSM 状态混淆

---

### Decision 3: Marble 持久化数据与 Combat Runtime 严格分离

**选择方案**：

- 局外长期数据使用 `MarblePersistentData`
- 进入远征时复制为 `MarblePersistentDataSnapshot`
- Combat 只消费远征内快照转换出的输入数据，结束后再将结果回写到快照，再由远征结算统一回写到持久化数据

**备选方案：**

- 让远征流程直接持有 `MarbleRuntimeData`
- 让 Combat 直接修改局外持久化对象

**理由：**

- 当前项目明确要求 Combat Runtime 只负责战斗域黑板和能力执行
- 如果远征流程直接依赖 `MarbleRuntimeData`，会破坏 Combat 与局外层的边界
- 使用快照能让“开始远征时读一次，远征结束时写一次”的数据流清晰稳定

---

### Decision 4: 统一使用 `Combat` 作为战斗域术语

**选择方案**：新能力、新桥接对象和新流程状态统一使用 `Combat`，如 `CombatSessionRequest`、`CombatSessionResult`、`ExpeditionFlowStateCombat`。

**备选方案：**

- 在远征层使用 `Battle`，在现有域继续使用 `Combat`
- 全量把现有 `Gameplay.Combat` 命名重构为 `Battle`

**理由：**

- 当前代码与文档已经形成 `Gameplay.Combat` 域概念，`Combat` 更适合表示系统域与框架层
- 在新代码里混入 `Battle` 会制造术语二义性，增加流程层与系统层命名割裂
- 全量改名成本高，且与本次最小循环目标无直接收益

---

### Decision 5: `Node` 明确对应 GDD 中的远征路线节点，首版仅保留 Event/Combat 两类

**选择方案**：

- `Node` 指远征路线上的一个推进单元
- 首版只实现：
  - `EventNode`：对应 GDD 的卡牌事件决策
  - `CombatNode`：对应 GDD 的一场具体 Combat 遭遇

最小路线固定为：

`Entry -> EventNode -> CombatNode -> Settlement -> Entry`

**备选方案：**

- 一开始就引入 `ShopNode`、`BossNode`、`BranchNode`、`RewardNode`
- 不建立节点抽象，直接把流程写死成一串 UI 切换

**理由：**

- GDD 已经把远征描述为由事件与战斗构成的线性节点链
- 节点抽象是后续扩路线、扩事件池和做多节点串联的稳定入口
- 首版只保留两类节点，既贴合 GDD，又不让最小循环被内容复杂度拖慢

---

### Decision 6: 首版使用单场景 + UI 状态切换，不做多场景流转

**选择方案**：远征入口、事件卡、Combat UI 和结算 UI 在同一主场景内切换，由远征 FSM 驱动状态变化。

**备选方案：**

- 入口场景与 Combat 场景分离，通过场景加载完成流转
- 先做完整 Home Scene，再接入远征

**理由：**

- 当前项目仍处于最小循环搭建阶段，单场景更利于快速验证闭环
- 多场景流转会提前引入加载、回收、状态恢复等额外复杂度
- 家园当前只承担入口职责，不值得为其单独建立完整场景语义

---

### Decision 7: UI 必须遵循 TEngine UI 开发流程，且当前只做功能型界面

**选择方案**：

- 远征入口、事件卡、结算界面都作为 TEngine `UIWindow` 接入
- UI 通过 `[Window]`、`GameModule.UI.ShowUIAsync`、`ScriptGenerator`、`RegisterEvent`、`OnRefresh` 等既有流程接入
- 当前阶段优先保证节点绑定、交互回调和流程联动正确，不等待美术资源齐备

**备选方案：**

- 先用临时 MonoBehaviour + 场景节点直绑实现界面逻辑，后续再迁移到 `UIWindow`
- 先等待正式美术 Prefab 再开始远征 UI 串联

**理由：**

- 当前项目已有 `UIWindow/UIModule` 基础设施，绕开它会制造二次迁移成本
- 后续 subagent 若不遵循 TEngine UI 规范，最容易在节点命名、生命周期和事件清理上出错
- “功能优先”比“美术优先”更符合当前最小循环验证目标

---

### Decision 8: 需要操作 Unity 内容时，统一通过 Unity MCP 完成

**选择方案**：

- 代码文件编辑仍走仓库文件修改
- 一旦涉及 Unity Editor 内对象，例如场景、Prefab、Canvas 层级、UI 节点、组件挂载与资源生成，统一通过 Unity MCP 操作

**备选方案：**

- 手工在文档中描述 Unity 改动，由实现者自行在编辑器中操作
- 混用本地文件修改与非 MCP 的 Unity 资源改动方式

**理由：**

- 用户已经明确配置好 Unity MCP，并要求 Unity 内容操作通过 MCP 执行
- 这能让后续 subagent 的 Unity 改动具备可重复性和可验证性
- 统一工具链能减少“代码已经写好但 Unity 资源未同步”的偏差

---

### Decision 9: 为减少歧义，首版范围额外固定四项实现选择

**选择方案**：

- 首版不实现撤退，远征结束条件仅包含当前路线完成或 Combat 失败
- `ExpeditionMainUI` 同时承担“入口 + 出征准备”职责，不再拆额外入口窗口
- 最小远征静态路线先写死在代码或轻量静态数据对象中，不立即接入配置表
- Marble 持久化先做到“运行期内的局外持久化回写”，不要求在本次 change 内完成真正的磁盘存档

**备选方案：**

- 同时实现撤退、入口拆窗、配置表接入与落盘存档
- 保留这些问题为实现阶段再自由决定

**理由：**

- 用户明确表示后续会启动 subagent 实现，当前最重要的是减少选择分叉
- 这四项若不提前固定，后续实现最容易产生范围漂移
- 该收束仍然保留了最小闭环验证价值，不会阻断未来扩展

## Risks / Trade-offs

- **FSM 状态过多导致实现成本上升** -> 保持首版状态集最小，只为真实存在的阶段建立状态，不提前为未来节点预留空状态
- **Marble 持久化与 Combat 结果回写不一致** -> 明确“开始远征读一次、远征结束写一次”的单向数据流，并以 `CombatSessionResult` 作为唯一回写来源
- **远征节点模型过度设计** -> 首版仅支持 `EventNode` 和 `CombatNode`，其他节点类型只在设计中留扩展位，不进入当前任务拆分
- **UI 与流程强耦合** -> 由 FSM 持有流程状态，UI 只订阅/驱动状态输入，不直接决定流程迁移
- **名称边界再次混乱** -> 在本次 change 中统一采纳 `Combat`、`Record`、`PersistentData` 这组术语，避免出现 `Battle` / `Progress` / 非持久化命名回潮
- **后续 agent 绕开 TEngine UI 约定** -> 在任务与规格中明确要求使用 `UIWindow/UIModule`、节点命名规范和功能型占位 UI，不允许自行发明平行 UI 接法
- **Unity 资源改动与代码实现脱节** -> 把 Unity 内容操作方式固定为 Unity MCP，并在实现任务中显式列出

## Migration Plan

1. 新增 `Gameplay/Expedition/` 目录下的远征运行时数据对象、流程 owner 和 FSM 状态类
2. 新增远征侧配置对象和最小静态路线定义，使流程可以在不依赖完整家园系统的前提下启动
3. 建立 `CombatSessionRequest` / `CombatSessionResult` 桥接契约，把远征流程接入现有 `Gameplay.Combat`
4. 按 TEngine `UIWindow/UIModule` 流程接入 `ExpeditionMainUI`、`EventCardUI`、`ExpeditionResultUI` 三个界面的状态输入与回调
5. 在入口界面中替代“直接进测试战斗”的路径，改为“发起最小远征”
6. 如需创建或修改 Unity 中的 UI 资源、Prefab 或层级结构，统一通过 Unity MCP 完成
7. 通过固定路线验证最小闭环跑通，再决定是否扩展更多节点和内容

回滚策略：

- 保留现有直接进入测试场景的路径作为开发调试后门
- 若远征 FSM 或桥接层未稳定，可先回退入口调用，不影响现有 Combat 模块单独运行

## Open Questions

1. 首版功能型 UI 是否直接复用现有空目录下的 Prefab/脚本资源，还是需要通过 Unity MCP 新建完整占位 Prefab
2. 运行期内局外持久化回写完成后，后续是否单独开 change 继续补“真正落盘存档”能力
