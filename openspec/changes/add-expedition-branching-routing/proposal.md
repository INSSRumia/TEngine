## Why

当前远征仍以线性节点链为核心，这能支撑最小闭环，但无法稳定表达按选项分叉、按黑板状态分叉、以及运行中动态插入事件这类更复杂的流程。现在需要把远征从“固定线性 route”升级为“可复用 Event 内容 + Node 级路由策略 + 运行时黑板与待执行队列”的模型，才能在不复制大量事件配置的前提下扩展远征设计。

## What Changes

- 为远征新增分支路由能力，支持固定出口、按选项出口、按条件出口三种节点级路由策略。
- 引入远征黑板，用于记录道具、标记、历史选择、计数器等可被后续分支读取的运行时状态。
- 将远征从固定线性 `route + currentIndex` 模式升级为“静态图 + 运行时待执行节点队列”模式，为动态插入节点提供基础。
- 明确 Event 只负责内容和选项，出口逻辑配置写在 Node 中，不允许将不同节点的出口含义硬塞进复用 Event 模板里。
- 为动态插入事件预留机制，使前序选择可以在后续节点触发插入事件或支线节点。
- 明确人工协作边界：若实现需要修改 `xlsx` 表结构或数据，agent 必须先通知用户手工修改，不允许自行编辑任何 `xlsx` 文件。

## Capabilities

### New Capabilities
- `expedition-branching-routing`: 定义远征节点级路由策略、黑板状态、条件分支与动态插入节点的运行模型。

### Modified Capabilities
- `expedition-run-loop`: 远征从线性节点推进改为基于节点图与运行时待执行队列推进，并支持分支与动态插入。
- `expedition-luban-static-config`: 远征静态配置从单纯线性节点列表扩展为节点级出口策略、选项路由映射和条件分支配置。
- `expedition-flow-fsm`: 远征 FSM 的节点推进与状态迁移来源改为运行时队列与节点级路由决策，而不是固定索引递增。

## Impact

- 主要影响远征配置定义与生成结果：
  - `Configs/GameConfig/Defines/`
  - `Configs/GameConfig/Datas/`
  - `UnityProject/Assets/GameScripts/HotFix/GameProto/GameConfig/Gameplay/Expedition/`
- 主要影响远征运行时代码：
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/`
- 可能影响远征 UI 对节点信息、后续路径和事件结果的展示方式：
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/UI/ExpeditionMainUI/`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/UI/EventCardUI/`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/UI/ExpeditionResultUI/`
- 人工协作影响：
  - 若 schema 变更需要补充或修改 `xlsx`，必须由用户手工处理，agent 只能给出清单与说明。
