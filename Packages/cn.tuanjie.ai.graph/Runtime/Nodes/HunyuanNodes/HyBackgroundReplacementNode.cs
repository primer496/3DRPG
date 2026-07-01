using System;
using System.Collections;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hunyuan/Background Replacement(Hunyuan)")]
    [UseProcessAsync]
    public class HyBackgroundReplacementNode : BaseHyImageNode
    {
        // Part 1: api的各项输入
        [Input(name = "Image")] 
        [Tooltip("Input image for background replacement. Aspect ratio must be within 2.5:1, size < 6MB, supports JPG/JPEG/PNG formats")]
        public Texture2D image;
        
        [Input(name = "Image Url")] 
        [Tooltip("URL of input image for background replacement. Aspect ratio must be within 2.5:1, size < 6MB, supports JPG/JPEG/PNG formats")]
        public string imageUrl;
        
        [Input(name = "Mask")] 
        [Tooltip("Mask image in grayscale space. Black areas remain unchanged, white areas will be replaced. Must be PNG format and match image resolution")]
        public Texture2D mask;
        
        [Input(name = "Mask Url")] 
        [Tooltip("URL of mask image. Black areas remain unchanged, white areas will be replaced. Supports JPG/JPEG formats")]
        public string maskUrl;
        
        [Input(name = "Prompt")] 
        [Tooltip("Description of the background content to replace with")]
        public string prompt;
        
        // [Input(name = "Version")] 
        // [Tooltip("Model version")]
        // public string version;
        
        public override string name => LocalizationManager.Instance.GetLocalizedText("Background Replacement(Hunyuan)");
        
        public override string description => DescriptionConstants.HyBackgroundReplacementNode;

        public override IEnumerator ProcessAsync()
        {
            // Part 2: 检查输入是否为空
            if (image == null && string.IsNullOrEmpty(imageUrl))
                throw new ArgumentException("Either Image or Image Url must be provided");
                
            if (mask == null && string.IsNullOrEmpty(maskUrl))
                throw new ArgumentException("Either Mask or Mask Url must be provided");
                
            if (string.IsNullOrEmpty(prompt))
                throw new ArgumentException("Prompt is required", nameof(prompt));
            
            // Part 3：生成对应的request并调用rest call
            var request = new HyBackgroundReplacementRequest()
            {
                // version = version,
                prompt = prompt,
                n = 1
            };
            
            if (image != null)
                request.image = image.ToBase64();
            else if (!string.IsNullOrEmpty(imageUrl))
                request.imageUrl = imageUrl;
                
            if (mask != null)
                request.mask = mask.ToBase64();
            else if (!string.IsNullOrEmpty(maskUrl))
                request.maskUrl = maskUrl;
            
            var restCall = new HyBackgroundReplacementRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
    }
}