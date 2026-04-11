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
        // 运行时输入与状态
        [HideInInspector]
        public Vector2 moveInput;
        [HideInInspector]
        public Vector3 velocity;
        [HideInInspector]
        public bool grounded;
        [HideInInspector]
        public Vector3 groundNormal = Vector3.up;

        // 基础移动参数（由配置资产覆盖）
        [HideInInspector]
        public float moveSpeed = 6f;
        [HideInInspector]
        public float accel = 40f;
        [Min(0.1f)]
        [HideInInspector]
        public float jumpHeight = 2.5f;
        [HideInInspector]
        public float jumpSpeed = 7f;
        [Range(0f, 0.3f)]
        [HideInInspector]
        public float jumpGroundDetachTime = 0.12f;
        [HideInInspector]
        public bool jumpPressed;
        [HideInInspector]
        public bool runHeld;
        [HideInInspector]
        public bool dodgePressed;
        [HideInInspector]
        public bool attackPressed;
        [HideInInspector]
        public float runSpeedMultiplier = 2f;

        // 能力开关（由配置资产覆盖）
        [HideInInspector]
        public bool enableLocomotion = true;
        [HideInInspector]
        public bool enableCombat = true;
        [HideInInspector]
        public bool enableJump = true;
        [HideInInspector]
        public bool enableTraversal = true;

        // 闪避参数（由配置资产覆盖）
        [HideInInspector]
        public float dodgeSpeed = 10f;
        [HideInInspector]
        public float dodgeDuration = 0.25f;

        // 翻越参数（由配置资产覆盖）
        [Min(0.05f)]
        [HideInInspector]
        public float vaultDuration = 0.42f;
        [Min(0f)]
        [HideInInspector]
        public float vaultEnterCrossFade = 0.08f;
        [Min(0f)]
        [HideInInspector]
        public float vaultExitCrossFade = 0.1f;
        [Range(0.6f, 0.99f)]
        [HideInInspector]
        public float vaultExitNormalizedTime = 0.9f;
        [Range(0.5f, 0.98f)]
        [HideInInspector]
        public float vaultLateDownStartNormalizedTime = 0.7f;
        [Range(0f, 8f)]
        [HideInInspector]
        public float vaultLateDownSpeed = 3.2f;
        [Min(0f)]
        [HideInInspector]
        public float vaultMinMoveSpeed = 0.2f;
        [HideInInspector]
        public LayerMask vaultWallMask;
        [Min(0.05f)]
        [HideInInspector]
        public float vaultDetectDistance = 0.75f;
        [Range(1f, 89f)]
        [HideInInspector]
        public float vaultMaxFacingAngle = 45f;
        [Min(0f)]
        [HideInInspector]
        public float vaultMinHeight = 0.75f;
        [Min(0f)]
        [HideInInspector]
        public float vaultMaxHeight = 1.2f;
        [Min(0f)]
        [HideInInspector]
        public float vaultSampleMinHeight = 0.2f;
        [Min(0f)]
        [HideInInspector]
        public float vaultSampleMaxHeight = 1.6f;
        [Range(3, 10)]
        [HideInInspector]
        public int vaultHeightSamples = 6;
        [HideInInspector]
        public bool vaultDebugLog = false;

        // 攀爬参数（由配置资产覆盖）
        [HideInInspector]
        public LayerMask climbWallMask;
        [Min(0.05f)]
        [HideInInspector]
        public float climbDetectDistance = 0.75f;
        [Range(1f, 89f)]
        [HideInInspector]
        public float climbMaxFacingAngle = 45f;
        [Min(0f)]
        [HideInInspector]
        public float climbSampleMinHeight = 0.1f;
        [Min(0f)]
        [HideInInspector]
        public float climbSampleMaxHeight = 2.4f;
        [Range(4, 16)]
        [HideInInspector]
        public int climbHeightSamples = 10;
        [Min(0f)]
        [HideInInspector]
        public float climbEnterCrossFade = 0.1f;
        [Min(0f)]
        [HideInInspector]
        public float climbExitCrossFade = 0.12f;
        [Range(0.7f, 0.99f)]
        [HideInInspector]
        public float climbExitNormalizedTime = 0.92f;
        [Range(0.8f, 0.995f)]
        [HideInInspector]
        public float climb17ExitNormalizedTime = 0.975f;
        [Range(0f, 2f)]
        [HideInInspector]
        public float climb17PlanarAssistSpeed = 0.45f;
        [Range(0f, 1f)]
        [HideInInspector]
        public float climb17MinPlanarSpeed = 0.08f;
        [HideInInspector]
        public bool climbDebugLog = false;
        [HideInInspector]
        public bool isClimbing;
        [HideInInspector]
        public bool exitedClimbThisFrame;
        [HideInInspector]
        public ClimbHeightTier detectedClimbTier;

        // 贴墙朝向参数（由配置资产覆盖）
        [Range(0.02f, 0.25f)]
        [HideInInspector]
        public float wallActionAlignDuration = 0.08f;
        [Range(0f, 45f)]
        [HideInInspector]
        public float wallActionAlignMinAngle = 8f;
        [HideInInspector]
        public Vector3 detectedWallNormal;
        [HideInInspector]
        public bool hasDetectedWallNormal;
        [HideInInspector]
        public float detectedWallHeight;

        // 贴墙高度拟合参数（由配置资产覆盖）
        [Min(0f)]
        [HideInInspector]
        public float vaultReferenceWallHeight = 1.0f;
        [Min(0f)]
        [HideInInspector]
        public float climb05ReferenceWallHeight = 0.5f;
        [Min(0f)]
        [HideInInspector]
        public float climb10ReferenceWallHeight = 1.0f;
        [Min(0f)]
        [HideInInspector]
        public float climb17ReferenceWallHeight = 1.5f;
        [Min(0f)]
        [HideInInspector]
        public float climb20ReferenceWallHeight = 2.0f;
        [Range(0.1f, 6f)]
        [HideInInspector]
        public float wallActionHeightAdjustSpeed = 2.4f;
        [Range(0f, 1.2f)]
        [HideInInspector]
        public float wallActionMaxUpOffset = 0.6f;
        [Range(0f, 1.2f)]
        [HideInInspector]
        public float wallActionMaxDownOffset = 0.45f;
        [HideInInspector]
        public float pendingWallActionHeightOffsetY;
        [HideInInspector]
        public float wallActionHeightOffsetRemainingY;

        // 急停参数（由配置资产覆盖）
        [Min(0f)]
        [HideInInspector]
        public float stopDuration = 0.26f;

        [Min(0f)]
        [HideInInspector]
        public float stopEnterSpeedThreshold = 0.32f;

        [Min(0f)]
        [HideInInspector]
        public float stopEnterCrossFade = 0.06f;

        [FormerlySerializedAs("stopSpeedDampTime")]
        [Min(0.001f)]
        [HideInInspector]
        public float stopSpeedDecayTime = 0.12f;

        // 运行时缓存
        [HideInInspector]
        public Vector2 lastStickWhileMoving;

        [HideInInspector]
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
        public bool hasFootPlantData;

        [HideInInspector]
        public bool lastPlantedFootIsRight;

        // 状态切换标记
        [HideInInspector]
        public bool exitedStopThisFrame;

        [HideInInspector]
        public bool justLanded;

        [HideInInspector]
        public bool exitedLandingThisFrame;
        [HideInInspector]
        public bool exitedVaultThisFrame;
        [HideInInspector]
        public bool isVaulting;
        [HideInInspector]
        public float jumpGroundDetachTimer;
        [HideInInspector]
        public bool combatRootMotionActive;

        // 战斗参数（由配置资产覆盖）
        [HideInInspector]
        public float comboResetTime = 0.6f;
        [Min(1)]
        [HideInInspector]
        public int maxComboSteps = 4;
        [HideInInspector]
        public bool useCombatRootMotion = true;
        [Range(0f, 2f)]
        [HideInInspector]
        public float combatRootMotionPlanarScale = 1f;

        // 连段窗口参数（由配置资产覆盖）
        [Range(0f, 1f)]
        [HideInInspector]
        public float combo1WindowStart = 0.38f;
        [Range(0f, 1f)]
        [HideInInspector]
        public float combo1WindowEnd = 0.70f;

        [Range(0f, 1f)]
        [HideInInspector]
        public float combo2WindowStart = 0.22f;
        [Range(0f, 1f)]
        [HideInInspector]
        public float combo2WindowEnd = 0.68f;

        [Range(0f, 1f)]
        [HideInInspector]
        public float combo3WindowStart = 0.20f;
        [Range(0f, 1f)]
        [HideInInspector]
        public float combo3WindowEnd = 0.66f;

        [Range(0f, 1f)]
        [HideInInspector]
        public float combo4WindowStart = 0.18f;
        [Range(0f, 1f)]
        [HideInInspector]
        public float combo4WindowEnd = 0.60f;

        [Range(0.5f, 1.2f)]
        [HideInInspector]
        public float comboExitNormalizedTime = 0.95f;

        // 运行时引用
        [HideInInspector]
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
        [HideInInspector]
        public Func<float> facingYawProvider;

        // 状态中只设置目标旋转，统一在 PlayerStateDriver.Update 末尾应用，避免多处写 Transform.rotation 造成抖动。
        [HideInInspector]
        public Quaternion rotationTarget;
        [HideInInspector]
        public bool hasRotationTarget;
        [HideInInspector]
        public float rotationTurnSpeed;
        [HideInInspector]
        public bool wallActionAlignActive;
        [HideInInspector]
        public float wallActionAlignElapsed;
        [HideInInspector]
        public float wallActionAlignDurationRuntime;
        [HideInInspector]
        public Quaternion wallActionAlignFrom;
        [HideInInspector]
        public Quaternion wallActionAlignTo;

        // 真实速度接口：用于动画 Speed 与物理速度的统一标定。
        public float GetWalkRealSpeed() => moveSpeed;
        public float GetRunRealSpeed() => moveSpeed * runSpeedMultiplier;
        public float GetTargetRealSpeed(float inputMagnitude) {
            var baseSpeed = runHeld ? GetRunRealSpeed() : GetWalkRealSpeed();
            return baseSpeed * Mathf.Clamp01(inputMagnitude);
        }

        public float GetFacingReferenceYaw() {
            if (facingYawProvider != null) {
                return facingYawProvider();
            }

            float yaw = ThirdPersonCamera.CurrentYawDeg;
            if (!float.IsNaN(yaw)) {
                return yaw;
            }

            var cam = Camera.main;
            return cam != null ? cam.transform.eulerAngles.y : 0f;
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
