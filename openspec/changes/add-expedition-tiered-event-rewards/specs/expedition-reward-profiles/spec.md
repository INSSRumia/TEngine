## ADDED Requirements

### Requirement: 远征 SHALL 定义独立的奖励档位配置
系统 MUST 支持独立的 `ExpeditionRewardProfileConfig`，用于定义一条远征在不同进度阶段下的奖励强度，而不是要求事件直接写死 money、经验、生命或招募数量。

#### Scenario: 远征引用奖励档位配置
- **WHEN** 系统读取一条远征主配置
- **THEN** 该远征能够引用一个奖励档位配置
- **AND** 运行时使用该奖励档位配置解析事件奖励的真实强度

### Requirement: 奖励档位 SHALL 同时区分奖励档位与远征进度阶段
系统 MUST 用“奖励档位”和“远征进度阶段”共同决定真实奖励值。首版进度阶段 MUST 至少支持 `early`、`mid`、`late` 三段。

#### Scenario: 同一奖励档位在不同阶段解析不同数值
- **WHEN** 两个 Effect 都请求相同的奖励档位
- **AND** 它们分别发生在远征的前期和后期
- **THEN** 系统解析出的真实奖励值可以不同
- **AND** 不要求事件配置直接写死这两个阶段的具体数值

### Requirement: 奖励解析 SHALL 先依赖远征上下文而非全局公式
系统 MUST 先通过当前远征配置和当前远征进度来解析奖励强度，不要求首版引入基地等级、全局经济或队伍总战力等复杂全局缩放因素。

#### Scenario: 从远征上下文解析奖励
- **WHEN** 运行时需要把奖励档位解析成真实数值
- **THEN** 系统根据当前远征的 reward profile 和当前进度阶段完成解析
- **AND** 不要求读取局外基地等级或全队战力才能完成首版解析

### Requirement: 招募奖励 SHALL 使用加权候选池
系统 MUST 支持在 `ExpeditionRewardProfileConfig` 中定义招募奖励候选池。每个候选条目 MUST 至少包含一个 `MarbleSpawnConfig`、一个权重，以及其所属奖励档位。

#### Scenario: 从匹配档位的候选池抽取招募结果
- **WHEN** 一个招募 Effect 请求某个奖励档位
- **THEN** 系统从 reward profile 中筛选出同档位的招募候选条目
- **AND** 系统按权重抽取实际的 MarbleSpawnConfig 结果

### Requirement: 同一事件文案 SHALL 能跨不同远征复用
系统 MUST 允许同一个事件文案在不同远征中复用，而事件奖励的真实强度由当前远征的 reward profile 决定。

#### Scenario: 相同事件在不同远征中给出不同强度奖励
- **WHEN** 同一个事件配置被两个不同远征使用
- **AND** 两条远征引用不同的 reward profile
- **THEN** 该事件在这两条远征中解析出的真实奖励值可以不同
- **AND** 不要求为奖励强度差异复制两份事件文案
