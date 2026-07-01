using System.Collections;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Asset/ParticleSystemRendererNode"), UseProcessAsync]
    public class ParticleSystemRendererNode : SDNode
    {
        [SerializeField, HideInInspector]
        public ParticleSystemRenderer m_Renderer;

        [SerializeField, HideInInspector]
        [Input("Mesh")] private Mesh m_Mesh;

        [SerializeField, HideInInspector]
        [Input("Materials")] private Material[] m_Materials;

        public ParticleSystemRenderer renderer
        {
            get => m_Renderer;
            set
            {
                if (m_Renderer != value)
                {
                    m_Renderer = value;
                    m_Mesh = m_Renderer.mesh;
                    m_Materials = m_Renderer != null && m_Renderer.sharedMaterials == null ? m_Renderer.sharedMaterials : null;
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
            m_Renderer.mesh = m_Mesh;
            yield return null;
        }

        public IEnumerator Generate() => ProcessAsync();
        public override void SetTarget(Object target)
        {
            if (target is ParticleSystemRenderer psRenderer)
            {
                renderer = psRenderer;
            }
        }
    }
}
