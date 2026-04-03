using UnityEngine;

namespace HSM {
    public class Dodge : State {
        readonly PlayerContext ctx;
        readonly Grounded groundedState;
        readonly PlayerRoot rootState;
        float remainingTime;
        Vector2 dodgeDir; // 2D 8向方向，用于动画混合（DodgeX/DodgeY）

        public Dodge(StateMachine m, State parent, PlayerRoot rootState, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            groundedState = parent as Grounded;
            this.rootState = rootState;
        }

        protected override State GetTransition() {
            if (!ctx.grounded) {
                return rootState.Airborne;
            }

            if (remainingTime <= 0f) {
                return groundedState.Move;
            }

            return null;
        }

        protected override void OnEnter() {
            remainingTime = ctx.dodgeDuration;

            // 实际位移保持当前逻辑：基于 rawInput 的 8 向量化 + 摄像机 yaw
            Vector2 rawInput = ctx.moveInput;
            if (rawInput.sqrMagnitude < 0.01f) {
                // 没有输入时，默认向前（假定正 Y 为前）
                rawInput = new Vector2(0f, 1f);
            }

            var inputN = rawInput.normalized;
            float inputAngle = Mathf.Atan2(inputN.x, inputN.y) * Mathf.Rad2Deg;
            Vector2 moveDodgeDir = QuantizeTo8Dir(inputAngle);

            // 将输入方向映射到世界坐标（按摄像机 yaw 解释 input 的“前/后/左/右”）
            float camYaw = 0f;
            var cam = Camera.main;
            if (cam != null) camYaw = cam.transform.eulerAngles.y;

            // moveDodgeDir: X=左右，Y=前后 -> 转成世界平面向量的 local(right, forward)
            Vector3 localDashDir = new Vector3(moveDodgeDir.x, 0f, moveDodgeDir.y);
            if (localDashDir.sqrMagnitude > 0.0001f) {
                // 对角线（±1,±1）需要归一化，避免冲刺速度变快
                localDashDir = localDashDir.normalized;
            }
            Vector3 worldDashDir = Quaternion.Euler(0f, camYaw, 0f) * localDashDir;

            ctx.velocity.x = worldDashDir.x * ctx.dodgeSpeed;
            ctx.velocity.z = worldDashDir.z * ctx.dodgeSpeed;

            // 让角色朝向冲刺方向（可选，但能确保视觉一致性）
            if (worldDashDir.sqrMagnitude > 0.0001f) {
                ctx.rotationTarget = Quaternion.LookRotation(
                    new Vector3(worldDashDir.x, 0f, worldDashDir.z),
                    Vector3.up
                );
                // 用很大的 turnSpeed 在统一旋转应用阶段“近似瞬转”，避免冲刺期间还在平滑插值
                ctx.rotationTurnSpeed = 1000f;
                ctx.hasRotationTarget = true;
            }

            // 动作方向：由“角色朝向”和“rawInput 方向（世界空间）”夹角决定（8向）
            // 前 DodgeY=1，后 DodgeY=-1，左 DodgeX=-1，右 DodgeX=1。
            Vector3 worldRawInputDir = Quaternion.Euler(0f, camYaw, 0f) * new Vector3(inputN.x, 0f, inputN.y);
            Vector3 characterForward = worldDashDir.sqrMagnitude > 0.0001f ? worldDashDir : Vector3.forward;
            if (ctx.cc != null) {
                characterForward = ctx.cc.transform.forward;
            }
            characterForward.y = 0f;
            worldRawInputDir.y = 0f;
            if (characterForward.sqrMagnitude < 0.0001f) characterForward = Vector3.forward;
            if (worldRawInputDir.sqrMagnitude < 0.0001f) worldRawInputDir = characterForward;
            characterForward.Normalize();
            worldRawInputDir.Normalize();
            float relativeAngle = Vector3.SignedAngle(characterForward, worldRawInputDir, Vector3.up);
            dodgeDir = QuantizeTo8Dir(relativeAngle);

            if (ctx.anim != null) {
                ctx.anim.SetFloat(AnimatorKeys.Params.DodgeX, dodgeDir.x);
                ctx.anim.SetFloat(AnimatorKeys.Params.DodgeY, dodgeDir.y);
                ctx.anim.CrossFade(AnimatorKeys.States.Dodge, 0.05f);
            }
        }

        protected override void OnUpdate(float deltaTime) {
            remainingTime -= deltaTime;
        }

        static Vector2 QuantizeTo8Dir(float signedAngleDeg) {
            int sector = Mathf.RoundToInt(signedAngleDeg / 45f);
            switch (sector) {
                case 0:  return new Vector2(0f, 1f);   // 前
                case 1:  return new Vector2(1f, 1f);   // 前右
                case 2:  return new Vector2(1f, 0f);   // 右
                case 3:  return new Vector2(1f, -1f);  // 后右
                case 4:
                case -4: return new Vector2(0f, -1f);  // 后
                case -3: return new Vector2(-1f, -1f); // 后左
                case -2: return new Vector2(-1f, 0f);  // 左
                case -1: return new Vector2(-1f, 1f);  // 前左
                default: return new Vector2(0f, 1f);
            }
        }
    }
}

