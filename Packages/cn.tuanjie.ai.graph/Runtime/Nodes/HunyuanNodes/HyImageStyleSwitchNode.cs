using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hunyuan/Image Style Switch(Hunyuan)")]
    [UseProcessAsync]
    public class HyImageStyleSwitchNode : BaseHyImageNode
    {
        [Input(name = "Image")] public Texture2D image;
        [Input(name = "Seed"), ShowAsDrawer] public int seed = 7758;
        
        [HideInInspector] public string style;
        public readonly List<string> styleChoices = new()
        {
            "去旅行风格", "像素风格", "清新日漫风格", "纯真动漫风格", "水彩风格"
        };
        
        public override string name => LocalizationManager.Instance.GetLocalizedText("StyleSwitch(Hunyuan)");
        
        public override string description => DescriptionConstants.HyImageStyleSwitchNode;

        public override IEnumerator ProcessAsync()
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            var request = new HyImageStyleSwitchRequest
            {
                style = style, n = 1, seed = seed,
                image = image.ToBase64()
            };
            var restCall = new HyImageStyleSwitchRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>
            {
                $"Seed: {seed}", $"Style: {style}"
            };
            return true;
        }
    }
}