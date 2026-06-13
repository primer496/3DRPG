# Capability Config Migration

This project now supports role-specific config sets:

- `PlayerCapabilityConfigSet`
- `EnemyCapabilityConfigSet`

## Migration Steps

1. Create module assets as needed:
   - `ActorMovementConfig`
   - `ActorCombatConfig`
   - `ActorJumpConfig` (optional)
   - `ActorTraversalConfig` (optional)

2. Create role config-set assets:
   - For player: `PlayerCapabilityConfigSet`
   - For enemy: `EnemyCapabilityConfigSet`

3. Assign module references into each config set.

4. On each actor's `PlayerStateDriver`:
   - assign `playerConfigSet` for player actors, or
   - assign `enemyConfigSet` for enemy actors.

5. Validate capability gates:
   - `enableJump` controls jump transitions.
   - `enableTraversal` controls vault/climb transitions.
   - `enableCombat` controls combat transition.

## Cleanup Guideline

- Keep all actor tuning in role config sets and module assets only.
- Avoid adding role-specific tuning back to `PlayerContext` unless it is runtime state.
