## MODIFIED Requirements

### Requirement: 事件配置 SHALL 使用 Effect 列表
系统 MUST 让远征事件选项的结果继续通过 `LstEffect` 表达，但奖励型 Effect 的配置内容 MUST 使用档位化奖励结构，而不是继续使用写死的 `money_delta`、`exp_delta`、`hp_delta` 或固定招募数量字段。事件配置负责声明奖励类型和奖励档位，不直接承担真实数值强度定义。

#### Scenario: 事件选项配置档位化奖励 Effect
- **WHEN** 运行时读取某个事件选项的配置
- **THEN** 该选项通过 `LstEffect` 提供一个或多个 Expedition Effect 配置
- **AND** 奖励型 Effect 在配置中声明奖励档位或缩放值结构
- **AND** 不要求在事件配置里直接写死真实奖励数值

## ADDED Requirements

### Requirement: Luban 配置 SHALL 定义远征奖励档位结构
系统 MUST 在远征 schema 中提供奖励档位相关配置结构，用于描述奖励类型、奖励档位、进度阶段和真实值映射关系。

#### Scenario: 配置远征奖励档位
- **WHEN** 配置者定义一条远征的 reward profile
- **THEN** 该配置能够为至少 `money`、`exp`、`hp`、`marble_count` 这几类奖励定义档位强度
- **AND** 该配置能够区分 `early`、`mid`、`late` 三段进度

### Requirement: Luban 配置 SHALL 定义可复用的缩放值配置
系统 MUST 在远征 schema 中提供可复用的缩放值配置结构，供奖励型 Effect 声明其请求的奖励档位，而不是各自重复发明字段。

#### Scenario: 多种奖励型 Effect 复用同一缩放值结构
- **WHEN** 配置者定义 money、exp、hp 或招募类 Effect
- **THEN** 这些 Effect 都能够复用统一的缩放值配置结构
- **AND** 不要求每种 Effect 分别定义一套完全独立的档位字段命名

### Requirement: Luban 配置 SHALL 定义招募奖励候选池
系统 MUST 在远征 schema 中提供招募奖励候选池结构。每个候选条目 MUST 至少包含一个 `MarbleSpawnConfig`、一个 `weight` 和一个 `reward_tier`。

#### Scenario: 配置招募奖励候选条目
- **WHEN** 配置者定义 reward profile 中的招募奖励候选池
- **THEN** 每个候选条目能够声明一条具体的 `MarbleSpawnConfig`
- **AND** 每个候选条目能够声明其权重和所属奖励档位

### Requirement: 远征主配置 SHALL 能引用 reward profile
系统 MUST 允许远征主配置显式引用一个 reward profile，用于控制该远征中的事件奖励强度。

#### Scenario: 远征配置 reward profile
- **WHEN** 配置者定义一条远征
- **THEN** 该远征能够引用一个 reward profile 配置 Id
- **AND** 运行时可通过该引用解析事件奖励的真实强度

### Requirement: Agent SHALL NOT 修改 xlsx 表格
实现该变更的 agent MUST NOT 创建、编辑、填充或修改任何 xlsx 表格。若 schema 变更需要表格新增 sheet、列或数据，agent MUST 暂停并通知用户手工修改。

#### Scenario: schema 变更需要表格配合
- **WHEN** 实现者修改 Luban xml schema 后发现需要更新 xlsx 内容
- **THEN** 实现者停止继续依赖生成代码的实现工作
- **AND** 实现者向用户列出需要修改的表格、sheet 和字段
- **AND** 等待用户修改表格并重新生成代码后再继续
