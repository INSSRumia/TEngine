## MODIFIED Requirements

### Requirement: 系统 SHALL 支持首批 3 种基础远征 Effect
系统 MUST 支持添加金钱、为玩家全队添加经验、为玩家全队修改血量，以及为玩家队伍添加 Marble 这 4 种基础远征 Effect。上述奖励型 Effect MUST 通过统一的奖励解析上下文，把事件中声明的奖励档位解析为真实结果，而不是继续依赖事件内写死的固定数值字段。

#### Scenario: 执行添加金钱 Effect
- **WHEN** 系统执行一个添加金钱的远征 Effect
- **THEN** 该 Effect 通过当前远征的奖励档位配置解析出真实 `money` 数值
- **AND** 该 Effect 更新远征或局外状态中的 `money` 相关字段

#### Scenario: 执行为玩家全队添加经验 Effect
- **WHEN** 系统执行一个为玩家全队添加经验的远征 Effect
- **THEN** 该 Effect 通过当前远征的奖励档位配置解析出真实经验值
- **AND** 该 Effect 对当前远征中的玩家 Marble 快照统一增加经验值

#### Scenario: 执行为玩家全队修改血量 Effect
- **WHEN** 系统执行一个为玩家全队修改血量的远征 Effect
- **THEN** 该 Effect 通过当前远征的奖励档位配置解析出真实生命变化值
- **AND** 该 Effect 对当前远征中的玩家 Marble 快照统一修改生命值
- **AND** 系统同步更新对应的死亡状态

#### Scenario: 执行添加 Marble Effect
- **WHEN** 系统执行一个为玩家队伍添加 Marble 的远征 Effect
- **THEN** 该 Effect 通过当前远征的奖励档位配置解析出真实招募数量和招募候选结果
- **AND** 该 Effect 将新增 Marble 加入当前远征的玩家队伍快照

## ADDED Requirements

### Requirement: 奖励型 Expedition Effect SHALL 使用统一的奖励解析上下文
系统 MUST 为奖励型 Expedition Effect 提供统一的奖励解析上下文，用于解析当前 reward profile、当前远征进度阶段以及对应奖励类型的真实值。

#### Scenario: 奖励型 Effect 读取奖励解析上下文
- **WHEN** 运行时执行一个奖励型远征 Effect
- **THEN** 该 Effect 能通过统一上下文读取当前远征的 reward profile 与当前进度阶段
- **AND** 不要求每个 Effect 自己重复拼装这些上下文信息

### Requirement: Effect summary SHALL 支持命名 token 替换
系统 MUST 允许 Expedition Effect 的 summary 使用命名 token 模板，例如 `{money}`、`{count}`、`{marble_name}`。每个 Effect MUST 自己提供本次执行可替换的 token 值，而不是依赖反射自动读取字段。

#### Scenario: AddMoneyEffect 渲染 summary token
- **WHEN** 一个添加金钱 Effect 的 summary 使用 `{money}`
- **THEN** 系统用本次解析出的真实金钱值替换该 token
- **AND** 渲染后的 summary 与实际结算值一致

#### Scenario: AddPlayerMarbleEffect 渲染多个 token
- **WHEN** 一个添加 Marble Effect 的 summary 使用 `{count}` 和 `{marble_name}`
- **THEN** 系统分别用本次解析出的招募数量和实际抽中的 Marble 名称替换这些 token
- **AND** 不要求把多个字段压缩成单一 `{value}` 才能显示
