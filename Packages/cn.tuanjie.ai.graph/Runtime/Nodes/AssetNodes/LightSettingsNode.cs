using System.Collections;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Asset/LightSettingsNode"), UseProcessAsync]
    public class LightSettingsNode : SDNode
    {
        [SerializeField, HideInInspector]
        [Input("Skybox Material")] private Material m_Material;

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

            RenderSettings.skybox = m_Material;
            yield return null;
        }

        public IEnumerator Generate() => ProcessAsync();
    }
}
