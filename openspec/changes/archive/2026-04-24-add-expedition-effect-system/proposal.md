## Why

当前远征中的事件与 Combat 奖励效果仍依赖少量固定字段表达，例如 `crystal_delta`、`exp_delta`、`hp_delta` 与胜利奖励数值，这会让远征内容扩展很快碰到表达上限。现在需要把这些固定字段升级为可组合的 Expedition Effect 体系，并统一代码中的资源命名为 `money`，让远征配置和运行时逻辑都更清晰、可扩展。

## What Changes

- 为远征新增 `IExpeditionEffect` 执行模型，并以配置驱动方式支持不同效果类型的创建与执行。
- 将 Event Option 中的固定效果字段替换为 `LstEffect`，由配置自由组合多个 Expedition Effect。
- 将 Combat 遭遇中的固定胜利奖励字段替换为两组效果列表：战斗胜利触发的 `LstVictoryEffect` 与战斗失败触发的 `LstDefeatEffect`。
- 首批实现 3 种远征 Effect：添加金钱、为玩家全队添加经验、为玩家全队修改血量。
- **BREAKING** 将远征相关代码中的内部资源命名从 `crystal` 统一替换为 `money`，玩家展示文案仍可继续使用“晶体”。
- 调整远征事件与 Combat 结果应用逻辑，使节点结果改为通过 Effect 执行上下文驱动，而不是写死在控制器内的固定数值加减逻辑。

## Capabilities

### New Capabilities
- `expedition-effect-system`: 定义远征 Effect 的接口、执行上下文、配置映射与首批基础 Effect 能力。

### Modified Capabilities
- `expedition-luban-static-config`: 远征事件与 Combat 遭遇配置从固定数值字段改为 Effect 列表配置，并统一资源字段语义为 `money`。
- `expedition-run-loop`: 远征节点结果应用改为通过 Expedition Effect 执行，支持事件选项和 Combat 胜负分别触发配置化效果列表。

## Impact

- 主要影响远征配置定义与生成结果：
  - `Configs/GameConfig/Defines/`
  - `Configs/GameConfig/Datas/`
  - `UnityProject/Assets/GameScripts/HotFix/GameProto/GameConfig/Gameplay/Expedition/`
- 主要影响远征运行时代码：
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/Gameplay/Expedition/`
- 可能影响远征 UI 展示层对资源字段和结果摘要的读取：
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/UI/ExpeditionMainUI/`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/UI/EventCardUI/`
  - `UnityProject/Assets/GameScripts/HotFix/GameLogic/UI/ExpeditionResultUI/`
- 影响后续配置 authoring 方式：
  - Event Option 与 Combat 遭遇不再填固定奖励字段，而是配置 Effect 列表
