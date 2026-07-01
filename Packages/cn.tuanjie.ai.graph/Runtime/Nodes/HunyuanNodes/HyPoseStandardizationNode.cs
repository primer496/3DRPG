using System;
using System.Collections;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hunyuan/Pose Standardization(Hunyuan)")]
    [UseProcessAsync]
    public class HyPoseStandardizationNode : BaseHyImageNode
    {

        [Input(name = "Image")] [Tooltip("Character pose image with background removed")]
        // [Tooltip("去过背景后的人物姿态图片")]
        public Texture2D image;
        
        public override string name => LocalizationManager.Instance.GetLocalizedText("PoseStandardization(Hunyuan)");
        public override string description => DescriptionConstants.HyPoseStandardizationNode;

        public override IEnumerator ProcessAsync()
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            var request = new HyImagePoseStandardizationRequest()
            {
                image = image.ToBase64(),
                n = 1
            };
            var restCall = new HyImagePoseStandardizationRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
    }
}

