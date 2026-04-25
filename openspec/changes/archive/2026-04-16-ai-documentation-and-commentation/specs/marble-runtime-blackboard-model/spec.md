## MODIFIED Requirements

### Requirement: MarbleRuntimeData SHALL act as a structured blackboard root
`MarbleRuntimeData` MUST 继续作为 Marble 能力系统统一访问的数据黑板根对象，但内部必须显式划分为运行时状态、配置投影和帧临时数据三个区域，而不是继续以单层平铺字段承载所有语义。该黑板模型 ALSO MUST 具备对应文档或关键注释说明，使阅读者可以直接理解 `State / Config / Frame` 的职责差异。

#### Scenario: Ability reads runtime data through a single blackboard root
- **WHEN** Marble Ability 需要读取或写入 Marble 相关数据
- **THEN** 它必须仍然可以通过统一的 `MarbleRuntimeData` 根对象访问数据
- **AND** 不需要额外查找多个外部分散的数据宿主

#### Scenario: Blackboard areas have clear semantic boundaries
- **WHEN** 开发者查看 `MarbleRuntimeData`
- **THEN** 必须能够清楚区分哪些字段属于状态数据、哪些字段属于配置投影、哪些字段属于帧临时数据
- **AND** 不应继续把三类语义混在同一层级的平铺字段中

#### Scenario: Reader understands blackboard zones from documentation
- **WHEN** AI 或开发者阅读 Runtime 黑板相关文档或关键注释
- **THEN** 它必须能够直接理解 `State / Config / Frame` 的用途和典型数据类型
- **AND** 不需要只靠字段名猜测黑板分区职责
