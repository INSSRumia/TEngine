## ADDED Requirements

### Requirement: 远征静态内容 SHALL 由 Luban 生成配置定义
系统 MUST 将远征的静态路线、事件与 Combat 遭遇内容定义在 `Gameplay.Expedition` 命名空间下的 Luban schema 中，并通过生成配置代码提供给运行时读取。

#### Scenario: 运行时读取远征主配置
- **WHEN** 系统需要根据远征标识启动一次远征
- **THEN** 系统从 Luban 生成的远征主表读取该远征的静态配置
- **AND** 不再依赖硬编码工厂中的静态路线常量作为唯一数据源

### Requirement: 远征主表 SHALL 使用线性节点列表表达最小路线
系统 MUST 使用远征主表中的线性节点列表表达首版最小远征路线，每个节点通过显式类型和显式引用指向事件或 Combat 遭遇内容。

#### Scenario: 线性节点按配置顺序定义
- **WHEN** 某条远征被配置为最小线性路线
- **THEN** 其节点列表按配置中的顺序表达推进路径
- **AND** 每个节点通过节点类型区分事件节点与 Combat 节点

### Requirement: 事件配置 SHALL 使用固定效果字段
系统 MUST 让远征事件选项的效果以固定字段表达，首版至少支持晶体变化、经验变化、生命变化和结果摘要文本。

#### Scenario: 事件选项配置固定字段效果
- **WHEN** 运行时读取某个事件选项的配置
- **THEN** 该选项可直接提供 `crystal_delta`、`exp_delta`、`hp_delta` 与 `summary` 这类固定字段
- **AND** 不要求通过额外多态事件效果系统才能解释首版最小事件结果

### Requirement: Combat 遭遇配置 SHALL 提供敌方编队与奖励
系统 MUST 让远征 Combat 遭遇配置显式提供敌方 Marble 列表、标题描述以及首版胜利奖励信息，用于构建 Combat 请求。

#### Scenario: 从遭遇配置构建 Combat 输入
- **WHEN** 远征流程进入一个 Combat 节点
- **THEN** 系统根据节点引用读取对应的 Combat 遭遇配置
- **AND** 该配置能够提供敌方 Marble 列表与首版胜利奖励数据
