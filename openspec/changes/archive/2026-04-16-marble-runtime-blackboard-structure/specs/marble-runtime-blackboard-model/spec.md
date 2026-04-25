## ADDED Requirements

### Requirement: MarbleRuntimeData SHALL act as a structured blackboard root
`MarbleRuntimeData` MUST 继续作为 Marble 能力系统统一访问的数据黑板根对象，但内部必须显式划分为运行时状态、配置投影和帧临时数据三个区域，而不是继续以单层平铺字段承载所有语义。

#### Scenario: Ability reads runtime data through a single blackboard root
- **WHEN** Marble Ability 需要读取或写入 Marble 相关数据
- **THEN** 它必须仍然可以通过统一的 `MarbleRuntimeData` 根对象访问数据
- **AND** 不需要额外查找多个外部分散的数据宿主

#### Scenario: Blackboard areas have clear semantic boundaries
- **WHEN** 开发者查看 `MarbleRuntimeData`
- **THEN** 必须能够清楚区分哪些字段属于状态数据、哪些字段属于配置投影、哪些字段属于帧临时数据
- **AND** 不应继续把三类语义混在同一层级的平铺字段中

### Requirement: Frame-only controller objects SHALL be isolated in frame data
所有仅用于帧内聚合、控制、缓存或中间计算的控制器对象 MUST 被放入 Marble 的帧临时数据区域，而不是继续与可同步状态字段混放。

#### Scenario: Priority managers are stored as frame data
- **WHEN** Marble 运行时需要维护 `PriorityValueManager` 或等价控制器对象
- **THEN** 这些对象必须位于帧临时数据区域
- **AND** 不得继续作为普通状态字段直接平铺在 `MarbleRuntimeData` 顶层

#### Scenario: Frame data is rebuildable
- **WHEN** 系统需要重新初始化 Marble 的帧内控制环境
- **THEN** 帧临时数据区域中的控制器对象必须能够被本地重新创建
- **AND** 不要求它们本身成为长期持久状态

### Requirement: Config-derived base values SHALL remain accessible through the blackboard
由配置初始化得到、但运行期仍需要被能力广泛读取的基础值 MUST 继续留在 Runtime 黑板体系中，但必须收敛到专门的配置数据区域。

#### Scenario: Ability reads config-derived attack and defense
- **WHEN** Marble Ability 需要读取基础攻击、防御或其他配置投影值
- **THEN** 它必须能通过 Runtime 黑板中的配置数据区域读取这些值
- **AND** 不需要额外直接访问外部配置表对象
