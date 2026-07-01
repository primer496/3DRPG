using System;
using System.Reflection;
using UnityEditor.AIGraph.InternalBridge;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

namespace UnityEditor.AIGraph
{
    public class SmartPreviewComponent : IMGUIContainer
    {
        public IPreviewRenderer previewRenderer;
        private FieldInfo targetField;
        private SDNode nodeTarget;
        private PreviewAttribute attribute;
        private bool m_Collapse = false;
        protected bool showFootnote;

        public bool collapse
        {
            get => m_Collapse;
            set => m_Collapse = value;
        }

        public SmartPreviewComponent(
            FieldInfo field,
            SDNode nodeTarget,
            PreviewAttribute attribute,
            bool showRiggingGO = false, bool showFootnote = false)
        {
            this.targetField = field;
            this.nodeTarget = nodeTarget;
            this.attribute = attribute;
            this.showFootnote = showFootnote;

            if (attribute.CustomRenderer != null)
            {
                previewRenderer = Activator.CreateInstance(attribute.CustomRenderer) as IPreviewRenderer;
            }
            else if (field.FieldType == typeof(GameObject) && showRiggingGO)
                previewRenderer = new RiggingPreviewRenderer();
            else
            {
                previewRenderer = PreviewRendererRegistry.GetRenderer(field.FieldType);
            }
            InitPreview();

            onGUIHandler += OnDrawPreview;
        }

        private void OnDrawPreview()
        {
            if (previewRenderer.HasPreviewGUI())
            {
                Rect toolbarRect = InternalAPI.Internal_EditorGUILayout_BeginHorizontal(GUIContent.none, EditorStyles.toolbar, GUILayout.Height(21f));
                {
                    // Label
                    string label = string.Empty;
                    label = previewRenderer.GetPreviewTitle();


                    GUILayout.Label(label, "ToolbarBoldLabel");

                    GUILayout.FlexibleSpace();
                    previewRenderer.OnPreviewSettings();
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                previewRenderer.OnPreviewGUI(GUILayoutUtility.GetRect(attribute.rectSize.x, attribute.rectSize.y), attribute.background);
                if (showFootnote)
                {
                    var target = targetField.GetValue(nodeTarget) as Texture;
                    const float width = 50f;
                    var rectWidth = attribute.rectSize.x - width;
                    var rectHeight = attribute.rectSize.y + 2f;
                    if (target)
                    {
                        var zoomLevel = Mathf.Min(Mathf.Min(attribute.rectSize.x / target.width, attribute.rectSize.y / target.height), 1);
                        rectWidth = (attribute.rectSize.x - target.width * zoomLevel) / 2;
                        rectWidth = attribute.rectSize.x - rectWidth - 45f;
                        rectHeight = (attribute.rectSize.y - target.height * zoomLevel) / 2;
                        rectHeight += target.height * zoomLevel + 2f;
                    }
                    GUILayout.BeginArea(new Rect(
                        rectWidth, // 从右侧偏移
                        rectHeight, // 从顶部偏移
                        width, // 宽度
                        20f // 高度
                    ));
                    {
                        var aiLabelStyle = new GUIStyle(EditorStyles.miniLabel)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            normal =
                            {
                                textColor = new Color(0.9f, 0.9f, 0.9f, 0.75f)
                            },
                            hover =
                            {
                                textColor = new Color(0.9f, 0.9f, 0.9f, 0.75f)
                            },
                            fontStyle = FontStyle.Bold
                        };
                        GUI.backgroundColor = new Color(0f, 0f, 0f, 0.0f);
                        GUI.contentColor = Color.white;
                        GUILayout.Label("团结AI", aiLabelStyle, GUILayout.Width(width), GUILayout.Height(20f));
                    }
                    GUILayout.EndArea();
                }
                GUILayout.EndHorizontal();

            }
        }

        public void InitPreview()
        {
            if (targetField.GetValue(nodeTarget) is not UnityEngine.Object)
                return;

            previewRenderer?.Initialize(targetField.GetValue(nodeTarget) as UnityEngine.Object, nodeTarget);
            MarkDirtyRepaint();
        }

        public void UpdatePreview()
        {
            previewRenderer?.Update(targetField.GetValue(nodeTarget) as UnityEngine.Object);
            MarkDirtyRepaint();
        }

        public void Cleanup()
        {
            previewRenderer?.Cleanup();
        }
    }
}
