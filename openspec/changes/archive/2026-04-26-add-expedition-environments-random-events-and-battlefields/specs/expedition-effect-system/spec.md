## ADDED Requirements

### Requirement: 远征 Effect SHALL 支持改变环境
系统 MUST 支持一种改变当前环境的 Expedition Effect。该 Effect MUST 通过配置指定目标环境，并通过统一的 `IExpeditionEffect` 执行入口修改远征运行态。

#### Scenario: 创建改变环境 Effect
- **WHEN** Effect 工厂读取到改变环境的 Effect 配置
- **THEN** 系统创建对应的 Expedition Effect 实例
- **AND** 该实例持有目标环境配置 Id

#### Scenario: 执行改变环境 Effect
- **WHEN** 改变环境 Effect 被执行
- **THEN** 系统更新远征运行态的当前环境
- **AND** 系统触发环境随机事件池的移除与添加规则
