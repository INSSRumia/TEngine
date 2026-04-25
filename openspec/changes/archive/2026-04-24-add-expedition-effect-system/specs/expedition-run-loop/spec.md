## ADDED Requirements

### Requirement: 远征节点结果 SHALL 通过 Expedition Effect 列表应用
系统 MUST 让事件选项结果与 Combat 胜负结果通过 Expedition Effect 列表应用到远征运行态，而不是继续在流程控制器中仅依赖固定字段分支处理。

#### Scenario: 事件节点通过 Effect 列表应用结果
- **WHEN** 玩家完成一个事件节点选择
- **THEN** 系统执行该选项配置的 `LstEffect`
- **AND** 节点结果通过 Effect 对远征运行态产生变更

#### Scenario: Combat 节点通过胜负 Effect 列表应用结果
- **WHEN** 一场 Combat 节点结束
- **THEN** 系统根据胜负结果执行对应的 `LstVictoryEffect` 或 `LstDefeatEffect`
- **AND** 节点结果通过 Effect 对远征运行态产生变更

## MODIFIED Requirements

### Requirement: 远征 SHALL 为每个节点保存执行记录
系统 SHALL 为本次远征中的每个节点生成一份 `ExpeditionNodeRecord`，用于记录节点实际执行后的选择、结果与产出。节点记录 MUST 能反映由 Expedition Effect 列表带来的结果摘要，而不是只记录固定字段奖励值。

#### Scenario: 事件节点记录结果
- **WHEN** 玩家完成一个事件节点选择
- **THEN** 系统为该节点写入一份 `ExpeditionNodeRecord`
- **AND** 记录中包含被选择的事件选项标识
- **AND** 记录中包含该节点通过 Effect 列表应用后的结果摘要

#### Scenario: Combat 节点记录结果
- **WHEN** 一场 Combat 节点结束
- **THEN** 系统为该节点写入一份 `ExpeditionNodeRecord`
- **AND** 记录中包含本次 `CombatSessionResult`
- **AND** 记录中包含该节点通过胜负 Effect 列表应用后的结果摘要

### Requirement: 远征结算 SHALL 返回入口并回写结果
系统 SHALL 在远征完成后返回最小入口界面，并把本次远征的最终结果写回入口层可见的数据。系统内部 MUST 使用 `money` 表示本次远征带来的资源变化，并在结算时一并回写由 Expedition Effect 修改后的 Marble 状态。

#### Scenario: 远征完成后返回入口
- **WHEN** 最后一条节点记录处理完成并进入远征结算
- **THEN** 系统计算本次远征的最终结果
- **AND** 系统将 `money` 收益与 Marble 状态变化回写到局外数据
- **AND** 系统返回入口界面并展示本次远征结果摘要
