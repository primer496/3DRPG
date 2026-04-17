using UnityEngine;

namespace HSM {
    [CreateAssetMenu(
        fileName = "ActorMovementConfig",
        menuName = "HSM/Configs/Actor Movement Config"
    )]
    public class ActorMovementConfig : ScriptableObject {
        [Header("Movement")]
        public float moveSpeed = 6f;
        public float accel = 40f;
        public float runSpeedMultiplier = 2f;

        [Header("Stop")]
        public bool enableStopState = true;
        public float stopDuration = 0.26f;
        public float stopEnterSpeedThreshold = 0.32f;
        public float stopEnterCrossFade = 0.06f;
        public float stopSpeedDecayTime = 0.12f;

        public void ApplyTo(PlayerContext ctx) {
            if (ctx == null) {
                return;
            }

            ctx.enableStopState = enableStopState;
            ctx.moveSpeed = moveSpeed;
            ctx.accel = accel;
        }
    }
}
