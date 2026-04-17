using System.Collections.Generic;
using UnityEngine;
using HSM;

public class EnemyAttackDetector : MonoBehaviour
{
    [Header("Sector Attack Parameters")]
    public float attackRadius = 3f;
    [Range(0, 360)]
    public float attackAngle = 120f;
    public LayerMask targetLayer;

    // 默认以当前Transform为中心，若需要特效/特定身体部位可挂载子节点
    public Transform attackOrigin;

    void Awake()
    {
        if (attackOrigin == null)
        {
            attackOrigin = transform;
        }
    }

    // 由敌人攻击动画（前摇结束，释放伤害的瞬间）通过 Animation Event 调用
    public void PerformSectorAttack()
    {
        // 取消了 targetLayer 限制，防止因 Inspector 面板没勾选 Layer 导致重叠球内永远为 0
        Collider[] hits = Physics.OverlapSphere(attackOrigin.position, attackRadius);

        foreach (var hit in hits)
        {
            // 通过 Tag 过滤出玩家，避免误伤其他物体
            if (!hit.CompareTag("Player"))
                continue;

            // 忽略自身（虽然通常敌人和玩家不是同一层或同一Tag，但加一层保险）
            if (hit.gameObject == this.gameObject || hit.gameObject == attackOrigin.gameObject)
                continue;

            Vector3 targetPos = hit.transform.position;

            // 获取方向向量并抹平高度差，计算平面扇形
            Vector3 dirToTarget = targetPos - attackOrigin.position;
            dirToTarget.y = 0;
            if (dirToTarget.sqrMagnitude < 0.0001f) continue; // 贴得太近
            dirToTarget.Normalize();

            Vector3 forward = attackOrigin.forward;
            forward.y = 0;
            if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
            forward.Normalize();

            // 如果处于前方扇形判定区内
            float angle = Vector3.Angle(forward, dirToTarget);
            if (angle <= attackAngle * 0.5f)
            {
                // 获取玩家身上的 PlayerStateDriver
                PlayerStateDriver driver = hit.GetComponentInParent<PlayerStateDriver>();
                if(driver != null) {
                    ApplyHit(driver);
                }
            }
            else {
            }
        }
    }

    private void ApplyHit(PlayerStateDriver driver)
    {
        if (driver != null)
        {
            // 告诉玩家被谁打的（原点坐标）用于后仰击退
            driver.ctx.currentHitSource = attackOrigin.position;
            // 触发HitReaction
            driver.ctx.isHit = true;
        }
    }

    // 在编辑器里画出扇形辅助线，方便调整距离和角度
    private void OnDrawGizmosSelected()
    {
        Transform origin = attackOrigin != null ? attackOrigin : transform;
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        
        Vector3 forward = origin.forward;
        Vector3 rightDir = Quaternion.Euler(0, attackAngle / 2, 0) * forward;
        Vector3 leftDir = Quaternion.Euler(0, -attackAngle / 2, 0) * forward;

        Gizmos.DrawRay(origin.position, rightDir * attackRadius);
        Gizmos.DrawRay(origin.position, leftDir * attackRadius);
        Gizmos.DrawWireSphere(origin.position, attackRadius);
    }
}
