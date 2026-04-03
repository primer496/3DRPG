using UnityEngine;

namespace HSM {
    public class Vault : State {
        const float DefaultDuration = 0.42f;
        const float DefaultEnterCrossFade = 0.08f;
        const float DefaultExitCrossFade = 0.1f;

        readonly PlayerContext ctx;
        readonly Grounded groundedState;
        float elapsed;
        float fallbackDuration;
        bool animationCompleted;

        public Vault(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            groundedState = parent as Grounded;
        }

        protected override State GetTransition() {
            if (animationCompleted || elapsed >= fallbackDuration) {
                ctx.exitedVaultThisFrame = true;
                return groundedState.Move;
            }

            return null;
        }

        protected override void OnEnter() {
            elapsed = 0f;
            fallbackDuration = ResolveDuration();
            animationCompleted = false;
            ctx.exitedVaultThisFrame = false;
            ctx.isVaulting = true;
            ctx.jumpPressed = false;
            ctx.hasRotationTarget = false;
            ctx.velocity.x = 0f;
            ctx.velocity.z = 0f;
            ctx.verticalVelocity = 0f;

            if (ctx.anim != null) {
                ctx.anim.applyRootMotion = true;
                ctx.anim.CrossFade(AnimatorKeys.States.Vault, ResolveEnterCrossFade());
            }
        }

        protected override void OnUpdate(float deltaTime) {
            elapsed += deltaTime;
            ResolveProgress(deltaTime);
            ctx.jumpPressed = false;

            if (ctx.anim != null) {
                ctx.anim.applyRootMotion = true;
                ctx.anim.SetFloat(AnimatorKeys.Params.MoveX, 0f);
                ctx.anim.SetFloat(AnimatorKeys.Params.MoveZ, 1f);
            }
        }

        protected override void OnExit() {
            ctx.isVaulting = false;
            ctx.velocity.x = 0f;
            ctx.velocity.z = 0f;
            ctx.verticalVelocity = 0f;

            if (ctx.anim != null) {
                ctx.anim.applyRootMotion = false;
                ctx.anim.CrossFade(AnimatorKeys.States.NormalMove, ResolveExitCrossFade());
            }
        }

        void ResolveProgress(float deltaTime) {
            if (ctx.anim == null) {
                return;
            }

            var info = ctx.anim.GetCurrentAnimatorStateInfo(0);
            if (!info.IsName(AnimatorKeys.States.Vault)) {
                return;
            }

            float raw = info.normalizedTime;
            float exitNormalizedTime = Mathf.Clamp(ctx.vaultExitNormalizedTime, 0.6f, 0.99f);
            if (raw >= exitNormalizedTime) {
                animationCompleted = true;
            }

            // 首次看到 Vault 状态时，按该状态时长更新兜底，避免参数与动画长度差距过大。
            if (info.length > 0.05f) {
                fallbackDuration = Mathf.Max(fallbackDuration, info.length + deltaTime);
            }
        }

        float ResolveDuration() => ctx.vaultDuration > 0.05f ? ctx.vaultDuration : DefaultDuration;
        float ResolveEnterCrossFade() => Mathf.Max(0f, ctx.vaultEnterCrossFade > 0f ? ctx.vaultEnterCrossFade : DefaultEnterCrossFade);
        float ResolveExitCrossFade() => Mathf.Max(0f, ctx.vaultExitCrossFade > 0f ? ctx.vaultExitCrossFade : DefaultExitCrossFade);
    }
}
