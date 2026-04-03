using UnityEngine;

namespace HSM {
    public class AirborneFall : State {
        readonly PlayerContext ctx;

        public AirborneFall(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
        }

        protected override void OnEnter() {
            if (ctx.anim == null) {
                return;
            }

            const int baseLayerIndex = 0;
            int airborneFallHash = Animator.StringToHash(AnimatorKeys.States.AirborneHang);
            if (ctx.anim.HasState(baseLayerIndex, airborneFallHash)) {
                ctx.anim.CrossFade(AnimatorKeys.States.AirborneHang, 0.1f);
                return;
            }

            ctx.anim.CrossFade(AnimatorKeys.States.Airborne, 0.1f);
        }
    }
}
