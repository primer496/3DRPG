using UnityEngine;
using TaskManager;
using FinalRPG.Utils;

/// <summary>
/// 玩家属性提供者 — MonoBehaviour 桥接层。
/// 挂载在 Player 根对象上。
/// 监听 EventBus 的奖励/伤害事件，路由到纯数据类 PlayerStats。
/// 外部通过 PlayerStatsProvider.Stats 获取数据实例（UI 绑定事件）。
/// </summary>
public class PlayerStatsProvider : MonoBehaviour
{
    [field: SerializeField]
    public PlayerStats Stats { get; private set; } = new PlayerStats();

    private void Awake()
    {
        Stats.Init();
    }

    private void OnEnable()
    {
        EventBus.Instance.OnGoldRewarded += AddGold;
        EventBus.Instance.OnExpRewarded  += AddExp;
        EventBus.Instance.OnPlayerDamaged += TakeDamage;
        RPGLog.Debug("Player", "已订阅 EventBus 奖励/伤害事件");
    }

    private void OnDisable()
    {
        EventBus.Instance.OnGoldRewarded -= AddGold;
        EventBus.Instance.OnExpRewarded  -= AddExp;
        EventBus.Instance.OnPlayerDamaged -= TakeDamage;
    }

    private void AddGold(int amount)
    {
        RPGLog.Debug("Player", $"收到金币奖励: {amount}");
        Stats.AddGold(amount);
    }

    private void AddExp(int amount)
    {
        RPGLog.Debug("Player", $"收到经验奖励: {amount}");
        Stats.AddExp(amount);
    }

    private void TakeDamage(int amount)
    {
        Stats.TakeDamage(amount);
    }
}
