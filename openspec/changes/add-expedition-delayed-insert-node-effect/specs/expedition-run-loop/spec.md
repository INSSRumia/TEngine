## ADDED Requirements

### Requirement: 远征运行态 SHALL 维护待到期插入节点列表
系统 SHALL 在 `ExpeditionRunState` 中维护 `LstPendingInsertNode` 或等价运行时结构，用于保存所有尚未到期的延迟插入节点请求。每个请求 MUST 至少包含剩余经过节点数、目标节点类型、目标标识、来源节点信息与稳定顺序信息。

#### Scenario: 远征运行时保存待插入请求
- **WHEN** 一个延迟插入节点 Effect 执行成功
- **THEN** 系统把对应请求保存到本次远征运行态的待插入列表
- **AND** 该请求不会直接替换当前节点或破坏当前主线后继

### Requirement: 待插入节点请求 SHALL 在每次节点结算后递减并在到期时插入队首
系统 MUST 在每个节点完成结算后，对所有待插入请求的剩余经过节点数统一递减一次。任一请求在递减后达到 `0` 时，系统 MUST 立即把对应临时节点插入 `PendingNodeQueue` 最前面。

#### Scenario: 计数为 1 的请求成为下一个节点
- **WHEN** 当前节点结算后存在一条 `passed_node_count = 1` 的待插入请求
- **THEN** 系统在本轮递减后将其变为 `0`
- **AND** 系统立即把该临时节点插入 `PendingNodeQueue` 最前面
- **AND** 该临时节点成为下一个执行节点

#### Scenario: 计数为 2 的请求在再经过一个节点后生效
- **WHEN** 当前节点结算后存在一条 `passed_node_count = 2` 的待插入请求
- **THEN** 系统在当前轮递减后保留该请求
- **AND** 仅在后续再完成一个节点结算后，系统才把该请求对应的临时节点插入队首

### Requirement: 同位到期的多个临时节点 SHALL 按登记顺序插入并以后登记者优先执行
系统 MUST 在多个待插入请求于同一节点结算轮次一起到期时，按待插入列表中的登记顺序依次执行“插入队首”操作。由于每次插入目标都是队首，后登记的请求对应节点 MUST 先于先登记的请求执行。

#### Scenario: 多个请求在同一位置一起到期
- **WHEN** 两条或多条待插入请求在同一节点结算后同时达到 `0`
- **THEN** 系统按它们在待插入列表中的顺序依次插入队首
- **AND** 最终执行顺序表现为后登记的临时节点先执行

### Requirement: 临时插入节点 SHALL 以隐式 fixed_next 语义返回原流程
系统 MUST 将由延迟插入请求生成的节点视为运行时临时节点，而不是要求它们事先存在于静态 `Route` 中。临时节点不配置 `transition` 与 `option_routes`，执行完成后 SHALL 默认回到 `PendingNodeQueue` 中原本已排队的后续节点继续推进。

#### Scenario: 临时 event 节点执行后返回原流程
- **WHEN** 一个临时 `event` 节点完成结算
- **THEN** 系统继续执行 `PendingNodeQueue` 中原本排在其后的节点
- **AND** 不要求该临时节点额外声明独立出口

#### Scenario: 临时 combat 节点执行后返回原流程
- **WHEN** 一个临时 `combat` 节点胜利结束
- **THEN** 系统继续执行 `PendingNodeQueue` 中原本排在其后的节点
- **AND** 不要求该临时节点额外声明独立出口
