#if UNITY_EDITOR

using System.Collections;
using GraphProcessor;
using UnityEditor;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Tools/AnimationClip Replacing"), UseProcessAsync]
    public class AnimationClipReplaceNode : SDNode
    {
        [Input(name = "Source AnimationClip"), SerializeField, HideInInspector, Preview, HideInPreviewSelector]
        private AnimationClip m_Source;
        public AnimationClip source
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

        [SerializeField] public AnimationClip m_Target;

        public override bool isRenamable => false;
        public override bool needTrigger => true;
        public override IEnumerator ProcessAsync()
        {
            if (m_Target == null || m_Source == null || !IsInAssetDatabaseAndNotBuiltin(m_Target))
                yield break;

            if (!EditorUtility.DisplayDialog("Confirm AnimationClip Replacement",
                $"Replace clip at '{AssetDatabase.GetAssetPath(m_Target)}' ? The orginal clip will be removed.\n\nThis operation cannot be undone.",
                "Replace", "Cancel"))
            {
                yield break;
            }

            source = m_Source;

            string name = m_Target.name;
            EditorUtility.CopySerialized(m_Source, m_Target);
            m_Target.name = name;
            EditorUtility.SetDirty(m_Target);
            AssetDatabase.SaveAssets();

            yield return null;
        }

        public IEnumerator Generate() => ProcessAsync();

        private static bool IsInAssetDatabaseAndNotBuiltin(AnimationClip material)
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
            if (target is AnimationClip clip)
            {
                m_Target = clip;
            }
        }
    }
}

#endif