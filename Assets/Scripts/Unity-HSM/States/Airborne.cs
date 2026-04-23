using UnityEngine;

namespace HSM {
    public class Airborne : State {
        readonly PlayerContext ctx;
        readonly PlayerRoot rootState;
        public readonly AirborneFall AirborneFall;
        public readonly Landing Landing;

        public Airborne(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            rootState = parent as PlayerRoot;
            AirborneFall = new AirborneFall(m, this, ctx);
            Landing = new Landing(m, this, ctx);
            Add(new ColorPhaseActivity(ctx.renderer){
                enterColor = Color.red, // runs while Airborne is activating
            });
        }

        float airborneTimer;

        protected override void OnExit() {
            ctx.justLanded = true;
        }

        protected override void OnEnter() {
            airborneTimer = 0f;
            // 继承地面最近 3 帧的平均水平速度
            Vector2 avg = ctx.GetAverageGroundVelocity();
            ctx.velocity.x = avg.x;
            ctx.velocity.z = avg.y;
        }

        protected override void OnUpdate(float deltaTime) {
            airborneTimer += deltaTime;
        }

        protected override State GetInitialState() {
            return AirborneFall;
        }

        // Airborne 层的转移交由子状态(Landing)最后决定切到 Grounded，
        // 或者在下落时碰到地面，就切到 Landing
        protected override State GetTransition() {
            // 当触地且当前不是 Landing 时，切给 Landing
            // 防止刚起跳的瞬间（增加保护时间）因为地面判定残留被强行切到 Landing
            State currentLeaf = Machine.Root.Leaf();
            if (ctx.grounded && currentLeaf != Landing) {
                if (currentLeaf == AirborneFall && airborneTimer < 0.3f) {
                    return null; // 保护起跳初期不被着陆打断
                }
                return Landing;
            }
            return null;
        }
    }
}
