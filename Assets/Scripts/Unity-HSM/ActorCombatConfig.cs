using UnityEngine;

namespace HSM {
    [CreateAssetMenu(
        fileName = "ActorCombatConfig",
        menuName = "HSM/Configs/Actor Combat Config"
    )]
    public class ActorCombatConfig : ScriptableObject {
        [Header("Combat")]
        public float attackRange = 1.5f;
        public float comboResetTime = 0.6f;
        public int maxComboSteps = 4;
        public bool useCombatRootMotion = true;
        public float combatRootMotionPlanarScale = 1f;
        public float comboExitNormalizedTime = 0.95f;
        public float hitReactionExitNormalizedTime = 0.6f;

        [Header("Impact & Feel")]
        public float hitKnockbackSpeed = 4f;
        public float hitKnockbackDecay = 15f;
        [Tooltip("攻击卡肉迟滞时长")]
        public float hitStopDuration = 0.15f;
        [Tooltip("卡肉期间由于根运动打断导致的继续前冲比例（默认0.1代表基本停住）")]
        public float hitStopRootMotionScale = 0.1f;

        [Header("Aim Assist")]
        public float aimAssistRadius = 6.0f;
        [Range(0f, 360f)]
        public float aimAssistAngle = 180f;

        [Header("Combo Windows (Normalized Time)")]
        public float combo1WindowStart = 0.38f;
        public float combo1WindowEnd = 0.7f;
        public float combo2WindowStart = 0.22f;
        public float combo2WindowEnd = 0.68f;
        public float combo3WindowStart = 0.2f;
        public float combo3WindowEnd = 0.66f;
        public float combo4WindowStart = 0.18f;
        public float combo4WindowEnd = 0.6f;

        public void ApplyTo(PlayerContext ctx) {
            if (ctx == null) {
                return;
            }

            ctx.attackRange = attackRange;
            ctx.comboResetTime = comboResetTime;
            ctx.maxComboSteps = maxComboSteps;
            ctx.useCombatRootMotion = useCombatRootMotion;
            ctx.combatRootMotionPlanarScale = combatRootMotionPlanarScale;
            ctx.comboExitNormalizedTime = comboExitNormalizedTime;
            ctx.hitReactionExitNormalizedTime = hitReactionExitNormalizedTime;

            ctx.hitKnockbackSpeed = hitKnockbackSpeed;
            ctx.hitKnockbackDecay = hitKnockbackDecay;
            ctx.hitStopDuration = hitStopDuration;
            ctx.hitStopRootMotionScale = hitStopRootMotionScale;
            ctx.aimAssistRadius = aimAssistRadius;
            ctx.aimAssistAngle = aimAssistAngle;

            ctx.combo1WindowStart = combo1WindowStart;
            ctx.combo1WindowEnd = combo1WindowEnd;
            ctx.combo2WindowStart = combo2WindowStart;
            ctx.combo2WindowEnd = combo2WindowEnd;
            ctx.combo3WindowStart = combo3WindowStart;
            ctx.combo3WindowEnd = combo3WindowEnd;
            ctx.combo4WindowStart = combo4WindowStart;
            ctx.combo4WindowEnd = combo4WindowEnd;
        }
    }
}
