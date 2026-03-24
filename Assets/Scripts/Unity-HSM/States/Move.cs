using System;
using Unity.Mathematics;
using UnityEngine;

namespace HSM {
    public class Move : State {
        readonly PlayerContext ctx;
        Vector2 smoothInput = Vector2.zero;
        Vector2 currentVelocity = Vector2.zero;
        const float inputDeadZone = 0.01f;
        // 接近 PWalk 的平滑转向手感（根据需要可微调）
        const float smoothTime = 0.1f;
        const float turnSpeed = 15f;
        const float speedSmoothTime = 0.08f;

        // 真实移动速度（单位/秒），用于平滑驱动动画 Speed
        float smoothMoveSpeed;
        float speedVelocity;
        public Move(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
        }

        protected override State GetTransition() {
            if (!ctx.grounded) {
                return ((PlayerRoot)Parent.Parent).Airborne;
            }

            if (ctx.dodgePressed) {
                ctx.dodgePressed = false;
                return ((Grounded)Parent).Dodge;
            }

            if (ctx.attackPressed) {
                ctx.attackPressed = false;
                return ((Grounded)Parent).Combat;
            }

            // 松开移动输入且当前仍有明显水平速度时，进入急停状态。
            // 速度很小时继续留在 NormalMove（由混合树自然回到 Idle），避免低速抖动频繁切换。
            var horizontalSpeed = new Vector3(ctx.velocity.x, 0f, ctx.velocity.z).magnitude;
            if (ctx.moveInput.magnitude <= inputDeadZone && horizontalSpeed > ctx.stopEnterSpeedThreshold) {
                return ((Grounded)Parent).Stop;
            }
            // grounded 下始终留在 Move：由 NormalMove 混合树根据 Speed(0/1/2) 表示站立/走/跑
            return null;
        }

        protected override void OnEnter() {
            // 从 Stop/Landing 退出时 Animator 已由过渡切到 Locomotion，不再 CrossFade；承接 Animator 当前的 Speed。
            if ((ctx.exitedStopThisFrame || ctx.exitedLandingThisFrame) && ctx.anim != null) {
                float walkReal = Mathf.Max(0.0001f, ctx.GetWalkRealSpeed());
                float animSpeed = ctx.anim.GetFloat("Speed");
                smoothMoveSpeed = animSpeed * walkReal;
            } else {
                smoothMoveSpeed = new Vector3(ctx.velocity.x, 0f, ctx.velocity.z).magnitude;
            }
            speedVelocity = 0f;

            if (ctx.anim != null && !ctx.exitedStopThisFrame && !ctx.exitedLandingThisFrame) {
                ctx.anim.CrossFade("NormalMove", 0.1f);
            }
            if (ctx.anim != null) {
                float walkReal = Mathf.Max(0.0001f, ctx.GetWalkRealSpeed());
                float animSpeed = Mathf.Clamp(smoothMoveSpeed / walkReal, 0f, 2f);
                ctx.anim.SetFloat("Speed", animSpeed);

                if (ctx.moveInput.magnitude > inputDeadZone) {
                    var dir = ctx.moveInput.normalized;
                    ctx.anim.SetFloat("MoveX", dir.x);
                    ctx.anim.SetFloat("MoveZ", dir.y);
                } else {
                    ctx.anim.SetFloat("MoveX", 0f);
                    ctx.anim.SetFloat("MoveZ", 0f);
                }
            }

            ctx.exitedStopThisFrame = false;
            ctx.exitedLandingThisFrame = false;
        }

        protected override void OnUpdate(float deltaTime) {
            Vector2 rawInput = ctx.moveInput;
            float inputMag = rawInput.magnitude;
            bool hasInput = inputMag > inputDeadZone;

            // 供急停使用：松开输入的当帧 moveInput 可能已是 0，需用“上一帧仍在移动”的摇杆方向（与冲刺同一套相机空间输入）。
            if (hasInput) {
                ctx.lastStickWhileMoving = rawInput.normalized;
            }

            // 1) 方向平滑（仅在有输入时更新），以模拟 PWalk 的转向手感
            if (hasInput) {
                float currentInputAngle = Mathf.Atan2(rawInput.x, rawInput.y) * Mathf.Rad2Deg;
                float lastInputAngle = Mathf.Atan2(smoothInput.x, smoothInput.y) * Mathf.Rad2Deg;
                float angleDiff = Mathf.Abs(Mathf.DeltaAngle(currentInputAngle, lastInputAngle));

                if (angleDiff > 90f) {
                    smoothInput = rawInput;
                    currentVelocity = Vector2.zero;
                } else {
                    smoothInput = Vector2.SmoothDamp(smoothInput, rawInput, ref currentVelocity, smoothTime).normalized;
                }
            }

            // 2) 朝向：仅在移动时（有输入时）由摄像机 + rawInput 决定
            // 直接使用相机脚本维护的 yaw，避免读取 Camera.main 欧拉角导致“上一帧角度”抖动。
            float camYaw = ThirdPersonCamera.CurrentYawDeg;
            if (float.IsNaN(camYaw)) {
                var cam = Camera.main;
                camYaw = cam != null ? cam.transform.eulerAngles.y : 0f;
            }

            Quaternion targetRot = Quaternion.identity;

            // 旋转统一在 PlayerStateDriver.FixedUpdate 里应用（用 MoveRotation），避免 Update 里直接写 Rigidbody.rotation 导致抖动。
            ctx.hasRotationTarget = false;
            if (hasInput) {
                float targetYaw = camYaw + Mathf.Atan2(smoothInput.x, smoothInput.y) * Mathf.Rad2Deg;
                targetRot = Quaternion.Euler(0f, targetYaw, 0f);
                ctx.rotationTarget = targetRot;
                ctx.rotationTurnSpeed = turnSpeed;
                ctx.hasRotationTarget = true;
            }

            // 3) 真实速度：由走/跑真实速度接口 + 输入大小得到目标速度，再 SmoothDamp 到当前速度
            float targetRealSpeed = ctx.GetTargetRealSpeed(inputMag);
            smoothMoveSpeed = Mathf.SmoothDamp(smoothMoveSpeed, targetRealSpeed, ref speedVelocity, speedSmoothTime);

            // 4) 非根运动：用目标速度驱动刚体水平速度
            Vector3 currentHorizontal = new Vector3(ctx.velocity.x, 0f, ctx.velocity.z);
            Vector3 targetHorizontalVelocity = Vector3.zero;
            if (hasInput && smoothMoveSpeed > 0.0001f) {
                Vector3 targetDir = targetRot * Vector3.forward; // world dir
                targetHorizontalVelocity =
                    new Vector3(targetDir.x, 0f, targetDir.z).normalized * smoothMoveSpeed;
            }

            Vector3 nextHorizontal = Vector3.MoveTowards(
                currentHorizontal,
                targetHorizontalVelocity,
                ctx.accel * deltaTime
            );
            ctx.velocity.x = nextHorizontal.x;
            ctx.velocity.z = nextHorizontal.z;

            // 5) 动画：Speed 映射为走=1 跑=2，并且不会突变
            if (ctx.anim != null) {
                float walkReal = Mathf.Max(0.0001f, ctx.GetWalkRealSpeed());
                float animSpeed = Mathf.Clamp(smoothMoveSpeed / walkReal, 0f, 2f);
                ctx.anim.SetFloat("Speed", animSpeed);

                if (hasInput) {
                    ctx.anim.SetFloat("MoveX", smoothInput.x);
                    ctx.anim.SetFloat("MoveZ", smoothInput.y);
                } else {
                    ctx.anim.SetFloat("MoveX", 0f);
                    ctx.anim.SetFloat("MoveZ", 0f);
                }
            }
        }
    }
}