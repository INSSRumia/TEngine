## ADDED Requirements

### Requirement: Combat 会话请求 SHALL 携带选定场地
系统 SHALL 在远征发起 Combat 会话前完成场地选择，并通过 `CombatSessionRequest` 或等价桥接对象携带选定场地配置 Id。远征侧 MUST 不直接解析 `CombatBattlefieldConfig`。

#### Scenario: 请求携带显式场地
- **WHEN** Combat 遭遇配置显式指定场地
- **THEN** 远征侧 Combat 会话请求携带该场地信息

#### Scenario: 请求携带环境随机场地
- **WHEN** Combat 遭遇未显式指定场地
- **AND** 系统从当前环境随机选择了场地
- **THEN** 远征侧 Combat 会话请求携带随机选中的场地信息

### Requirement: Combat 层 SHALL 根据桥接场地加载作战空间
Combat 层 SHALL 使用远征侧会话请求提供的场地配置 Id，通过 `BattlefieldFactory` 在 `Assets/AssetRaw/Actor/Prefabs/Battlefield` 目录下加载同名作战空间 prefab，并在该场地中生成参战 Marble。

#### Scenario: Combat 使用请求场地
- **WHEN** Combat 层收到包含场地信息的会话请求
- **THEN** Combat 层按场地配置 Id 加载同名 prefab
- **AND** Combat 层在加载出的场地中生成玩家和敌方 Marble
