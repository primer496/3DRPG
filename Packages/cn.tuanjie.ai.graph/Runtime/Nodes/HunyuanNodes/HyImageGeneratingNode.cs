using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hunyuan/Image Generating(Hunyuan)")]
    [UseProcessAsync]
    public class HyImageGeneratingNode : BaseHyImageNode
    {
        [Input(name = "Prompt")] public string prompt;
        [Input(name = "Image")] public Texture2D image;
        [Input(name = "Revise"), ShowAsDrawer] 
        [Tooltip("Enable automatic prompt optimization")]
        public bool revise = true;

        [Input(name = "Seed"), ShowAsDrawer] public int seed = 7758;

        [Input(name = "Ignore Style For Irag"), ShowAsDrawer]
        [Tooltip("When set to true, ignore the influence of style on intent distribution.")]
        public bool ignoreStyleForIrag = false;
        
        [Output(name = "Revised Prompt")] public string revisedPrompt;

        [HideInInspector] public string size = "1024x1024";
        [HideInInspector] public string style;

        public override string name => LocalizationManager.Instance.GetLocalizedText("ImageGenerating(Hunyuan)");
        
        public override string description => DescriptionConstants.HyImageGeneratingNode;

        public override void UpdateOutputPorts()
        {
            base.UpdateOutputPorts();
            revisedPrompt = artifact.m_ReceivedData.revisedPrompt;
        }

        public override IEnumerator ProcessAsync()
        {
            if (image == null && string.IsNullOrEmpty(prompt))
                throw new NullReferenceException("image and prompt should not be null at the same time");
            var request = new HyImageGeneratingRequest()
            {
                prompt = prompt,
                ignoreStyleForIrag = ignoreStyleForIrag,
                n = 1, revise = revise, seed = seed, size = size, style = style
            };
            if (image != null)
                request.image = image.ToBase64();
            var restCall = new HyImageGeneratingRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>
            {
                $"Prompt: {prompt}", $"Seed: {seed}", $"Ignore Style For Irag: {ignoreStyleForIrag}",
                $"Size: {size}", $"Style: {style}", $"Revised Prompt: {revisedPrompt}"
            };
            return true;
        }
    }
}

