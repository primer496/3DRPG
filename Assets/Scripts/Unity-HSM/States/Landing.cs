using UnityEngine;

namespace HSM {
    public class Landing : State {
        readonly PlayerContext ctx;
        readonly PlayerRoot rootState;
        float elapsed;
        bool hasBeenInLandingState;
        bool waitAnimatorExitOnDeactivate;

        public Landing(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            // Now Landing is in Airborne, its parent's parent is PlayerRoot
            rootState = parent.Parent as PlayerRoot;
            Add(new AnimatorStateExitActivity(
                animatorProvider: () => this.ctx.anim,
                stateName: AnimatorKeys.States.Landing,
                layerIndex: 0,
                timeoutSeconds: 2f,
                requireSeenStateBeforeExit: true,
                shouldWait: () => waitAnimatorExitOnDeactivate
            ));
        }

        protected override void OnExit() {
            ctx.exitedLandingThisFrame = true;
        }

        protected override State GetTransition() {
            if (ctx.anim != null) {
                var info = ctx.anim.GetCurrentAnimatorStateInfo(0);
                if (info.IsName(AnimatorKeys.States.Landing)) hasBeenInLandingState = true;
            }
            if ((hasBeenInLandingState && ctx.anim != null && !ctx.anim.GetCurrentAnimatorStateInfo(0).IsName(AnimatorKeys.States.Landing)) || elapsed >= 0.5f) {
                waitAnimatorExitOnDeactivate = false;
                ctx.exitedLandingThisFrame = true;
                if (ctx.anim != null)
                {
                    ctx.anim.CrossFade(AnimatorKeys.States.NormalMove, 0.1f);
                }
                return rootState.Grounded;
            }
            return null;
        }

        protected override void OnEnter() {
            elapsed = 0f;
            hasBeenInLandingState = false;
            waitAnimatorExitOnDeactivate = false;
            ctx.velocity.x = 0f;
            ctx.velocity.z = 0f;
            if (ctx.anim != null) {
                ctx.anim.CrossFade(AnimatorKeys.States.Landing, 0.05f);
                ctx.anim.SetFloat(AnimatorKeys.Params.Speed, 0f);
            }
        }

        protected override void OnUpdate(float deltaTime) {
            elapsed += deltaTime;
            // 确保着陆阶段持续清零速度防止惯性滑动
            ctx.velocity.x = 0f;
            ctx.velocity.z = 0f;
            if (ctx.anim != null) {
                ctx.anim.SetFloat(AnimatorKeys.Params.Speed, 0f);
            }
        }
    }
}
