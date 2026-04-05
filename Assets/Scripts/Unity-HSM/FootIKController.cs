using UnityEngine;

namespace HSM {
    [DisallowMultipleComponent]
    public class FootIKController : MonoBehaviour {
        [Header("References")]
        [Tooltip("Optional manual reference. Auto-finds in parent when empty.")]
        public PlayerStateDriver driver;

        [Header("Ground Probe")]
        [Min(0.05f)]
        public float rayStartHeight = 0.3f;
        [Min(0.1f)]
        public float rayLength = 0.8f;
        [Range(0f, 0.2f)]
        public float sphereRadius = 0.06f;
        [Range(0f, 0.08f)]
        public float footOffset = 0.03f;
        [Tooltip("When true, uses sphere cast for more stable edge detection.")]
        public bool useSphereCast = true;

        [Header("Smoothing")]
        [Min(1f)]
        public float positionLerp = 14f;
        [Min(1f)]
        public float rotationLerp = 14f;
        [Min(1f)]
        public float weightLerp = 8f;
        [Min(1f)]
        public float pelvisLerp = 10f;
        [Range(0f, 0.15f)]
        public float maxPelvisOffset = 0.08f;

        [Header("Runtime Switch")]
        [Range(0f, 1f)]
        public float maxWeight = 1f;
        [Tooltip("Enable foot IK only when slope angle is at least this value.")]
        [Range(0f, 45f)]
        public float minSlopeAngleForIK = 4f;
        [Tooltip("When false, IK is disabled while run input is held.")]
        public bool allowIKWhileRunning = false;
        [Tooltip("Disable foot IK only when upward speed is above this threshold.")]
        [Min(0f)]
        public float maxVerticalSpeedForIK = 0.35f;

        Animator animator;
        float currentMasterWeight;
        float currentPelvisOffset;

        FootRuntime leftFoot;
        FootRuntime rightFoot;

        struct FootRuntime {
            public float currentWeight;
            public Vector3 currentPosition;
            public Quaternion currentRotation;
            public bool initialized;
            public bool hasGroundHit;
            public float animatedToIkOffsetY;
        }

        void Awake() {
            animator = GetComponent<Animator>();
            if (driver == null) {
                driver = GetComponentInParent<PlayerStateDriver>();
            }
        }

        void OnAnimatorIK(int layerIndex) {
            if (animator == null || driver == null || driver.ctx == null) {
                return;
            }

            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            bool ikAllowed = IsIKAllowed(driver.ctx);
            float targetMasterWeight = ikAllowed ? Mathf.Clamp01(maxWeight) : 0f;
            currentMasterWeight = Mathf.MoveTowards(currentMasterWeight, targetMasterWeight, weightLerp * dt);

            SolveFoot(AvatarIKGoal.LeftFoot, ref leftFoot, currentMasterWeight, dt);
            SolveFoot(AvatarIKGoal.RightFoot, ref rightFoot, currentMasterWeight, dt);
            ApplyPelvisOffset(currentMasterWeight, dt);
        }

        bool IsIKAllowed(PlayerContext ctx) {
            if (ctx.isVaulting || ctx.isClimbing) {
                return false;
            }

            if (!ctx.grounded) {
                return false;
            }

            if (!allowIKWhileRunning && ctx.runHeld) {
                return false;
            }

            float slopeAngle = Vector3.Angle(ctx.groundNormal, Vector3.up);
            float requiredSlope = Mathf.Max(minSlopeAngleForIK, driver != null ? driver.minProjectSlopeAngle : 0f);
            if (slopeAngle < requiredSlope) {
                return false;
            }

            // Grounded controller keeps a small negative hold velocity to stick to slopes.
            // Only block IK during strong upward movement (jump takeoff), not downward hold.
            if (ctx.verticalVelocity > Mathf.Max(0f, maxVerticalSpeedForIK)) {
                return false;
            }

            return true;
        }

        void SolveFoot(AvatarIKGoal goal, ref FootRuntime foot, float masterWeight, float dt) {
            Vector3 animatedFootPos = animator.GetIKPosition(goal);
            Quaternion animatedFootRot = animator.GetIKRotation(goal);

            bool hitGround = TryGetGroundHit(animatedFootPos, out var hitPoint, out var hitNormal);
            float targetWeight = hitGround ? masterWeight : 0f;
            foot.currentWeight = Mathf.MoveTowards(foot.currentWeight, targetWeight, weightLerp * dt);

            Vector3 targetPos = animatedFootPos;
            Quaternion targetRot = animatedFootRot;
            foot.hasGroundHit = hitGround;
            foot.animatedToIkOffsetY = 0f;

            if (hitGround) {
                targetPos = hitPoint + hitNormal * footOffset;
                foot.animatedToIkOffsetY = targetPos.y - animatedFootPos.y;

                Vector3 forwardOnPlane = Vector3.ProjectOnPlane(transform.forward, hitNormal);
                if (forwardOnPlane.sqrMagnitude <= 0.0001f) {
                    forwardOnPlane = Vector3.Cross(transform.right, hitNormal);
                }

                if (forwardOnPlane.sqrMagnitude > 0.0001f) {
                    targetRot = Quaternion.LookRotation(forwardOnPlane.normalized, hitNormal);
                }
            }

            if (!foot.initialized) {
                foot.currentPosition = targetPos;
                foot.currentRotation = targetRot;
                foot.initialized = true;
            } else {
                float posT = Mathf.Clamp01(positionLerp * dt);
                float rotT = Mathf.Clamp01(rotationLerp * dt);
                foot.currentPosition = Vector3.Lerp(foot.currentPosition, targetPos, posT);
                foot.currentRotation = Quaternion.Slerp(foot.currentRotation, targetRot, rotT);
            }

            animator.SetIKPositionWeight(goal, foot.currentWeight);
            animator.SetIKRotationWeight(goal, foot.currentWeight);
            animator.SetIKPosition(goal, foot.currentPosition);
            animator.SetIKRotation(goal, foot.currentRotation);
        }

        void ApplyPelvisOffset(float masterWeight, float dt) {
            float targetOffset = 0f;
            if (masterWeight > 0.0001f && (leftFoot.hasGroundHit || rightFoot.hasGroundHit)) {
                float leftOffset = leftFoot.hasGroundHit ? leftFoot.animatedToIkOffsetY : 0f;
                float rightOffset = rightFoot.hasGroundHit ? rightFoot.animatedToIkOffsetY : 0f;
                targetOffset = Mathf.Min(leftOffset, rightOffset);
                targetOffset = Mathf.Clamp(targetOffset, -Mathf.Abs(maxPelvisOffset), 0f);
            }

            currentPelvisOffset = Mathf.MoveTowards(currentPelvisOffset, targetOffset, pelvisLerp * dt);

            Vector3 bodyPos = animator.bodyPosition;
            bodyPos.y += currentPelvisOffset;
            animator.bodyPosition = bodyPos;
        }

        bool TryGetGroundHit(Vector3 footWorldPosition, out Vector3 hitPoint, out Vector3 hitNormal) {
            hitPoint = footWorldPosition;
            hitNormal = Vector3.up;

            Vector3 origin = footWorldPosition + Vector3.up * Mathf.Max(0.05f, rayStartHeight);
            float distance = Mathf.Max(0.1f, rayLength);
            int mask = ResolveGroundMask();

            bool hasHit = false;
            RaycastHit hit;
            if (useSphereCast && sphereRadius > 0.0001f) {
                hasHit = Physics.SphereCast(
                    origin,
                    sphereRadius,
                    Vector3.down,
                    out hit,
                    distance,
                    mask,
                    QueryTriggerInteraction.Ignore
                );
            } else {
                hasHit = Physics.Raycast(
                    origin,
                    Vector3.down,
                    out hit,
                    distance,
                    mask,
                    QueryTriggerInteraction.Ignore
                );
            }

            if (!hasHit) {
                return false;
            }

            float maxGroundAngle = driver != null ? driver.maxGroundAngle : 60f;
            float slope = Vector3.Angle(hit.normal, Vector3.up);
            if (slope > maxGroundAngle) {
                return false;
            }

            hitPoint = hit.point;
            hitNormal = hit.normal;
            return true;
        }

        int ResolveGroundMask() {
            if (driver == null) {
                return ~0;
            }

            if (driver.groundMask.value == 0) {
                return ~0;
            }

            return driver.groundMask.value;
        }
    }
}
