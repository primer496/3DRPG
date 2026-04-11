using UnityEngine;

namespace HSM {
    /// <summary>
    /// Minimal phase-1 enemy intent provider: idle, chase, and attack.
    /// </summary>
    public class EnemyBrain : AIIntentProvider {
        [Header("Target")]
        public Transform target;

        [Header("Ranges")]
        [Min(0f)]
        public float detectRange = 10f;
        [Min(0f)]
        public float attackRange = 1.5f;

        [Header("Pacing")]
        [Min(0f)]
        public float runDistanceThreshold = 5f;
        [Min(0.05f)]
        public float attackCooldown = 0.7f;

        float attackCooldownTimer;

        public override void WriteIntent(PlayerContext ctx) {
            attackCooldownTimer = Mathf.Max(0f, attackCooldownTimer - Time.deltaTime);
            ConfigureFacingProvider(ctx);
            ResetIntent(ctx);

            if (target == null) {
                return;
            }

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;

            if (distance > detectRange) {
                return;
            }

            if (distance <= attackRange) {
                if (attackCooldownTimer <= 0f) {
                    ctx.attackPressed = true;
                    attackCooldownTimer = attackCooldown;
                }
                return;
            }

            // Move forward in facing space; facing yaw is provided by ConfigureFacingProvider.
            ctx.moveInput = new Vector2(0f, 1f);
            ctx.runHeld = distance > runDistanceThreshold;
        }

        void ConfigureFacingProvider(PlayerContext ctx) {
            ctx.facingYawProvider = () => {
                if (target == null) {
                    return transform.eulerAngles.y;
                }

                Vector3 toTarget = target.position - transform.position;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude < 0.0001f) {
                    return transform.eulerAngles.y;
                }

                return Quaternion.LookRotation(toTarget.normalized, Vector3.up).eulerAngles.y;
            };
        }

        static void ResetIntent(PlayerContext ctx) {
            ctx.moveInput = Vector2.zero;
            ctx.jumpPressed = false;
            ctx.runHeld = false;
            ctx.dodgePressed = false;
            ctx.attackPressed = false;
        }
    }
}
