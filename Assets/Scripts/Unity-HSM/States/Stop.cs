using UnityEngine;

namespace HSM {
    public class Stop : State {
        readonly PlayerContext ctx;
        readonly Grounded groundedState;
        readonly PlayerRoot rootState;
        float remainingTime;
        bool waitAnimatorExitOnDeactivate;

        public Stop(StateMachine m, State parent, PlayerRoot rootState, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            groundedState = parent as Grounded;
            this.rootState = rootState;
            Add(new AnimatorStateExitActivity(
                animatorProvider: () => this.ctx.anim,
                stateName: AnimatorKeys.States.StopType,
                layerIndex: 0,
                timeoutSeconds: 2f,
                requireSeenStateBeforeExit: true,
                shouldWait: () => waitAnimatorExitOnDeactivate
            ));
        }

        protected override State GetTransition() {
            if (!ctx.grounded) {
                waitAnimatorExitOnDeactivate = false;
                return rootState.Airborne;
            }

            if (ctx.dodgePressed) {
                ctx.dodgePressed = false;
                ctx.exitedStopThisFrame = false;
                waitAnimatorExitOnDeactivate = false;
                return groundedState.Dodge;
            }

            if (ctx.attackPressed) {
                ctx.attackPressed = false;
                if (ctx.enableCombat) {
                    ctx.exitedStopThisFrame = false;
                    waitAnimatorExitOnDeactivate = false;
                    return groundedState.Combat;
                }
            }

            // 最短停留时间到后申请切回 Move；离开 StopType 的等待由退出 Activity 处理。
            if (remainingTime <= 0f) {
                waitAnimatorExitOnDeactivate = ctx.anim != null;
                ctx.exitedStopThisFrame = true;
                return groundedState.Move;
            }

            if (ctx.moveInput.magnitude > 0.01f) {
                ctx.exitedStopThisFrame = false;
                waitAnimatorExitOnDeactivate = false;
                return groundedState.Move;
            }

            return null;
        }

        protected override void OnEnter() {
            ctx.exitedStopThisFrame = false;
            waitAnimatorExitOnDeactivate = false;
            remainingTime = Mathf.Max(0f, ctx.stopDuration);
            if (ctx.anim != null) {
                // 使用 Stop 子状态机（二维混合树：走急停 / 跑急停）
                // HH.controller 里对应状态名是 StopType
                ctx.anim.CrossFade(AnimatorKeys.States.StopType, Mathf.Max(0f, ctx.stopEnterCrossFade));

                // 与 Move/Dodge 一致：从 context 获取统一的朝向参考 yaw。
                float camYaw = ctx.GetFacingReferenceYaw();

                // 世界平面“急停方向”：优先水平速度；过小则用上一帧仍按住时的摇杆（相机空间 → 世界），避免松开当帧 input=0 抽方向
                Vector3 worldStopDir = GetWorldStopDirection(camYaw);

                // 与冲刺动画一致：在「角色面朝」下量化为四向（MoveX=左右，MoveZ=前后，与 NormalMove 混合树约定一致）
                Vector2 dirLocal = WorldDirToCharacterCardinalMoveXZ(worldStopDir);

                ctx.anim.SetFloat(AnimatorKeys.Params.MoveX, dirLocal.x);
                ctx.anim.SetFloat(AnimatorKeys.Params.MoveZ, dirLocal.y);

                // 左右脚：仅由走路/跑步 clip 的 OnFootPlant 事件提供；未收到事件前 StopFoot=0。
                ctx.anim.SetFloat(AnimatorKeys.Params.StopFoot, ComputeStopFoot());

                // 进急停时固定一次 Speed，用于 StopType 在走停/跑停之间分流。
                float runReal = Mathf.Max(0.0001f, ctx.GetRunRealSpeed());
                float horizontalSpeed = new Vector3(ctx.velocity.x, 0f, ctx.velocity.z).magnitude;
                float stopAnimSpeed = Mathf.Clamp01(horizontalSpeed / runReal);
                ctx.anim.SetFloat(AnimatorKeys.Params.Speed, stopAnimSpeed);
            }
        }

        /// <summary>
        /// 世界平面急停方向：速度优先，否则上一帧摇杆经相机旋转到世界。
        /// </summary>
        static Vector3 GetWorldStopDirection(PlayerContext ctx, float camYaw) {
            var h = new Vector3(ctx.velocity.x, 0f, ctx.velocity.z);
            if (h.sqrMagnitude > 0.0001f) {
                return h.normalized;
            }

            if (ctx.lastStickWhileMoving.sqrMagnitude > 0.0001f) {
                var stick = ctx.lastStickWhileMoving.normalized;
                Vector3 worldFromStick = Quaternion.Euler(0f, camYaw, 0f) * new Vector3(stick.x, 0f, stick.y);
                worldFromStick.y = 0f;
                if (worldFromStick.sqrMagnitude > 0.0001f) {
                    return worldFromStick.normalized;
                }
            }

            return Vector3.forward;
        }

        Vector3 GetWorldStopDirection(float camYaw) => GetWorldStopDirection(ctx, camYaw);

        float ComputeStopFoot() {
            if (!ctx.hasFootPlantData) {
                return 0f;
            }
            return ctx.lastPlantedFootIsRight ? 1f : 0f;
        }

        /// <summary>
        /// 将世界移动方向投影到角色左右/前后轴，取主导轴得到四向 MoveX/MoveZ（与 WASD 驱动面朝时的本地前后左右一致）。
        /// </summary>
        Vector2 WorldDirToCharacterCardinalMoveXZ(Vector3 worldDir) {
            worldDir.y = 0f;
            if (worldDir.sqrMagnitude < 0.0001f) {
                return new Vector2(0f, 1f);
            }
            worldDir.Normalize();

            Transform t = ctx.cc != null ? ctx.cc.transform : null;
            Vector3 forward = t != null ? t.forward : Vector3.forward;
            Vector3 right = t != null ? t.right : Vector3.right;
            forward.y = 0f;
            right.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
            if (right.sqrMagnitude < 0.0001f) right = Vector3.right;
            forward.Normalize();
            right.Normalize();

            float f = Vector3.Dot(worldDir, forward);
            float r = Vector3.Dot(worldDir, right);

            if (Mathf.Abs(f) >= Mathf.Abs(r)) {
                return f >= 0f ? new Vector2(0f, 1f) : new Vector2(0f, -1f);
            }
            return r >= 0f ? new Vector2(1f, 0f) : new Vector2(-1f, 0f);
        }

        protected override void OnUpdate(float deltaTime) {
            remainingTime -= deltaTime;
            var currentHorizontal = new Vector3(ctx.velocity.x, 0f, ctx.velocity.z);
            var nextHorizontal = Vector3.MoveTowards(
                currentHorizontal,
                Vector3.zero,
                ctx.accel * 2f * deltaTime
            );
            ctx.velocity.x = nextHorizontal.x;
            ctx.velocity.z = nextHorizontal.z;

        }
    }
}

