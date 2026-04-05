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
        [Header("Air Wall Handling")]
        [Tooltip("Air control forward wall detection mask. Leave empty to auto-use Wall layer.")]
        public LayerMask airWallMask;
        [Range(0.05f, 0.6f)]
        [Tooltip("Forward wall check distance for removing into-wall velocity while airborne.")]
        public float airWallBlockDistance = 0.22f;
        [Range(0f, 0.2f)]
        [Tooltip("Keep last valid wall normal for this long to avoid frame-by-frame wall constraint flicker.")]
        public float airWallNormalHoldTime = 0.08f;
        [Range(0f, 2f)]
        [Tooltip("Extra planar detach speed from wall near apex/fall to prevent top-edge sticking.")]
        public float airWallEdgeDetachSpeed = 0.8f;
        public bool drawGizmos = true;
        string lastPath;

        CharacterController cc;
        StateMachine machine;
        State root;
        Vector3 lastAppliedPlanarVelocity;
        float ungroundedTimer;
        bool hasGroundHitThisFrame;
        Vector3 cachedAirWallNormalPlanar;
        float airWallNormalHoldTimer;
        readonly InputReader inputReader = new InputReader();
        readonly GroundChecker groundProbe = new GroundChecker();

        // Input System actions
        public InputAction moveAction;
        public InputAction jumpAction;
        public InputAction runAction;
        public InputAction dodgeAction;
        public InputAction attackAction;

        void Awake() {
            InitializeCharacterController();
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
            ApplyCharacterMotion();
            ApplyQueuedRotation();
            SyncAirborneVerticalSpeed();
            LogStatePathIfChanged();
        }

        void InitializeCharacterController() {
            cc = GetComponent<CharacterController>();
            if (cc == null) {
                Debug.LogError("PlayerStateDriver requires CharacterController on the same GameObject.");
                enabled = false;
            }
        }

        void InitializeContextReferences() {
            ctx.cc = cc;
            ctx.verticalVelocity = 0f;
            ctx.combatRootMotionActive = false;
            ctx.anim = GetComponentInChildren<Animator>();
            if (ctx.anim != null) {
                ctx.anim.applyRootMotion = false;
            }
            ctx.renderer = GetComponent<Renderer>();
        }

        void BuildStateMachine() {
            root = new PlayerRoot(null, ctx);
            var builder = new StateMachineBuilder(root);
            machine = builder.Build();
        }

        void EnsureGroundCheck() {
            // fallback: create a groundCheck near CharacterController feet.
            if (groundCheck == null) {
                var t = new GameObject("groundCheck").transform;
                t.SetParent(transform, false);
                float y = -0.5f;
                if (cc != null) {
                    float halfHeight = cc.height * 0.5f;
                    y = cc.center.y - halfHeight + cc.radius + 0.01f;
                }
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
            if (ctx.isVaulting || ctx.isClimbing) {
                ctx.grounded = true;
                ungroundedTimer = 0f;
                hasGroundHitThisFrame = true;
                return;
            }

            bool inAirborneState = IsInAirborneState();
            // Allow a small near-apex window as "still airborne" to avoid immediate re-grounding
            // right after jump transition (before vertical speed clearly becomes positive).
            bool risingInAir = inAirborneState && ctx.verticalVelocity > -0.1f;
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
            if (!hitGround && cc != null && cc.isGrounded && ctx.verticalVelocity <= 0.5f) {
                hitGround = true;
            }

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
            bool movingUpWithoutGround = ctx.verticalVelocity > 0.2f;
            bool canUseGrace = ctx.verticalVelocity <= 0.5f && !movingUpWithoutGround;
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
            float jumpTakeoffSpeed = ctx.GetJumpTakeoffSpeed();
            if (IsAirborneState(leaf) && ctx.anim != null && jumpTakeoffSpeed > 0.0001f) {
                float vy = ctx.verticalVelocity;
                float vSpeed = Mathf.Clamp(vy / jumpTakeoffSpeed, -1f, 1f);
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

        void ApplyCharacterMotion() {
            if (IsRootMotionTranslationActive()) {
                // Vault/Climb/Combat root motion translation is handled in OnAnimatorMove.
                return;
            }

            bool forceAirControl = IsInAirborneState();
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

                // Keep a mild downward bias only on temporary probe misses.
                // Applying this continuously on slopes causes unwanted downhill sliding.
                bool needsSnapDown = !hasGroundHitThisFrame;
                if (needsSnapDown && ctx.verticalVelocity <= 0.5f && ctx.verticalVelocity > -groundedSnapDownVelocity) {
                    ctx.verticalVelocity = -groundedSnapDownVelocity;
                }
            } else {
                // Air control keeps velocity-driven planar motion and explicit gravity.
                desiredPlanar = RemoveIntoWallComponentWhileAirborne(desiredPlanar);
                ctx.verticalVelocity += Physics.gravity.y * Time.deltaTime;
            }

            var motion = desiredPlanar + Vector3.up * ctx.verticalVelocity;
            CollisionFlags flags = cc != null ? cc.Move(motion * Time.deltaTime) : CollisionFlags.None;
            bool touchedGroundByMove = (flags & CollisionFlags.Below) != 0;

            if ((flags & CollisionFlags.Above) != 0 && ctx.verticalVelocity > 0f) {
                ctx.verticalVelocity = 0f;
            }

            if ((treatAsGrounded || touchedGroundByMove) && ctx.verticalVelocity <= 0f) {
                // Apply tiny down hold only after move resolution so jump takeoff won't be cancelled.
                ctx.verticalVelocity = -Mathf.Max(0.05f, groundedSnapDownVelocity * 0.35f);
            }

            if (touchedGroundByMove) {
                hasGroundHitThisFrame = true;
            }

            if (treatAsGrounded) {
                ctx.velocity.x = lastAppliedPlanarVelocity.x;
                ctx.velocity.z = lastAppliedPlanarVelocity.z;
                return;
            }

            ctx.velocity.x = desiredPlanar.x;
            ctx.velocity.z = desiredPlanar.z;
        }

        void OnAnimatorMove() {
            if (!IsRootMotionTranslationActive() || ctx.anim == null || cc == null) {
                return;
            }

            Vector3 deltaPos = ctx.anim.deltaPosition;
            if (ctx.combatRootMotionActive) {
                deltaPos = BuildCombatRootMotionDelta(deltaPos);
            }

            cc.Move(deltaPos);
            // Root-motion states use translation only. Rotation stays camera-driven to avoid camera jitter.

            // Keep state velocity neutral while root motion controls displacement.
            if (ctx.combatRootMotionActive && Time.deltaTime > 0.0001f) {
                // Preserve a meaningful planar speed sample so later combat-exit blending can reuse it.
                ctx.velocity.x = deltaPos.x / Time.deltaTime;
                ctx.velocity.z = deltaPos.z / Time.deltaTime;
            } else {
                ctx.velocity.x = 0f;
                ctx.velocity.z = 0f;
            }
            ctx.verticalVelocity = 0f;
        }

        bool IsRootMotionTranslationActive() {
            return ctx.isVaulting || ctx.isClimbing || ctx.combatRootMotionActive;
        }

        Vector3 BuildCombatRootMotionDelta(Vector3 animatorDeltaPosition) {
            float planarScale = Mathf.Max(0f, ctx.combatRootMotionPlanarScale);
            // For grounded melee, only keep planar displacement from animation.
            // Vertical combat motion can be introduced later by specific skills (launcher, leap slash, etc.).
            var proposedDelta = new Vector3(
                animatorDeltaPosition.x * planarScale,
                0f,
                animatorDeltaPosition.z * planarScale
            );
            return ResolveCombatRootMotionWithFutureHooks(proposedDelta);
        }

        Vector3 ResolveCombatRootMotionWithFutureHooks(Vector3 proposedDelta) {
            // Placeholder interface for future enemy interaction:
            // 1) Stop-on-hit: block or reduce proposedDelta when colliding with target.
            // 2) Push-target: keep forward delta while pushing enemy (requires enemy motor contract).
            // 3) Pass-through skill: temporarily allow overlap by per-skill policy.
            // Current implementation intentionally does not alter root motion.
            return proposedDelta;
        }

        void ApplyQueuedRotation() {
            if (ctx.wallActionAlignActive) {
                float duration = Mathf.Max(0.02f, ctx.wallActionAlignDurationRuntime);
                ctx.wallActionAlignElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(ctx.wallActionAlignElapsed / duration);
                transform.rotation = Quaternion.Lerp(ctx.wallActionAlignFrom, ctx.wallActionAlignTo, t);
                if (t >= 1f) {
                    ctx.wallActionAlignActive = false;
                }
                ctx.hasRotationTarget = false;
                return;
            }

            if (ctx.isVaulting || ctx.isClimbing) {
                ctx.hasRotationTarget = false;
                return;
            }

            if (ctx.hasRotationTarget) {
                float t = ctx.rotationTurnSpeed * Time.deltaTime;
                if (t >= 1f) {
                    transform.rotation = ctx.rotationTarget;
                } else {
                    transform.rotation = Quaternion.Slerp(transform.rotation, ctx.rotationTarget, t);
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

        bool IsInAirborneState() {
            if (machine == null || machine.Root == null) {
                return false;
            }

            return IsAirborneState(machine.Root.Leaf());
        }

        static bool IsAirborneState(State state) {
            for (State current = state; current != null; current = current.Parent) {
                if (current is Airborne) {
                    return true;
                }
            }

            return false;
        }

        Vector3 RemoveIntoWallComponentWhileAirborne(Vector3 desiredPlanar) {
            if (desiredPlanar.sqrMagnitude <= 0.0001f) {
                return desiredPlanar;
            }

            int wallMask = ResolveAirWallMask();
            if (wallMask == 0 || cc == null) {
                return desiredPlanar;
            }

            Vector3 wallNormalPlanar;
            if (TryGetWallNormalPlanar(desiredPlanar, wallMask, out wallNormalPlanar)) {
                cachedAirWallNormalPlanar = wallNormalPlanar;
                airWallNormalHoldTimer = airWallNormalHoldTime;
            } else if (airWallNormalHoldTimer > 0f && cachedAirWallNormalPlanar.sqrMagnitude > 0.0001f) {
                airWallNormalHoldTimer = Mathf.Max(0f, airWallNormalHoldTimer - Time.deltaTime);
                wallNormalPlanar = cachedAirWallNormalPlanar;
            } else {
                airWallNormalHoldTimer = 0f;
                return desiredPlanar;
            }
            float intoWallSpeed = Vector3.Dot(desiredPlanar, -wallNormalPlanar);
            if (intoWallSpeed <= 0f) {
                return desiredPlanar;
            }

            // Strict rule: wall handling only changes planar motion (X/Z), never vertical speed (Y).
            var adjustedPlanar = desiredPlanar + wallNormalPlanar * intoWallSpeed;
            if (ctx.verticalVelocity <= 0.05f) {
                adjustedPlanar += wallNormalPlanar * airWallEdgeDetachSpeed;
            }
            ctx.hasRotationTarget = false;
            return adjustedPlanar;
        }

        bool TryGetWallNormalPlanar(Vector3 desiredPlanar, int wallMask, out Vector3 wallNormalPlanar) {
            wallNormalPlanar = Vector3.zero;
            Vector3 desiredDirection = desiredPlanar.normalized;

            if (TryGetWallHit(desiredDirection, wallMask, out var hitFromPlanar)) {
                return TryBuildPlanarNormal(hitFromPlanar.normal, out wallNormalPlanar);
            }

            Vector3 forward = transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude > 0.0001f) {
                forward.Normalize();
                if (TryGetWallHit(forward, wallMask, out var hitFromForward)) {
                    return TryBuildPlanarNormal(hitFromForward.normal, out wallNormalPlanar);
                }
            }

            return false;
        }

        static bool TryBuildPlanarNormal(Vector3 normal, out Vector3 wallNormalPlanar) {
            wallNormalPlanar = new Vector3(normal.x, 0f, normal.z);
            if (wallNormalPlanar.sqrMagnitude <= 0.0001f) {
                return false;
            }

            wallNormalPlanar.Normalize();
            return true;
        }

        int ResolveAirWallMask() {
            if (airWallMask.value != 0) {
                return airWallMask.value;
            }

            int wallLayer = LayerMask.NameToLayer("Wall");
            return wallLayer >= 0 ? 1 << wallLayer : 0;
        }

        bool TryGetWallHit(Vector3 direction, int wallMask, out RaycastHit hit) {
            hit = default;
            if (cc == null) {
                return false;
            }

            var bounds = cc.bounds;
            float radius = Mathf.Clamp(Mathf.Min(bounds.extents.x, bounds.extents.z) * 0.8f, 0.1f, 0.35f);
            Vector3 origin = bounds.center;
            origin.y = Mathf.Lerp(bounds.min.y + radius, bounds.center.y, 0.65f);
            float distance = Mathf.Max(0.05f, airWallBlockDistance);

            var hits = Physics.SphereCastAll(
                origin,
                radius,
                direction,
                distance,
                wallMask,
                QueryTriggerInteraction.Ignore
            );

            if (hits == null || hits.Length == 0) {
                return false;
            }

            float nearestDistance = float.MaxValue;
            bool found = false;
            for (int i = 0; i < hits.Length; i++) {
                var candidate = hits[i];
                if (candidate.collider == null) {
                    continue;
                }

                // Only treat near-vertical surfaces as walls.
                if (candidate.normal.y > 0.35f) {
                    continue;
                }

                if (candidate.distance < nearestDistance) {
                    nearestDistance = candidate.distance;
                    hit = candidate;
                    found = true;
                }
            }

            return found;
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
                // Important: when Wall is included in groundMask, side walls can overlap the sphere.
                // Only accept fallback as grounded if we can recover a walkable slope normal.
                if (Physics.CheckSphere(groundCheck.position, groundRadius, groundMask, QueryTriggerInteraction.Ignore)) {
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
                            return true;
                        }
                    }

                    return false;
                }

                return false;
            }
        }
    }
}