## ADDED Requirements

### Requirement: 玩家可以从最小入口发起一次远征
系统 SHALL 提供一个最小入口界面，用于发起一次远征；该入口界面在本次变更中不承担完整家园玩法，只负责进入远征流程。

#### Scenario: 从入口开始远征
- **WHEN** 玩家在入口界面点击开始远征
- **THEN** 系统创建一份新的 `ExpeditionRunState`
- **AND** 系统将本次入口选择的 Marble 持久化数据复制为远征内快照
- **AND** 远征流程进入准备完成后的首个节点推进阶段

### Requirement: 远征 SHALL 按线性节点顺序推进
系统 SHALL 将一次最小远征表示为一条线性节点链，并按节点顺序依次推进。

#### Scenario: 最小路线推进
- **WHEN** 系统启动一条最小远征路线
- **THEN** 远征节点按既定顺序推进
- **AND** 最小版本至少支持 `EventNode -> CombatNode` 这一条基础路线

### Requirement: 远征 SHALL 为每个节点保存执行记录
系统 SHALL 为本次远征中的每个节点生成一份 `ExpeditionNodeRecord`，用于记录节点实际执行后的选择、结果与产出。

#### Scenario: 事件节点记录结果
- **WHEN** 玩家完成一个事件节点选择
- **THEN** 系统为该节点写入一份 `ExpeditionNodeRecord`
- **AND** 记录中包含被选择的事件选项标识
- **AND** 记录中包含该节点带来的资源或 Buff 结果

#### Scenario: Combat 节点记录结果
- **WHEN** 一场 Combat 节点结束
- **THEN** 系统为该节点写入一份 `ExpeditionNodeRecord`
- **AND** 记录中包含本次 `CombatSessionResult`

### Requirement: 远征结算 SHALL 返回入口并回写结果
系统 SHALL 在远征完成后返回最小入口界面，并把本次远征的最终结果写回入口层可见的数据。

#### Scenario: 远征完成后返回入口
- **WHEN** 最后一条节点记录处理完成并进入远征结算
- **THEN** 系统计算本次远征的最终结果
- **AND** 系统将资源收益与 Marble 状态变化回写到局外数据
- **AND** 系统返回入口界面并展示本次远征结果摘要

### Requirement: 远征相关界面 SHALL 按 TEngine UIWindow 流程接入
系统 SHALL 让远征入口、事件卡和结算界面以 TEngine `UIWindow` 方式接入，并通过 `UIModule` 驱动显示、隐藏与刷新。

#### Scenario: 打开远征入口界面
- **WHEN** 系统需要显示远征入口与出征准备界面
- **THEN** 系统通过 `UIModule` 打开 `ExpeditionMainUI`
- **AND** 界面逻辑通过 `UIWindow` 生命周期完成节点绑定、事件注册与刷新

#### Scenario: 打开事件或结算界面
- **WHEN** 远征流程切换到事件阶段或结算阶段
- **THEN** 系统通过 `UIModule` 打开对应的 `EventCardUI` 或 `ExpeditionResultUI`
- **AND** 不使用绕开 `UIWindow/UIModule` 的临时界面控制方式

### Requirement: 首版远征界面 SHALL 以功能验证为主，不依赖最终美术资源
系统 SHALL 允许远征相关界面使用功能型占位节点与占位视觉资源完成流程验证，而不要求等待最终美术资源齐备。

#### Scenario: 使用功能型占位界面验证流程
- **WHEN** 首版远征界面尚未接入最终美术资源
- **THEN** 系统仍可通过占位 UI 完成入口、事件与结算流程验证
- **AND** 节点命名、交互回调和状态展示保持可用
