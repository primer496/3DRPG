using UnityEngine;

namespace HSM {
    [CreateAssetMenu(
        fileName = "EnemyCapabilityConfigSet",
        menuName = "HSM/Configs/Enemy Capability Config Set"
    )]
    public class EnemyCapabilityConfigSet : ScriptableObject, ICapabilityConfigApplier {
        [Header("Capabilities")]
        public bool enableLocomotion = true;
        public bool enableCombat = true;
        public bool enableJump = false;
        public bool enableTraversal = false;

        [Header("Modules")]
        public ActorMovementConfig movement;
        public ActorCombatConfig combat;

        [Header("Optional Future Modules")]
        public ActorJumpConfig jump;
        public ActorTraversalConfig traversal;

        public void ApplyTo(PlayerContext ctx) {
            if (ctx == null) {
                return;
            }

            ctx.enableLocomotion = enableLocomotion;
            ctx.enableCombat = enableCombat;
            ctx.enableJump = enableJump;
            ctx.enableTraversal = enableTraversal;

            movement?.ApplyTo(ctx);
            combat?.ApplyTo(ctx);

            if (enableJump) {
                jump?.ApplyTo(ctx);
            }

            if (enableTraversal) {
                traversal?.ApplyTo(ctx);
            }
        }
    }
}
