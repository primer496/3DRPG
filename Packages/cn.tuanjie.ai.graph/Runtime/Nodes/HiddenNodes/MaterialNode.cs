using GraphProcessor;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hidden/MaterialNode")]
    public class MaterialNode : SDNode, ICreateNodeFrom<Material>
    {
        [Preview, SerializeField, HideInInspector]
        [Output("Material")] private Material m_Material;

        public Material material
        {
            get => m_Material;
            set
            {
                if (m_Material != value)
                {
                    m_Material = value;
                    this?.NotifyFieldChanged("m_Material");
                }
            }
        }

        protected override void Enable()
        {
            hasSettings = true;
            base.Enable();
        }

        public override bool isRenamable => true;

        public bool InitializeNodeFromObject(Material value)
        {
            material = value;
            return true;
        }
    }
}
