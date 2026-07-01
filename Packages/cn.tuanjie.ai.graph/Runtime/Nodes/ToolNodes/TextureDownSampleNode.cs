using System.Collections;
using System.Collections.Generic;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Tools/TextureDownSampleNode"), UseProcessAsync]
    public class TextureDownSampleNode : SDNode
    {
        [SerializeField, Preview, HideInInspector]
        [Output("Down Sampled")] private Texture2D m_OutputTexture;

        [SerializeField, HideInInspector]
        [Input("Image")] private Texture2D m_Input;

        [SerializeField, HideInInspector] public string currMaxSize = "1024";
        public Dictionary<string, int> maxSize = new()
        {
            { "32", 32 }, { "64", 64 }, { "128", 128 }, { "256", 256 }, { "512", 512 },
            { "1024", 1024 }, { "2048", 2048 }, { "4096", 4096 }, { "8192", 8192 }, { "16384", 16384 }
        };

        public Texture2D output
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
            output = DownsampleTexture(m_Input, maxSize[currMaxSize]);
            output.name = m_Input.name + "_processed";
            yield return null;
        }

        private static Texture2D DownsampleTexture(Texture2D sourceTexture, int maxSize)
        {
            if (sourceTexture == null)
            {
                return null;
            }

            int newWidth, newHeight;
            CalculateDownsampledSize(sourceTexture.width, sourceTexture.height, maxSize, out newWidth, out newHeight);

            if (newWidth >= sourceTexture.width && newHeight >= sourceTexture.height)
            {
                return sourceTexture;
            }

            RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            RenderTexture.active = rt;

            Graphics.Blit(sourceTexture, rt);

            Texture2D downsampledTexture = new Texture2D(newWidth, newHeight, TextureFormat.ARGB32, false);
            downsampledTexture.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
            downsampledTexture.Apply();

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);

            return downsampledTexture;
        }

        private static void CalculateDownsampledSize(int originalWidth, int originalHeight, int maxSize, out int newWidth, out int newHeight)
        {
            if (originalWidth > originalHeight)
            {
                newWidth = Mathf.Min(originalWidth, maxSize);
                newHeight = Mathf.RoundToInt((float)originalHeight / originalWidth * newWidth);
            }
            else
            {
                newHeight = Mathf.Min(originalHeight, maxSize);
                newWidth = Mathf.RoundToInt((float)originalWidth / originalHeight * newHeight);
            }

            newWidth = Mathf.Max(1, newWidth);
            newHeight = Mathf.Max(1, newHeight);
        }

        public IEnumerator Generate() => ProcessAsync();
    }
}
