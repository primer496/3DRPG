using System;
using System.Collections;
using GraphProcessor;
#if UNITY_EDITOR
using UnityEditor;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Tools/Model Snapshot"), UseProcessAsync]
    public class ModelSnapshotNode : SDNode
    {
        [Input(name = "GameObject", immediateUpdate = true), SerializeField] private GameObject m_Go;

        public GameObject go
        {
            get => m_Go;
            set
            {
                if (m_Go != value)
                {
                    m_Go = value;
                }
            }
        }

        [Output(name = "Snapshot"), SerializeField, HideInInspector] private Texture2D m_OutputTexture;

        public override bool isRenamable => false;
        public override bool needTrigger => true;

        public Func<EditorWindow> getSnapshotWindow;

        public Func<Texture2D> getSnapshot = null;

        public Func<Texture2D> getSnapshotAsync = null;

        public override IEnumerator ProcessAsync()
        {
            if (go == null)
                yield break;

            if (!EditorUtility.DisplayDialog("Need Model Snapshot",
                $"You can edit snapshot view by pressing Edit. Otherwise, the snapshot view will be the same as in preview area.",
                "Edit", "Continue"))
            {
                m_OutputTexture = getSnapshot.Invoke();
                yield break;
            }

            var win = getSnapshotWindow.Invoke();
            yield return new WaitUntil(() => win == null);

            m_OutputTexture = getSnapshotAsync.Invoke();

            yield return null;
        }

        public IEnumerator Generate() => ProcessAsync();
    }
}
#endif