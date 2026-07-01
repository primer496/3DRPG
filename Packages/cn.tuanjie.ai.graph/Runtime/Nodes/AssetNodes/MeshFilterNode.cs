using System.Collections;
using GraphProcessor;

#if UNITY_EDITOR
#endif

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Asset/MeshFilterNode"), UseProcessAsync]
    public class MeshFilterNode : SDNode
    {
        [SerializeField, HideInPreviewSelector, HideInInspector]
        [Input(name = "MeshFilter")] private Mesh m_Mesh;
        
        [Output(name = "Mesh Name")] public string meshName;

        [Preview]
        private Mesh m_PreviewMesh;

        [SerializeField, HideInPreviewSelector, HideInInspector]
        private GameObject m_Owner;

        public GameObject owner
        {
            get => m_Owner;
            set
            {
                if (m_Owner != value)
                {
                    m_Owner = value;
                    if (value.GetComponent<MeshFilter>() == null)
                        value.AddComponent<MeshFilter>();
                    m_Mesh = value.GetComponent<MeshFilter>().sharedMesh;
                    m_PreviewMesh = m_Mesh;
                    NotifyFieldChanged("m_PreviewMesh");
                }
            }
        }

        public override bool isRenamable => true;
        public override bool needTrigger => true;   

        public override IEnumerator ProcessAsync()
        {
            if (m_Mesh == null || owner == null)
                yield break;

            owner.GetComponent<MeshFilter>().sharedMesh = m_Mesh;
            m_PreviewMesh = m_Mesh;
            meshName = m_Mesh.name;
            NotifyFieldChanged("m_PreviewMesh");
            yield return null;
        }

        public IEnumerator Generate() => ProcessAsync();

        public override void SetTarget(Object target)
        {
            if (target is GameObject obj)
            {
                owner = obj;
            } else if (target is MeshFilter filter)
            {
                owner = filter.gameObject;
            }
            if (owner == null || owner.GetComponent<MeshFilter>() == null)
            {
                Debug.LogWarning($"Lack of MeshFilter on {target?.name}");
            }
        }
    }
}