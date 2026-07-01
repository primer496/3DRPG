using GraphProcessor;
#if UNITY_EDITOR
#endif

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Asset/GameObject Node")]
    public class GameObjectAssetNode : SDNode
    {
        [Input(name = "Game Object")] public GameObject inputGO;
        
        [Preview, SerializeField, HideInPreviewSelector, HideInInspector]
        [Output(name = "Game Object")] private GameObject m_obj;

        public GameObject obj
        {
            get => m_obj;
            set
            {
                if (m_obj != value)
                {
                    m_obj = value;
                    this?.NotifyFieldChanged("m_obj");
                }
            }
        }

        public override bool isRenamable => true;
        public override void Process()
        {
            var hasInputEdge = GetInputEdges().Count > 0;
            if (hasInputEdge)
                obj = inputGO;
        }
    }
}