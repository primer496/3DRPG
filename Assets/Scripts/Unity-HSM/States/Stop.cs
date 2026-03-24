using UnityEngine;

namespace HSM {
    public class Stop : State {
        readonly PlayerContext ctx;
        float remainingTime;
        float stopAnimSpeed;
        bool waitAnimatorExitOnDeactivate;

        public Stop(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            Add(new AnimatorStateExitActivity(
                animatorProvider: () => this.ctx.anim,
                stateName: "StopType",
                layerIndex: 0,
                timeoutSeconds: 2f,
                requireSeenStateBeforeExit: true,
                shouldWait: () => waitAnimatorExitOnDeactivate
            ));
        }

        protected override State GetTransition() {
            if (!ctx.grounded) {
                waitAnimatorExitOnDeactivate = false;
                return ((PlayerRoot)Parent.Parent).Airborne;
            }

            if (ctx.dodgePressed) {
                ctx.dodgePressed = false;
                ctx.exitedStopThisFrame = false;
                waitAnimatorExitOnDeactivate = false;
                return ((Grounded)Parent).Dodge;
            }

            if (ctx.attackPressed) {
                ctx.attackPressed = false;
                ctx.exitedStopThisFrame = false;
                waitAnimatorExitOnDeactivate = false;
                return ((Grounded)Parent).Combat;
            }

            // 最短停留时间到后申请切回 Move；离开 StopType 的等待由退出 Activity 处理。
            if (remainingTime <= 0f) {
                waitAnimatorExitOnDeactivate = ctx.anim != null;
                ctx.exitedStopThisFrame = true;
                return ((Grounded)Parent).Move;
            }

            if (ctx.moveInput.magnitude > 0.01f) {
                ctx.exitedStopThisFrame = false;
                waitAnimatorExitOnDeactivate = false;
                return ((Grounded)Parent).Move;
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
                ctx.anim.CrossFade("StopType", Mathf.Max(0f, ctx.stopEnterCrossFade));

                // 与 Move/Dodge 一致：相机 yaw 来自 ThirdPersonCamera（与冲刺同一套空间）
                float camYaw = ThirdPersonCamera.CurrentYawDeg;
                if (float.IsNaN(camYaw)) {
                    var cam = Camera.main;
                    camYaw = cam != null ? cam.transform.eulerAngles.y : 0f;
                }

                // 世界平面“急停方向”：优先水平速度；过小则用上一帧仍按住时的摇杆（相机空间 → 世界），避免松开当帧 input=0 抽方向
                Vector3 worldStopDir = GetWorldStopDirection(camYaw);

                // 与冲刺动画一致：在「角色面朝」下量化为四向（MoveX=左右，MoveZ=前后，与 NormalMove 混合树约定一致）
                Vector2 dirLocal = WorldDirToCharacterCardinalMoveXZ(worldStopDir);

                ctx.anim.SetFloat("MoveX", dirLocal.x);
                ctx.anim.SetFloat("MoveZ", dirLocal.y);

                // 左右脚：仅由走路/跑步 clip 的 OnFootPlant 事件提供；未收到事件前 StopFoot=0。
                ctx.anim.SetFloat("StopFoot", ComputeStopFoot());

                // 进急停时固定一次 Speed，用于 StopType 在走停/跑停之间分流。
                float walkReal = Mathf.Max(0.0001f, ctx.GetWalkRealSpeed());
                float horizontalSpeed = new Vector3(ctx.velocity.x, 0f, ctx.velocity.z).magnitude;
                stopAnimSpeed = Mathf.Clamp(horizontalSpeed / walkReal, 0f, 2f);
                ctx.anim.SetFloat("Speed", stopAnimSpeed);
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

            Transform t = ctx.rb != null ? ctx.rb.transform : null;
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

            if (ctx.anim != null) {
                // 指数衰减：比 SmoothDamp 在极小时间常数下更稳定，避免速度项突变。
                // τ 与帧时间取 max，避免 τ→0 时单帧把 Speed 打成 0 造成跳变。
                float tau = Mathf.Max(ctx.stopSpeedDecayTime, 3f * deltaTime);
                stopAnimSpeed *= Mathf.Exp(-deltaTime / tau);
                if (stopAnimSpeed < 1e-4f) {
                    stopAnimSpeed = 0f;
                }
                ctx.anim.SetFloat("Speed", Mathf.Clamp(stopAnimSpeed, 0f, 2f));
            }
        }
    }
}

