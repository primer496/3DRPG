using GraphProcessor;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hidden/MeshNode")]
    public class MeshNode : SDNode, ICreateNodeFrom<Mesh>
    {
        [HideInInspector, SerializeField, Preview]
        [Input("Mesh")] private Mesh m_Mesh;
        public Mesh mesh
        {
            get => m_Mesh;
            set
            {
                if (m_Mesh != value)
                {
                    m_Mesh = value;
                    this?.NotifyFieldChanged("m_Mesh");
                }
            }
        }

        protected override void Enable()
        {
            hasSettings = true;
            base.Enable();
        }

        public override bool isRenamable => true;

        public bool InitializeNodeFromObject(Mesh value)
        {
            mesh = value;
            return true;
        }
    }
}
