using UnityEngine;

/// <summary>
/// 怪物 RPG 属性 ScriptableObject。
/// 在 Assets 右键 → Create → RPG → Monster Stats 创建，
/// 拖入 EnemyHealth / EnemyAttackDetector 组件的 stats 字段。
/// </summary>
[CreateAssetMenu(fileName = "MonsterStats", menuName = "RPG/Monster Stats")]
public class MonsterStats : ScriptableObject
{
    [Header("—— 基础属性 ——")]
    [Tooltip("用于任务系统匹配击杀目标")]
    public string enemyId = "ForestMonster";
    public int maxHealth = 3;

    [Header("—— 掉落奖励 ——")]
    public int expReward = 10;
    public int goldReward = 5;

    [Header("—— 攻击参数 ——")]
    public int attackDamage = 10;
}
