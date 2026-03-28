using UnityEngine;

namespace HSM {
    public class Grounded : State {
        readonly PlayerContext ctx;
        readonly PlayerRoot rootState;
        public readonly Idle Idle;
        public readonly Move Move;
        public readonly Stop Stop;
        public readonly Combat Combat;
        public readonly Dodge Dodge;
        public readonly Landing Landing;

        public Grounded(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            rootState = parent as PlayerRoot;
            Idle = new Idle(m, this, rootState, ctx);
            Move = new Move(m, this, rootState, ctx);
            Stop = new Stop(m, this, rootState, ctx);
            Combat = new Combat(m, this, rootState, ctx);
            Dodge = new Dodge(m, this, rootState, ctx);
            Landing = new Landing(m, this, ctx);
            Add(new ColorPhaseActivity(ctx.renderer){
                enterColor = Color.yellow,  // runs while Grounded is activating
            });
        }
        
        // 从空中落地时先进入 Landing，否则进入 Move
        protected override State GetInitialState() {
            if (ctx.justLanded) {
                ctx.justLanded = false;
                return Landing;
            }
            return Move;
        }

        protected override State GetTransition() {
            if (!ctx.grounded) {
                return rootState.Airborne;
            }

            if (ctx.jumpPressed) {
                ctx.jumpPressed = false;
                var rb = ctx.rb;
                ctx.jumpGroundDetachTimer = Mathf.Max(0f, ctx.jumpGroundDetachTime);

                if (rb != null) {
                    var v = rb.velocity;
                    v.y = ctx.jumpSpeed;
                    rb.velocity = v;
                }
                return rootState.Airborne;
            }

            return null;
        }
    }
}