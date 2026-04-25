## Why

当前最小远征循环已经能够跑通，但远征静态内容仍主要由 `ExpeditionModels.cs` 与 `ExpeditionStaticRouteFactory` 中的手写 config 类和硬编码数据承载，这会持续放大后续扩展事件、遭遇和路线时的维护成本。现在需要把远征静态配置正式迁入 Luban，让远征外围内容与现有战斗配置体系保持一致，同时明确 agent 不允许直接创建或修改 `xlsx`，避免自动改表带来的沟通与数据失真问题。

## What Changes

- 将当前远征中的静态 config 类改为由 Luban schema 定义并自动生成，不再继续手写远征节点、事件、遭遇配置数据类。
- 为远征新增一套配置化静态结构，采用“远征主流程表 + 事件表 + 战斗遭遇表”的拆分方式。
- 将远征节点改为线性 `route` 配置，由远征主表中的节点列表按顺序驱动最小循环。
- 保持事件效果第一版为最小固定字段结构，仅支持 `crystal_delta`、`exp_delta`、`hp_delta` 与 `summary` 这类简单结果表达。
- 调整远征运行时代码的静态配置来源，使 `ExpeditionFlowController` 和相关桥接逻辑改为读取 Luban 生成配置，而不是依赖 `ExpeditionStaticRouteFactory` 中的硬编码内容。
- 明确本次变更中的人工边界：agent 不允许创建、编辑或填充任何 `xlsx` 数据表；当 schema 变更需要同步数据表时，必须明确通知用户手工修改。

## Capabilities

### New Capabilities
- `expedition-luban-static-config`: 定义远征静态配置的 Luban schema、表拆分规则和运行时代码消费边界。

### Modified Capabilities
- `expedition-run-loop`: 远征最小循环的静态路线、事件与遭遇来源改为 Luban 配置，而不是手写工厂数据。

## Impact

- 主要影响配置定义与生成链路：
  - `Configs/GameConfig/Defines/`
  - `Configs/GameConfig/Datas/__tables__.xlsx`
  - `Configs/GameConfig/Datas/`
  - `UnityProject/Assets/GameScripts/HotFix/GameProto/GameConfig/`
- 主要影响远征运行时代码：
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/ExpeditionModels.cs`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/ExpeditionFlowController.cs`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/CombatSessionModels.cs`
- 可能影响远征入口与节点展示层对静态文案和节点信息的读取方式：
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/UI/ExpeditionMainUI/`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/UI/EventCardUI/`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/UI/ExpeditionResultUI/`
- 人工协作影响：
  - 若实现过程中需要补表、改表或新增 `xlsx` 数据，agent 必须暂停并通知用户手工处理，不能自行修改表格文件。
