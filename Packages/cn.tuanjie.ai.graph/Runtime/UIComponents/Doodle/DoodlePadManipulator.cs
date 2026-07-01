using System;
using UnityEngine.UIElements;

namespace UnityEngine.AIGraph
{
    internal class DoodlePadManipulator : Manipulator
    {
        DoodlePad m_DoodlePad;

        const float k_DefaultBrushSize = 5.0f;
        const float k_MinBrushSize = 1.0f;
        const float k_MaxBrushSize = 20.0f;

        int scaleCoeff => (m_Size.x + m_Size.y) / 1024;
        public float initRadius => k_DefaultBrushSize * scaleCoeff;
        public float minRadius => k_MinBrushSize * scaleCoeff;
        public float maxRadius => k_MaxBrushSize * scaleCoeff;

        Vector2Int m_Size;
        float m_Opacity;

        public event Action onDoodleUpdate;
        public event Action<byte[]> onValueChanged;

        public DoodleModifierState currentState => m_DoodlePad.modifierState;

        public DoodlePadManipulator(bool seamless)
            : this(new Vector2Int(2, 2), 0.7f)
        {
            SetMaskSeamless(seamless);
        }

        public void SetMaskSeamless(bool seamless)
        {
            m_DoodlePad.SetSeamless(seamless);
        }

        public DoodlePadManipulator(Vector2Int size, float opacity = 1.0f)
        {
            m_Size = size;
            m_Opacity = opacity;

            m_DoodlePad = new DoodlePad(m_Opacity);
            m_DoodlePad.SetBrushSize(initRadius);
            m_DoodlePad.SetDoodleSize(m_Size);
        }

        public bool isClear
        {
            get => m_DoodlePad.isClear;
            set => m_DoodlePad.isClear = value;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.Add(m_DoodlePad);
            m_DoodlePad.StretchToParentSize();
            m_DoodlePad.onModifierStateChanged += OnModifierStateChanged;
            m_DoodlePad.RegisterValueChangedCallback(OnValueChanged);
            m_DoodlePad.onDoodleStart += onDoodleUpdate;
            m_DoodlePad.onDoodleUpdate += onDoodleUpdate;
            m_DoodlePad.onDoodleEnd += onDoodleUpdate;

            //m_DoodlePad.SetBrush();
        }

        void OnModifierStateChanged(DoodleModifierState state)
        {
            onModifierStateChanged?.Invoke(state);
        }

        void OnValueChanged(ChangeEvent<byte[]> evt)
        {
            onValueChanged?.Invoke(evt.newValue);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            if (m_DoodlePad == null)
                return;

            m_DoodlePad.SetNone();

            m_DoodlePad.onModifierStateChanged -= OnModifierStateChanged;
            m_DoodlePad.UnregisterValueChangedCallback(OnValueChanged);
            m_DoodlePad.onDoodleStart -= onDoodleUpdate;
            m_DoodlePad.onDoodleUpdate -= onDoodleUpdate;
            m_DoodlePad.onDoodleEnd -= onDoodleUpdate;

            //m_DoodlePad.Dispose();
            target.Remove(m_DoodlePad);
            //m_DoodlePad = null;
        }

        public void SetRadius(float size)
        {
            m_DoodlePad.SetBrushSize(size);
        }

        public float GetRadius() => m_DoodlePad.brushRadius;

        public void IncreaseBrushSize(float step)
        {
            if(m_DoodlePad == null)
                return;

            var size = Mathf.Min(maxRadius, m_DoodlePad.brushRadius + step);
            m_DoodlePad.SetBrushSize(size);
        }

        public void DecreaseBrushSize(float step)
        {
            if(m_DoodlePad == null)
                return;

            var size = Mathf.Max(minRadius, m_DoodlePad.brushRadius - step);
            m_DoodlePad.SetBrushSize(size);
        }

        public void SetValue(byte[] newValue)
        {
            m_DoodlePad?.SetDoodle(newValue);
        }

        public void SetValueWithoutNotify(byte[] newValue)
        {
            m_DoodlePad?.SetValueWithoutNotify(newValue);
        }

        public event Action<DoodleModifierState> onModifierStateChanged;

        public void SetEraserMode(bool erase)
        {
            if (erase)
                SetEraser();
            else
                SetBrush();
        }

        public void ToggleBrush()
        {
            if(m_DoodlePad.modifierState != DoodleModifierState.Brush)
                m_DoodlePad.SetBrush();
            else
                m_DoodlePad.SetNone();
        }

        public void SetBrush()
        {
            if(m_DoodlePad.modifierState != DoodleModifierState.Brush)
                m_DoodlePad.SetBrush();
        }

        public void ToggleEraser()
        {
            if(m_DoodlePad.modifierState != DoodleModifierState.Erase)
                m_DoodlePad.SetEraser();
            else
                m_DoodlePad.SetNone();
        }

        public void SetEraser()
        {
            if(m_DoodlePad.modifierState != DoodleModifierState.Erase)
                m_DoodlePad.SetEraser();
        }

        public void SetNone()
        {
            m_DoodlePad.SetNone();
        }

        public void ClearPainting()
        {
            if (m_DoodlePad != null && !m_DoodlePad.isClear)
                m_DoodlePad.SetDoodle(null);
        }

        public void Resize(Vector2Int newSize)
        {
            m_Size = newSize;
            m_DoodlePad.SetBrushSize(initRadius);
            m_DoodlePad.SetDoodleSize(newSize);
        }
    }
}
