using UnityEngine;

namespace HSM {
    [CreateAssetMenu(
        fileName = "PlayerCapabilityConfigSet",
        menuName = "HSM/Configs/Player Capability Config Set"
    )]
    public class PlayerCapabilityConfigSet : ScriptableObject, ICapabilityConfigApplier {
        [Header("Capabilities")]
        public bool enableLocomotion = true;
        public bool enableCombat = true;
        public bool enableJump = true;
        public bool enableTraversal = true;

        [Header("Modules")]
        public ActorMovementConfig movement;
        public ActorCombatConfig combat;
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
            jump?.ApplyTo(ctx);

            if (enableTraversal) {
                traversal?.ApplyTo(ctx);
            }
        }
    }
}
