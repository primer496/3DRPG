using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using TaskManager;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class ThirdPersonCamera : MonoBehaviour
{
    private GameObject _mainCamera;
    [Header("Cinemachine")]
    public GameObject CameraTarget;
    public float TopClamp = 70.0f;
    public float BottomClamp = -30.0f;

    public float horizontalsensitivity;
    public float verticalsensitivity;
    [Header("Touch Camera")]
    [Tooltip("触摸灵敏度倍率")]
    public float touchSensitivity = 0.5f;
    [Header("Look Input")]
    public float lookDeadZone = 0.01f;
    // 濡傛灉浣犵殑 Look 鏄憞鏉嗚酱鍊硷紙闈為紶鏍嘾elta锛夛紝寤鸿寮€鍚紱榧犳爣涓€鑸缓璁叧闂�
    public bool multiplyByDeltaTime = false;
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private bool _lookLocked;

    // 缁欒鑹茬Щ鍔ㄩ€昏緫锛圡ove state锛変娇鐢紝閬垮厤瀹冭鍙栧埌涓婁竴甯х殑鐩告満娆ф媺瑙掑鑷存姈鍔ㄣ€�
    public static float CurrentYawDeg = float.NaN;
    private bool _initialized;
    private Transform _followTarget;
    private Vector3 _targetOffset;

    private void Awake()
    {
        if (CameraTarget == null) return;

        _followTarget = CameraTarget.transform.parent;
        _targetOffset = CameraTarget.transform.localPosition;
        CameraTarget.transform.SetParent(null); // Detach to prevent orbit jitter

        var euler = CameraTarget.transform.rotation.eulerAngles;
        _cinemachineTargetYaw = euler.y;
        _cinemachineTargetPitch = euler.x;
        CurrentYawDeg = _cinemachineTargetYaw;
        _initialized = true;
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        EventBus.Instance.OnInputLockStateChanged += OnInputLockChanged;
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
        EventBus.Instance.OnInputLockStateChanged -= OnInputLockChanged;
    }

    private void OnInputLockChanged(bool locked)
    {
        _lookLocked = locked;
    }
    private void Start()
    {
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        // 鍒濆鍖� yaw/pitch锛堜娇鐢ㄤ笘鐣屾棆杞級锛岄伩鍏嶅惎鐢ㄥ悗绗竴甯у彂鐢熻烦鍙�
        // 浣跨敤涓栫晫鏃嬭浆鍙互纭繚鈥滆鑹茶浆鍚戜笉浼氬甫鍔ㄧ浉鏈洪澶栨棆杞€濄€�
        if (!_initialized && CameraTarget != null)
        {
            var euler = CameraTarget.transform.rotation.eulerAngles;
            _cinemachineTargetYaw = euler.y;
            _cinemachineTargetPitch = euler.x;
            CurrentYawDeg = _cinemachineTargetYaw;
            _initialized = true;
        }
    }
    private void Update()
    {
        if (CameraTarget == null) return;

        // 移动端相机：EnhancedTouch API，右半屏=转视角
        if (!_lookLocked)
        {
            foreach (var t in Touch.activeTouches)
            {
                if (t.phase == UnityEngine.InputSystem.TouchPhase.Moved && t.screenPosition.x > Screen.width * 0.45f)
                {
                    _cinemachineTargetYaw += t.delta.x * horizontalsensitivity * touchSensitivity;
                    _cinemachineTargetPitch -= t.delta.y * verticalsensitivity * touchSensitivity;
                }
            }
        }

        // yaw 鐜粫鍒� [-360, 360]锛岄伩鍏� 0/360 杈圭晫鍑虹幇琛ㄧず璺冲彉
        if (_cinemachineTargetYaw < -360f) _cinemachineTargetYaw += 720f;
        if (_cinemachineTargetYaw >  360f) _cinemachineTargetYaw -= 720f;

        // pitch 闇€瑕� clamp锛堜繚鎸佸拰鍘熷閫昏緫涓€鑷达細姣忓抚閮� clamp锛岄伩鍏嶉甯�/鏃犺緭鍏ユ椂璺冲彉锛�
        _cinemachineTargetPitch = Mathf.Clamp(_cinemachineTargetPitch, BottomClamp, TopClamp);

        CurrentYawDeg = _cinemachineTargetYaw;
    }

    // 鍦� LateUpdate 鍐欏叆 CameraTarget 涓栫晫浣嶇疆鍜屾棆杞細鏅氫簬鏈抚 PlayerStateDriver 瀵硅鑹茬殑鏃嬭浆锛�
    // 閬垮厤 LookRoot 浣滀负瀛愮墿浣撳湪 Update 鍐呰鐖惰妭鐐规棆杞€滃甫鍋忊€濓紝杩涜€岃 Cinemachine 鎶栧姩銆�
    private void LateUpdate()
    {
        if (CameraTarget == null) return;

        if (_followTarget != null) {
            CameraTarget.transform.position = _followTarget.position + _targetOffset;
        }

        CameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0.0f);
    }

    public void OnLook(InputValue value)
    {
        if (CameraTarget == null) return;
        if (_lookLocked) return;

        // PC 鼠标相机（触摸已改用 Update 原生API，永不冲突）
        var look = value.Get<Vector2>();
        if (look.sqrMagnitude < lookDeadZone * lookDeadZone) return;

        float dt = multiplyByDeltaTime ? Time.deltaTime : 1f;

        _cinemachineTargetPitch += look.y * verticalsensitivity * dt;
        _cinemachineTargetYaw += look.x * horizontalsensitivity * dt;

        // yaw 鐜粫鍒� [-360, 360]锛岄伩鍏� 0/360 杈圭晫鍑虹幇琛ㄧず璺冲彉
        if (_cinemachineTargetYaw < -360f) _cinemachineTargetYaw += 720f;
        if (_cinemachineTargetYaw >  360f) _cinemachineTargetYaw -= 720f;

        // pitch 闇€瑕� clamp
        _cinemachineTargetPitch = Mathf.Clamp(_cinemachineTargetPitch, BottomClamp, TopClamp);

        CurrentYawDeg = _cinemachineTargetYaw;
    }
}
