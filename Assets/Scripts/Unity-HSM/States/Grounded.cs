using UnityEngine;

namespace HSM {
    public class Grounded : State {
        readonly PlayerContext ctx;
        public readonly Idle Idle;
        public readonly Move Move;
        public readonly Stop Stop;
        public readonly Combat Combat;
        public readonly Dodge Dodge;
        public readonly Landing Landing;

        public Grounded(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            Idle = new Idle(m, this, ctx);
            Move = new Move(m, this, ctx);
            Stop = new Stop(m, this, ctx);
            Combat = new Combat(m, this, ctx);
            Dodge = new Dodge(m, this, ctx);
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
                return ((PlayerRoot)Parent).Airborne;
            }

            if (ctx.jumpPressed) {
                ctx.jumpPressed = false;
                var rb = ctx.rb;

                if (rb != null) {
                    var v = rb.velocity;
                    v.y = ctx.jumpSpeed;
                    rb.velocity = v;
                }
                return ((PlayerRoot)Parent).Airborne;
            }

            return null;
        }
    }
}