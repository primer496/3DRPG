using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
//using UnityUtils;

namespace HSM {
    public class PlayerStateDriver : MonoBehaviour {
        public PlayerContext ctx = new PlayerContext();
        public Transform groundCheck;
        public float groundRadius = 0.2f;
        public LayerMask groundMask;
        public bool drawGizmos = true;
        string lastPath;

        Rigidbody rb;
        StateMachine machine;
        State root;

        // Input System actions
        public InputAction moveAction;
        public InputAction jumpAction;
        public InputAction runAction;
        public InputAction dodgeAction;
        public InputAction attackAction;

        void Awake() {
            rb = GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            // ƽ����������Ⱦ֮֡���λ�ˣ�������������������������µĶ�����
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            ctx.rb = rb;
            ctx.anim = GetComponentInChildren<Animator>();
            ctx.renderer = GetComponent<Renderer>();
            root = new PlayerRoot(null, ctx);
            var builder = new StateMachineBuilder(root);
            machine = builder.Build();

            // fallback: create a groundCheck just below the collider's bounds
            if (groundCheck == null) {
                var col = GetComponent<Collider>();
                var t = new GameObject("groundCheck").transform;
                t.SetParent(transform, false);
                var y = col ? (-col.bounds.extents.y + 0.01f) : -0.5f;
                t.localPosition = new Vector3(0, y, 0);
                groundCheck = t;
            }

            if (ctx.swordObject != null) {
                ctx.swordObject.SetActive(false);
            }
        }

        void OnEnable() {
            moveAction?.Enable();
            jumpAction?.Enable();
            runAction?.Enable();
            dodgeAction?.Enable();
            attackAction?.Enable();
        }

        void OnDisable() {
            moveAction?.Disable();
            jumpAction?.Disable();
            runAction?.Disable();
            dodgeAction?.Disable();
            attackAction?.Disable();
        }

        void Update() {

            var move = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
            ctx.moveInput = move;


            ctx.jumpPressed   = jumpAction  != null && jumpAction.WasPressedThisFrame();
            ctx.runHeld       = runAction   != null && runAction.IsPressed();
            ctx.dodgePressed  = dodgeAction != null && dodgeAction.WasPressedThisFrame();
            ctx.attackPressed = attackAction!= null && attackAction.WasPressedThisFrame();

            ctx.grounded = Physics.CheckSphere(
                groundCheck.position,
                groundRadius,
                groundMask
            );

            if (ctx.grounded) {
                ctx.PushGroundVelocity(ctx.velocity.x, ctx.velocity.z);
            }

            machine.Tick(Time.deltaTime);

            // 空中时在 Update 末尾更新 VerticalSpeed，保证每帧写入（避免状态机更新时机问题）
            var leaf = machine.Root.Leaf();
            if (leaf is Airborne && ctx.anim != null && ctx.rb != null && ctx.jumpSpeed > 0.0001f) {
                float vy = ctx.rb.velocity.y;
                float vSpeed = Mathf.Clamp(vy / ctx.jumpSpeed, -1f, 1f);
                ctx.anim.SetFloat("VerticalSpeed", vSpeed);
            }

            var path = StatePath(leaf);

            if (path != lastPath) {
                Debug.Log("State"+path);
                lastPath = path;
            }
        }

        void FixedUpdate() {
            var v = rb.velocity;
            v.x = ctx.velocity.x;
            v.z = ctx.velocity.z;
            rb.velocity = v;

            ctx.velocity.x = rb.velocity.x;
            ctx.velocity.z = rb.velocity.z;

            // ???? FixedUpdate ?????????????? Update ?????�� Rigidbody.rotation ?????????
            if (ctx.hasRotationTarget) {
                float t = ctx.rotationTurnSpeed * Time.fixedDeltaTime;
                if (t >= 1f) {
                    rb.MoveRotation(ctx.rotationTarget);
                } else {
                    rb.MoveRotation(Quaternion.Slerp(rb.rotation, ctx.rotationTarget, t));
                }

                ctx.hasRotationTarget = false;
            }
        }

        void OnDrawGizmosSelected() {
            if (!drawGizmos || groundCheck == null) return;

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }

        static string StatePath(State s) {
            return string.Join(" > ", s.PathToRoot().Reverse().Select(n => n.GetType().Name));
        }
        public void OnLook(InputValue value)
        {
            ctx.lookInput = value.Get<Vector2>();
        }

        /// <summary>
        /// 若 Animator 与本脚本在同一物体上，也可直接把事件指到本方法；否则请用 <see cref="FootPlantAnimationEvents"/>。
        /// </summary>
        public void OnFootPlant(int foot) {
            ctx.RegisterFootPlantFromAnimation(foot);
        }
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

        [Header("Movement")]
        public float moveSpeed = 6f;
        public float accel = 40f;
        public float jumpSpeed = 7f;
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
        public Rigidbody rb;
        [HideInInspector]
        public Renderer renderer;
        [HideInInspector]
        public Vector2 lookInput;

        // ????? State ???????????? FixedUpdate ????��?? Rigidbody?????? Update �� rotation ??????
        [HideInInspector]
        public Quaternion rotationTarget;
        [HideInInspector]
        public bool hasRotationTarget;
        [HideInInspector]
        public float rotationTurnSpeed;

        // ??/??????????????????/??????????????????????? Speed ???????
        public float GetWalkRealSpeed() => moveSpeed;
        public float GetRunRealSpeed() => moveSpeed * runSpeedMultiplier;
        public float GetTargetRealSpeed(float inputMagnitude) {
            var baseSpeed = runHeld ? GetRunRealSpeed() : GetWalkRealSpeed();
            return baseSpeed * Mathf.Clamp01(inputMagnitude);
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