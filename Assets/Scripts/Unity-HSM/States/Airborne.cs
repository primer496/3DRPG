using UnityEngine;

namespace HSM {
    public class Airborne : State {
        readonly PlayerContext ctx;
        readonly PlayerRoot rootState;
        public readonly AirborneFall AirborneFall;

        public Airborne(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            rootState = parent as PlayerRoot;
            AirborneFall = new AirborneFall(m, this, ctx);
            Add(new ColorPhaseActivity(ctx.renderer){
                enterColor = Color.red, // runs while Airborne is activating
            });
        }

        protected override State GetInitialState() => AirborneFall;
        
        protected override State GetTransition() => ctx.grounded ? rootState.Grounded : null;

        protected override void OnExit() {
            ctx.justLanded = true;
        }

        protected override void OnEnter() {
            // 继承地面最近 3 帧的平均水平速度
            Vector2 avg = ctx.GetAverageGroundVelocity();
            ctx.velocity.x = avg.x;
            ctx.velocity.z = avg.y;
        }
    }
}
