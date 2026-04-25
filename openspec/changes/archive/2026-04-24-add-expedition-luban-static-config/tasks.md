## 1. 远征 Luban schema 定义

- [x] 1.1 在 `Configs/GameConfig/Defines/` 中新增远征相关 schema，建立 `Gameplay.Expedition` 命名空间下的主流程、事件与 Combat 遭遇配置定义
- [x] 1.2 定义远征主表所需的线性节点 bean，包括节点类型与事件/遭遇引用字段
- [x] 1.3 定义事件配置所需的选项与固定效果字段，首版仅包含 `crystal_delta`、`exp_delta`、`hp_delta` 与 `summary`
- [x] 1.4 定义 Combat 遭遇配置所需的敌方 Marble 列表、标题描述与首版胜利奖励字段

## 2. 人工改表协作边界

- [x] 2.1 根据新增 schema 梳理需要补充或修改的 `xlsx` 数据表清单
- [x] 2.2 明确通知用户手工修改 `Configs/GameConfig/Datas/__tables__.xlsx` 与对应远征数据表
- [x] 2.3 在实现说明中再次确认 agent 不创建、不编辑、不填充任何 `xlsx` 文件，等待用户完成手工改表后再继续后续步骤

## 3. Luban 生成与配置接入

- [x] 3.1 在用户完成手工改表后，运行项目既有 Luban 生成流程，产出远征配置代码与开发期 JSON 数据
- [x] 3.2 检查 `UnityProject/Assets/GameScripts/HotFix/GameProto/GameConfig/` 中新增的远征配置生成结果是否符合预期
- [x] 3.3 确认远征配置生成结果与当前 `ConfigSystem` 的加载方式兼容，不引入额外的 JSON/bytes 切换改动

## 4. 远征运行时代码迁移

- [x] 4.1 将远征运行时代码的静态配置来源切换为 Luban 生成配置，替换对硬编码静态路线的直接依赖
- [x] 4.2 将 `ExpeditionFlowController` 的远征启动、节点读取与 Combat 遭遇构建逻辑改为基于远征主表、事件表与遭遇表
- [x] 4.3 保持 `ExpeditionRunState`、`ExpeditionNodeRecord`、`MarblePersistentData` 与 `CombatSessionResult` 等运行态/持久化结构继续手写，不误迁为静态配置类
- [x] 4.4 清理或收敛已失去职责的手写远征静态 config 类与硬编码静态工厂数据

## 5. 验证与交付

- [x] 5.1 验证最小远征仍能按配置化的线性 `EventNode -> CombatNode` 路线推进
- [x] 5.2 验证事件选项效果、Combat 遭遇输入与远征结算回写在配置切换后保持可用
- [x] 5.3 进行编译与基础运行验证，确认远征配置迁移未破坏现有最小循环
- [x] 5.4 整理实现结果与人工改表前置条件，确保后续 agent 能清楚知道自己的修改边界

## 人工改表交接清单

- `Configs/GameConfig/Datas/__tables__.xlsx`
  - 注册 `TbExpedition`
  - 注册 `TbExpeditionEvent`
  - 注册 `TbExpeditionCombatEncounter`
- `Configs/GameConfig/Datas/expedition.xlsx`
  - 维护远征主表数据，至少包含 `expedition_id`、`name`、`description` 与线性 `route`
- `Configs/GameConfig/Datas/expedition_event.xlsx`
  - 维护事件表数据，至少包含 `event_id`、基础文案、选项列表和固定效果字段
- `Configs/GameConfig/Datas/expedition_combat_encounter.xlsx`
  - 维护 Combat 遭遇表数据，至少包含 `combat_encounter_id`、标题描述、奖励与敌方 Marble 列表

> 协作边界：agent 不允许创建、编辑、填充或修改任何 `xlsx` 文件。后续实现必须等待用户手工完成以上表格修改后才能继续执行第 3 阶段及之后的任务。

## 验证结论

- 用户已完成 Unity 内最小远征回归验证，确认配置化的线性 `EventNode -> CombatNode` 路线推进正常。
- 用户已确认事件选项效果、Combat 遭遇输入与远征结算回写在配置切换后保持可用。
- 本地 `dotnet build UnityProject/GameLogic.csproj -nologo` 已通过；当前 change 的实现、验证与交付任务均已完成，可进入归档。
