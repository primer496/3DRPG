using UnityEngine;

namespace HSM {
    public class Landing : State {
        readonly PlayerContext ctx;
        float elapsed;
        bool hasBeenInLandingState;
        bool waitAnimatorExitOnDeactivate;

        public Landing(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            Add(new AnimatorStateExitActivity(
                animatorProvider: () => this.ctx.anim,
                stateName: "Landing",
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
                if (info.IsName("Landing")) hasBeenInLandingState = true;
            }

            // 一旦确认进入过 Landing，就申请切回 Move；离开 Landing 的等待由退出 Activity 处理。
            // 没有 Animator 时保留 2s 超时兜底，避免卡死。
            if (hasBeenInLandingState || elapsed >= 2f) {
                waitAnimatorExitOnDeactivate = hasBeenInLandingState && ctx.anim != null;
                ctx.exitedLandingThisFrame = true;
                return ((Grounded)Parent).Move;
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
                ctx.anim.CrossFade("Landing", 0.1f);
                ctx.anim.SetFloat("Speed", 0f);
            }
        }

        protected override void OnUpdate(float deltaTime) {
            elapsed += deltaTime;
        }
    }
}
