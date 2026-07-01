using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hunyuan/Image Controlnet Gray Scale(Hunyuan)")]
    [UseProcessAsync]
    public class HyImageControlnetGrayScaleNode : BaseHyImageNode
    {
        [Input(name = "Prompt")] public string prompt;
        [Input(name = "Image")] public Texture2D image;
        [Input(name = "Seed"), ShowAsDrawer] public int seed = 7758;

        public override string name => LocalizationManager.Instance.GetLocalizedText("ControlnetGrayScale(Hunyuan)");
        
        public override string description => DescriptionConstants.HyImageControlnetGrayScaleNode;

        public override IEnumerator ProcessAsync()
        {
            if (string.IsNullOrEmpty(prompt))
                throw new ArgumentNullException(nameof(prompt));
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            var request = new HyImageControlnetGrayScaleRequest
            {
                prompt = prompt, seed = seed,
                image = image.ToBase64()
            };
            var restCall = new HyImageControlnetGrayScaleRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>
            {
                $"Prompt: {prompt}", $"Seed: {seed}"
            };
            return true;
        }
    }
}

