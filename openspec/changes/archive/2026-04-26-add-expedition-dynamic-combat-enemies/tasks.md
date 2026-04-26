## 1. Schema 设计与人工协作边界

- [x] 1.1 阅读本 change 的 `proposal.md`、`design.md` 和全部 `specs/**/*.md`，确认实现范围只包含环境敌人池、敌人强度档位、固定敌人与动态敌人混合遭遇及桥接调整。
- [x] 1.2 检查当前远征、Combat、环境、Luban 配置和桥接代码，确认现有目录结构、命名方式以及固定敌人进入 Combat 的现有路径。
- [x] 1.3 修改 `Configs/GameConfig/Defines/expedition.xml`，加入 enemy profile、环境敌人候选池、动态敌人组和远征引用 enemy profile 的 schema 定义。
- [x] 1.4 完成 schema 修改后立即暂停，不创建、编辑、填充或修改任何 `xlsx` 表格。
- [x] 1.5 向用户列出需要手工修改的表格、sheet、字段和示例含义，等待用户修改表格并重新生成 Luban 代码。
- [x] 1.6 用户确认表格已修改且生成代码已更新后，再继续运行时代码接入；不要自行伪造缺失的生成类。

### 手工改表清单

- `Configs/GameConfig/Datas/expedition.xlsx`
  - 远征主配置增加 `enemy_profile_config_id`
- `Configs/GameConfig/Datas/expedition_environment.xlsx`
  - 环境配置增加 `lst_enemy_candidate`
  - 每个候选条目至少包含 `marble_spawn_config` 和 `weight`
- 如你的表结构已拆分到其他 `xlsx`
  - 需要由用户把上述字段同步加到实际承载远征、环境和 Combat 遭遇数据的表中
- `Configs/GameConfig/Datas/expedition_reward_profile.xlsx`
  - 由于 reward 和 enemy 的阶段档位结构已在 schema 内通用化，原先 reward profile 内部条目如果使用了 `reward_tier` 字段名，需要同步改成 `tier`
  - `lst_money`、`lst_exp`、`lst_hp`、`lst_marble_count` 仍然保持原有业务含义不变，只是内部档位值条目字段名统一
- `Configs/GameConfig/Datas/expedition_combat_encounter.xlsx`
  - Combat 遭遇配置增加 `lst_dynamic_enemy_group`
  - 每个动态敌人组至少包含 `count_tier` 和 `level_tier`
- `Configs/GameConfig/Datas/expedition_enemy_profile.xlsx`
  - 新增敌人强度档位表
  - 至少包含 `enemy_profile_config_id`、`name`、`description`、`lst_enemy_count`、`lst_enemy_level`
  - `lst_enemy_count` 和 `lst_enemy_level` 需要按 `early / mid / late` 分段，并在每段内配置档位到真实值的映射
  - 内部档位值条目统一使用 `tier`，不要再使用 `enemy_tier`
- `Configs/GameConfig/Datas/__tables__.xlsx`
  - 若本地 Luban 流程要求显式注册新增 sheet 或结构引用，由用户手工核对和维护

## 2. 动态敌人配置解析

- [x] 2.1 在远征运行时实现敌人强度档位解析逻辑，能够基于当前远征阶段分别解析动态敌人组的真实数量与真实等级。
- [x] 2.2 实现当前环境敌人候选池读取与加权抽取逻辑，动态敌人类型按放回方式抽取，不维护池内去重状态。
- [x] 2.3 为动态敌人生成流程提供统一的敌方 roster 构建入口，输入为 Combat 遭遇配置、当前环境和当前远征上下文。
- [x] 2.4 生成动态敌人时使用环境候选 `MarbleSpawnConfig` 作为类型基础，并用动态敌人组解析出的等级覆盖候选等级。
- [x] 2.5 当动态敌人组存在但当前环境没有有效敌人候选池时，输出清晰错误并阻止 Combat 静默继续。

## 3. Combat 遭遇与桥接接入

- [x] 3.1 保持现有固定敌人配置继续可用，不因动态敌人组引入而破坏固定敌人路径。
- [x] 3.2 在 Combat 节点发起前，将固定敌人和动态敌人生成结果合并为最终敌方 roster。
- [x] 3.3 调整 `CombatSessionRequest` 或等价桥接对象的组装逻辑，确保 Combat 收到的是已解析完成的敌方 roster，而不是未解析的动态敌人配置。
- [x] 3.4 如有必要，为节点记录或调试日志补充本场 Combat 实际生成的敌方 roster 信息，便于定位配置和抽取结果。

## 4. 验证与回归

- [ ] 4.1 验证只配置固定敌人的 Combat 遭遇仍能按旧逻辑正常进入战斗。
- [ ] 4.2 验证只配置动态敌人组的 Combat 遭遇能够从当前环境敌人池生成敌人。
- [ ] 4.3 验证固定敌人与动态敌人组混合配置时，最终敌方 roster 同时包含两部分结果。
- [ ] 4.4 验证同一环境候选敌人可以在同一场 Combat 中被重复抽中，且不会因一次抽取而从候选池中移除。
- [ ] 4.5 验证相同数量档位和等级档位在不同远征阶段可解析出不同真实值。
- [ ] 4.6 验证缺失环境敌人池时会输出清晰错误并阻止 Combat 静默开始。
- [x] 4.7 运行可用的编译或静态检查；如果问题疑似 Unity 或 csproj 未刷新导致，停止并告知用户去 Unity 刷新，不手写 `.meta` 文件。
