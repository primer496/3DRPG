using System;
using System.Collections;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hunyuan/Generating Character Three View(Hunyuan)")]
    [UseProcessAsync]
    public class HyImageThreeViewNode : BaseHyImageNode
    {
        [Input(name = "Image")] public Texture2D image;
        
        public override string name => LocalizationManager.Instance.GetLocalizedText("GenThreeView(Hunyuan)");
        
        public override string description => DescriptionConstants.HyImageThreeViewNode;

        public override IEnumerator ProcessAsync()
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            var request = new HyImageCharacterThreeViewRequest()
            {
                image = image.ToBase64(),
            };
            var restCall = new HyImageCharacterThreeViewRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
    }
}

