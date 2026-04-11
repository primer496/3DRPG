using UnityEngine;

namespace HSM {
    [CreateAssetMenu(
        fileName = "ActorJumpConfig",
        menuName = "HSM/Configs/Actor Jump Config"
    )]
    public class ActorJumpConfig : ScriptableObject {
        [Header("Jump")]
        public float jumpHeight = 2.5f;
        public float jumpSpeed = 7f;
        public float jumpGroundDetachTime = 0.12f;

        public void ApplyTo(PlayerContext ctx) {
            if (ctx == null) {
                return;
            }

            ctx.jumpHeight = jumpHeight;
            ctx.jumpSpeed = jumpSpeed;
            ctx.jumpGroundDetachTime = jumpGroundDetachTime;
        }
    }
}
