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

            ctx.anim.CrossFade(AnimatorKeys.States.AirborneFall, 0.1f);
        }
    }
}
