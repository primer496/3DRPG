using UnityEngine;
using UnityEngine.UIElements;

namespace FinalRPG.UI
{
    /// <summary>
    /// 纯 UI Toolkit 虚拟摇杆。
    /// 监听 Pointer 事件实现拖拽，输出归一化方向向量。
    /// 不依赖 Canvas / uGUI，无需额外预制件。
    /// </summary>
    public class UIToolkitJoystick
    {
        /// <summary>当前输入方向（归一化，原点为零向量）。每帧读取。</summary>
        public Vector2 InputDirection { get; private set; }

        private readonly VisualElement _area;
        private readonly VisualElement _handle;
        private readonly float _radius;
        private readonly float _deadZone;

        private int _capturedPointerId = PointerId.invalidPointerId;

        /// <param name="area">摇杆触摸区域（底座容器）</param>
        /// <param name="handle">摇杆手柄（小球）</param>
        /// <param name="radius">手柄最大移动半径（像素），默认55</param>
        /// <param name="deadZone">死区比例 (0~1)，默认0.1</param>
        public UIToolkitJoystick(VisualElement area, VisualElement handle, float radius = 55f, float deadZone = 0.1f)
        {
            _area = area;
            _handle = handle;
            _radius = radius;
            _deadZone = Mathf.Clamp(deadZone, 0f, 1f);

            _area.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _area.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _area.RegisterCallback<PointerUpEvent>(OnPointerUp);
            _area.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            _capturedPointerId = evt.pointerId;
            _area.CapturePointer(_capturedPointerId);
            UpdateHandle(evt.localPosition);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (_area.HasPointerCapture(_capturedPointerId))
            {
                UpdateHandle(evt.localPosition);
            }
        }

        private void OnPointerUp(IPointerEvent evt)
        {
            if (_capturedPointerId == evt.pointerId)
            {
                ReleasePointer();
            }
        }

        private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            if (_capturedPointerId == evt.pointerId)
            {
                ReleasePointer();
            }
        }

        private void ReleasePointer()
        {
            _area.ReleasePointer(_capturedPointerId);
            _capturedPointerId = PointerId.invalidPointerId;
            InputDirection = Vector2.zero;
            _handle.style.translate = new StyleTranslate(new Translate(0, 0));
        }

        private Vector2 GetCenter()
        {
            var rect = _area.contentRect;
            return new Vector2(rect.width * 0.5f, rect.height * 0.5f);
        }

        private void UpdateHandle(Vector2 localPos)
        {
            Vector2 center = GetCenter();
            if (center.x <= 0f) return; // 布局尚未完成

            // UI Toolkit Y 轴向下（0=顶），游戏 Y+ 向前 → 翻转 Y
            Vector2 offset = new Vector2(localPos.x - center.x, -(localPos.y - center.y));
            float magnitude = offset.magnitude;

            // 手柄视觉偏移（不翻转 Y，因为 UI Toolkit 需要原始坐标系）
            Vector2 visualOffset = localPos - center;
            if (magnitude > _radius)
            {
                visualOffset = visualOffset.normalized * _radius;
            }
            _handle.style.translate = new StyleTranslate(new Translate(visualOffset.x, visualOffset.y));

            // 死区判断
            if (magnitude < _radius * _deadZone)
            {
                InputDirection = Vector2.zero;
            }
            else
            {
                InputDirection = offset.normalized * Mathf.InverseLerp(_radius * _deadZone, _radius, Mathf.Min(magnitude, _radius));
            }
        }

        /// <summary>手动释放触摸/清理事件回调。</summary>
        public void Dispose()
        {
            ReleasePointer();
            _area.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            _area.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            _area.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            _area.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }
    }
}
