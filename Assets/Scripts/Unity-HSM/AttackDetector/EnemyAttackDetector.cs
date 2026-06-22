using System.Collections.Generic;
using UnityEngine;
using HSM;
using TaskManager;
using FinalRPG.Utils;

public class EnemyAttackDetector : MonoBehaviour
{
    [Header("Sector Attack Parameters")]
    public float attackRadius = 3f;
    [Range(0, 360)]
    public float attackAngle = 120f;
    public LayerMask targetLayer;

    [Header("—— SO 配置（优先）——")]
    [Tooltip("拖入 MonsterStats 资产，留空则使用下方回退默认值")]
    [SerializeField] private MonsterStats _stats;

    [Header("—— 回退默认值（_stats 为空时生效）——")]
    public int attackDamage = 10;

    private int _resolvedAttackDamage;

    // 默认以当前Transform为中心，若需要特效/特定身体部位可挂载子节点
    public Transform attackOrigin;

    // 单次攻击去重：玩家身上可能有多个 Collider，同一次扇形扫描只命中一次
    private HashSet<PlayerStateDriver> _hitDrivers = new HashSet<PlayerStateDriver>();

    void Awake()
    {
        if (attackOrigin == null)
            attackOrigin = transform;

        _resolvedAttackDamage = _stats != null ? _stats.attackDamage : attackDamage;
    }

    // 由敌人攻击动画（前摇结束，释放伤害的瞬间）通过 Animation Event 调用
    public void PerformSectorAttack()
    {
        // 取消 targetLayer 限制，防止因 Inspector 面板没勾选 Layer 导致重叠球内永远为 0
        Collider[] hits = Physics.OverlapSphere(attackOrigin.position, attackRadius);

        _hitDrivers.Clear();

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
                if(driver != null && !_hitDrivers.Contains(driver)) {
                    _hitDrivers.Add(driver);
                    RPGLog.Debug("Combat", $"扇形命中玩家！damage={_resolvedAttackDamage}");
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

            // 通过 EventBus 广播伤害，由 PlayerStatsProvider 订阅处理
            EventBus.Instance.RaiseDamage(_resolvedAttackDamage);
            // 广播跳字事件（世界坐标取玩家位置）
            EventBus.Instance.RaiseDamagePopup(_resolvedAttackDamage, driver.transform.position);
            // 广播命中事件（敌人攻击 → 仅触发震屏，不冻结）
            EventBus.Instance.RaiseAttackHit(driver.transform.position, isPlayerAttack: false);
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
