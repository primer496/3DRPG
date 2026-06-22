using DG.Tweening;
using TaskManager;
using UnityEngine;
using FinalRPG.Utils;

/// <summary>
/// 攻击命中帧冻结控制器 — 仅玩家命中敌人时触发。
/// 用 DOTween 驱动 Time.timeScale 短时冻结 + 缓出恢复。
/// </summary>
public class FrameFreezeController : MonoBehaviour
{
    [Header("Frame Freeze")]
    [Tooltip("冻结持续时长（秒）")]
    [SerializeField] private float _freezeDuration = 0.05f;

    [Tooltip("冻结期间的 timeScale 值，0 为完全暂停")]
    [Range(0f, 1f)]
    [SerializeField] private float _freezeTimeScale = 0.1f;

    [Tooltip("冻结恢复的缓动曲线")]
    [SerializeField] private Ease _freezeEase = Ease.OutQuad;

    private Tween _freezeTween;

    private void OnEnable()
    {
        EventBus.Instance.OnAttackHit += OnAttackHit;
    }

    private void OnDisable()
    {
        EventBus.Instance.OnAttackHit -= OnAttackHit;
        _freezeTween?.Kill();
    }

    private void OnAttackHit(Vector3 worldPos, bool isPlayerAttack)
    {
        // 仅玩家攻击命中敌人才触发帧冻结（业界标准）
        if (!isPlayerAttack) return;

        RPGLog.Debug("Combat", $"帧冻结触发 duration={_freezeDuration}s scale={_freezeTimeScale}");

        // 杀死旧 tween 防止连段时多个 tween 竞争 timeScale
        _freezeTween?.Kill();

        _freezeTween = DOTween
            .To(
                () => Time.timeScale,
                v => Time.timeScale = v,
                1f,
                _freezeDuration
            )
            .From(_freezeTimeScale)
            .SetEase(_freezeEase)
            .SetUpdate(true) // 用 unscaled time，不受自身冻结影响
            .OnComplete(() => _freezeTween = null);
    }
}
