using UnityEngine;
using Yarn.Unity;
using TaskManager;
using FinalRPG.Utils;

namespace HSM
{
    /// <summary>
    /// 敌人血量组件。挂在敌人根对象上。
    /// 扣血归零时销毁自身并向 EventBus 广播击杀事件。
    /// 优先从 MonsterStats SO 读取数值；SO 未配置时使用 Inspector 默认值作为回退。
    /// </summary>
    public class EnemyHealth : MonoBehaviour
    {
        [Header("—— SO 配置（优先）——")]
        [Tooltip("拖入 MonsterStats 资产，留空则使用下方 Inspector 默认值")]
        [SerializeField] private MonsterStats _stats;

        [Header("—— 回退默认值（_stats 为空时生效）——")]
        public int maxHealth = 3;
        public int expReward = 10;
        public int goldReward = 5;

        [Tooltip("向 EventBus 报告的敌人ID，用于任务系统匹配击杀目标")]
        public string enemyId = "ForestMonster";

        [Tooltip("敌人死亡后在 Yarn 变量存储中设置的变量名（如 $MonsterKilled），留空则不设置")]
        public string yarnKilledVariable = "$MonsterKilled";

        // 运行时的实际值（SO 优先，回退到 Inspector 默认值）
        private int _resolvedMaxHealth;
        private int _resolvedExpReward;
        private int _resolvedGoldReward;
        private string _resolvedEnemyId;
        private int currentHealth;

        private void Awake()
        {
            ResolveStats();
            currentHealth = _resolvedMaxHealth;
        }

        /// <summary>优先从 SO 读取数值，SO 为空则使用 Inspector 默认值。</summary>
        private void ResolveStats()
        {
            _resolvedMaxHealth  = _stats != null ? _stats.maxHealth   : maxHealth;
            _resolvedExpReward  = _stats != null ? _stats.expReward   : expReward;
            _resolvedGoldReward = _stats != null ? _stats.goldReward  : goldReward;
            _resolvedEnemyId    = _stats != null ? _stats.enemyId     : enemyId;
        }

        public void TakeDamage(int amount)
        {
            if (currentHealth <= 0) return;

            currentHealth -= amount;
            RPGLog.Debug("Combat", $"{gameObject.name} 受到 {amount} 点伤害，剩余 {currentHealth}/{_resolvedMaxHealth}");

            if (currentHealth <= 0)
                Die();
        }

        private void Die()
        {
            RPGLog.Debug("Combat", $"{gameObject.name} 死亡，向 EventBus 报告击杀 {_resolvedEnemyId}");
            EventBus.Instance.Raise(TargetType.Kill, _resolvedEnemyId, 1);

            // 在 Yarn 变量存储中标记已击杀，供对话分支判断
            if (!string.IsNullOrEmpty(yarnKilledVariable))
            {
                var storage = Object.FindFirstObjectByType<InMemoryVariableStorage>();
                if (storage != null)
                    storage.SetValue(yarnKilledVariable, true);
            }

            // 通过 EventBus 广播奖励，由 PlayerStatsProvider 订阅处理
            EventBus.Instance.RaiseExpReward(_resolvedExpReward);
            EventBus.Instance.RaiseGoldReward(_resolvedGoldReward);

            Destroy(gameObject);
        }
    }
}
