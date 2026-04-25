## ADDED Requirements

### Requirement: Marble 局外数据 SHALL 独立于 Combat Runtime 保存
系统 SHALL 使用独立的 Marble 持久化数据对象保存局外状态，并与 `MarbleRuntimeData` 保持分离。

#### Scenario: 持久化数据与 Runtime 分离
- **WHEN** 系统加载入口层可见的 Marble 数据
- **THEN** 系统读取的是局外 Marble 持久化数据
- **AND** 该数据对象不依赖 Combat 场景中的 `MarbleRuntimeData`

### Requirement: 远征 SHALL 使用 Marble 持久化快照参与运行
系统 SHALL 在远征开始时，将局外 Marble 持久化数据复制为本次远征使用的 Marble 快照，并在远征过程中只修改快照。

#### Scenario: 远征开始时复制快照
- **WHEN** 玩家发起一次远征
- **THEN** 系统将参战 Marble 的持久化数据复制为远征内快照
- **AND** 后续事件与 Combat 结果只作用于这些快照对象

### Requirement: 远征结算 SHALL 将快照结果回写到 Marble 持久化数据
系统 SHALL 在远征结算阶段，将远征内 Marble 快照的最终状态回写到局外 Marble 持久化数据。

#### Scenario: 结算回写生命与经验
- **WHEN** 一次远征进入结算阶段
- **THEN** 系统将每个参战 Marble 的最终生命、经验和死亡状态从快照回写到持久化数据

#### Scenario: 未参战 Marble 不受影响
- **WHEN** 系统完成一次远征结算
- **THEN** 未参与本次远征的 Marble 持久化数据保持不变
