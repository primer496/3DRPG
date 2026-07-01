using GraphProcessor;
#if UNITY_EDITOR
#endif

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Asset/Texture2DNode")]
    public class TextureAssetNode : SDNode
    {
        [Preview, SerializeField, HideInPreviewSelector, HideInInspector]
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

        public override bool isRenamable => true;
    }
}