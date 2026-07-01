using System.Collections;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Asset/SkyboxComponentNode"), UseProcessAsync]
    public class SkyboxComponentNode : SDNode
    {
        [SerializeField, HideInInspector]
        public Skybox m_Renderer;

        [SerializeField, HideInInspector]
        [Input("Material")] private Material m_Material;

        public Skybox renderer
        {
            get => m_Renderer;
            set
            {
                if (m_Renderer != value)
                {
                    m_Renderer = value;
                    m_Material = m_Renderer != null && m_Renderer.material == null ? m_Renderer.material : null;
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
            if (m_Material == null)
                yield break;

            m_Renderer.material = m_Material;
            yield return null;
        }

        public IEnumerator Generate() => ProcessAsync();
    }
}
