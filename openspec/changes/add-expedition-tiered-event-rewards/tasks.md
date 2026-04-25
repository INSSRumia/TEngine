## 1. Schema 与配置结构

- [x] 1.1 阅读本 change 的 `proposal.md`、`design.md` 和全部 `specs/**/*.md`，确认范围只包含远征奖励档位化、reward profile、招募候选池与 summary token，不实现公式系统、Buff、Item 或全局战力缩放。
- [x] 1.2 检查当前 `expedition.xml`、相关 Effect 运行时和远征运行态，确认旧固定 delta 字段的影响范围与需要删除的配置结构。
- [x] 1.3 修改 `Configs/GameConfig/Defines/expedition.xml`，加入 reward tier、progress stage、scaled value、reward profile、招募候选池与远征引用 reward profile 的 schema 结构。
- [x] 1.4 以破坏性方案重构奖励型 Effect 的配置结构，移除旧的固定 delta / count 方案，不保留双轨兼容。
- [x] 1.5 完成 schema 修改后立即暂停，不创建、编辑、填充或修改任何 xlsx 表格。
- [x] 1.6 向用户列出需要手工修改的表格、sheet、字段与示例含义，等待用户修改表格并重新生成 Luban 代码。

## 2. 生成代码后的运行时改造

- [x] 2.1 用户确认表格已修改且 Luban 代码已重新生成后，检查生成代码中 reward profile、scaled value 与招募候选池类型是否齐全。
- [x] 2.2 在远征运行时中加入统一的奖励解析上下文，至少包含当前远征配置、reward profile 和当前远征进度阶段。
- [x] 2.3 实现统一的 reward resolver，把档位化奖励解析为真实 money、经验、生命变化值和招募数量。
- [x] 2.4 实现招募候选池解析逻辑，按 reward tier 从候选条目中筛选并按权重抽取 `MarbleSpawnConfig`。
- [x] 2.5 修改 `AddMoneyEffect`、`AddPlayerMarbleExpEffect`、`AddPlayerMarbleHpEffect`、`AddPlayerMarbleEffect`，使其通过 reward resolver 获取真实奖励而不是直接读取固定值字段。
- [x] 2.6 确保 `AddPlayerMarbleEffect` 把解析出的 Marble 奖励加入当前远征队伍快照，并继续沿用现有结算流程回写到局外数据。

## 3. Summary 与文本渲染

- [x] 3.1 为 Expedition Effect summary 增加命名 token 替换能力，例如 `{money}`、`{count}`、`{marble_name}`。
- [x] 3.2 让各奖励型 Effect 在执行后提供自己的 summary token 字典，不使用反射自动读取字段。
- [x] 3.3 约定 token 缺失时的回退行为，并在调试日志或 summary 处理中保持结果可诊断。

## 4. 验证与事件迁移

- [x] 4.1 选取至少 2 到 3 个现有事件样例，验证相同事件文案在不同远征 reward profile 下可解析出不同强度奖励。
- [x] 4.2 验证同一远征的 early / mid / late 三段能够让相同奖励档位解析出不同结果。
- [x] 4.3 验证招募奖励会从匹配档位的候选池中按权重抽取实际 Marble，而不是继续写死固定兵种。
- [x] 4.4 验证 summary 模板中的 `{money}`、`{count}`、`{marble_name}` 等 token 能正确渲染为本次实际结算值。
- [x] 4.5 运行可用的编译或静态检查流程；如因 Unity 或 csproj 未刷新导致问题，停止并通知用户回 Unity 刷新，不手写 `.meta` 文件。
