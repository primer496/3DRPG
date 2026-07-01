using GraphProcessor;
#if UNITY_EDITOR
#endif

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hidden/Texture2DNode")]
    public class TextureNode : SDNode, ICreateNodeFrom<Texture2D>
    {
        [Preview, SerializeField, HideInInspector]
        [Output(name = "Texture2D")] private Texture2D m_OutputTexture;
        public Texture2D outputTexture
        {
            get => m_OutputTexture;
            set
            {
                if (m_OutputTexture != value)
                {
                    m_OutputTexture = value;
                    this?.NotifyFieldChanged("m_OutputTexture");
                }
            }
        }

        protected override void Enable()
        {
            hasSettings = true;
            base.Enable(); 
        }

        public override bool isRenamable => true;

        // Q: ?这个value是从哪里来的？创建一个节点的时候怎么传递输入？只能函数调用吗？
        // A: 当有object被drag drop到graph view时会触发这个函数
        public bool InitializeNodeFromObject(Texture2D value)
        {
            if (value == null) return false;
            outputTexture = value;
            return true;
        }
    }
}