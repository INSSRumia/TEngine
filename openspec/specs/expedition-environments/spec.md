## ADDED Requirements

### Requirement: 远征 SHALL 支持初始环境
系统 SHALL 允许远征配置声明一个初始环境。开始远征时，运行态 MUST 设置当前环境，并激活该环境提供的随机事件池。

#### Scenario: 开始远征时设置初始环境
- **WHEN** 系统创建一次远征运行态
- **THEN** 系统根据远征配置设置当前环境
- **AND** 系统将当前环境配置的随机事件池加入激活池集合

### Requirement: 环境 SHALL 配置随机事件池和场地列表
系统 SHALL 支持环境配置声明任意数量的随机事件池和任意数量的场地候选项。场地候选项 MUST 支持权重，用于 Combat 未显式指定场地时的随机选择。

#### Scenario: 读取环境随机事件池
- **WHEN** 运行时进入某个环境
- **THEN** 系统读取该环境配置的随机事件池列表
- **AND** 这些池成为后续随机事件抽取的候选来源

#### Scenario: 读取环境场地候选
- **WHEN** 运行时需要从当前环境选择 Combat 场地
- **THEN** 系统读取该环境配置的场地候选列表
- **AND** 每个候选场地提供场地配置引用和权重

### Requirement: 环境切换 SHALL 只影响未来随机事件抽取
系统 SHALL 在环境切换时移除旧环境提供的随机事件池，并添加新环境提供的随机事件池。已经抽出、已经插入待执行队列或已经执行的事件 MUST 不因环境离开而回滚或移除。

#### Scenario: 切换环境更新激活池
- **WHEN** 远征当前环境从环境 A 切换到环境 B
- **THEN** 系统移除环境 A 来源的激活随机事件池
- **AND** 系统添加环境 B 来源的随机事件池

#### Scenario: 环境移除不回滚已插入事件
- **WHEN** 环境 A 的随机事件池曾经抽出并插入一个事件
- **AND** 当前环境离开环境 A
- **THEN** 已插入的事件仍保留在待执行流程中
- **AND** 系统不因为环境 A 被移除而删除该事件

### Requirement: 远征 SHALL 支持通过 Effect 改变当前环境
系统 SHALL 提供改变当前环境的 Expedition Effect。该 Effect 执行后 MUST 更新远征运行态的当前环境，并按环境切换规则维护激活随机事件池。

#### Scenario: 执行改变环境 Effect
- **WHEN** 系统执行一个改变环境的 Expedition Effect
- **THEN** 远征运行态的当前环境变为 Effect 指定的环境
- **AND** 激活随机事件池按照新旧环境来源完成更新

### Requirement: 环境 SHALL 为后续 Buff 系统保留扩展边界
系统 SHALL 将环境作为可被后续 Buff 或 GameTag 系统读取的运行态上下文，但本次变更 MUST 不实现环境 Buff 计算、Buff 应用或 GameTag 匹配。

#### Scenario: 当前版本不应用环境 Buff
- **WHEN** 远征当前环境发生变化
- **THEN** 系统只更新环境运行态、随机事件池和场地候选上下文
- **AND** 系统不要求创建或应用任何环境 Buff
