using UnityEngine;

namespace HSM {
    public class Idle : State {
        readonly PlayerContext ctx;

        public Idle(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
        }

        protected override State GetTransition() {
            if (!ctx.grounded) {
                return ((PlayerRoot)Parent.Parent).Airborne;
            }

            if (ctx.dodgePressed) {
                ctx.dodgePressed = false;
                return ((Grounded)Parent).Dodge;
            }

            if (ctx.attackPressed) {
                ctx.attackPressed = false;
                return ((Grounded)Parent).Combat;
            }

            if (ctx.moveInput.sqrMagnitude > 0.0001f) {
                return ((Grounded)Parent).Move;
            }

            return null;
        }

        protected override void OnEnter() {
            ctx.velocity.x = 0f;
            ctx.velocity.z = 0f;
            if (ctx.anim != null) {
                // HH.controller 的实际状态名是 NormalMove（不是 Locomotion）
                ctx.anim.CrossFade("NormalMove", 0.1f);
                ctx.anim.SetFloat("MoveX", 0f);
                ctx.anim.SetFloat("MoveZ", 0f);
            }
        }
    }
}