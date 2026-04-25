## Why

当前远征事件中的 money、经验、生命恢复和招募数量大多直接写死在事件 Effect 中，导致同一批事件文案虽然可以在前中后期复用，但实际奖励强度会随着游戏进度快速失衡。现在需要把事件奖励从“固定值”改成“档位值 + 远征上下文解析”，让事件内容长期可复用，同时保持不同远征和不同阶段的奖励强度合理。

## What Changes

- **BREAKING** 将远征事件中 `AddMoneyEffect`、`AddPlayerMarbleExpEffect`、`AddPlayerMarbleHpEffect`、`AddPlayerMarbleEffect` 的固定数值配置改为档位化奖励配置，不保留旧的固定 delta 字段结构。
- 新增远征奖励档位配置 `ExpeditionRewardProfileConfig`，用于定义不同远征在前期、中期、后期的 money、经验、生命和招募奖励强度。
- 新增招募奖励候选池配置，允许按档位从加权 MarbleSpawnConfig 列表中抽取奖励兵种，而不是在事件里写死具体兵种与数量。
- 新增统一的奖励解析上下文与解析器，根据当前远征配置和远征进度阶段，把奖励档位解析成真实数值或真实招募结果。
- 将 Effect 的 summary 从单一 `{value}` 占位思路升级为命名 token 替换，例如 `{count}`、`{money}`、`{marble_name}`，由各个 Effect 自己提供可替换字段。
- 更新事件设计与配置约定，使事件文案层和奖励强度层解耦，支持前期事件在后期远征中继续使用。

## Capabilities

### New Capabilities
- `expedition-reward-profiles`: 定义远征奖励档位、阶段划分、招募候选池和运行时解析规则。

### Modified Capabilities
- `expedition-effect-system`: 将远征 Effect 从固定数值结算改为支持档位解析与命名 summary token 替换。
- `expedition-luban-static-config`: 扩展远征 schema，加入奖励 profile、缩放值配置和招募候选池配置，并移除旧固定字段方案。

## Impact

- 影响 `Configs/GameConfig/Defines/expedition.xml` 及相关 Luban 生成代码。
- 影响远征 Effect 运行时，包括 `AddMoneyEffect`、`AddPlayerMarbleExpEffect`、`AddPlayerMarbleHpEffect`、`AddPlayerMarbleEffect`、Effect Factory 和 summary 生成逻辑。
- 影响远征运行态，需要提供奖励解析上下文，例如当前远征配置、当前节点进度阶段等。
- 影响事件配置与策划工作流，后续事件不再直接写死奖励数值，而是引用奖励档位。
- 不允许 agent 创建或修改任何 xlsx；如果需要改表，必须停下并通知用户手工修改并重新生成 Luban 代码。
