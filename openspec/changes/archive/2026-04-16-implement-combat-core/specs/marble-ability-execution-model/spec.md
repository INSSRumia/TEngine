## MODIFIED Requirements

### Requirement: Marble host SHALL only iterate subscribed polling abilities
Marble 宿主 MUST 仅遍历声明参与对应执行阶段的 Ability 集合，不得在每一帧对所有已挂载 Ability 做统一调用后再由各 Ability 自行空返回。对于显式结算触发点，宿主 MUST 额外维护与事件阶段对应的 Ability 分类缓存，并仅分发订阅该事件阶段的能力集合。

#### Scenario: Update dispatch skips non-update abilities
- **WHEN** 宿主执行一帧逻辑更新
- **THEN** 仅声明参与逻辑帧更新的 Ability 会被调用

#### Scenario: Fixed dispatch skips event-only abilities
- **WHEN** 宿主执行一帧物理更新
- **THEN** 仅声明参与物理帧更新的 Ability 会被调用，事件型 Ability 不会进入该轮询

#### Scenario: Event-stage dispatch uses dedicated subscribed cache
- **WHEN** 宿主显式触发一次伤害、治疗或护盾结算阶段
- **THEN** 宿主必须只遍历订阅该结算阶段的 Ability 缓存集合，而不是扫描全部已挂载 Ability 再逐个做接口判断

### Requirement: Marble state resolution SHALL support explicit trigger points
Marble 战斗域 MUST 提供显式结算触发点，用于在状态变化后触发离散逻辑，例如伤害结算、死亡判定和升级检测。此类逻辑 MUST 不依赖逐帧被动轮询才能生效。对于多阶段战斗结算，显式触发点 MUST 允许宿主在单次请求中按确定顺序完成接收、计算、应用与后续判定。

#### Scenario: Damage resolution triggers after pending values change
- **WHEN** Marble 的待处理伤害或治疗值发生变化并进入结算阶段
- **THEN** 宿主或结算入口必须显式触发对应能力完成生命与护盾结算

#### Scenario: Exp gain triggers level-up check
- **WHEN** Marble 的经验值被增加
- **THEN** 系统必须在该次经验变更后触发升级检测，而不是等待后续任意一帧轮询

#### Scenario: Single request completes full staged resolution
- **WHEN** 宿主接收到一次需要阶段化处理的伤害或治疗请求
- **THEN** 系统必须在该次显式触发过程中完成全部必需结算阶段与后续判定，而不是把单次结算拆散到多个未来轮询帧中
