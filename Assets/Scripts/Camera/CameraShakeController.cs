using Cinemachine;
using TaskManager;
using UnityEngine;
using FinalRPG.Utils;

/// <summary>
/// 攻击命中镜头晃动控制器 — 通过 Cinemachine Impulse 实现。
/// 玩家命中敌人（轻晃）和敌人命中玩家（重晃）使用不同强度。
/// </summary>
[RequireComponent(typeof(CinemachineImpulseSource))]
public class CameraShakeController : MonoBehaviour
{
    [Header("Shake Intensity")]
    [Tooltip("玩家攻击命中敌人时的晃动强度")]
    [Range(0f, 1f)]
    [SerializeField] private float _playerAttackShakeIntensity = 0.2f;

    [Tooltip("敌人攻击命中玩家时的晃动强度")]
    [Range(0f, 1f)]
    [SerializeField] private float _enemyAttackShakeIntensity = 0.5f;

    private CinemachineImpulseSource _impulseSource;

    private void Awake()
    {
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void OnEnable()
    {
        EventBus.Instance.OnAttackHit += OnAttackHit;
    }

    private void OnDisable()
    {
        EventBus.Instance.OnAttackHit -= OnAttackHit;
    }

    private void OnAttackHit(Vector3 worldPos, bool isPlayerAttack)
    {
        if (_impulseSource == null) return;

        float intensity = isPlayerAttack ? _playerAttackShakeIntensity : _enemyAttackShakeIntensity;
        if (intensity <= 0f) return;

        // 晃动方向：从命中点指向相机的方向
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 shakeDir = (cam.transform.position - worldPos).normalized;
        Vector3 velocity = shakeDir * intensity;

        RPGLog.Debug("Combat",
            $"震屏触发 isPlayerAttack={isPlayerAttack} intensity={intensity} dir={shakeDir}");

        _impulseSource.GenerateImpulse(velocity);
    }
}
