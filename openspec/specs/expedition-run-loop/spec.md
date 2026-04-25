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

### Requirement: 远征运行态 SHALL 维护当前环境
系统 SHALL 在远征运行态中保存当前环境配置 Id。该状态 MUST 在远征开始时由远征初始环境设置，并可被环境切换 Effect 更新。

#### Scenario: 初始化当前环境
- **WHEN** 系统创建远征运行态
- **THEN** 当前环境来自远征配置的初始环境

#### Scenario: 更新当前环境
- **WHEN** 改变环境的 Expedition Effect 执行成功
- **THEN** 远征运行态保存的新当前环境可被后续随机事件与 Combat 场地选择读取

### Requirement: 远征运行态 SHALL 维护激活随机事件池
系统 SHALL 在远征运行态中维护当前激活随机事件池集合，并区分池来源。远征基础池 MUST 在远征开始时激活且不因环境切换被移除；环境池 MUST 随当前环境进入和离开而添加或移除。

#### Scenario: 初始化激活池
- **WHEN** 系统创建远征运行态
- **THEN** 远征基础随机事件池被激活
- **AND** 初始环境提供的随机事件池被激活

#### Scenario: 环境切换维护激活池
- **WHEN** 当前环境发生变化
- **THEN** 系统只移除旧环境来源的随机事件池
- **AND** 系统保留远征基础随机事件池
- **AND** 系统添加新环境来源的随机事件池

### Requirement: 远征流程 SHALL 推进 RandomEvent 节点
系统 SHALL 在远征流程进入 `RandomEvent` 节点时执行随机事件抽取，并根据抽取结果进入事件展示或跳过节点。

#### Scenario: RandomEvent 节点进入事件展示
- **WHEN** `RandomEvent` 节点成功抽到一个事件
- **THEN** 远征流程进入事件展示状态
- **AND** 当前展示内容来自抽到的事件配置

#### Scenario: RandomEvent 节点空池跳过
- **WHEN** `RandomEvent` 节点没有抽到事件
- **THEN** 远征流程不进入事件展示状态
- **AND** 远征流程按该节点的默认后续路由继续推进

### Requirement: RandomEvent 节点记录 SHALL 保存实际抽到的事件
系统 SHALL 在节点记录中保存 `RandomEvent` 节点实际抽到的事件配置 Id。若节点因无可抽事件被跳过，记录 MUST 能表达本次节点未展示事件。

#### Scenario: 记录随机事件节点结果
- **WHEN** 玩家完成一个成功抽到事件的 `RandomEvent` 节点
- **THEN** 系统记录该节点实际抽到的 Event 配置 Id
- **AND** 系统记录玩家选择和 Effect 执行摘要

#### Scenario: 记录随机事件节点跳过
- **WHEN** `RandomEvent` 节点因无可抽事件被跳过
- **THEN** 系统记录该节点未抽到事件或被跳过
- **AND** 系统不记录不存在的玩家选项选择
