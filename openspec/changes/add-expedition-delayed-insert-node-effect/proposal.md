## Why

当前远征已经支持 `event`、`random_event`、`combat` 三类节点，也已经具备运行时待执行节点队列，但仍然缺少一种简单、稳定、可配置的方式去表达“在若干节点后插入一个新的事件或战斗”。如果继续把这类需求硬塞进静态主路线或随机事件本体，会让配置迅速变得绕且难复用，因此需要补上一种最小化的延迟插入节点 Effect。

## What Changes

- 为远征新增一种 Expedition Effect，用于在经过指定数量的后续节点后，向本次远征运行时插入一个临时节点。
- 该 Effect 的配置包含两部分：
  - `passed_node_count`：使用 `1` 表示“下一个节点插入”，`2` 表示“下下个节点插入”。
  - `pending_node`：一个简单节点配置，仅包含 `node_type` 与 `id`。
- `pending_node.node_type` 仅支持 `event` 与 `combat`：
  - 当 `node_type = event` 时，`id` 表示 `event_config_id`
  - 当 `node_type = combat` 时，`id` 表示 `combat_encounter_config_id`
- 远征运行态新增 `LstPendingInsertNode` 或等价结构，用于保存所有尚未到期的延迟插入节点请求。
- 每当后续节点完成结算后，系统递减所有待插入项的剩余经过节点数；当某项减到 `0` 时，系统立即把对应临时节点插入到 `PendingNodeQueue` 最前面。
- 同一时刻若有多个待插入节点一起到期，系统按 `LstPendingInsertNode` 中的登记顺序依次插入；由于插入目标是队首，越靠后的节点越先执行。
- 动态插入生成的临时节点不要求出现在静态 `Route` 配置中；其路由语义固定为 `fixed_next`，不配置 `transition` 与 `option_routes`，执行完成后默认回到原有待执行队列继续推进。
- 随机事件池与 `event / random_event / combat` 现有节点体系保持不变；本次能力只补充一种新的延迟后果表达方式，不引入新的节点大类。
- 若后续实现需要调整 Luban schema 与 `xlsx` 数据，agent 只能修改 schema、列出手工改表清单，并等待用户手工修改表格；不得自行编辑任何 `xlsx`。

## Capabilities

### New Capabilities

无

### Modified Capabilities

- `expedition-effect-system`: 新增延迟插入节点 Effect，并定义其配置语义、合法节点类型与执行结果。
- `expedition-run-loop`: 远征运行时新增待插入节点列表，并在每个后续节点结算后递减、到期前插，保证临时节点插队后仍能回到原流程继续推进。
- `expedition-luban-static-config`: 远征 Effect 配置新增延迟插入节点所需的 schema 结构，包括经过节点数与简单节点配置。

## Impact

- 主要影响远征运行时代码：
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/`
- 主要影响远征 Effect 执行链路：
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/Effects/`
- 主要影响远征 Luban schema 与生成配置：
  - `Configs/GameConfig/Defines/`
  - `UnityProject/Assets/GameScripts/HotFix/GameProto/GameConfig/Gameplay/Expedition/`
- 若进入实现阶段，预计需要用户手工补充或调整相关 `xlsx` 表结构与样例数据，但本次计划书阶段不直接修改任何 `xlsx`。
