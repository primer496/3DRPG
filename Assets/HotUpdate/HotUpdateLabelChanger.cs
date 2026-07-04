using UnityEngine;
using UnityEngine.UIElements;

namespace HotUpdate
{
    /// <summary>
    /// 热更新验证脚本 — 挂载在 Addressables 预制体上。
    /// 被实例化后，修改场景中的验证 Label 文本。
    /// </summary>
    public class HotUpdateLabelChanger : MonoBehaviour
    {
        [Header("Verification")]
        [Tooltip("热更后显示的文本")]
        public string Message = "热更新成功！";

        [Header("Color")]
        [Tooltip("文字颜色（可选）")]
        public Color LabelColor = Color.green;

        private void Start()
        {
            // Find the verification UIDocument in scene
            var uiDocs = FindObjectsOfType<UIDocument>();
            foreach (var doc in uiDocs)
            {
                var label = doc.rootVisualElement?.Q<Label>("hot-update-label");
                if (label != null)
                {
                    label.text = Message;
                    label.style.color = LabelColor;
                    Debug.Log($"[HotUpdate] Label updated: '{Message}' color={LabelColor}");
                    return;
                }
            }
            Debug.LogWarning("[HotUpdate] Could not find Label 'hot-update-label' in any UIDocument.");
        }
    }
}
