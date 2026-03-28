using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
//using UnityUtils;

namespace HSM {
    public class PlayerStateDriver : MonoBehaviour {
        const string VerticalSpeedParam = "VerticalSpeed";
        public PlayerContext ctx = new PlayerContext();
        public Transform groundCheck;
        public float groundRadius = 0.2f;
        public float groundProbeDistance = 0.3f;
        [Range(1f, 89f)]
        public float maxGroundAngle = 55f;
        [Range(0f, 30f)]
        public float minProjectSlopeAngle = 3f;
        public float groundedSnapDownVelocity = 2f;
        [Tooltip("Ground probe miss grace time to avoid false airborne flicker on slope seams.")]
        public float groundUngroundedGraceTime = 0.08f;
        public LayerMask groundMask;
        public bool drawGizmos = true;
        string lastPath;

        Rigidbody rb;
        StateMachine machine;
        State root;
        Vector3 lastAppliedPlanarVelocity;
        float ungroundedTimer;
        bool hasGroundHitThisFrame;
        readonly InputReader inputReader = new InputReader();
        readonly GroundChecker groundProbe = new GroundChecker();

        // Input System actions
        public InputAction moveAction;
        public InputAction jumpAction;
        public InputAction runAction;
        public InputAction dodgeAction;
        public InputAction attackAction;

        void Awake() {
            InitializeRigidbody();
            InitializeContextReferences();
            BuildStateMachine();
            EnsureGroundCheck();
            InitializeSwordState();
        }

        void OnEnable() {
            ToggleInputActions(true);
        }

        void OnDisable() {
            ToggleInputActions(false);
        }

        void Update() {
            ReadInputIntoContext();
            UpdateGroundedState();
            TickStateMachine();
            SyncAirborneVerticalSpeed();
            LogStatePathIfChanged();
        }

        void FixedUpdate() {
            ApplyPlanarMotionToRigidbody();
            SyncPlanarVelocityFromRigidbody();
            ApplyQueuedRotation();
        }

        void InitializeRigidbody() {
            rb = GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            // ƽ����������Ⱦ֮֡���λ�ˣ�������������������������µĶ�����
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        void InitializeContextReferences() {
            ctx.rb = rb;
            ctx.anim = GetComponentInChildren<Animator>();
            ctx.renderer = GetComponent<Renderer>();
        }

        void BuildStateMachine() {
            root = new PlayerRoot(null, ctx);
            var builder = new StateMachineBuilder(root);
            machine = builder.Build();
        }

        void EnsureGroundCheck() {
            // fallback: create a groundCheck just below the collider's bounds
            if (groundCheck == null) {
                var col = GetComponent<Collider>();
                var t = new GameObject("groundCheck").transform;
                t.SetParent(transform, false);
                var y = col ? (-col.bounds.extents.y + 0.01f) : -0.5f;
                t.localPosition = new Vector3(0, y, 0);
                groundCheck = t;
            }
        }

        void InitializeSwordState() {
            if (ctx.swordObject != null) {
                ctx.swordObject.SetActive(false);
            }
        }

        void ToggleInputActions(bool enabled) {
            if (enabled) {
                moveAction?.Enable();
                jumpAction?.Enable();
                runAction?.Enable();
                dodgeAction?.Enable();
                attackAction?.Enable();
                return;
            }

            moveAction?.Disable();
            jumpAction?.Disable();
            runAction?.Disable();
            dodgeAction?.Disable();
            attackAction?.Disable();
        }

        void ReadInputIntoContext() {
            var input = inputReader.Read(
                moveAction,
                jumpAction,
                runAction,
                dodgeAction,
                attackAction
            );

            ctx.moveInput = input.Move;
            ctx.jumpPressed = input.JumpPressed;
            ctx.runHeld = input.RunHeld;
            ctx.dodgePressed = input.DodgePressed;
            ctx.attackPressed = input.AttackPressed;
        }

        void UpdateGroundedState() {
            bool inAirborneState = machine != null && machine.Root != null && machine.Root.Leaf() is Airborne;
            // Allow a small near-apex window as "still airborne" to avoid immediate re-grounding
            // right after jump transition (before vertical speed clearly becomes positive).
            bool risingInAir = inAirborneState && rb != null && rb.velocity.y > -0.1f;
            if (ctx.jumpGroundDetachTimer > 0f) {
                ctx.jumpGroundDetachTimer = Mathf.Max(0f, ctx.jumpGroundDetachTimer - Time.deltaTime);
            }

            bool hitGround = groundProbe.IsGrounded(
                groundCheck,
                groundRadius,
                groundProbeDistance,
                maxGroundAngle,
                groundMask,
                out var normal,
                out var hasReliableNormal
            );

            // During jump ascent, feet probes can still overlap slope/collider briefly.
            // Ignore those hits to prevent snapping back to grounded and killing the jump.
            if (risingInAir || ctx.jumpGroundDetachTimer > 0f) {
                hitGround = false;
            }
            hasGroundHitThisFrame = hitGround;

            if (hitGround) {
                // Keep previous stable normal when probe only confirms contact but cannot provide
                // a reliable surface normal (common on slope seams / tiny ledges).
                if (hasReliableNormal) {
                    ctx.groundNormal = normal;
                }
                ctx.grounded = true;
                ungroundedTimer = 0f;
                ctx.PushGroundVelocity(ctx.velocity.x, ctx.velocity.z);
                return;
            }

            // Safety: if probe missed and we're still moving upward, never keep grounded grace.
            // This prevents rare "grounded while floating" cases after dash-jump land/touch sequences.
            bool movingUpWithoutGround = rb != null && rb.velocity.y > 0.2f;
            bool canUseGrace = rb != null && rb.velocity.y <= 0.5f && !movingUpWithoutGround;
            if (ctx.grounded && canUseGrace && ungroundedTimer < groundUngroundedGraceTime) {
                ungroundedTimer += Time.deltaTime;
                return;
            }

            ctx.grounded = false;
            ungroundedTimer = 0f;
        }

        void TickStateMachine() {
            machine.Tick(Time.deltaTime);
        }

        void SyncAirborneVerticalSpeed() {
            // 空中时在 Update 末尾更新 VerticalSpeed，保证每帧写入（避免状态机更新时机问题）
            var leaf = machine.Root.Leaf();
            if (leaf is Airborne && ctx.anim != null && ctx.rb != null && ctx.jumpSpeed > 0.0001f) {
                float vy = ctx.rb.velocity.y;
                float vSpeed = Mathf.Clamp(vy / ctx.jumpSpeed, -1f, 1f);
                ctx.anim.SetFloat(VerticalSpeedParam, vSpeed);
            }
        }

        void LogStatePathIfChanged() {
            var leaf = machine.Root.Leaf();
            var path = StatePath(leaf);

            if (path != lastPath) {
                Debug.Log("State"+path);
                lastPath = path;
            }
        }

        void ApplyPlanarMotionToRigidbody() {
            bool forceAirControl = machine != null && machine.Root.Leaf() is Airborne;
            bool treatAsGrounded = ctx.grounded && !forceAirControl;
            bool reliableGround = treatAsGrounded && hasGroundHitThisFrame;
            var desiredPlanar = new Vector3(ctx.velocity.x, 0f, ctx.velocity.z);
            float slopeAngle = 0f;
            if (reliableGround) {
                var n = ctx.groundNormal.sqrMagnitude > 0.0001f ? ctx.groundNormal.normalized : Vector3.up;
                slopeAngle = Vector3.Angle(n, Vector3.up);
                if (slopeAngle >= minProjectSlopeAngle) {
                    desiredPlanar = Vector3.ProjectOnPlane(desiredPlanar, n);
                }
            }

            lastAppliedPlanarVelocity = desiredPlanar;

            if (treatAsGrounded) {
                // During grace frames (probe miss), never accumulate vertical climb from stale slope normal.
                if (!reliableGround) {
                    desiredPlanar.y = 0f;
                    lastAppliedPlanarVelocity = desiredPlanar;
                }

                // Ground motor: drive horizontal velocity only.
                // Writing projected Y directly can cancel natural jump/fall motion near ground
                // and cause "hovering" when state flips back to grounded too early.
                var v = rb.velocity;
                v.x = desiredPlanar.x;
                v.z = desiredPlanar.z;

                // Keep a mild downward bias only on temporary probe misses.
                // Applying this continuously on slopes causes unwanted downhill sliding.
                bool needsSnapDown = !hasGroundHitThisFrame;
                if (needsSnapDown && v.y <= 0.5f && v.y > -groundedSnapDownVelocity) {
                    v.y = -groundedSnapDownVelocity;
                }

                rb.velocity = v;
                return;
            }

            // Air control keeps velocity-driven motion.
            var airV = rb.velocity;
            airV.x = desiredPlanar.x;
            airV.z = desiredPlanar.z;
            rb.velocity = airV;
        }

        void SyncPlanarVelocityFromRigidbody() {
            bool forceAirControl = machine != null && machine.Root.Leaf() is Airborne;
            if (ctx.grounded && !forceAirControl) {
                ctx.velocity.x = lastAppliedPlanarVelocity.x;
                ctx.velocity.z = lastAppliedPlanarVelocity.z;
                return;
            }

            ctx.velocity.x = rb.velocity.x;
            ctx.velocity.z = rb.velocity.z;
        }

        void ApplyQueuedRotation() {
            // ???? FixedUpdate ?????????????? Update ?????�� Rigidbody.rotation ?????????
            // Contacts on slopes can inject Y angular velocity and fight with MoveRotation,
            // causing visible spin jitter. Keep yaw fully driven by state logic.
            var av = rb.angularVelocity;
            if (Mathf.Abs(av.y) > 0.0001f) {
                rb.angularVelocity = new Vector3(av.x, 0f, av.z);
            }

            if (ctx.hasRotationTarget) {
                float t = ctx.rotationTurnSpeed * Time.fixedDeltaTime;
                if (t >= 1f) {
                    rb.MoveRotation(ctx.rotationTarget);
                } else {
                    rb.MoveRotation(Quaternion.Slerp(rb.rotation, ctx.rotationTarget, t));
                }

                ctx.hasRotationTarget = false;
                return;
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

        readonly struct InputSnapshot {
            public readonly Vector2 Move;
            public readonly bool JumpPressed;
            public readonly bool RunHeld;
            public readonly bool DodgePressed;
            public readonly bool AttackPressed;

            public InputSnapshot(
                Vector2 move,
                bool jumpPressed,
                bool runHeld,
                bool dodgePressed,
                bool attackPressed
            ) {
                Move = move;
                JumpPressed = jumpPressed;
                RunHeld = runHeld;
                DodgePressed = dodgePressed;
                AttackPressed = attackPressed;
            }
        }

        sealed class InputReader {
            public InputSnapshot Read(
                InputAction moveAction,
                InputAction jumpAction,
                InputAction runAction,
                InputAction dodgeAction,
                InputAction attackAction
            ) {
                var move = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
                var jumpPressed = jumpAction != null && jumpAction.WasPressedThisFrame();
                var runHeld = runAction != null && runAction.IsPressed();
                var dodgePressed = dodgeAction != null && dodgeAction.WasPressedThisFrame();
                var attackPressed = attackAction != null && attackAction.WasPressedThisFrame();

                return new InputSnapshot(move, jumpPressed, runHeld, dodgePressed, attackPressed);
            }
        }

        sealed class GroundChecker {
            public bool IsGrounded(
                Transform groundCheck,
                float groundRadius,
                float probeDistance,
                float maxGroundAngle,
                LayerMask groundMask,
                out Vector3 groundNormal,
                out bool hasReliableNormal
            ) {
                groundNormal = Vector3.up;
                hasReliableNormal = false;
                if (groundCheck == null) return false;

                var origin = groundCheck.position + Vector3.up * Mathf.Max(0f, probeDistance);
                var castDistance = Mathf.Max(groundRadius + probeDistance, groundRadius + 0.05f);

                if (Physics.SphereCast(
                    origin,
                    groundRadius,
                    Vector3.down,
                    out var hit,
                    castDistance,
                    groundMask,
                    QueryTriggerInteraction.Ignore
                )) {
                    var slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                    if (slopeAngle <= maxGroundAngle) {
                        groundNormal = hit.normal;
                        hasReliableNormal = true;
                        return true;
                    }
                }

                // Fallback for edge cases (ledge transitions / tiny gaps).
                if (Physics.CheckSphere(groundCheck.position, groundRadius, groundMask, QueryTriggerInteraction.Ignore)) {
                    // Try to recover a usable normal for slope projection stability.
                    var fallbackOrigin = groundCheck.position + Vector3.up * 0.1f;
                    if (Physics.Raycast(
                        fallbackOrigin,
                        Vector3.down,
                        out var fallbackHit,
                        Mathf.Max(probeDistance + groundRadius + 0.2f, 0.35f),
                        groundMask,
                        QueryTriggerInteraction.Ignore
                    )) {
                        var fallbackSlopeAngle = Vector3.Angle(fallbackHit.normal, Vector3.up);
                        if (fallbackSlopeAngle <= maxGroundAngle) {
                            groundNormal = fallbackHit.normal;
                            hasReliableNormal = true;
                        }
                    }
                    return true;
                }

                return false;
            }
        }
    }
}