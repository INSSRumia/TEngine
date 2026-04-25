## ADDED Requirements

### Requirement: 远征 SHALL 支持随机事件池配置
系统 SHALL 支持通过配置定义随机事件池。每个随机事件池 MUST 包含若干可抽取事件条目，每个条目 MUST 至少引用一个 `EventConfigId` 并提供大于等于 0 的权重。

#### Scenario: 读取随机事件池配置
- **WHEN** 运行时初始化远征随机事件池
- **THEN** 系统读取池中的事件条目
- **AND** 每个条目能够提供事件配置引用和抽取权重

### Requirement: 随机事件池 SHALL 按池内无放回抽取
系统 SHALL 为每次远征运行维护随机事件池的剩余条目状态。同一个池子内已经被抽中的条目 MUST 从该池的剩余可抽条目中移除；不同池子中配置了相同 Event 时，MUST 允许这些条目分别被抽中。

#### Scenario: 同一池内事件不重复抽取
- **WHEN** 某个随机事件池中的一个条目被抽中
- **THEN** 该条目从该池本次远征的剩余可抽条目中移除
- **AND** 后续从同一池抽取时不会再次抽到该条目

#### Scenario: 不同池中的相同事件可以分别出现
- **WHEN** 两个随机事件池都配置了相同 `EventConfigId`
- **THEN** 系统允许这两个池的对应条目在本次远征中分别被抽中
- **AND** 不通过事件历史对 `EventConfigId` 做全局去重

### Requirement: 随机事件抽取 SHALL 先按激活池总权重选池再选事件
系统 MUST 从当前激活且仍有可抽条目的随机事件池中进行加权抽取。抽取流程 MUST 先计算所有有效激活池的总权重并定位命中的池，再使用转换后的池内局部权重抽取具体事件条目。

#### Scenario: 从多个激活池中抽取事件
- **WHEN** 当前远征存在多个非空激活随机事件池
- **THEN** 系统先按所有池的剩余条目总权重定位一个池
- **AND** 系统再在命中的池中按剩余条目权重抽取一个事件

#### Scenario: 跳过空池和无效权重池
- **WHEN** 某个激活随机事件池没有剩余条目或总权重小于等于 0
- **THEN** 系统不把该池纳入本次随机抽取范围

### Requirement: RandomEvent 节点 SHALL 使用激活随机事件池提供事件内容
系统 SHALL 支持 `RandomEvent` 远征节点类型。进入该节点时，系统 MUST 从当前激活随机事件池抽取一个 Event，并使用抽到的 Event 内容进行展示和选项结算。

#### Scenario: RandomEvent 节点抽到事件
- **WHEN** 远征流程进入 `RandomEvent` 节点且存在可抽事件
- **THEN** 系统从激活随机事件池抽取一个 Event
- **AND** 系统展示该 Event 的标题、描述和选项
- **AND** 玩家选择后执行该选项配置的 Effect 列表

#### Scenario: RandomEvent 节点无事件可抽
- **WHEN** 远征流程进入 `RandomEvent` 节点且所有激活池都没有可抽事件
- **THEN** 系统跳过事件展示和选项选择
- **AND** 系统按该节点的默认后续路由继续推进

### Requirement: RandomEvent 节点 SHALL 保持 Node 路由语义
系统 MUST 让 `RandomEvent` 节点使用抽到的 Event 作为内容模板，但后续路由 MUST 由当前 `RandomEvent` 节点自身的路由策略决定，而不是由被抽到的 Event 决定。

#### Scenario: 随机抽到的 Event 不决定出口
- **WHEN** `RandomEvent` 节点抽到一个可被多个节点复用的 Event
- **THEN** 该 Event 只提供内容与选项效果
- **AND** 当前 `RandomEvent` 节点根据自己的路由策略决定下一节点
