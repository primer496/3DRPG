# FinalRPG AI Handoff

This document is a project-wide handoff for the next AI agent.

## 1) Project Snapshot

- Engine: `Unity 2022.3.62t7` (`ProjectSettings/ProjectVersion.txt`)
- Main build scene: `Assets/Scenes/SampleScene.unity` (`ProjectSettings/EditorBuildSettings.asset`)
- Core gameplay code: `Assets/Scripts/Unity-HSM`
- Camera code: `Assets/Scripts/Camera/ThirdPersonCamera.cs`
- Input System asset: `Assets/FinalRPG.inputactions`

## 2) Important Environment Notes

- `Packages/manifest.json` includes a machine-specific local package path:
  - `com.boxqkrtm.ide.cursor: file:D:/BaiduNetdiskDownload/...`
  - This may break package resolution on other machines.
- `ProjectSettings/PackageManagerSettings.asset` uses `https://packages.tuanjie.cn`.
- `Packages/packages-lock.json` is ignored by git, so reproducibility relies mostly on `manifest.json`.

## 3) Core Runtime Architecture (Gameplay)

Intent -> Context -> HSM -> Motion/Animator

1. Intent provider writes per-frame input-like intent into `PlayerContext`.
2. `PlayerStateDriver` ticks HSM and applies motion via `CharacterController`.
3. States (`Move`, `Combat`, etc.) consume `PlayerContext` and update velocity/anim params.
4. Rotation is queued in context and applied centrally in `PlayerStateDriver`.

### Key files

- `Assets/Scripts/Unity-HSM/PlayerStateDriver.cs`: runtime entry, update loop, state machine tick, movement/rotation apply.
- `Assets/Scripts/Unity-HSM/PlayerContext.cs`: shared runtime data and config-derived parameters.
- `Assets/Scripts/Unity-HSM/IIntentProvider.cs`: intent abstraction boundary.
- `Assets/Scripts/Unity-HSM/PlayerInputProvider.cs`: player input mapping.
- `Assets/Scripts/Unity-HSM/States/Move.cs`: locomotion state and motion intent consumption.
- `Assets/Scripts/Unity-HSM/States/Combat.cs`: combo state and attack consumption.
- `Assets/Scripts/Unity-HSM/AnimatorKeys.cs`: animator parameter/state key contract.
- `Assets/Scripts/Camera/ThirdPersonCamera.cs`: yaw source used by movement reference.

## 4) Enemy AI Status (Already Implemented)

File: `Assets/Scripts/Unity-HSM/EnemyBrain.cs`

Enemy decision logic has been migrated to a minimal behavior-tree style execution that only outputs intent to `PlayerContext`.

### Implemented behavior set

- `PatrolIdle`: timed idle phase.
- `PatrolWalk`: walk to patrol point (or random point near spawn when no patrol points exist).
- `ChaseTarget`: run chase when target is within detect range.
- `AttackMeleeOnce`: trigger one-frame attack intent when in attack range and cooldown ready.
- `AttackCooldownHold`: keep facing target and micro-adjust distance while waiting for cooldown.

### BT priority

- Combat > Chase > Patrol

### Integration boundary (kept intact)

- Still uses `IIntentProvider.WriteIntent(ctx)` path.
- Does not directly drive state transitions or animator state machine.
- Continues to rely on existing `Move`/`Combat`/`Grounded` transitions.

### Added debugging support

- `OnDrawGizmosSelected` in `EnemyBrain` draws:
  - detect range
  - attack range
  - patrol radius fallback
  - patrol points and links

## 5) Config/Data Locations

- Player module configs: `Assets/GameConfigs/Modules/Player`
- Enemy module configs: `Assets/GameConfigs/Modules/Enemy`
- Config sets:
  - `Assets/GameConfigs/Sets/Player/PlayerCapabilityConfigSet.asset`
  - `Assets/GameConfigs/Sets/Enemy/EnemyCapabilityConfigSet.asset`
- Migration note: `Assets/Scripts/Unity-HSM/CapabilityConfigMigration.md`

## 6) Known Risks / Debt

- `StateMachine.cs` / `StateMachineBuilder.cs` source visibility may differ by workspace snapshot; meta files exist.
- `enableLocomotion` is present in context/config but appears underused in current scripts.
- Logging in runtime state transitions can be noisy in production profiling.
- Animator contract mismatch risk if enemy controller keys differ from `AnimatorKeys`.
- If enemy object keeps active player `InputAction` bindings, intent contamination is possible; ensure `intentProviderOverride` is set to `EnemyBrain`.

## 7) Recommended First Steps for Next AI

1. Open `SampleScene` and verify one enemy prefab has:
   - `PlayerStateDriver.intentProviderOverride -> EnemyBrain`
   - `EnemyBrain.target` assigned to player transform
   - `enemyConfigSet` assigned
2. Playtest acceptance baseline:
   - patrol loop works without player input
   - enters chase when player within detect range
   - single melee attacks in attack range with cooldown behavior
3. Validate animator contract:
   - `NormalMove`, combat states, and parameters in `AnimatorKeys` are present for enemy.
4. If needed, tune:
   - `detectRange`, `attackRange`, `attackCooldown`
   - `patrolIdleMin`, `patrolIdleMax`, `patrolReachDistance`, `patrolRandomRadius`

## 8) Suggested Follow-up Improvements (Optional)

- Split generic BT node classes from `EnemyBrain.cs` into dedicated files for maintainability.
- Add simple runtime AI debug panel (current node, cooldown, phase).
- Add lightweight playmode tests around intent output invariants for patrol/chase/attack transitions.

