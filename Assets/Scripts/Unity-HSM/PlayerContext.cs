using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace HSM {
    public enum ClimbHeightTier {
        None,
        Climb05,   // <= 0.75m
        Climb10,   // 0.75 ~ 1.2m
        Climb17,   // 1.2 ~ 1.85m
        Climb20    // 1.85 ~ 2.2m
    }

    [Serializable]
    public class PlayerContext {
        [Header("Runtime Input/State")]
        [HideInInspector]
        public Vector2 moveInput;
        [HideInInspector]
        public Vector3 velocity;
        [HideInInspector]
        public bool grounded;
        [HideInInspector]
        public Vector3 groundNormal = Vector3.up;

        [Header("Movement")]
        public float moveSpeed = 6f;
        public float accel = 40f;
        [Tooltip("目标跳跃高度（米）；实际起跳速度将由该高度自动计算。")]
        [Min(0.1f)]
        public float jumpHeight = 2.5f;
        [Tooltip("旧版本起跳速度参数（兼容保留）。当 jumpHeight 非法时用作兜底。")]
        public float jumpSpeed = 7f;
        [Tooltip("起跳后忽略地面重判时长（秒），用于避免吃跳。")]
        [Range(0f, 0.3f)]
        public float jumpGroundDetachTime = 0.12f;
        [HideInInspector]
        public bool jumpPressed;
        [HideInInspector]
        public bool runHeld;
        [HideInInspector]
        public bool dodgePressed;
        [HideInInspector]
        public bool attackPressed;
        public float runSpeedMultiplier = 2f;

        [Header("Dodge")]
        public float dodgeSpeed = 10f;
        public float dodgeDuration = 0.25f;

        [Header("Vault")]
        [Tooltip("翻越状态总时长（秒）。")]
        [Min(0.05f)]
        public float vaultDuration = 0.42f;
        [Tooltip("进入翻越动画的 CrossFade 时长（秒）。")]
        [Min(0f)]
        public float vaultEnterCrossFade = 0.08f;
        [Tooltip("翻越结束切回 Locomotion 的 CrossFade 时长（秒）。")]
        [Min(0f)]
        public float vaultExitCrossFade = 0.1f;
        [Tooltip("Vault 动画播放到该归一化时间后即可退出（减少完整播放占比）。")]
        [Range(0.6f, 0.99f)]
        public float vaultExitNormalizedTime = 0.9f;
        [Tooltip("走跑中触发翻越所需的最小水平速度。")]
        [Min(0f)]
        public float vaultMinMoveSpeed = 0.2f;
        [Tooltip("翻越墙体检测图层。为空时自动使用 Wall 图层。")]
        public LayerMask vaultWallMask;
        [Tooltip("前向墙体检测最大距离（米）。")]
        [Min(0.05f)]
        public float vaultDetectDistance = 0.75f;
        [Tooltip("角色前向与入墙方向（-墙法线）夹角阈值（度）。")]
        [Range(1f, 89f)]
        public float vaultMaxFacingAngle = 45f;
        [Tooltip("翻越墙体可接受最小高度（米）。")]
        [Min(0f)]
        public float vaultMinHeight = 0.75f;
        [Tooltip("翻越墙体可接受最大高度（米）。")]
        [Min(0f)]
        public float vaultMaxHeight = 1.2f;
        [Tooltip("用于估算墙高的最低采样高度（相对脚底，米）。")]
        [Min(0f)]
        public float vaultSampleMinHeight = 0.2f;
        [Tooltip("用于估算墙高的最高采样高度（相对脚底，米）。")]
        [Min(0f)]
        public float vaultSampleMaxHeight = 1.6f;
        [Tooltip("墙高采样层数。")]
        [Range(3, 10)]
        public int vaultHeightSamples = 6;
        [Tooltip("输出翻越判定失败原因日志，调试用。")]
        public bool vaultDebugLog = true;

        [Header("Climb")]
        [Tooltip("攀爬墙体检测图层。为空时自动使用 Wall 图层。")]
        public LayerMask climbWallMask;
        [Tooltip("前向墙体检测最大距离（米）。")]
        [Min(0.05f)]
        public float climbDetectDistance = 0.75f;
        [Tooltip("角色前向与入墙方向夹角阈值（度）。")]
        [Range(1f, 89f)]
        public float climbMaxFacingAngle = 45f;
        [Tooltip("用于估算墙高的最低采样高度（相对脚底，米）。")]
        [Min(0f)]
        public float climbSampleMinHeight = 0.1f;
        [Tooltip("用于估算墙高的最高采样高度（相对脚底，米）。")]
        [Min(0f)]
        public float climbSampleMaxHeight = 2.4f;
        [Tooltip("墙高采样层数。")]
        [Range(4, 16)]
        public int climbHeightSamples = 10;
        [Tooltip("进入攀爬动画的 CrossFade 时长（秒）。")]
        [Min(0f)]
        public float climbEnterCrossFade = 0.1f;
        [Tooltip("攀爬结束切回 Locomotion 的 CrossFade 时长（秒）。")]
        [Min(0f)]
        public float climbExitCrossFade = 0.12f;
        [Tooltip("攀爬动画播放到该归一化时间后即可退出。")]
        [Range(0.7f, 0.99f)]
        public float climbExitNormalizedTime = 0.92f;
        [Tooltip("输出攀爬判定失败原因日志，调试用。")]
        public bool climbDebugLog = true;
        [HideInInspector]
        [Tooltip("攀爬执行中：屏蔽父状态的跳跃等中断转移。")]
        public bool isClimbing;
        [HideInInspector]
        [Tooltip("从 Climb 退出到 Move 时不再重复 CrossFade。")]
        public bool exitedClimbThisFrame;
        [HideInInspector]
        [Tooltip("本次攀爬检测到的墙高档位。")]
        public ClimbHeightTier detectedClimbTier;

        [Header("Stop Tuning")]
        [Tooltip("急停状态最短停留时间（秒）；HSM 在此时间后才跟随 Animator 的 StopType->Locomotion 过渡切回 Move。")]
        [Min(0f)]
        public float stopDuration = 0.26f;

        [Tooltip("速度高于该值才会从 Move 进入 Stop。")]
        [Min(0f)]
        public float stopEnterSpeedThreshold = 0.32f;

        [Tooltip("进入 StopType 的 CrossFade 时长（秒）。")]
        [Min(0f)]
        public float stopEnterCrossFade = 0.06f;

        [Tooltip("急停期间 Animator Speed 指数衰减的时间常数 τ（秒），供 Animator 条件过渡使用。")]
        [FormerlySerializedAs("stopSpeedDampTime")]
        [Min(0.001f)]
        public float stopSpeedDecayTime = 0.12f;

        [Header("Runtime / Cached")]
        [HideInInspector]
        [Tooltip("上一帧仍按住移动时的摇杆输入（相机空间 XZ），松开当帧用于急停方向，与冲刺输入空间一致。")]
        public Vector2 lastStickWhileMoving;

        [HideInInspector]
        [Tooltip("地面上最近 3 帧的水平速度（XZ），用于空中继承动量。")]
        readonly Vector2[] groundVelocityHistory = new Vector2[3];
        int groundVelocityWriteIndex;
        int groundVelocityCount;

        /// <summary>
        /// 地面时每帧调用，记录当前水平速度。
        /// </summary>
        public void PushGroundVelocity(float vx, float vz) {
            groundVelocityHistory[groundVelocityWriteIndex] = new Vector2(vx, vz);
            groundVelocityWriteIndex = (groundVelocityWriteIndex + 1) % 3;
            if (groundVelocityCount < 3) groundVelocityCount++;
        }

        /// <summary>
        /// 空中时获取地面最近 3 帧的平均水平速度。
        /// </summary>
        public Vector2 GetAverageGroundVelocity() {
            if (groundVelocityCount == 0) return Vector2.zero;
            Vector2 sum = Vector2.zero;
            for (int i = 0; i < groundVelocityCount; i++)
                sum += groundVelocityHistory[i];
            return sum / groundVelocityCount;
        }

        [HideInInspector]
        [Tooltip("是否至少收到过一次脚步动画事件（未收到前急停用交替/固定）。")]
        public bool hasFootPlantData;

        [HideInInspector]
        [Tooltip("最后着地的是否为右脚（OnFootPlant 传入 1 时为 true）。")]
        public bool lastPlantedFootIsRight;

        [Header("Runtime Flags")]
        [HideInInspector]
        public bool exitedStopThisFrame;

        [HideInInspector]
        [Tooltip("刚从空中落地，进入 Grounded 时应先进入 Landing。")]
        public bool justLanded;

        [HideInInspector]
        [Tooltip("从 Landing 退出到 Move 时 Animator 已由过渡切到 Locomotion，不再 CrossFade。")]
        public bool exitedLandingThisFrame;
        [HideInInspector]
        [Tooltip("从 Vault 退出到 Move 时 Animator 已做过渡，不再重复 CrossFade。")]
        public bool exitedVaultThisFrame;
        [HideInInspector]
        [Tooltip("翻越执行中：用于屏蔽父状态的跳跃等中断转移。")]
        public bool isVaulting;
        [HideInInspector]
        [Tooltip("起跳后剩余离地锁定时间（秒），倒计时期间忽略 grounded 重判。")]
        public float jumpGroundDetachTimer;

        [Header("Combat")]
        public float comboResetTime = 0.6f;
        [Min(1)]
        public int maxComboSteps = 4;

        [Header("Combo Windows (Normalized Time)")]
        [Range(0f, 1f)]
        [Tooltip("第一段连段输入窗口起点（适当后延，避免过早抢输入）。")]
        public float combo1WindowStart = 0.38f;
        [Range(0f, 1f)]
        public float combo1WindowEnd = 0.70f;

        [Range(0f, 1f)]
        public float combo2WindowStart = 0.22f;
        [Range(0f, 1f)]
        public float combo2WindowEnd = 0.68f;

        [Range(0f, 1f)]
        public float combo3WindowStart = 0.20f;
        [Range(0f, 1f)]
        public float combo3WindowEnd = 0.66f;

        [Range(0f, 1f)]
        [Tooltip("最后一段通常不再连下一段，保留参数便于后续扩展。")]
        public float combo4WindowStart = 0.18f;
        [Range(0f, 1f)]
        public float combo4WindowEnd = 0.60f;

        [Range(0.5f, 1.2f)]
        [Tooltip("当前段超过该归一化时间且无可连输入时，退出 Combat。")]
        public float comboExitNormalizedTime = 0.95f;

        [Header("References")]
        [Tooltip("攻击时显示/非攻击时隐藏的武器对象（使用 SetActive 控制）。")]
        public GameObject swordObject;
        [HideInInspector]
        public Animator anim;
        [HideInInspector]
        public CharacterController cc;
        [HideInInspector]
        public float verticalVelocity;
        [HideInInspector]
        public Renderer renderer;
        [HideInInspector]
        public Vector2 lookInput;

        // 状态中只设置目标旋转，统一在 PlayerStateDriver.Update 末尾应用，避免多处写 Transform.rotation 造成抖动。
        [HideInInspector]
        public Quaternion rotationTarget;
        [HideInInspector]
        public bool hasRotationTarget;
        [HideInInspector]
        public float rotationTurnSpeed;

        // 真实速度接口：用于动画 Speed 与物理速度的统一标定。
        public float GetWalkRealSpeed() => moveSpeed;
        public float GetRunRealSpeed() => moveSpeed * runSpeedMultiplier;
        public float GetTargetRealSpeed(float inputMagnitude) {
            var baseSpeed = runHeld ? GetRunRealSpeed() : GetWalkRealSpeed();
            return baseSpeed * Mathf.Clamp01(inputMagnitude);
        }

        public float GetJumpTakeoffSpeed() {
            float safeHeight = Mathf.Max(0f, jumpHeight);
            if (safeHeight > 0.0001f) {
                float gravity = Mathf.Max(0.01f, -Physics.gravity.y);
                return Mathf.Sqrt(2f * gravity * safeHeight);
            }

            return Mathf.Max(0.0001f, jumpSpeed);
        }

        /// <summary>
        /// 由 Animation Event <c>OnFootPlant</c> 调用：0=左脚，1=右脚。
        /// </summary>
        public void RegisterFootPlantFromAnimation(int foot) {
            lastPlantedFootIsRight = foot == 0;
            hasFootPlantData = true;
        }
    }
}
