using System.Collections.Generic;
using UnityEngine;
using HSM;

public class WeaponDetector : MonoBehaviour
{
    [Header("Weapon Detection Points")]
    public Transform weaponBase;
    public Transform weaponTip;
    
    [Header("Detection Settings")]
    public float attackRadius = 1.0f; // 挥剑杀伤半徑补正，代表剑刃的判定粗细
    public LayerMask targetLayer;     // 指定要检测的层级（如 Enemy层）
    public int raycastSegments = 5;

    private bool isAttacking = false;
    private Vector3[] previousPoints;
    private HashSet<PlayerStateDriver> hitDrivers = new HashSet<PlayerStateDriver>();
    
    // 由挥剑动画的开头事件调用（Animation Event）
    public void BeginAttack()
    {
        if (weaponBase == null || weaponTip == null) return;
        Debug.Log("WeaponDetector: 【开启武器打击判定】");
        isAttacking = true;
        hitDrivers.Clear();
        SaveControlPoints();
    }

    // 由挥剑动画的结束事件调用（Animation Event）
    public void EndAttack()
    {
        isAttacking = false;
    }

    void Update()
    {
        if (!isAttacking || weaponBase == null || weaponTip == null) return;

        Vector3[] currentPoints = GetControlPoints();

        for (int i = 0; i < currentPoints.Length; i++)
        {
            Vector3 prev = previousPoints[i];
            Vector3 curr = currentPoints[i];
            Vector3 dir = curr - prev;
            float dist = dir.magnitude;

            if (dist > 0.001f)
            {
                // 使用 SphereCastAll 替代遍历去寻找有 Collider 的实体
                RaycastHit[] hits = Physics.SphereCastAll(prev, attackRadius, dir.normalized, dist, targetLayer);
                foreach (var hit in hits)
                {
                    ProcessHit(hit.collider);
                }
            }
            else
            {
                // 没显著移动时，用原地的 OverlapSphere
                Collider[] hits = Physics.OverlapSphere(curr, attackRadius, targetLayer);
                foreach (var col in hits)
                {
                    ProcessHit(col);
                }
            }
        }

        previousPoints = currentPoints;
    }

    private void ProcessHit(Collider col)
    {
        // 避免打中自己
        if (col.transform.root == this.transform.root) return;

        Debug.Log("WeaponDetector: 武器碰撞扫描到对象 -> " + col.name);

        PlayerStateDriver driver = col.GetComponentInParent<PlayerStateDriver>();
        if (driver != null && !hitDrivers.Contains(driver))
        {
            Debug.Log("【玩家挥剑命中敌人成功】触发敌人受击硬直！对象 -> " + driver.gameObject.name);
            hitDrivers.Add(driver);
            ApplyHit(driver);
        }
    }

    private Vector3[] GetControlPoints()
    {
        Vector3[] points = new Vector3[raycastSegments];
        for (int i = 0; i < raycastSegments; i++)
        {
            float t = (float)i / (Mathf.Max(1, raycastSegments - 1));
            points[i] = Vector3.Lerp(weaponBase.position, weaponTip.position, t);
        }
        return points;
    }

    private void SaveControlPoints()
    {
        previousPoints = GetControlPoints();
    }

    private void ApplyHit(PlayerStateDriver driver)
    {
        if (driver != null)
        {
            // 记录是被谁打的，把自己的根坐标传过去用于敌人击推后退
            driver.ctx.currentHitSource = this.transform.root.position;
            // 触发敌人 HitReaction
            driver.ctx.isHit = true;

            // 为玩家自己挂上攻击滞帧，防止因为招式 Root Motion 冲过头穿模
            PlayerStateDriver attackerDriver = this.transform.root.GetComponent<PlayerStateDriver>();
            if (attackerDriver != null) {
                attackerDriver.ctx.hitSlowdownTimer = attackerDriver.ctx.hitStopDuration;
            }
        }
    }
}
