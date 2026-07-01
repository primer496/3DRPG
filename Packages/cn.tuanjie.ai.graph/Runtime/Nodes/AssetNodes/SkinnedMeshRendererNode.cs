using System;
using System.Collections;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Asset/SkinnedMeshRendererNode"), UseProcessAsync]
    public class SkinnedMeshRendererNode : SDNode
    {
        [SerializeField, HideInInspector]
        public SkinnedMeshRenderer m_Renderer;

        [SerializeField, HideInInspector]
        [Input("Mesh")] private Mesh m_Mesh;

        [SerializeField, HideInInspector]
        [Input("Materials")] private Material[] m_Materials;

        [Output("Gameobject"), SerializeField, Preview, HideInInspector, HideInPreviewSelector]
        private GameObject m_GO;

        public SkinnedMeshRenderer renderer
        {
            get => m_Renderer;
            set
            {
                if (m_Renderer != value)
                {
                    m_Renderer = value;
                    m_GO = !m_Renderer ? null : m_Renderer.gameObject;
                    this?.NotifyFieldChanged("m_GO");
                }
            }
        }

        public override bool isRenamable => true;

        public override bool needTrigger => true;

        public override IEnumerator ProcessAsync()
        {
            if (m_Materials == null)
                yield break;
            if (m_Renderer == null)
                throw new ArgumentException("SkinnedMeshRenderer is null");

            m_Renderer.sharedMaterials = m_Materials;
            m_Renderer.sharedMesh = m_Mesh;
            this?.NotifyFieldChanged("m_GO");
            yield return null;
        }

        public IEnumerator Generate() => ProcessAsync();
        public override void SetTarget(Object target)
        {
            if (target is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                renderer = skinnedMeshRenderer;
            } else if (target is GameObject obj)
            {
                renderer = obj.GetComponent<SkinnedMeshRenderer>();
            }
        }
    }
}
