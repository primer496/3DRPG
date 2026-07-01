using System.Collections;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Asset/MeshRendererNode"), UseProcessAsync]
    public class MeshRendererNode : SDNode
    {
        [SerializeField, HideInInspector]
        public MeshRenderer m_Renderer;

        [SerializeField, HideInInspector]
        [Input("Materials")] private Material[] m_Materials;

        [SerializeField, Preview, HideInInspector, HideInPreviewSelector]
        private GameObject m_GO;

        public MeshRenderer renderer
        {
            get => m_Renderer;
            set
            {
                if (m_Renderer != value)
                {
                    m_Renderer = value;
                    m_Materials = m_Renderer != null && m_Renderer.sharedMaterials == null ? m_Renderer.sharedMaterials : null;
                    m_GO = m_Renderer == null ? null : m_Renderer.gameObject;
                    this?.NotifyFieldChanged("m_GO");
                }
            }
        }

        protected override void Enable()
        {
            base.Enable();
        }

        public override bool isRenamable => true;

        public override bool needTrigger => true;

        protected override void Destroy()
        {
            base.Destroy();
        }

        public override void CollectSubAssets()
        {
            base.CollectSubAssets();
        }

        public override IEnumerator ProcessAsync()
        {
            if (m_Materials == null)
                yield break;

            m_Renderer.sharedMaterials = m_Materials;
            this?.NotifyFieldChanged("m_GO");
            yield return null;
        }

        public IEnumerator Generate() => ProcessAsync();
        public override void SetTarget(Object target)
        {
            if (target is GameObject obj)
            {
                renderer = obj.GetComponent<MeshRenderer>();
            } else if (target is MeshRenderer meshRenderer)
            {
                renderer = meshRenderer;
            }

            if (renderer == null)
            {
                Debug.LogWarning($"Lack of MeshRenderer on {target?.name}！");
            }
        }
    }
}
