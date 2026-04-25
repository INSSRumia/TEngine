## ADDED Requirements

### Requirement: Entity registration SHALL be centrally managed by EntityModule
The system SHALL provide an `EntityModule` that owns registration and deregistration of all managed `Entity` instances. A managed `Entity` MUST be registered exactly once during its alive period and MUST be removed when destroyed.

#### Scenario: Entity registers on creation
- **WHEN** a managed `Entity` instance is created and reaches its Unity bridge initialization point
- **THEN** it is added to `EntityModule` managed collection exactly once

#### Scenario: Entity deregisters on destruction
- **WHEN** a managed `Entity` is destroyed
- **THEN** `EntityModule` removes it from managed collection before subsequent lifecycle dispatch

### Requirement: Entity lifecycle SHALL be dispatched by EntityModule instead of Unity order
The system SHALL define custom entity lifecycle callbacks and MUST dispatch them through `EntityModule` in framework-controlled phases. Business initialization and update logic for managed entities MUST run in module-dispatched callbacks, not by relying on Unity `Awake/Start/Update` ordering. When an entity hosts extensible sub-behaviors such as combat abilities, the entity host MUST support dispatching only the sub-behaviors that subscribe to the active lifecycle phase instead of invoking all sub-behaviors uniformly.

#### Scenario: Module dispatches update phase
- **WHEN** framework update loop advances one frame
- **THEN** `EntityModule` dispatches entity update callback for each valid managed entity according to ordering rules

#### Scenario: Unity lifecycle used only as bridge
- **WHEN** a managed `Entity` executes Unity `Awake` and `OnDestroy`
- **THEN** these callbacks only perform registration bridge actions and do not host cross-entity business sequencing

#### Scenario: Host dispatches only subscribed sub-behaviors
- **WHEN** a managed entity enters a lifecycle phase that supports sub-behavior dispatch
- **THEN** the entity host invokes only the sub-behaviors that explicitly subscribe to that phase and skips those declared as event-only

### Requirement: Entity execution order SHALL follow priority with stable tie-break
Each managed `Entity` SHALL expose a numeric `Priority` value. `EntityModule` MUST dispatch lifecycle callbacks in ascending priority order. For entities with equal priority, dispatch order MUST be stable and deterministic based on registration sequence.

#### Scenario: Higher priority entity executes earlier
- **WHEN** two managed entities have priorities 10 and 20
- **THEN** the entity with priority 10 receives lifecycle callback before priority 20 in the same phase

#### Scenario: Equal priority keeps deterministic order
- **WHEN** two managed entities share the same priority and registration order is A then B
- **THEN** dispatch order remains A then B across frames unless either entity is removed

### Requirement: Lifecycle dispatch SHALL be safe under runtime collection mutation
`EntityModule` MUST tolerate runtime additions/removals during lifecycle iteration. The module SHALL avoid direct mutation of the active iteration collection and MUST ensure newly added entities and pending removals are applied at defined safe points.

#### Scenario: Add entity during dispatch
- **WHEN** a managed entity is registered while a lifecycle phase is currently being iterated
- **THEN** the new entity is queued and joined at the next safe merge point without breaking current iteration

#### Scenario: Remove entity during dispatch
- **WHEN** a managed entity is deregistered while a lifecycle phase is currently being iterated
- **THEN** it is marked for removal and is not invoked in subsequent eligible phases after cleanup point

### Requirement: Module shutdown SHALL guarantee lifecycle cleanup
On module shutdown, `EntityModule` MUST invoke shutdown lifecycle callback for remaining valid managed entities exactly once and then clear all managed state.

#### Scenario: Shutdown triggers entity cleanup
- **WHEN** framework enters module shutdown stage with managed entities still registered
- **THEN** each valid managed entity receives shutdown callback once and module containers are cleared
