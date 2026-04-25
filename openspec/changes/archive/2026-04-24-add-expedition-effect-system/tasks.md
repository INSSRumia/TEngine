## 1. Expedition Effect schema 与配置模型

- [x] 1.1 在远征 Luban 定义中新增 Expedition Effect 相关抽象配置类型，并建立 `LstEffect`、`LstVictoryEffect`、`LstDefeatEffect` 所需的 schema 结构
- [x] 1.2 将 Event Option 的固定效果字段替换为 `LstEffect`
- [x] 1.3 将 Combat Encounter 的固定胜利奖励字段替换为 `LstVictoryEffect` 与 `LstDefeatEffect`
- [x] 1.4 将远征相关内部资源字段命名从 `crystal` 统一收敛为 `money`

## 2. Expedition Effect 运行时执行体系

- [x] 2.1 新增 `IExpeditionEffect` 接口与统一的 `ExpeditionEffectExecutionContext`
- [x] 2.2 新增 Expedition Effect 工厂，将 Luban 配置映射为运行时 Effect 实例
- [x] 2.3 实现首批 3 种基础 Effect：添加金钱、为玩家全队添加经验、为玩家全队修改血量
- [x] 2.4 让 Effect 执行逻辑直接操作远征领域状态，而不是依赖零散 manager 参数

## 3. 远征节点结果应用改造

- [x] 3.1 将事件选项结果应用逻辑改为执行 `LstEffect`
- [x] 3.2 将 Combat 节点结果应用逻辑改为按胜负执行 `LstVictoryEffect` 或 `LstDefeatEffect`
- [x] 3.3 调整 `ExpeditionNodeRecord` 的结果摘要生成方式，使其能反映 Effect 列表带来的结果
- [x] 3.4 调整远征结算逻辑，统一回写 `money` 与 Effect 修改后的 Marble 状态

## 4. 命名收敛与兼容清理

- [x] 4.1 清理远征运行时代码中仍使用 `crystal` 表示内部金钱资源的字段与方法名
- [x] 4.2 检查 UI 和结果摘要层，确保内部命名改为 `money` 后玩家可见文案仍可继续显示“晶体”
- [x] 4.3 清理不再需要的固定字段处理分支，避免新旧两套结果应用逻辑并存

## 5. 手工改表协作

- [x] 5.1 梳理本次 schema 变更需要用户手工修改的 `xlsx` 表格与字段清单
- [x] 5.2 明确通知用户手工更新远征相关数据表与 `__tables__.xlsx` 注册项
- [x] 5.3 在实现阶段遵守约束：agent 不创建、不编辑、不填充任何 `xlsx` 文件，等待用户完成改表后再继续生成与联调

备注：按当前项目约定，表注册在 `Defines/*.xml` 中完成，这次手工改表仅涉及远征相关数据表本身，不需要修改 `__tables__.xlsx`。

## 6. 验证与回归

- [x] 6.1 验证事件选项可通过 `LstEffect` 正确组合并应用多个 Effect
- [x] 6.2 验证 Combat 节点可根据胜负分别执行不同的 Effect 列表
- [x] 6.3 验证首批 3 种 Effect 在最小远征循环中可正确影响 `money`、经验和血量
- [x] 6.4 进行编译与基础流程验证，确认远征 Effect 体系替换后最小循环仍可正常结算

备注：本次验证包含 Luban 重新生成、生成 JSON 结构核对、Effect 语义脚本模拟与 `dotnet build UnityProject/GameLogic.csproj -nologo` 编译通过；尚未在 Unity Play Mode 中做人工点击回归。
