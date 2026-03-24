using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    private GameObject _mainCamera;
    [Header("Cinemachine")]
    public GameObject CameraTarget;
    public float TopClamp = 70.0f;
    public float BottomClamp = -30.0f;

    public float horizontalsensitivity;
    public float verticalsensitivity;
    [Header("Look Input")]
    public float lookDeadZone = 0.01f;
    // 如果你的 Look 是摇杆轴值（非鼠标delta），建议开启；鼠标一般建议关闭
    public bool multiplyByDeltaTime = false;
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // 给角色移动逻辑（Move state）使用，避免它读取到上一帧的相机欧拉角导致抖动。
    public static float CurrentYawDeg = float.NaN;
    private bool _initialized;

    private void Awake()
    {
        if (CameraTarget == null) return;

        var euler = CameraTarget.transform.rotation.eulerAngles;
        _cinemachineTargetYaw = euler.y;
        _cinemachineTargetPitch = euler.x;
        CurrentYawDeg = _cinemachineTargetYaw;
        _initialized = true;
    }
    private void Start()
    {
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        // 初始化 yaw/pitch（使用世界旋转），避免启用后第一帧发生跳变
        // 使用世界旋转可以确保“角色转向不会带动相机额外旋转”。
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

        // yaw 环绕到 [-360, 360]，避免 0/360 边界出现表示跳变
        if (_cinemachineTargetYaw < -360f) _cinemachineTargetYaw += 720f;
        if (_cinemachineTargetYaw >  360f) _cinemachineTargetYaw -= 720f;

        // pitch 需要 clamp（保持和原始逻辑一致：每帧都 clamp，避免首帧/无输入时跳变）
        _cinemachineTargetPitch = Mathf.Clamp(_cinemachineTargetPitch, BottomClamp, TopClamp);

        CurrentYawDeg = _cinemachineTargetYaw;

        // 使用世界旋转，确保角色转向不会带动相机转向
        CameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0.0f);
    }
    public  void OnLook(InputValue value)
    {
        if (CameraTarget == null) return;

        // 鼠标的 InputValue 通常是“delta”，直接叠加到 yaw/pitch。
        // 这样 Move state 在 Update 里读取 CurrentYawDeg 时不会落后一帧。
        var look = value.Get<Vector2>();
        if (look.sqrMagnitude < lookDeadZone * lookDeadZone) return;

        float dt = multiplyByDeltaTime ? Time.deltaTime : 1f;

        _cinemachineTargetPitch += look.y * verticalsensitivity * dt;
        _cinemachineTargetYaw += look.x * horizontalsensitivity * dt;

        // yaw 环绕到 [-360, 360]，避免 0/360 边界出现表示跳变
        if (_cinemachineTargetYaw < -360f) _cinemachineTargetYaw += 720f;
        if (_cinemachineTargetYaw >  360f) _cinemachineTargetYaw -= 720f;

        // pitch 需要 clamp
        _cinemachineTargetPitch = Mathf.Clamp(_cinemachineTargetPitch, BottomClamp, TopClamp);

        CurrentYawDeg = _cinemachineTargetYaw;
    }
}
