# Role capability config sets over ad-hoc PlayerContext tuning

Actor tuning lives in `PlayerCapabilityConfigSet` / `EnemyCapabilityConfigSet` composed of module assets (`ActorMovementConfig`, `ActorCombatConfig`, optional jump/traversal). Gates such as `enableJump` / `enableCombat` / `enableTraversal` control HSM transitions. Chosen so player and enemy share code paths while remaining data-driven; do not push role-specific constants back onto runtime context unless they are true runtime state.

## Setup

1. Create module assets as needed, then a role config set that references them.
2. On each Actor's `PlayerStateDriver`, assign `playerConfigSet` or `enemyConfigSet`.
3. Validate capability gates against the intended transitions.
