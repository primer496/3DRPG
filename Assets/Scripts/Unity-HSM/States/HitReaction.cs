using UnityEngine;

namespace HSM {
    public class HitReaction : State {
        readonly PlayerContext ctx;
        readonly PlayerRoot rootState;
        float stateElapsed;

        public HitReaction(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            rootState = parent as PlayerRoot;
            Add(new ColorPhaseActivity(ctx.renderer) {
                enterColor = Color.red
            });
        }

        protected override void OnEnter() {
            ExecuteHitImpact();
        }

        // 把受击表现抽离为一个独立方法，方便在状态内重复调用
        void ExecuteHitImpact() {
            stateElapsed = 0f;
            ctx.isHit = false; // 消耗掉这发攻击标记

            // 强行关闭受击者的 RootMotion 锁定
            ctx.combatRootMotionActive = false;
            if (ctx.anim != null) {
                ctx.anim.applyRootMotion = false;
                // 强制从0秒开始播放受击动作
                ctx.anim.CrossFade("HitReaction", 0.05f, 0, 0f); 
            }

            // 【改善手感：引入真实击退】
            Vector3 knockbackDir = Vector3.zero;
            if (ctx.currentHitSource.sqrMagnitude > 0.001f) {
                knockbackDir = (ctx.anim != null ? ctx.anim.transform.position : Vector3.zero) - ctx.currentHitSource;
                knockbackDir.y = 0;
            }

            if (knockbackDir.sqrMagnitude > 0.001f) {
                ctx.velocity = knockbackDir.normalized * ctx.hitKnockbackSpeed; 
            } else {
                if (ctx.anim != null) ctx.velocity = -ctx.anim.transform.forward * ctx.hitKnockbackSpeed; 
            }
        }

        protected override void OnUpdate(float deltaTime) {
            stateElapsed += deltaTime;

            // --- 区分玩家和敌人的连续受击逻辑（无敌帧 vs 无限硬直） ---
            if (ctx.isHit) {
                ctx.isHit = false; // 无论如何都消化掉这次受击事件

                bool isPlayer = ctx.anim != null && ctx.anim.transform.CompareTag("Player");
                if (!isPlayer) {
                    // 敌人：只要还在受击状态里又被打中，就马上刷新动作和击退（也就是常说的重新打出硬直/浮空连段）
                    ExecuteHitImpact();
                } else {
                    // 玩家：在受击动作播放完之前，受到攻击只扣血但不刷新受击动作（无敌帧/霸体保护机制）
                    // 这样可以防止玩家被小怪围殴时卡死在原地什么都干不了
                }
            }

            // 摩擦力衰减算法：利用插值把击退的动能瞬间滑停
            ctx.velocity = Vector3.Lerp(ctx.velocity, Vector3.zero, ctx.hitKnockbackDecay * deltaTime);
        }

        protected override State GetTransition() {
            if (ctx.anim == null) {
                return stateElapsed > 0.5f ? rootState.Grounded : null;
            }

            var info = ctx.anim.GetCurrentAnimatorStateInfo(0);

            // 当状态机确实进入了受击动画后：
            if (info.IsName("HitReaction")) {
                if (GetNormalized01(info) >= ctx.hitReactionExitNormalizedTime) {
                    return rootState.Grounded; 
                }
            } 
            // 兜底保护：过了0.15秒还没有进入 HitReaction 状态，防止永久死锁
            else if (stateElapsed > 0.15f && !ctx.anim.IsInTransition(0)) {
                return rootState.Grounded;
            }

            return null;
        }
        
        // 提取0-1归一化时间
        float GetNormalized01(AnimatorStateInfo info) {
            float n = info.normalizedTime;
            if (n >= 1.0f) {
                return n;
            }
            return n - Mathf.Floor(n); 
        }
    }
}
