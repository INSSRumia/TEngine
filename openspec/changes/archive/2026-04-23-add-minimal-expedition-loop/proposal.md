## Why

当前项目已经完成最小战斗能力验证，但玩家仍然只能直接进入测试战斗，尚未形成“入口发起远征 -> 经过事件/Combat 节点 -> 结算返回入口”的最小可玩闭环。继续扩展单点 Combat 能力的收益已经低于先把远征主流程串起来，因为只有先建立闭环，后续家园、成长、事件、撤退和结算系统才有稳定的落点。

现在推进这项变更，是为了尽快把项目从“战斗原型”升级成“可反复进入并完成一次 Run 的玩法骨架”，并为后续局外养成和远征内容扩展建立统一的流程、数据和状态机边界。

## What Changes

- 新增最小远征循环：从极简入口界面发起远征，进入固定路线，依次经过事件节点与 Combat 节点，最终完成结算并返回入口
- 新增远征运行时数据模型：定义 `ExpeditionRunState`、节点记录、Marble 持久化快照和远征结束结果，承载一次远征的完整状态
- 新增远征流程状态机：基于 TEngine FSM 实现 `ExpeditionFlowController`，统一编排准备、进入节点、事件选择、进入 Combat、应用节点结果、远征结算等阶段
- 新增远征与 Combat 的桥接层：定义远征侧请求/结果数据包，将远征流程与现有 `Gameplay.Combat` 模块解耦
- 新增最小事件与结算链路：支持事件选项结果写回、Combat 结果写回、资源结算与 Marble 持久化状态回写
- 新增最小 UI 串联：补齐远征入口、事件卡和远征结算三个界面所需的数据与流程接点，并明确遵循 TEngine `UIWindow/UIModule` 开发流程
- 明确当前阶段的 UI 实现目标：只交付可运行的功能型界面，不以美术资源齐备为前提
- 明确 Unity 侧操作约束：若实现过程中需要修改 Unity 场景、Prefab 或 UI 资源，统一通过已配置的 Unity MCP 完成

## Capabilities

### New Capabilities
- `expedition-run-loop`: 最小远征循环能力，定义一次远征从发起、节点推进、结果写回到结算返回的行为边界
- `expedition-flow-fsm`: 远征流程状态机能力，定义 `ExpeditionFlowController` 及其状态迁移规则
- `combat-session-bridge`: 远征侧与 Combat 模块之间的会话桥接能力，定义输入/输出契约
- `marble-persistent-data`: Marble 局外持久化数据与远征内快照能力，确保 Combat 结果能稳定回写到远征和入口层

### Modified Capabilities

无

## Impact

- **受影响模块**：`UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/`、`UnityProject/Assets/GameScripts/HotFix/GameLogic/UI/ExpeditionMainUI/`、`UnityProject/Assets/GameScripts/HotFix/GameLogic/UI/EventCardUI/`、`UnityProject/Assets/GameScripts/HotFix/GameLogic/UI/ExpeditionResultUI/`
- **协作模块**：现有 `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/` 模块将作为远征中的单场 Combat 执行域被接入，但本变更不直接重写 Combat 核心能力骨架
- **依赖关系**：依赖 TEngine FSM 模块、现有 CombatManager / MarbleDeath 事件 / Marble Runtime 数据，以及后续最小持久化落盘方案
- **影响范围**：玩法主流程、远征运行时数据模型、UI 串联方式、远征与 Combat 的边界约定
- **执行约束**：后续开发该 change 的 agent 必须遵循 TEngine UI 开发流程，并在需要改动 Unity 内容时优先使用 Unity MCP，避免在代码与资源侧各自走不同流程
