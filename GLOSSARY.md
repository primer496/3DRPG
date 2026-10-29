# FinalRPG

Unity 2.5D/3D RPG demo: player and enemies share one motion stack; quests and dialogue drive progression; inventory and stats persist across sessions.

## Language

### Actors & motion

**Actor**:
A controllable character in the world (player or enemy) driven by the shared hierarchical state machine.
_Avoid_: Character, unit, pawn (except when referring to Unity's CharacterController component)

**Intent**:
Per-frame desired actions written into shared context (move, jump, attack, look). Actors never drive animation or state transitions directly from AI or input code.
_Avoid_: Input, command, action request

**Intent Provider**:
The boundary that produces Intent each frame (player input, mobile bridge, or enemy brain).
_Avoid_: Controller, input handler (when meaning this boundary)

**Capability**:
A gated ability set for an Actor (movement, combat, jump, traversal) loaded from a role config set, not hardcoded on the runtime context.

### Progression

**Quest**:
A player-facing objective chain with accept/progress/complete lifecycle, configured as data and advanced by world or dialogue events.
_Avoid_: Mission, task (except the legacy `TaskManager` namespace name)

**Objective**:
A single step inside a Quest (kill, collect, talk) identified by a target id.

**Dialogue**:
A Yarn-driven conversation with an NPC that may accept quests, grant items, or branch on Yarn variables.
_Avoid_: Conversation script, cutscene (unless it is a non-interactive sequence)

**Yarn Variable**:
Named dialogue state (`$QuestAccepted`, `$InvestigationProgress`, …) that must stay in sync with Quest progress and the save file.

### Economy & stats

**Item**:
A stackable or unique inventory entry whose id matches the ScriptableObject asset name under package configs.
_Avoid_: Goods, loot entry

**Inventory**:
The player's collection of Items; mutated through rewards and dialogue commands, never by cross-system direct calls.

**Player Stats**:
Runtime vitals and progression numbers (HP, gold, exp) owned by the player data layer and shown by UI.

### Integration

**Event Bus**:
The sole cross-system publish/subscribe channel for rewards, damage, input lock, and NPC interact availability.
_Avoid_: Service locator, direct manager calls between systems
