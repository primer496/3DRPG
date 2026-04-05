using UnityEngine;

namespace HSM {
    public class Grounded : State {
        readonly PlayerContext ctx;
        readonly PlayerRoot rootState;
        public readonly Idle Idle;
        public readonly Move Move;
        public readonly Stop Stop;
        public readonly Combat Combat;
        public readonly Dodge Dodge;
        public readonly Vault Vault;
        public readonly Landing Landing;
        public readonly Climb Climb;

        public Grounded(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            rootState = parent as PlayerRoot;
            Idle = new Idle(m, this, rootState, ctx);
            Move = new Move(m, this, rootState, ctx);
            Stop = new Stop(m, this, rootState, ctx);
            Combat = new Combat(m, this, rootState, ctx);
            Dodge = new Dodge(m, this, rootState, ctx);
            Vault = new Vault(m, this, ctx);
            Landing = new Landing(m, this, ctx);
            Climb = new Climb(m, this, ctx);
            Add(new ColorPhaseActivity(ctx.renderer){
                enterColor = Color.yellow,  // runs while Grounded is activating
            });
        }
        
        // 从空中落地时先进入 Landing，否则进入 Move
        protected override State GetInitialState() {
            if (ctx.justLanded) {
                ctx.justLanded = false;
                return Landing;
            }
            return Move;
        }

        protected override State GetTransition() {
            // 攀爬/翻越执行中不中断。
            if (ctx.isVaulting || ctx.isClimbing) {
                return null;
            }

            // 跳跃键按下 -> 先做墙体检测，决定攀爬/翻越/普通跳跃。
            if (ctx.jumpPressed) {
                ClimbHeightTier tier = DetectWallHeightTier();

                if (tier != ClimbHeightTier.None) {
                    bool isMoving = ctx.moveInput.sqrMagnitude > 0.01f;

                    // 1m 档位 + 移动中 -> 翻越。
                    if (tier == ClimbHeightTier.Climb10 && isMoving) {
                        if (TryBuildVaultTarget()) {
                            ctx.jumpPressed = false;
                            return Vault;
                        }
                        // Vault 检测失败则 fallthrough 到普通跳跃。
                    } else {
                        // 其他档位（或 1m 站立）-> 攀爬。
                        ctx.detectedClimbTier = tier;
                        ctx.jumpPressed = false;
                        return Climb;
                    }
                }

                // 无墙 / 不满足攀爬条件 -> 普通跳跃。
                if (!ctx.grounded) {
                    return rootState.Airborne;
                }

                ctx.jumpPressed = false;
                ctx.jumpGroundDetachTimer = Mathf.Max(0f, ctx.jumpGroundDetachTime);
                ctx.verticalVelocity = ctx.GetJumpTakeoffSpeed();
                return rootState.Airborne;
            }

            if (!ctx.grounded) {
                return rootState.Airborne;
            }

            return null;
        }

        bool TryBuildVaultTarget() {
            if (ctx.cc == null || !ctx.grounded) {
                return false;
            }

            float horizontalSpeed = new Vector3(ctx.velocity.x, 0f, ctx.velocity.z).magnitude;
            if (horizontalSpeed < Mathf.Max(0f, ctx.vaultMinMoveSpeed)) {
                LogVaultFail($"speed {horizontalSpeed:F2} < min {ctx.vaultMinMoveSpeed:F2}");
                return false;
            }

            int wallMask = ResolveVaultWallMask();
            if (wallMask == 0) {
                LogVaultFail("wall mask is 0 (Wall layer not found?)");
                return false;
            }

            Vector3 forward = ctx.cc.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude <= 0.0001f) {
                LogVaultFail("forward is near zero");
                return false;
            }
            forward.Normalize();

            var bounds = ctx.cc.bounds;
            float detectDistance = Mathf.Max(0.05f, ctx.vaultDetectDistance);
            float originBackOffset = Mathf.Min(0.15f, ctx.cc.radius * 0.6f);
            float castDistance = detectDistance + originBackOffset;
            float feetY = bounds.min.y + 0.05f;
            float sampleRadius = Mathf.Clamp(ctx.cc.radius * 0.22f, 0.04f, 0.12f);
            Vector3 baseOrigin = new Vector3(bounds.center.x, bounds.center.y, bounds.center.z) - forward * originBackOffset;

            bool hasPrimaryHit = false;
            RaycastHit primaryHit = default;
            bool seenHitBand = false;
            float highestHitHeight = -1f;
            float firstMissAfterHitHeight = -1f;
            int hitSamples = 0;

            int samples = Mathf.Max(3, ctx.vaultHeightSamples);
            float sampleMin = Mathf.Max(0f, ctx.vaultSampleMinHeight);
            float sampleMax = Mathf.Max(sampleMin + 0.05f, ctx.vaultSampleMaxHeight);
            for (int i = 0; i < samples; i++) {
                float u = samples == 1 ? 0f : (float)i / (samples - 1);
                float h = Mathf.Lerp(sampleMin, sampleMax, u);
                Vector3 origin = new Vector3(baseOrigin.x, feetY + h, baseOrigin.z);
                bool hasHit = Physics.SphereCast(
                    origin,
                    sampleRadius,
                    forward,
                    out var hit,
                    castDistance,
                    wallMask,
                    QueryTriggerInteraction.Ignore
                ) && hit.normal.y <= 0.35f;

                if (hasHit) {
                    if (!hasPrimaryHit) {
                        hasPrimaryHit = true;
                        primaryHit = hit;
                    }

                    hitSamples++;
                    seenHitBand = true;
                    highestHitHeight = h;
                    continue;
                }

                if (seenHitBand) {
                    firstMissAfterHitHeight = h;
                    break;
                }
            }

            if (!hasPrimaryHit || highestHitHeight < 0f || hitSamples < 2) {
                LogVaultFail($"wall sampling failed (hasHit={hasPrimaryHit}, hitSamples={hitSamples})");
                return false;
            }

            Vector3 intoWall = new Vector3(-primaryHit.normal.x, 0f, -primaryHit.normal.z);
            if (intoWall.sqrMagnitude <= 0.0001f) {
                LogVaultFail("wall normal planar magnitude too small");
                return false;
            }
            intoWall.Normalize();
            float facingAngle = Vector3.Angle(forward, intoWall);
            if (facingAngle > Mathf.Clamp(ctx.vaultMaxFacingAngle, 1f, 89f)) {
                LogVaultFail($"facing angle {facingAngle:F1} > max {ctx.vaultMaxFacingAngle:F1}");
                return false;
            }

            if (primaryHit.distance > castDistance) {
                LogVaultFail($"hit distance {primaryHit.distance:F2} > max {castDistance:F2}");
                return false;
            }

            float estimatedWallHeight = firstMissAfterHitHeight > 0f
                ? 0.5f * (highestHitHeight + firstMissAfterHitHeight)
                : highestHitHeight;
            float minHeight = Mathf.Min(ctx.vaultMinHeight, ctx.vaultMaxHeight);
            float maxHeight = Mathf.Max(ctx.vaultMinHeight, ctx.vaultMaxHeight);
            if (estimatedWallHeight < minHeight || estimatedWallHeight > maxHeight) {
                LogVaultFail($"height {estimatedWallHeight:F2} not in [{minHeight:F2}, {maxHeight:F2}]");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 从高到低打射线扫描前方墙体，返回匹配的高度档位。
        /// 检测逻辑与 Vault 类似：角度 ±climbMaxFacingAngle，SphereCast 从高到低。
        /// </summary>
        ClimbHeightTier DetectWallHeightTier() {
            if (ctx.cc == null || !ctx.grounded) {
                return ClimbHeightTier.None;
            }

            int wallMask = ResolveClimbWallMask();
            if (wallMask == 0) {
                LogClimbFail("wall mask is 0 (Wall layer not found?)");
                return ClimbHeightTier.None;
            }

            Vector3 forward = ctx.cc.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude <= 0.0001f) {
                LogClimbFail("forward is near zero");
                return ClimbHeightTier.None;
            }
            forward.Normalize();

            var bounds = ctx.cc.bounds;
            float detectDistance = Mathf.Max(0.05f, ctx.climbDetectDistance);
            float originBackOffset = Mathf.Min(0.15f, ctx.cc.radius * 0.6f);
            float castDistance = detectDistance + originBackOffset;
            float feetY = bounds.min.y + 0.05f;
            float sampleRadius = Mathf.Clamp(ctx.cc.radius * 0.22f, 0.04f, 0.12f);
            Vector3 baseOrigin = bounds.center - forward * originBackOffset;

            bool hasPrimaryHit = false;
            RaycastHit primaryHit = default;
            float highestHitHeight = -1f; // first hit height when scanning from high to low
            float nearestMissAboveHitHeight = -1f; // nearest miss sample just above highest hit
            float lastMissHeightBeforeAnyHit = -1f;
            int hitSamples = 0;

            int samples = Mathf.Max(4, ctx.climbHeightSamples);
            float sampleMin = Mathf.Max(0f, ctx.climbSampleMinHeight);
            float sampleMax = Mathf.Max(sampleMin + 0.05f, ctx.climbSampleMaxHeight);

            // 从高到低扫描。
            for (int i = samples - 1; i >= 0; i--) {
                float u = samples == 1 ? 0f : (float)i / (samples - 1);
                float h = Mathf.Lerp(sampleMin, sampleMax, u);
                Vector3 origin = new Vector3(baseOrigin.x, feetY + h, baseOrigin.z);
                bool hasHit = Physics.SphereCast(
                    origin, sampleRadius, forward, out var hit,
                    castDistance, wallMask, QueryTriggerInteraction.Ignore
                ) && hit.normal.y <= 0.35f;

                if (hasHit) {
                    if (!hasPrimaryHit) {
                        hasPrimaryHit = true;
                        primaryHit = hit;
                        // First hit from top. The nearest previous miss is just above wall top.
                        nearestMissAboveHitHeight = lastMissHeightBeforeAnyHit;
                    }
                    hitSamples++;
                    // 从高到低扫，所以第一次命中就是最高命中。
                    if (highestHitHeight < 0f) {
                        highestHitHeight = h;
                    }
                    continue;
                }

                if (!hasPrimaryHit) {
                    lastMissHeightBeforeAnyHit = h;
                }
            }

            if (!hasPrimaryHit || highestHitHeight < 0f || hitSamples < 2) {
                LogClimbFail($"wall sampling failed (hasHit={hasPrimaryHit}, hitSamples={hitSamples})");
                return ClimbHeightTier.None;
            }

            // 角度检查。
            Vector3 intoWall = new Vector3(-primaryHit.normal.x, 0f, -primaryHit.normal.z);
            if (intoWall.sqrMagnitude <= 0.0001f) {
                LogClimbFail("wall normal planar magnitude too small");
                return ClimbHeightTier.None;
            }
            intoWall.Normalize();
            float facingAngle = Vector3.Angle(forward, intoWall);
            if (facingAngle > Mathf.Clamp(ctx.climbMaxFacingAngle, 1f, 89f)) {
                LogClimbFail($"facing angle {facingAngle:F1} > max {ctx.climbMaxFacingAngle:F1}");
                return ClimbHeightTier.None;
            }

            // 估算墙顶高度。
            float estimatedWallHeight = nearestMissAboveHitHeight > 0f
                ? 0.5f * (highestHitHeight + nearestMissAboveHitHeight)
                : highestHitHeight;

            if (ctx.climbDebugLog) {
                Debug.Log($"[ClimbCheck] estimatedHeight={estimatedWallHeight:F2}, topHit={highestHitHeight:F2}, missAbove={nearestMissAboveHitHeight:F2}, hitSamples={hitSamples}");
            }

            return ClassifyHeight(estimatedWallHeight);
        }

        static ClimbHeightTier ClassifyHeight(float height) {
            if (height <= 0.75f) return ClimbHeightTier.Climb05;
            if (height <= 1.2f) return ClimbHeightTier.Climb10;
            // Recalibrated: Climb17 clip behaves closer to ~1.5m in-game.
            if (height <= 1.65f) return ClimbHeightTier.Climb17;
            if (height <= 2.2f) return ClimbHeightTier.Climb20;
            return ClimbHeightTier.None;
        }

        int ResolveClimbWallMask() {
            if (ctx.climbWallMask.value != 0) {
                return ctx.climbWallMask.value;
            }

            int wallLayer = LayerMask.NameToLayer("Wall");
            return wallLayer >= 0 ? 1 << wallLayer : 0;
        }

        void LogClimbFail(string reason) {
            if (!ctx.climbDebugLog) {
                return;
            }

            Debug.Log($"[ClimbCheck] {reason}");
        }

        int ResolveVaultWallMask() {
            if (ctx.vaultWallMask.value != 0) {
                return ctx.vaultWallMask.value;
            }

            int wallLayer = LayerMask.NameToLayer("Wall");
            return wallLayer >= 0 ? 1 << wallLayer : 0;
        }

        void LogVaultFail(string reason) {
            if (!ctx.vaultDebugLog) {
                return;
            }

            Debug.Log($"[VaultCheck] {reason}");
        }
    }
}