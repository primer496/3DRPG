using UnityEngine;

namespace HSM {
    [CreateAssetMenu(
        fileName = "ActorTraversalConfig",
        menuName = "HSM/Configs/Actor Traversal Config"
    )]
    public class ActorTraversalConfig : ScriptableObject {
        [Header("Vault")]
        public float vaultDuration = 0.42f;
        public float vaultEnterCrossFade = 0.08f;
        public float vaultExitCrossFade = 0.1f;
        public float vaultExitNormalizedTime = 0.9f;
        public float vaultLateDownStartNormalizedTime = 0.7f;
        public float vaultLateDownSpeed = 3.2f;
        public float vaultMinMoveSpeed = 0.2f;
        public LayerMask vaultWallMask;
        public float vaultDetectDistance = 0.75f;
        public float vaultMaxFacingAngle = 45f;
        public float vaultMinHeight = 0.75f;
        public float vaultMaxHeight = 1.2f;
        public float vaultSampleMinHeight = 0.2f;
        public float vaultSampleMaxHeight = 1.6f;
        public int vaultHeightSamples = 6;
        public bool vaultDebugLog;

        [Header("Climb")]
        public LayerMask climbWallMask;
        public float climbDetectDistance = 0.75f;
        public float climbMaxFacingAngle = 45f;
        public float climbSampleMinHeight = 0.1f;
        public float climbSampleMaxHeight = 2.4f;
        public int climbHeightSamples = 10;
        public float climbEnterCrossFade = 0.1f;
        public float climbExitCrossFade = 0.12f;
        public float climbExitNormalizedTime = 0.92f;
        public float climb17ExitNormalizedTime = 0.975f;
        public float climb17PlanarAssistSpeed = 0.45f;
        public float climb17MinPlanarSpeed = 0.08f;
        public bool climbDebugLog;

        [Header("Wall Action Facing")]
        public float wallActionAlignDuration = 0.08f;
        public float wallActionAlignMinAngle = 8f;

        [Header("Wall Action Height Fit")]
        public float vaultReferenceWallHeight = 1f;
        public float climb05ReferenceWallHeight = 0.5f;
        public float climb10ReferenceWallHeight = 1f;
        public float climb17ReferenceWallHeight = 1.5f;
        public float climb20ReferenceWallHeight = 2f;
        public float wallActionHeightAdjustSpeed = 2.4f;
        public float wallActionMaxUpOffset = 0.6f;
        public float wallActionMaxDownOffset = 0.45f;

        public virtual void ApplyTo(PlayerContext ctx) {
            if (ctx == null) {
                return;
            }

            ctx.vaultDuration = vaultDuration;
            ctx.vaultEnterCrossFade = vaultEnterCrossFade;
            ctx.vaultExitCrossFade = vaultExitCrossFade;
            ctx.vaultExitNormalizedTime = vaultExitNormalizedTime;
            ctx.vaultLateDownStartNormalizedTime = vaultLateDownStartNormalizedTime;
            ctx.vaultLateDownSpeed = vaultLateDownSpeed;
            ctx.vaultMinMoveSpeed = vaultMinMoveSpeed;
            ctx.vaultWallMask = vaultWallMask;
            ctx.vaultDetectDistance = vaultDetectDistance;
            ctx.vaultMaxFacingAngle = vaultMaxFacingAngle;
            ctx.vaultMinHeight = vaultMinHeight;
            ctx.vaultMaxHeight = vaultMaxHeight;
            ctx.vaultSampleMinHeight = vaultSampleMinHeight;
            ctx.vaultSampleMaxHeight = vaultSampleMaxHeight;
            ctx.vaultHeightSamples = vaultHeightSamples;
            ctx.vaultDebugLog = vaultDebugLog;

            ctx.climbWallMask = climbWallMask;
            ctx.climbDetectDistance = climbDetectDistance;
            ctx.climbMaxFacingAngle = climbMaxFacingAngle;
            ctx.climbSampleMinHeight = climbSampleMinHeight;
            ctx.climbSampleMaxHeight = climbSampleMaxHeight;
            ctx.climbHeightSamples = climbHeightSamples;
            ctx.climbEnterCrossFade = climbEnterCrossFade;
            ctx.climbExitCrossFade = climbExitCrossFade;
            ctx.climbExitNormalizedTime = climbExitNormalizedTime;
            ctx.climb17ExitNormalizedTime = climb17ExitNormalizedTime;
            ctx.climb17PlanarAssistSpeed = climb17PlanarAssistSpeed;
            ctx.climb17MinPlanarSpeed = climb17MinPlanarSpeed;
            ctx.climbDebugLog = climbDebugLog;

            ctx.wallActionAlignDuration = wallActionAlignDuration;
            ctx.wallActionAlignMinAngle = wallActionAlignMinAngle;

            ctx.vaultReferenceWallHeight = vaultReferenceWallHeight;
            ctx.climb05ReferenceWallHeight = climb05ReferenceWallHeight;
            ctx.climb10ReferenceWallHeight = climb10ReferenceWallHeight;
            ctx.climb17ReferenceWallHeight = climb17ReferenceWallHeight;
            ctx.climb20ReferenceWallHeight = climb20ReferenceWallHeight;
            ctx.wallActionHeightAdjustSpeed = wallActionHeightAdjustSpeed;
            ctx.wallActionMaxUpOffset = wallActionMaxUpOffset;
            ctx.wallActionMaxDownOffset = wallActionMaxDownOffset;
        }
    }
}
