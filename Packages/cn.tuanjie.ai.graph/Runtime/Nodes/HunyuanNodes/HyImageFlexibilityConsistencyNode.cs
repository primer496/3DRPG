using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hunyuan/Character Image Flexible Editing(Hunyuan)")]
    [UseProcessAsync]
    public class HyImageFlexibilityConsistencyNode : BaseHyImageNode
    {
        [Input(name = "Prompt")] public string prompt;
        [Input(name = "Image")] public Texture2D image;
        [Input(name = "Seed"), ShowAsDrawer] public int seed = 7758;

        [HideInInspector] [Range(768, 1408)] public int width = 1024;
        [HideInInspector] [Range(768, 1408)] public int height = 1024;
        [HideInInspector] public string size = "1024x1024";

        public override string name => LocalizationManager.Instance.GetLocalizedText("CharacterEditing(Hunyuan)");

        public override string description => DescriptionConstants.HyImageFlexibilityConsistencynNode;

        public override IEnumerator ProcessAsync()
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            var w8 = Math.Clamp(width, 768, 1408) >> 3 << 3;
            var h8 = Math.Clamp(height, 768, 1408) >> 3 << 3;
            var request = new HyImageFlexibilityConsistencyRequest()
            {
                image = image.ToBase64(),
                prompt = prompt, size = $"{w8}x{h8}", n = 1, seed = seed
            };
            var restCall = new HyImageFlexibilityConsistencyRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>
            {
                $"Prompt: {prompt}", $"Seed: {seed}", $"Size: {size}"
            };
            return true;
        }
    }
}