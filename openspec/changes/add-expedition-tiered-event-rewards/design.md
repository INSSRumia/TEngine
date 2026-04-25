## Context

当前远征事件已经具备配置化 Event、统一的 `IExpeditionEffect` 执行接口，以及 money / 经验 / 生命 / 招募 / 环境切换等基础 Effect。但是这些 Effect 的奖励强度仍然直接写死在事件配置中，导致同一个事件文案虽然可以在不同远征中复用，却无法随着远征长度、远征进度和整体游戏阶段自然缩放。

这次改动是一次有意的破坏性重构。用户明确接受不保留旧字段兼容性，因此目标不是在旧固定值方案上叠加补丁，而是直接把“事件奖励数值”抽象成“奖励档位”，再由远征运行态结合奖励 profile 解析成真实结果。

约束与前提：

- 所有产出物必须使用简体中文。
- 不允许 agent 创建或修改任何 xlsx；schema 调整后必须停下通知用户手工改表。
- 当前远征代码已经有 `ExpeditionRunState`、事件 Effect 管线、`AddPlayerMarbleEffect` 和环境系统，可以承接奖励上下文与招募候选池逻辑。
- 当前 summary 机制已经存在，但仍以 Effect 自己拼接文本为主，需要升级为命名 token 替换。

## Goals / Non-Goals

**Goals:**

- 将远征事件奖励从固定值改为档位化配置。
- 为每条远征引入独立的 `ExpeditionRewardProfileConfig`，定义前期 / 中期 / 后期奖励强度。
- 让 money、经验、生命、招募数量都可以按 reward tier 解析。
- 为招募奖励提供加权候选池，而不是在事件里写死固定兵种。
- 为 Effect summary 提供命名 token 替换能力，支持 `{money}`、`{count}`、`{marble_name}` 这类模板。
- 保持事件文案层和奖励强度层解耦，让同一事件可跨远征和跨阶段复用。

**Non-Goals:**

- 本次不引入公式脚本、表达式解释器或通用数学公式语言。
- 本次不实现基于玩家基地等级、全局经济或队伍总战力的复杂缩放公式。
- 本次不实现 Buff、Item、Flag、动态插入事件与奖励 profile 的联动。
- 本次不修改 xlsx 数据，也不在本任务书中推进实际代码实现。

## Decisions

### 决策 1：采用破坏性重构，移除旧固定 delta 字段

方案：

- 直接废弃 `money_delta`、`exp_delta`、`hp_delta`、`count` 这类固定值字段在事件奖励中的主表达方式。
- 改为统一使用“缩放值配置 + reward profile 解析”结构。

原因：

- 用户明确说明当前几乎没有旧数据，可以接受破坏性方案。
- 双轨兼容会让 schema、Luban 生成代码、运行时 Effect 都变得更脏，后续维护成本高。

备选方案：

- 保留固定字段，并增加新字段。
  不采用，因为会延长旧方案寿命，增加策划与实现双重心智负担。

### 决策 2：将奖励强度定义为“档位 + 进度阶段”，而不是直接公式化

方案：

- 引入 reward tier，例如 `tiny / small / medium / large / huge`。
- 引入 progress stage，例如 `early / mid / late`。
- 每个远征 reward profile 为每种奖励类型配置三段强度表。

原因：

- 配表可读性强，方便策划直接理解和调数。
- 相比公式语言更容易验证，不会把当前系统复杂度一下拉高。
- 先满足“前期事件后期仍可复用”的核心诉求，再考虑更复杂缩放。

备选方案：

- 直接用通用公式或脚本表达式。
  不采用，因为过早引入表达式系统会增加调试、验证和文案理解成本。

### 决策 3：新增独立 capability `expedition-reward-profiles`

方案：

- 将 reward profile 作为新能力，而不是塞进 `expedition-effect-system` 或 `expedition-luban-static-config` 里。
- 它负责定义奖励档位、阶段划分、招募候选池与运行时解析行为。

原因：

- 奖励 profile 是一套独立的领域概念，既有配置结构，也有运行时解析逻辑。
- 独立 capability 能让 spec 边界更清楚，后续扩展也更自然。

备选方案：

- 把奖励 profile 仅作为 `expedition-luban-static-config` 的附属配置。
  不采用，因为那会只描述“表长什么样”，无法完整覆盖解析规则。

### 决策 4：招募奖励使用加权候选池，而不是裸 `list<MarbleSpawnConfig>`

方案：

- `ExpeditionRewardProfileConfig` 包含招募候选池条目。
- 每个条目至少包含：`MarbleSpawnConfig`、`weight`、`reward_tier`。
- `AddPlayerMarbleEffect` 声明自己请求的招募档位，运行时从匹配档位的候选池中加权抽取。

原因：

- 裸列表只支持等概率抽取，很快会变得不可控。
- 奖励档位和招募质量天然相关，应该把“招什么兵”交给 reward profile 决定。

备选方案：

- 让事件自己直接写死某个 `MarbleSpawnConfig`。
  不采用，因为这会让事件重新绑定具体数值强度和奖励质量，破坏事件复用目标。

### 决策 5：summary 使用“命名 token 替换”，不使用反射

方案：

- `summary` 支持命名占位符，例如 `{money}`、`{count}`、`{marble_name}`。
- 每个 Effect 在执行后提供自己的 token 字典，统一交给模板替换器完成渲染。

原因：

- 反射对字段名重构极其脆弱，不适合热更和配置驱动系统。
- 命名 token 可读、稳定、可测试，也更方便让不同 Effect 自己控制文案语义。

备选方案：

- 继续使用单一 `{value}`。
  不采用，因为多值 Effect 无法表达。

- 用反射自动读取字段名替换。
  不采用，因为维护风险高、对象转字符串语义不可控。

### 决策 6：奖励上下文先只依赖远征自身信息

方案：

- 第一版 reward context 仅依赖当前远征配置、当前节点推进进度、当前 progress stage。
- 不把玩家基地等级、全局经济、全队总战力纳入首版缩放输入。

原因：

- 当前诉求是事件跨远征、跨阶段复用，而不是建立整套全局经济公式。
- 先用远征内可稳定获得的上下文，能更快落地并降低歧义。

备选方案：

- 一开始就接入局外养成和全队战力。
  不采用，因为这会扩大影响面并增加调平复杂度。

## Risks / Trade-offs

- [Risk] 破坏性重构会让现有事件配置全部需要重填  
  → Mitigation：在 tasks 中明确“先改 schema，再停下通知用户改表”，避免半兼容状态。

- [Risk] reward profile 配得过粗会让不同事件奖励手感过于接近  
  → Mitigation：保留 `tiny / small / medium / large / huge` 五档，并允许不同奖励类型分别调表。

- [Risk] 招募候选池按档位抽取后，事件个性可能下降  
  → Mitigation：事件仍保留文案语义，reward profile 只决定“强度与兵种质量”，不决定事件内容。

- [Risk] summary token 若缺失会导致玩家可见文案异常  
  → Mitigation：定义统一模板替换规则，缺失 token 时输出空字符串或回退默认文本，并在调试日志中记录。

- [Risk] 当前 effect summary 和结果汇总链路会被这次改动波及  
  → Mitigation：把模板解析集中在 Effect 执行上下文或公共工具中，不让每个调用点各自拼接。

## Migration Plan

1. 修改 `proposal/design/specs`，明确破坏性重构边界。
2. 修改 `expedition.xml` schema，引入 reward profile、缩放值配置与招募候选池结构。
3. 立即暂停，并通知用户需要更新哪些 xlsx 与哪些字段。
4. 用户手工修改表格并重新生成 Luban 代码后，再修改运行时 Effect 与 reward resolver。
5. 迁移第一批事件配置到档位化方案，验证早中后期奖励解析是否合理。

回滚策略：

- 若实现阶段发现 reward profile 结构不合理，可回滚整个 change，而不是尝试与旧固定字段混合并存。

## Open Questions

- progress stage 最终按节点推进百分比划分，还是按战斗序号 / 远征段落划分，需要用户最终拍板。
- 招募奖励是否仅缩放数量，还是允许 reward profile 同时控制兵种等级与兵种质量，需要在实现前细化。
- summary token 的缺失处理是静默置空还是保留原样，需要统一规则。
