## ADDED Requirements

### Requirement: Luban 配置 SHALL 定义敌人强度档位结构
系统 MUST 在远征 schema 中提供敌人强度档位相关配置结构，用于描述敌人数量档位、敌人等级档位、远征阶段和真实值映射关系。

#### Scenario: 配置敌人强度档位
- **WHEN** 配置者定义一条远征的 enemy profile
- **THEN** 该配置能够为至少“敌人数量”和“敌人等级”两类强度定义档位映射
- **AND** 该配置能够区分 `early`、`mid`、`late` 三段远征阶段

### Requirement: Luban 配置 SHALL 定义环境敌人候选池结构
系统 MUST 在远征 schema 中提供环境敌人候选池结构。每个候选条目 MUST 至少包含一个 `MarbleSpawnConfig` 和一个 `weight`。

#### Scenario: 配置环境敌人候选条目
- **WHEN** 配置者定义一个环境
- **THEN** 该环境能够声明任意数量的敌人候选条目
- **AND** 每个候选条目能够声明具体候选敌人与抽取权重

### Requirement: Luban 配置 SHALL 定义动态敌人组结构
系统 MUST 在 Combat 遭遇相关 schema 中提供动态敌人组结构。每个动态敌人组 MUST 至少声明一个数量档位和一个等级档位。

#### Scenario: 配置动态敌人组
- **WHEN** 配置者定义一条 Combat 遭遇
- **THEN** 该遭遇能够继续声明固定敌人
- **AND** 该遭遇也能够额外声明任意数量的动态敌人组
- **AND** 每个动态敌人组能够声明数量档位和等级档位

### Requirement: 远征主配置 SHALL 能引用 enemy profile
系统 MUST 允许远征主配置显式引用一个 enemy profile，用于控制该远征中的动态敌人强度。

#### Scenario: 远征配置 enemy profile
- **WHEN** 配置者定义一条远征
- **THEN** 该远征能够引用一个敌人强度档位配置 Id
- **AND** 运行时可通过该引用解析动态敌人的真实数量与等级

### Requirement: Agent SHALL NOT 修改 xlsx 表格
实现该变更的 agent MUST NOT 创建、编辑、填充或修改任何 xlsx 表格。若 schema 变更需要表格新增 sheet、列或数据，agent MUST 暂停并通知用户手工修改。

#### Scenario: schema 变更需要表格配合
- **WHEN** 实现者修改 Luban xml schema 后发现需要更新 xlsx 内容
- **THEN** 实现者停止继续依赖生成代码的实现工作
- **AND** 实现者向用户列出需要修改的表格、sheet 和字段
- **AND** 等待用户修改表格并重新生成代码后再继续
