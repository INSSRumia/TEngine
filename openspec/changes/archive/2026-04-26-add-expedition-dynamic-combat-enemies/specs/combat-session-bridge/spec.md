## MODIFIED Requirements

### Requirement: 远征与 Combat SHALL 通过桥接数据包交互
系统 SHALL 使用远征侧请求/结果数据包将远征流程与 `Gameplay.Combat` 模块连接起来，而不是让远征流程直接持有 Combat 场景对象引用。

#### Scenario: 发起 Combat 会话
- **WHEN** 远征流程进入一个 `CombatNode`
- **THEN** 系统构造一份 `CombatSessionRequest`
- **AND** 请求中包含玩家参战 Marble 快照、已解析完成的敌方 Marble roster、当前远征 Buff 和选定场地信息
- **AND** Combat 层不要求自行解析远征动态敌人配置或当前环境敌人池

#### Scenario: 接收 Combat 会话结果
- **WHEN** 一场 Combat 结束
- **THEN** Combat 模块返回一份 `CombatSessionResult`
- **AND** 该结果中包含胜负状态、Marble 结果与本场奖励
