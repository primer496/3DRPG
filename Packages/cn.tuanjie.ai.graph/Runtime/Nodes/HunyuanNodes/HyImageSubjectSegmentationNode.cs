using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hunyuan/Remove Image Background(Hunyuan)")]
    [UseProcessAsync]
    public class HyImageSubjectSegmentationNode : BaseHyImageNode
    {
        [Input(name = "Image")] public Texture2D image;
        [Input(name = "Segmentation Threshold"), ShowAsDrawer] public float segmentationThreshold = 0.05f;

        [Output(name = "Background Mask")] public Texture2D maskImage;
        
        public override string name => LocalizationManager.Instance.GetLocalizedText("RemoveBackground(Hunyuan)");
        
        public override string description => DescriptionConstants.HyImageSubjectSegmentationNode;

        public override IEnumerator ProcessAsync()
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            var request = new HyImageSubjectSegmentationRequest()
            {
                image = image.ToBase64(),
                segmentationThreshold = segmentationThreshold
            };
            var restCall = new HyImageSubjectSegmentationRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
            // process mask
            if (outputTexture == null)
                yield break;
            if (maskImage == null || maskImage.width != outputTexture.width || 
                maskImage.height != outputTexture.height)
                maskImage = new Texture2D(outputTexture.width, outputTexture.height, TextureFormat.R8, false);
            maskImage = TextureUtils.ConvertAlphaToGrayscale(outputTexture, true);
        }

        public IEnumerator Generate() => ProcessAsync();
        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>
            {
                $"Segmentation Threshold: {segmentationThreshold}"
            };
            return true;
        }
    }
}

