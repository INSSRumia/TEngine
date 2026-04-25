## Why

当前远征已经具备最小流程、分支路由和 Effect 扩展点，但随机遭遇、环境变化和 Combat 场地仍缺少统一配置与运行时规则。引入随机事件池、环境和场地后，远征可以从线性配置扩展为更有变化的可复用内容系统，同时继续保持 Event 内容与 Node 路由语义分离。

## What Changes

- 新增随机事件池配置与运行时抽取规则：远征可配置多个基础随机事件池，环境也可提供随机事件池，运行时从当前激活池中按权重抽取事件。
- 新增 `RandomEvent` 远征节点类型：进入该节点时从激活随机事件池中抽取一个 Event 内容来展示；若所有池子都已空或无可用权重，则跳过该节点并继续路由。
- 随机事件池按“池内无放回”运行：同一个池子内抽中过的条目不会再次从该池抽出；不同池子中配置了相同 Event 时，仍可分别被抽中。
- 新增环境配置与运行态：远征配置可指定初始环境，环境可携带随机事件池与场地列表，并可通过 Expedition Effect 改变当前环境。
- 新增改变环境的 Expedition Effect：切换环境时移除旧环境提供的随机事件池，添加新环境提供的随机事件池；已抽出或已插入的事件不回滚。
- 新增场地配置与 Combat 场地选择规则：Combat 遭遇可显式指定场地；未指定时从当前环境的场地列表按权重随机选择，场地抽取是放回的。
- 暂不实现环境 Buff：环境 Buff 与阵营/标签交互留给后续 GameTag 与 Buff 系统，本次只保留设计扩展点。
- 不允许 agent 创建、编辑或填充任何 xlsx 表格；当 schema 变化需要表格内容配合修改时，必须暂停并告知用户需要修改哪些表格字段。

## Capabilities

### New Capabilities

- `expedition-random-event-pools`: 定义随机事件池配置、激活池运行态、池内无放回加权抽取和 `RandomEvent` 节点行为。
- `expedition-environments`: 定义远征环境配置、初始环境、环境切换 Effect、环境提供随机事件池与场地列表的运行规则。
- `combat-battlefields`: 定义 Combat 场地配置、遭遇显式场地选择、环境默认场地加权选择和向 Combat 会话传递场地信息的要求。

### Modified Capabilities

- `expedition-luban-static-config`: 增加随机事件池、环境、场地、随机事件节点、初始环境与 Combat 遭遇可选场地相关 schema 要求。
- `expedition-run-loop`: 增加随机事件节点推进、激活随机事件池维护、环境运行态维护和空池跳过行为。
- `expedition-effect-system`: 增加改变当前环境的 Expedition Effect 类型。
- `combat-session-bridge`: 增加远征侧向 Combat 会话请求传递选定场地配置的要求。

## Impact

- 影响 Luban schema：`Configs/GameConfig/Defines/expedition.xml` 增加远征环境与随机事件池；Combat Battlefield 配置归属 `Gameplay.Combat` 命名空间，远征侧只引用 `battlefield_config_id`。
- 影响 Luban 生成代码使用方：远征配置读取、节点构建、Combat 遭遇读取和 Effect 工厂。
- 影响远征运行态：需要记录当前环境、激活随机事件池、各池剩余可抽条目和随机事件节点本次抽到的 Event。
- 影响 Combat 桥接：`CombatSessionRequest` 或等价请求对象只携带场地配置 Id，Combat 层负责解析 `CombatBattlefieldConfig` 与创建场地。
- 影响 Unity 资源加载：场地 prefab 从 `Assets/AssetRaw/Actor/Prefabs/Battlefield` 下按 `battlefield_config_id` 同名规则加载；如需创建或修改 Unity 资源，应通过 Unity MCP 操作，不手写 `.meta` 文件。
