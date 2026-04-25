## Context

`marble.xml` 已经为 Marble 等级配置定义了 `lst_equipment_id`，并且装备进一步区分为 `ArmorConfig`、`WeaponConfig` 和 `BowConfig`。这说明装备不是简单的静态数值附加，而是会随 Marble 等级生成、挂载到固定槽位，并在运行时参与减伤、碰撞伤害与主动攻击逻辑。

当前 Marble 战斗域已经具备 `RuntimeData + ASC<T> + Ability<T> + Factory` 的基本模式，也已支持基于接口能力的分流执行模型。但装备系统尚未落地，缺少宿主、运行时数据、工厂和行为能力，无法承接弓自动瞄准与射击、盾牌先吃伤害等配置语义。

同时，当前设计约束明确要求运行时数据尽量不存对象引用，因此装备与 Marble 的归属关系需要放在宿主对象层，而不是写入 `EquipmentRuntimeData`。

## Goals / Non-Goals

**Goals:**
- 建立一套可挂在 Marble 上的轻量 Equipment 宿主系统。
- 让装备运行时数据保持纯数据，不存 `Owner` 等对象引用。
- 支持护具、普通武器、弓类武器三类装备的分层运行时行为。
- 让 MarbleFactory 在生成 Marble 时，能按配置自动生成并挂载装备。
- 为后续投射物、索敌和更复杂武器逻辑预留扩展点。

**Non-Goals:**
- 本次不实现完整投射物系统、美术表现或 UI 展示。
- 不引入完整背包/换装/掉落系统，装备来源仅基于 Marble 等级配置。
- 不在本次设计中实现所有武器种类，只优先覆盖通用武器和弓的最小行为模型。

## Decisions

### 1. 装备采用“轻量附属宿主”而不是完整独立战斗单位
- 决策：新增 `Equipment<T>` 宿主，复用 `ASC<T>` 与 Ability 模式，但将其定位为依附于 Marble 的附属对象，而不是第二套 Marble。
- 原因：配置中的装备语义要求独立行为，但又明显从属于 Marble，轻量宿主最符合当前战斗架构。
- 备选方案：
  - 方案 A：把所有装备行为继续塞进 Marble Ability。缺点是弓、盾牌等行为会让 Marble 逻辑膨胀。
  - 方案 B：让装备成为与 Marble 等级等价的独立实体。缺点是过重，不符合槽位附属关系。

### 2. Owner 引用放在 Equipment 宿主，不进入 RuntimeData
- 决策：`Equipment` 持有 `OwnerMarble`，`EquipmentRuntimeData` 仅保留 `ConfigId`、`Slot`、状态值和纯数据字段。
- 原因：满足“数据类尽量不存引用”的约束，也利于后续存档、复制和调试。
- 备选方案：
  - 在 `RuntimeData` 中直接存 `Marble` 引用。实现简单，但污染数据层。
  - 只存 `OwnerInstId`。适合同步/回放，但当前最小 Demo 不必先做。

### 3. 运行时数据按配置分层，而行为靠 Ability 组合
- 决策：建立 `EquipmentRuntimeData`、`ArmorRuntimeData`、`WeaponRuntimeData`、`BowRuntimeData` 四层数据结构；行为由 Ability 组合决定，而不是为每种装备类型都写一个重量级宿主子类。
- 原因：配置分层已经非常明确，数据按配置分层最自然；行为层用 Ability 组合更符合当前 Marble 模式。
- 备选方案：
  - 全部装备只用一个数据类。缺点是字段会迅速膨胀，Bow 专属字段会污染普通武器。

### 4. 装备初始化由 MarbleFactory 驱动，具体创建交给 EquipmentFactory
- 决策：`MarbleFactory` 在创建 Marble 后读取当前等级的 `lst_equipment_id`，并委托 `EquipmentFactory` 按配置类型实例化装备并挂载到 Marble。
- 原因：装备来源已经绑定在 Marble 等级配置上，把装配时机放在 Marble 创建流程里最一致。
- 备选方案：
  - 战斗开始后再单独扫描补挂。会打散初始化时序。

### 5. 防具、武器、弓按能力链分层处理
- 决策：
  - 防具：区分“不可破坏减伤”和“可破坏吸收伤害”
  - 武器：支持碰撞伤害与冷却
  - 弓：拆成索敌、瞄准、射击三段行为
- 原因：这与配置字段完全对应，且能避免一个“万能武器 Ability”承担过多职责。
- 备选方案：
  - 为 Bow 直接写一个单类处理全部逻辑。短期可行，但不利于后续扩展其他主动武器。

## Risks / Trade-offs

- [Risk] 装备宿主加入后，Marble 与 Equipment 间的触发链会变复杂  
  → Mitigation：统一由 `EquipmentFactory` 和挂载约定管理关系，避免外部随意拼装。

- [Risk] 弓类逻辑会隐式引入目标选择、投射物和冷却状态，导致最小实现范围扩大  
  → Mitigation：先在 spec 中定义边界，实施阶段优先做最小可用的索敌+瞄准+发射入口，投射物表现后补。

- [Risk] 防具伤害拦截会和 Marble 现有伤害链冲突  
  → Mitigation：明确防具参与的是 Marble 伤害进入 `PendingDamage` 之前的拦截阶段，避免与最终结算能力重复扣减。

- [Risk] 装备跟随槽位同步如果设计过早绑定表现层，后续改 prefab 结构会受限  
  → Mitigation：将槽位定义为逻辑挂点，位置偏移和具体表现留给独立同步 Ability 或配置扩展。
