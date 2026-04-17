using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HSM {
    public interface ICapabilityConfigApplier {
        void ApplyTo(PlayerContext ctx);
    }

    public class PlayerStateDriver : MonoBehaviour, IMove {
        public PlayerContext ctx = new PlayerContext();
        public Transform groundCheck;
        public float groundRadius = 0.2f;
        public float groundProbeDistance = 0.3f;
        [Range(1f, 89f)]
        public float maxGroundAngle = 55f;
        [Range(0f, 30f)]
        public float minProjectSlopeAngle = 3f;
        public float groundedSnapDownVelocity = 2f;
        public float groundUngroundedGraceTime = 0.08f;
        public LayerMask groundMask;
        // 空中贴墙处理
        public LayerMask airWallMask;
        [Range(0.05f, 0.6f)]
        public float airWallBlockDistance = 0.22f;
        [Range(0f, 0.2f)]
        public float airWallNormalHoldTime = 0.08f;
        [Range(0f, 2f)]
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
        bool stopSpeedDecayActive;
        float stopSpeedParam;
        readonly GroundChecker groundProbe = new GroundChecker();

        public InputAction moveAction;
        public InputAction jumpAction;
        public InputAction runAction;
        public InputAction dodgeAction;
        public InputAction attackAction;

        // 意图来源：为空则默认读取玩家输入
        public MonoBehaviour intentProviderOverride;
        // 配置集入口：玩家配置优先于敌人配置
        public ScriptableObject playerConfigSet;
        public ScriptableObject enemyConfigSet;

        IIntentProvider intentProvider;
        PlayerInputProvider defaultInputProvider;

        void Awake() {
            InitializeCharacterController();
            InitializeContextReferences();
            ApplyRoleConfigOverrides();
            BuildStateMachine();
            EnsureGroundCheck();
            InitializeSwordState();
            InitializeIntentProvider();
        }

        void OnEnable() {
            ToggleInputActions(true);
        }

        void OnDisable() {
            ToggleInputActions(false);
        }

        void Update() {
            if (ctx.hitSlowdownTimer > 0) {
                ctx.hitSlowdownTimer -= Time.deltaTime;
            }

            ReadInputIntoContext();
            UpdateGroundedState();
            EnsureCombatRootMotionSafety();
            TickStateMachine();
            UpdateStopAnimatorSpeedDecay();
            ApplyCharacterMotion();
            ApplyQueuedRotation();
            LogStatePathIfChanged();
        }

        public Vector3 Velocity => ctx.velocity;
        public bool IsGrounded => ctx.grounded;

        // --- 核心修复：用于接收 Animator 动画事件，并将其转发给剑上的 WeaponDetector ---
        public void BeginAttack() {
            if (ctx.swordObject != null) {
                var wd = ctx.swordObject.GetComponent<WeaponDetector>();
                if (wd != null) {
                    wd.BeginAttack();
                } else {
                    Debug.LogWarning("PlayerStateDriver: [剑] 对象上未找到 WeaponDetector 组件！");
                }
            }
        }

        public void EndAttack() {
            if (ctx.swordObject != null) {
                var wd = ctx.swordObject.GetComponent<WeaponDetector>();
                if (wd != null) wd.EndAttack();
            }
        }
        // -------------------------------------------------------------------------

        Vector3 IMove.Velocity { get => Velocity; set => throw new NotImplementedException(); }

        public void Move(Vector3 moveDelta) {
            if (cc != null && cc.enabled) {
                cc.Move(moveDelta);
            } else {
                transform.position += moveDelta;
            }
        }

        public void SetRotation(Quaternion rotation) {
            transform.rotation = rotation;
            ctx.rotationTarget = rotation;
            ctx.hasRotationTarget = false;
        }

        void InitializeCharacterController() {
            var customMover = GetComponents<IMove>().FirstOrDefault(m => m != (IMove)this);
            if (customMover != null) {
                ctx.moveDriver = customMover;
            } else {
                ctx.moveDriver = this;
                cc = GetComponent<CharacterController>();
                if (cc == null) {
                    Debug.LogError("PlayerStateDriver requires CharacterController when acting as default Move driver.");
                    enabled = false;
                }
            }
        }

        void InitializeContextReferences() {
            ctx.cc = cc;
            ctx.verticalVelocity = 0f;
            ctx.combatRootMotionActive = false;
            ctx.pendingWallActionHeightOffsetY = 0f;
            ctx.wallActionHeightOffsetRemainingY = 0f;
            ctx.anim = GetComponentInChildren<Animator>();
            if (ctx.anim != null) {
                ctx.anim.applyRootMotion = false;
            }
            ctx.renderer = GetComponent<Renderer>();
        }

        void ApplyRoleConfigOverrides() {
            if (playerConfigSet != null && enemyConfigSet != null) {
                Debug.LogWarning(
                    $"Both playerConfigSet and enemyConfigSet are assigned on {name}. playerConfigSet takes precedence."
                );
            }

            if (TryApplyConfigSet(playerConfigSet, "playerConfigSet")) {
                return;
            }

            TryApplyConfigSet(enemyConfigSet, "enemyConfigSet");
        }

        bool TryApplyConfigSet(ScriptableObject configSet, string fieldName) {
            if (configSet == null) {
                return false;
            }

            if (configSet is ICapabilityConfigApplier applier) {
                applier.ApplyTo(ctx);
                return true;
            }

            Debug.LogError(
                $"{name}.{fieldName} ({configSet.GetType().Name}) must implement ICapabilityConfigApplier.",
                configSet
            );
            return false;
        }

        void BuildStateMachine() {
            root = new PlayerRoot(null, ctx);
            var builder = new StateMachineBuilder(root);
            machine = builder.Build();
        }

        void EnsureGroundCheck() {
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
            intentProvider?.WriteIntent(ctx);
        }

        void InitializeIntentProvider() {
            if (intentProviderOverride != null) {
                if (intentProviderOverride is IIntentProvider overrideProvider) {
                    intentProvider = overrideProvider;
                    return;
                }

                Debug.LogError(
                    $"Intent provider override on {name} must implement IIntentProvider.",
                    intentProviderOverride
                );
            }

            defaultInputProvider = new PlayerInputProvider {
                moveAction = moveAction,
                jumpAction = jumpAction,
                runAction = runAction,
                dodgeAction = dodgeAction,
                attackAction = attackAction
            };
            intentProvider = defaultInputProvider;
        }

        void UpdateGroundedState() {
            if (ctx.isVaulting || ctx.isClimbing) {
                ctx.grounded = true;
                ungroundedTimer = 0f;
                hasGroundHitThisFrame = true;
                return;
            }

            bool inAirborneState = IsInAirborneState();
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

            if (risingInAir || ctx.jumpGroundDetachTimer > 0f) {
                hitGround = false;
            }
            hasGroundHitThisFrame = hitGround;

            if (hitGround) {
                if (hasReliableNormal) {
                    ctx.groundNormal = normal;
                }
                ctx.grounded = true;
                ungroundedTimer = 0f;
                ctx.PushGroundVelocity(ctx.velocity.x, ctx.velocity.z);
                return;
            }

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

        void UpdateStopAnimatorSpeedDecay() {
            if (ctx.anim == null || machine == null || machine.Root == null) {
                stopSpeedDecayActive = false;
                return;
            }

            var leaf = machine.Root.Leaf();
            bool isStopLeaf = leaf is Stop;
            if (!isStopLeaf) {
                stopSpeedDecayActive = false;
                return;
            }

            if (!stopSpeedDecayActive) {
                stopSpeedParam = Mathf.Clamp01(ctx.anim.GetFloat(AnimatorKeys.Params.Speed));
                stopSpeedDecayActive = true;
            }

            float tau = Mathf.Max(ctx.stopSpeedDecayTime, 3f * Time.deltaTime);
            stopSpeedParam *= Mathf.Exp(-Time.deltaTime / tau);
            if (stopSpeedParam < 1e-4f) {
                stopSpeedParam = 0f;
            }

            ctx.anim.SetFloat(AnimatorKeys.Params.Speed, Mathf.Clamp01(stopSpeedParam));
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
                if (!reliableGround) {
                    desiredPlanar.y = 0f;
                    lastAppliedPlanarVelocity = desiredPlanar;
                }

                bool needsSnapDown = !hasGroundHitThisFrame;
                if (needsSnapDown && ctx.verticalVelocity <= 0.5f && ctx.verticalVelocity > -groundedSnapDownVelocity) {
                    ctx.verticalVelocity = -groundedSnapDownVelocity;
                }
            } else {
                desiredPlanar = RemoveIntoWallComponentWhileAirborne(desiredPlanar);
                ctx.verticalVelocity += Physics.gravity.y * Time.deltaTime;
            }

            var motion = desiredPlanar + Vector3.up * ctx.verticalVelocity;
            if (ctx.moveDriver == (IMove)this) {
                CollisionFlags flags = cc != null ? cc.Move(motion * Time.deltaTime) : CollisionFlags.None;
                bool touchedGroundByMove = (flags & CollisionFlags.Below) != 0;

                if ((flags & CollisionFlags.Above) != 0 && ctx.verticalVelocity > 0f) {
                    ctx.verticalVelocity = 0f;
                }

                if ((treatAsGrounded || touchedGroundByMove) && ctx.verticalVelocity <= 0f) {
                    ctx.verticalVelocity = -Mathf.Max(0.05f, groundedSnapDownVelocity * 0.35f);
                }

                if (touchedGroundByMove) {
                    hasGroundHitThisFrame = true;
                }
            } else if (ctx.moveDriver != null) {
                ctx.moveDriver.Move(motion * Time.deltaTime);
                // Non-CC controllers usually handle their own grounding flags
                bool touchedGroundByMove = ctx.moveDriver.IsGrounded;
                if ((treatAsGrounded || touchedGroundByMove) && ctx.verticalVelocity <= 0f) {
                    ctx.verticalVelocity = -Mathf.Max(0.05f, groundedSnapDownVelocity * 0.35f);
                }
                if (touchedGroundByMove) {
                    hasGroundHitThisFrame = true;
                }
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
            if (!IsRootMotionTranslationActive() || ctx.anim == null || ctx.moveDriver == null) {
                return;
            }

            Vector3 deltaPos = ctx.anim.deltaPosition;
            if (ctx.combatRootMotionActive) {
                deltaPos = BuildCombatRootMotionDelta(deltaPos);
            } else if (ctx.isClimbing) {
                deltaPos = BuildClimbRootMotionDelta(deltaPos);
            }
            if (ctx.isVaulting || ctx.isClimbing) {
                deltaPos = ApplyWallActionHeightOffset(deltaPos);
            }
            if (ctx.isVaulting) {
                deltaPos = ApplyVaultLateDownwardBias(deltaPos);
            }

            ctx.moveDriver.Move(deltaPos);
            if (ctx.combatRootMotionActive && Time.deltaTime > 0.0001f) {
                // 不将RootMotion速度写回ctx.velocity，避免下一次非RootMotion计算帧拿着这个速度继续飘行
                ctx.velocity.x = 0f;
                ctx.velocity.z = 0f;
            } else {
                ctx.velocity.x = 0f;
                ctx.velocity.z = 0f;
            }
            ctx.verticalVelocity = 0f;
        }

        bool IsRootMotionTranslationActive() {
            return ctx.isVaulting || ctx.isClimbing || (ctx.combatRootMotionActive && ctx.grounded);
        }

        void EnsureCombatRootMotionSafety() {
            if (!ctx.combatRootMotionActive) {
                return;
            }

            if (ctx.grounded) {
                return;
            }

            ctx.combatRootMotionActive = false;
            if (ctx.anim != null && !ctx.isVaulting && !ctx.isClimbing) {
                ctx.anim.applyRootMotion = false;
            }
            if (ctx.swordObject != null) {
                ctx.swordObject.SetActive(false);
            }
        }

        Vector3 BuildCombatRootMotionDelta(Vector3 animatorDeltaPosition) {
            float planarScale = Mathf.Max(0f, ctx.combatRootMotionPlanarScale);
            // 【核心修复】如果在攻击滞帧期间（砍中敌人），则按照配置缩减运动位移
            if (ctx.hitSlowdownTimer > 0) {
                planarScale *= ctx.hitStopRootMotionScale;
            }

            var proposedDelta = new Vector3(
                animatorDeltaPosition.x * planarScale,
                0f,
                animatorDeltaPosition.z * planarScale
            );
            return ResolveCombatRootMotionWithFutureHooks(proposedDelta);
        }

        Vector3 BuildClimbRootMotionDelta(Vector3 animatorDeltaPosition) {
            if (ctx.detectedClimbTier != ClimbHeightTier.Climb17 || Time.deltaTime <= 0.0001f) {
                return animatorDeltaPosition;
            }

            Vector3 planarDelta = new Vector3(animatorDeltaPosition.x, 0f, animatorDeltaPosition.z);
            float planarSpeed = planarDelta.magnitude / Time.deltaTime;
            float minPlanarSpeed = Mathf.Max(0f, ctx.climb17MinPlanarSpeed);
            if (planarSpeed >= minPlanarSpeed) {
                return animatorDeltaPosition;
            }

            Vector3 forward = transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude <= 0.0001f) {
                return animatorDeltaPosition;
            }

            forward.Normalize();
            float assistDistance = Mathf.Max(0f, ctx.climb17PlanarAssistSpeed) * Time.deltaTime;
            return animatorDeltaPosition + forward * assistDistance;
        }

        Vector3 ApplyWallActionHeightOffset(Vector3 animatorDeltaPosition) {
            float remaining = ctx.wallActionHeightOffsetRemainingY;
            if (Mathf.Abs(remaining) <= 0.0001f || Time.deltaTime <= 0.0001f) {
                return animatorDeltaPosition;
            }

            float maxSpeed = Mathf.Max(0.1f, ctx.wallActionHeightAdjustSpeed);
            float maxStep = maxSpeed * Time.deltaTime;
            float step = Mathf.Clamp(remaining, -maxStep, maxStep);
            ctx.wallActionHeightOffsetRemainingY = remaining - step;
            animatorDeltaPosition.y += step;
            return animatorDeltaPosition;
        }

        Vector3 ApplyVaultLateDownwardBias(Vector3 animatorDeltaPosition) {
            if (ctx.anim == null || Time.deltaTime <= 0.0001f) {
                return animatorDeltaPosition;
            }

            var info = ctx.anim.GetCurrentAnimatorStateInfo(0);
            if (!info.IsName(AnimatorKeys.States.Vault)) {
                return animatorDeltaPosition;
            }

            float normalized = info.normalizedTime - Mathf.Floor(info.normalizedTime);
            float start = Mathf.Clamp(ctx.vaultLateDownStartNormalizedTime, 0.5f, 0.98f);
            if (normalized < start) {
                return animatorDeltaPosition;
            }

            float downSpeed = Mathf.Max(0f, ctx.vaultLateDownSpeed);
            animatorDeltaPosition.y -= downSpeed * Time.deltaTime;
            return animatorDeltaPosition;
        }

        Vector3 ResolveCombatRootMotionWithFutureHooks(Vector3 proposedDelta) {
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