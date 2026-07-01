using System;
using System.Collections;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hunyuan/Image Clarity(Hunyuan)")]
    [UseProcessAsync]
    public class HyImageClarityNode : BaseHyImageNode
    {
        // Part 1: api的各项输入
        [Input(name = "Image")] 
        [Tooltip("Input image for clarity enhancement. Must be in JPEG/JPG/PNG format, size < 6MB, and aspect ratio within 2:1.")]
        public Texture2D image;
        
        [Input(name = "Version")] 
        [Tooltip("Model version")]
        public string version;
        
        public override string name => LocalizationManager.Instance.GetLocalizedText("Image Clarity(Hunyuan)");
        
        public override string description => DescriptionConstants.HyImageClarityNode;

        public override IEnumerator ProcessAsync()
        {
            // Part 2: 检查输入是否为空
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            
            // Part 3：生成对应的request并调用rest call
            var request = new HyImageClarityRequest()
            {
                version = version,
                n = 1,
                image = image.ToBase64()
            };
            var restCall = new HyImageClarityRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
    }
}