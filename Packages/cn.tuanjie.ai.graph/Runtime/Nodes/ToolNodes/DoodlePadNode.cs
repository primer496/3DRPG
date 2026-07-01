using GraphProcessor;
using UnityEditor;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Tools/Generate Mask")]
    public class DoodlePadNode : SDNode
    {
        [Input(name = "Input Image", immediateUpdate = true), Preview, HideInInspector]
        public Texture2D inputImage;
        
        [Output(name = "Mask Image"), Preview, HideInInspector, SerializeField]
        internal Texture2D m_maskImage;
        public Texture2D maskImage
        {
            get => m_maskImage;
            set
            {
                if (m_maskImage == value) return;
                m_maskImage = value;
                this?.NotifyFieldChanged("m_maskImage");
            }
        }
        /// <summary>
        /// get input image
        /// </summary>
        public virtual Texture GetTexture() { return inputImage; }
        /// <summary>
        /// set mask image
        /// </summary>
        public virtual void SaveMask(Texture2D mask)
        {
            maskImage = mask;
        }
        public virtual Texture2D GetMask() { return maskImage; }

        public override void CollectSubAssets()
        {
#if UNITY_EDITOR
            if (maskImage != null)
            {
                var assetPath = AssetDatabase.GetAssetPath(maskImage);
                if (string.IsNullOrEmpty(assetPath))
                {
                    assetPath = ExportUtils.SaveAsset(maskImage, GetResourceFolder(), $"mask_{GUID}",
                        ExportUtils.OverwriteAction.Overwrite);
                    maskImage = ImportUtils.Import<Texture2D>(assetPath);
                }
            }
#endif
            base.CollectSubAssets();
        }
    }
}