using UnityEngine;

namespace HSM {
    public class Combat : State {
        readonly PlayerContext ctx;
        readonly Grounded groundedState;
        readonly PlayerRoot rootState;
        int currentComboStep;
        bool queuedNextAttack;

        public Combat(StateMachine m, State parent, PlayerRoot rootState, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            groundedState = parent as Grounded;
            this.rootState = rootState;
        }

        protected override State GetTransition() {
            if (!ctx.grounded) {
                return rootState.Airborne;
            }

            if (ctx.anim == null) {
                return groundedState.Move;
            }

            // 动画播放到收尾且没有成功连段输入时，退出战斗状态。
            var info = ctx.anim.GetCurrentAnimatorStateInfo(0);
            if (IsCurrentComboState(info) && GetNormalized01(info) >= ctx.comboExitNormalizedTime) {
                return groundedState.Move;
            }

            return null;
        }

        protected override void OnEnter() {
            currentComboStep = 1;
            queuedNextAttack = false;
            ctx.attackPressed = false;
            EnableCombatRootMotionIfNeeded();
            StopHorizontalMotion();
            if (ctx.swordObject != null) {
                ctx.swordObject.SetActive(true);
            }
            AimAssist(); // 先触发开局辅瞄转身
            PlayCurrentComboAnimation(); // 再播动画
        }

        protected override void OnExit() {
            DisableCombatRootMotion();
            if (ctx.swordObject != null) {
                ctx.swordObject.SetActive(false);
            }
        }

        protected override void OnUpdate(float deltaTime) {
            if (ctx.anim == null) return;

            if (ctx.attackPressed) {
                ctx.attackPressed = false;
                queuedNextAttack = true;
            }

            if (currentComboStep >= ctx.maxComboSteps) return;

            var info = ctx.anim.GetCurrentAnimatorStateInfo(0);
            if (!IsCurrentComboState(info)) return;

            float normalized = GetNormalized01(info);
            GetWindow(currentComboStep, out float windowStart, out float windowEnd);
            bool inWindow = normalized >= windowStart && normalized <= windowEnd;
            bool windowExpired = normalized > windowEnd;

            // 每段攻击独立窗口：只允许在本段窗口内衔接下一段。
            if (queuedNextAttack && inWindow) {
                queuedNextAttack = false;
                currentComboStep++;
                AimAssist(); // 先触发辅瞄瞬间转身
                PlayCurrentComboAnimation(); // 再播动画
                return;
            }

            if (windowExpired) {
                queuedNextAttack = false;
            }
        }

        void PlayCurrentComboAnimation() {
            if (ctx.anim == null) return;

            switch (currentComboStep) {
                case 1:
                    ctx.anim.CrossFade(AnimatorKeys.ComboState(1), 0.05f);
                    break;
                case 2:
                    ctx.anim.CrossFade(AnimatorKeys.ComboState(2), 0.05f);
                    break;
                case 3:
                    ctx.anim.CrossFade(AnimatorKeys.ComboState(3), 0.05f);
                    break;
                case 4:
                    ctx.anim.CrossFade(AnimatorKeys.ComboState(4), 0.05f);
                    break;
            }
        }

        bool IsCurrentComboState(AnimatorStateInfo info) {
            return info.IsName(AnimatorKeys.ComboState(currentComboStep));
        }

        static float GetNormalized01(AnimatorStateInfo info) {
            float t = info.normalizedTime;
            return t - Mathf.Floor(t);
        }

        void GetWindow(int comboStep, out float start, out float end) {
            switch (comboStep) {
                case 1:
                    start = ctx.combo1WindowStart;
                    end = ctx.combo1WindowEnd;
                    break;
                case 2:
                    start = ctx.combo2WindowStart;
                    end = ctx.combo2WindowEnd;
                    break;
                case 3:
                    start = ctx.combo3WindowStart;
                    end = ctx.combo3WindowEnd;
                    break;
                case 4:
                    start = ctx.combo4WindowStart;
                    end = ctx.combo4WindowEnd;
                    break;
                default:
                    start = 1f;
                    end = 1f;
                    break;
            }

            if (end < start) {
                end = start;
            }
        }

        void StopHorizontalMotion() {
            ctx.velocity.x = 0f;
            ctx.velocity.z = 0f;
            // 移除了 ctx.hasRotationTarget = false; 防止干扰动画里的旋转过渡
        }

        void EnableCombatRootMotionIfNeeded() {
            ctx.combatRootMotionActive = ctx.useCombatRootMotion;
            if (ctx.anim != null) {
                ctx.anim.applyRootMotion = ctx.combatRootMotionActive;
            }
        }

        void AimAssist() {
            if (ctx.anim == null) return;
            Transform origin = ctx.anim.transform;
            if (origin.parent != null) {
                origin = origin.parent;
            }
            
            // 【核心修复】防止敌人在共用的 Combat 状态下乱吸附！只允许打上 Player 标签的主角使用辅助瞄准
            if (!origin.CompareTag("Player")) return;

            float detectRadius = ctx.aimAssistRadius; // 辅助瞄准范围
            Collider[] hits = Physics.OverlapSphere(origin.position, detectRadius);

            Transform bestTarget = null;
            float minDistance = float.MaxValue;

            foreach (var hit in hits) {
                if (!hit.CompareTag("Enemy")) continue; 

                Vector3 dir = hit.transform.position - origin.position;
                dir.y = 0;
                if (dir.sqrMagnitude < 0.001f) continue;

                float angle = Vector3.Angle(origin.forward, dir.normalized);
                float dist = dir.magnitude;

                // 限定在前方一定角度内优先吸附（比如面前 90度 内最近的敌人）
                if (angle < ctx.aimAssistAngle) {
                    if (dist < minDistance) {
                        minDistance = dist;
                        bestTarget = hit.transform;
                    }
                }
            }
            
            if (bestTarget != null) {
                Vector3 dirToBest = bestTarget.position - origin.position;
                dirToBest.y = 0;
                if (dirToBest.sqrMagnitude > 0.001f) {
                    Quaternion lookRot = Quaternion.LookRotation(dirToBest.normalized, Vector3.up);
                    origin.rotation = lookRot; // 直接刚性转向平滑
                    ctx.rotationTarget = lookRot;
                    ctx.hasRotationTarget = true;
                }
            }
        }

        void DisableCombatRootMotion() {
            ctx.combatRootMotionActive = false;
            if (ctx.anim != null) {
                ctx.anim.applyRootMotion = false;
            }
        }
    }
}

