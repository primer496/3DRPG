#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEditor;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Tools/Material Replacing"), UseProcessAsync]
    public class MaterialReplaceNode : SDNode
    {
        [Input(name = "Source Material"), SerializeField, HideInInspector] 
        public Material[] m_Sources;

        [SerializeField, HideInInspector, Preview, HideInPreviewSelector]
        private Material m_Source;
        public Material source
        {
            get => m_Source;
            set
            {
                if (m_Source != value)
                {
                    m_Source = value;
                    this?.NotifyFieldChanged("m_Source");
                }
            }
        }

        [SerializeField] public Material m_Target;

        [HideInInspector]
        public Action onReplace = null;

        public override bool isRenamable => false;
        public override bool needTrigger => true;
        public override IEnumerator ProcessAsync()
        {
            if (m_Target == null || m_Sources == null || m_Sources.Length == 0 || !IsMaterialInAssetDatabaseAndNotBuiltin(m_Target))
                yield break;

            if (!EditorUtility.DisplayDialog("Confirm Material Replacement",
                $"Replace material at '{AssetDatabase.GetAssetPath(m_Target)}' ? The orginal material will be removed.\n\nThis operation cannot be undone.",
                "Replace", "Cancel"))
            {
                yield break;
            }

            source = m_Sources[0];

            if (m_Source.shader != m_Target.shader)
            {
                m_Target.shader = m_Source.shader;
            }
            string name = m_Target.name;
            EditorUtility.CopySerialized(m_Source, m_Target);
            m_Target.name = name;
            EditorUtility.SetDirty(m_Target);
            AssetDatabase.SaveAssets();

            onReplace?.Invoke();
            yield return null;
        }

        [CustomPortInput(nameof(m_Sources), new Type[] { typeof(Material[]), typeof(Material) })]
        public void PullSubGraphInput(List<SerializableEdge> edges, NodePort outputPort = null)
        {
            if (edges == null || edges.Count == 0) return;

            foreach (var e in edges)
            {
                if (e.passThroughBuffer == null)
                    continue;
                if (typeof(Material).IsAssignableFrom(e.passThroughBuffer.GetType()))
                {
                    m_Sources = new Material[1] { e.passThroughBuffer as Material };
                }
                else if (typeof(List<Texture2D>).IsAssignableFrom(e.passThroughBuffer.GetType()))
                {
                    m_Sources = e.passThroughBuffer as Material[];
                }
            }
        }

        public IEnumerator Generate() => ProcessAsync();

        private static bool IsMaterialInAssetDatabaseAndNotBuiltin(Material material)
        {
            if (material == null)
                return false;
            string path = AssetDatabase.GetAssetPath(material);
            if (string.IsNullOrEmpty(path))
                return false;

            if (path.StartsWith("Assets/") || path.StartsWith("Packages/"))
                return true;
            return false;
        }

        public override void SetTarget(Object target)
        {
            if (target is Material mat)
            {
                m_Target = mat;
            }
        }
    }
}

#endif