using UnityEngine;

namespace HSM {
    /// <summary>
    /// 攀爬状态。根据 ctx.detectedClimbTier 播放对应高度的攀爬动画，
    /// 全程使用 Root Motion 驱动位移，动画播放到 exitNormalizedTime 后退出。
    /// </summary>
    public class Climb : State {
        readonly PlayerContext ctx;
        readonly Grounded groundedState;

        float elapsed;
        float fallbackDuration;
        bool animationCompleted;
        string currentClimbStateName;
        bool hasEnteredTargetState;

        public Climb(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            groundedState = parent as Grounded;
        }

        protected override State GetTransition() {
            if (animationCompleted || elapsed >= fallbackDuration) {
                ctx.exitedClimbThisFrame = true;
                return groundedState.Move;
            }
            return null;
        }

        protected override void OnEnter() {
            elapsed = 0f;
            animationCompleted = false;
            hasEnteredTargetState = false;
            ctx.exitedClimbThisFrame = false;
            ctx.isClimbing = true;
            ctx.jumpPressed = false;
            ctx.hasRotationTarget = false;

            // 冻结物理速度，全部交给 Root Motion。
            ctx.velocity.x = 0f;
            ctx.velocity.z = 0f;
            ctx.verticalVelocity = 0f;

            currentClimbStateName = TierToAnimState(ctx.detectedClimbTier);
            fallbackDuration = TierToFallbackDuration(ctx.detectedClimbTier);

            if (ctx.climbDebugLog) {
                Debug.Log($"[Climb] Enter tier={ctx.detectedClimbTier}, animState={currentClimbStateName}");
            }

            if (ctx.anim != null) {
                ctx.anim.applyRootMotion = true;
                float crossFade = Mathf.Max(0f, ctx.climbEnterCrossFade);
                // Force replay from clip start; avoids resuming cached normalized time on repeated climbs.
                ctx.anim.CrossFade(currentClimbStateName, crossFade, 0, 0f);
            }
        }

        protected override void OnUpdate(float deltaTime) {
            elapsed += deltaTime;
            ctx.jumpPressed = false;

            if (ctx.anim != null) {
                ctx.anim.applyRootMotion = true;

                var info = ctx.anim.GetCurrentAnimatorStateInfo(0);
                if (info.IsName(currentClimbStateName)) {
                    hasEnteredTargetState = true;
                    float raw = info.normalizedTime;
                    float exitTime = Mathf.Clamp(ctx.climbExitNormalizedTime, 0.7f, 0.99f);
                    // Only allow exit after we have actually entered the target state.
                    if (hasEnteredTargetState && raw >= exitTime) {
                        animationCompleted = true;
                    }
                    if (info.length > 0.05f) {
                        fallbackDuration = Mathf.Max(fallbackDuration, info.length + deltaTime);
                    }
                }
            }
        }

        protected override void OnExit() {
            ctx.isClimbing = false;
            ctx.velocity.x = 0f;
            ctx.velocity.z = 0f;
            ctx.verticalVelocity = 0f;

            if (ctx.anim != null) {
                ctx.anim.applyRootMotion = false;
                float crossFade = Mathf.Max(0f, ctx.climbExitCrossFade);
                ctx.anim.CrossFade(AnimatorKeys.States.NormalMove, crossFade);
            }
        }

        static string TierToAnimState(ClimbHeightTier tier) {
            switch (tier) {
                case ClimbHeightTier.Climb05: return AnimatorKeys.States.Climb05;
                case ClimbHeightTier.Climb10: return AnimatorKeys.States.Climb10;
                case ClimbHeightTier.Climb17: return AnimatorKeys.States.Climb17;
                case ClimbHeightTier.Climb20: return AnimatorKeys.States.Climb20;
                default: return AnimatorKeys.States.Climb10;
            }
        }

        static float TierToFallbackDuration(ClimbHeightTier tier) {
            switch (tier) {
                case ClimbHeightTier.Climb05: return 0.6f;
                case ClimbHeightTier.Climb10: return 0.8f;
                case ClimbHeightTier.Climb17: return 1.2f;
                case ClimbHeightTier.Climb20: return 1.5f;
                default: return 1.0f;
            }
        }
    }
}
