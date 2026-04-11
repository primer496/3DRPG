using UnityEngine;

namespace HSM {
    [CreateAssetMenu(
        fileName = "ActorCombatConfig",
        menuName = "HSM/Configs/Actor Combat Config"
    )]
    public class ActorCombatConfig : ScriptableObject {
        [Header("Combat")]
        public float comboResetTime = 0.6f;
        public int maxComboSteps = 4;
        public bool useCombatRootMotion = true;
        public float combatRootMotionPlanarScale = 1f;
        public float comboExitNormalizedTime = 0.95f;

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

            ctx.comboResetTime = comboResetTime;
            ctx.maxComboSteps = Mathf.Max(1, maxComboSteps);
            ctx.useCombatRootMotion = useCombatRootMotion;
            ctx.combatRootMotionPlanarScale = combatRootMotionPlanarScale;
            ctx.comboExitNormalizedTime = comboExitNormalizedTime;
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
