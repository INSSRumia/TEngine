# AI 项目阅读入口

本文档面向首次进入项目的 AI 与开发者，目标是先建立正确的项目心智模型，再进入具体实现文件。

## 1. 推荐阅读顺序

1. `README.md`
   - 了解 TEngine、HybridCLR、YooAsset、Luban 和当前仓库的整体结构。
2. `UnityProject/CLAUDE.md`
   - 了解项目约束、热更边界、模块访问方式、AI 工作流要求，以及必须遵守的代码规范。
3. `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Combat模块说明.md`
   - 建立 Combat 模块整体结构、数据流、Factory 装配模型和 Runtime 黑板认知。
4. `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/远征流程设计.md`
   - 建立配置驱动远征、FSM、随机事件池、分支路由、环境、奖励档位、Effect、UI 约束和 Combat 边界认知。
5. `Configs/GameConfig/Defines/initial.xml`
   - 了解开局选择如何指向当前阵营。
6. `Configs/GameConfig/Defines/camp.xml`
   - 了解初始 money、初始 Marble、可用远征列表和阵营表现数据。
7. `Configs/GameConfig/Defines/expedition.xml`
   - 了解远征路线、事件、随机事件池、环境、奖励档位、Effect 和 Combat 遭遇配置。
8. `Configs/GameConfig/Defines/battlefield.xml`
   - 了解 Combat 场地配置，以及 `battlefield_config_id` 与同名 prefab 的关系。
9. `Configs/GameConfig/Defines/marble.xml`
   - 了解 Marble 等级配置、固定骨架能力和扩展能力入口。
10. `Configs/GameConfig/Defines/equip.xml`
   - 了解装备层级结构、装备骨架能力和类型差异。
11. `Configs/GameConfig/Defines/projectile.xml`
   - 了解发射物骨架能力、追踪模式和扩展能力入口。
12. 关键代码入口
   - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/Data/ExpeditionConfigBridge.cs`
   - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/Data/ExpeditionRunState.cs`
   - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/Data/ExpeditionRouting.cs`
   - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/Effects/ExpeditionEffectFactory.cs`
   - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/Effects/ExpeditionEffectExecutionContext.cs`
   - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Marble/MarbleFactory.cs`
   - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Battlefield/BattlefieldFactory.cs`

## 2. 项目核心目录

- `UnityProject/Assets/GameScripts/HotFix/GameLogic/`
  - 热更主业务代码。后续绝大多数玩法改动都在这里完成。
- `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/`
  - 当前 Combat 系统核心目录，包含 Marble、Equipment、Projectile、Battlefield、Ability、RuntimeData 与工厂装配逻辑。
- `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/`
  - 当前远征主流程目录，包含远征运行时数据、FSM、Effect、随机事件池、环境和 Combat 桥接。
- `UnityProject/Assets/GameScripts/HotFix/GameProto/`
  - Luban 生成的配置类与 `ConfigSystem` 入口。
- `Configs/GameConfig/Defines/`
  - Luban XML schema 定义。这里描述配置结构、字段语义和生成代码的源头。
- `Configs/GameConfig/`
  - 配置输入与生成结果所在区域。修改 schema 时要结合 Excel 与生成流程理解影响面。
- `openspec/specs/`
  - 主规范目录。查看当前系统已经同步落地的正式规则。
- `openspec/changes/`
  - 需求、设计和任务拆解来源。做较大改动前优先查看对应 change。

## 3. Combat 模块的最小心智模型

- `Marble`
  - 战斗主体，拥有 Runtime 黑板、装备列表和核心结算能力。
- `Equipment`
  - 附着在 Marble 上的装备。护甲负责承伤或减伤，武器负责瞄准、伤害计算与发射/碰撞。
- `Projectile`
  - 独立飞行的发射物，不注册到 CombatManager，自行依据 `SourceCombatSide` 做敌我判断。
- `Battlefield`
  - Combat 场地组件，负责按 `CombatSide` 将 Marble 放入双方出生 Bounds。
- `Factory`
  - 负责“实例化 + RuntimeData 初始化 + 固定骨架能力挂载 + 配置扩展能力挂载”。
- `Ability`
  - 战斗行为的最小执行单元。核心能力维持骨架流程，可选能力通过 Luban 配置扩展玩法。
- `RuntimeData`
  - 运行时黑板。Marble 明确拆分为 `State / Config / Frame` 三个区域。

## 4. 进入 Combat 前需要记住的约定

- Factory 装配遵循统一骨架：
  - 先创建实体与 RuntimeData。
  - 先挂固定核心能力。
  - 再挂 `lst_ability` 这类配置驱动扩展能力。
- Marble 的运行时数据不是简单属性集合，而是能力系统共享的黑板根对象。
- Luban XML 里的显式字段通常代表固定骨架能力参数；`lst_ability` 一类列表才是玩法扩展入口。
- 不要仅凭类名猜系统边界，先看模块文档和 schema 注释，再读具体实现。

## 5. 进入 Expedition 前需要记住的约定

- Expedition 层统一使用 `Combat` 术语，不新建并行的 `Battle` 命名体系。
- 局外数据使用 `MarblePersistentData`；远征运行中会基于这些数据建立 `ExpeditionRunState` 内的 Marble 快照。
- 远征只通过 `CombatSessionRequest / CombatSessionResult` 与 Combat 域交互。
- `ExpeditionNodeRecord` 记录节点实际运行结果，不再用 `Progress` 来描述节点执行记录。
- Event 只提供内容和选项，Node 才负责路由出口。
- RandomEvent 节点从当前激活随机事件池抽事件，但出口仍由当前 Node 的 `route_policy` 决定。
- 环境当前只负责随机事件池和场地候选，不实现 Buff / GameTag。
- 奖励值已经改成“事件声明 reward tier，远征 reward profile 解析真实结果”的模式。
- 招募奖励不再在事件里写死固定兵种与数量，而是通过 reward profile 的招募候选池解析。
- Effect summary 支持 `{money}`、`{count}`、`{marble_name}` 这类命名 token。
- UI 必须走 `UIWindow/UIModule` 流程；如果未来要改 Prefab 或 Canvas，优先通过 Unity MCP。

## 6. AI 修改时的优先检查点

- 先确认是改“固定骨架字段”还是“扩展能力列表”。
- 先确认能力由哪个 Factory 挂载，而不是在能力类里反推来源。
- 先确认数据应写入 `State`、`Config` 还是 `Frame`，避免把长期状态和逐帧临时值混在一起。
- 先确认改动是否同时影响 Luban schema、运行时工厂和关键文档。
- 先确认当前问题属于局外层、远征层还是 Combat 层，避免跨层硬连。
- 先阅读 `UnityProject/CLAUDE.md` 中的“代码规范（新增/修改代码必须遵守）”后再开始编码。

## 7. 推荐继续深读的文件

- 远征主流程
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/Controller/ExpeditionFlowController.cs`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/Controller/ExpeditionFlowController.Flow.cs`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/Controller/States/ExpeditionFlowStateEnterNode.cs`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/Controller/States/ExpeditionFlowStateCombat.cs`
- 远征运行态与路由
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/Data/ExpeditionRunState.cs`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/Data/ExpeditionRouting.cs`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/Data/ExpeditionRecord.cs`
- 远征奖励与 Effect
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/Effects/ExpeditionEffectFactory.cs`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/Effects/AddMoneyEffect.cs`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/Effects/AddPlayerMarbleEffect.cs`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/Effects/ExpeditionEffectExecutionContext.cs`
- 战斗结算链路
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Marble/Ability/Core/MarbleReceiveDamageAbility.cs`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Marble/Ability/Core/MarbleDamagePipelineAbility.cs`
- 发射物链路
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Projectile/ProjectileFactory.cs`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Combat/Projectile/ProjectileRuntimeData.cs`
