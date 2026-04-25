## MODIFIED Requirements

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
