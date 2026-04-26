## ADDED Requirements

### Requirement: 环境 SHALL 配置动态敌人候选池
系统 SHALL 支持环境配置声明任意数量的动态敌人候选条目。每个候选条目 MUST 至少包含一个 `MarbleSpawnConfig` 和一个权重。

#### Scenario: 读取环境敌人候选池
- **WHEN** 运行时读取某个环境配置
- **THEN** 系统能够获取该环境的动态敌人候选池
- **AND** 每个候选条目能够提供候选敌人类型与抽取权重

### Requirement: 当前环境 SHALL 成为动态敌人类型来源
系统 SHALL 在远征运行态中把当前环境视为动态敌人类型的来源。进入 Combat 时，若遭遇包含动态敌人组，系统 MUST 从当前环境的敌人候选池中生成具体敌人类型。

#### Scenario: Combat 使用当前环境作为敌人来源
- **WHEN** 远征当前环境为环境 A
- **AND** 当前 Combat 遭遇存在动态敌人组
- **THEN** 系统从环境 A 的敌人候选池中抽取动态敌人类型
- **AND** 系统不要求 Combat 遭遇自身重复维护一套敌人类型候选列表
