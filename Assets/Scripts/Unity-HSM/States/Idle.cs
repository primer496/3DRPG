using UnityEngine;

namespace HSM {
    public class Idle : State {
        readonly PlayerContext ctx;
        readonly Grounded groundedState;
        readonly PlayerRoot rootState;

        public Idle(StateMachine m, State parent, PlayerRoot rootState, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            groundedState = parent as Grounded;
            this.rootState = rootState;
        }

        protected override State GetTransition() {
            if (!ctx.grounded) {
                return rootState.Airborne;
            }

            if (ctx.dodgePressed) {
                ctx.dodgePressed = false;
                return groundedState.Dodge;
            }

            if (ctx.attackPressed) {
                ctx.attackPressed = false;
                return groundedState.Combat;
            }

            if (ctx.moveInput.sqrMagnitude > 0.0001f) {
                return groundedState.Move;
            }

            return null;
        }

        protected override void OnEnter() {
            ctx.velocity.x = 0f;
            ctx.velocity.z = 0f;
            if (ctx.anim != null) {
                // HH.controller 的实际状态名是 NormalMove（不是 Locomotion）
                ctx.anim.CrossFade(AnimatorKeys.States.NormalMove, 0.1f);
                ctx.anim.SetFloat(AnimatorKeys.Params.MoveX, 0f);
                ctx.anim.SetFloat(AnimatorKeys.Params.MoveZ, 0f);
            }
        }
    }
}