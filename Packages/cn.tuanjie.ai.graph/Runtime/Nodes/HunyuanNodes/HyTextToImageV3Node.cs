using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hunyuan/Advanced Text to Image(Hunyuan)")]
    [UseProcessAsync]
    public class HyTexttoImageV3Node : BaseHyImageNode
    {
        [Input(name = "Prompt"), Tooltip("字符串长度不超过8192")] public string prompt;
        [Input(name = "Revise"), ShowAsDrawer]
        [Tooltip("Enable automatic prompt optimization")]
        public bool revise = true;
        [Input(name = "Enable Thinking"), ShowAsDrawer]
        [Tooltip("改写是否开启thinking模式, 开启thinking改写和生图效果会提升，但耗时会增加，最大到60s。")]
        public bool enableThinking = true;

        [Input(name = "Seed"), ShowAsDrawer] public int seed = 0;

        [Output(name = "Revised Prompt")] public string revisedPrompt;

        [HideInInspector] public string size = "1024x1024";

        public override string name => LocalizationManager.Instance.GetLocalizedText("Advanced Text To Image(Hunyuan)");

        public override string description => DescriptionConstants.HyImageGeneratingNode;

        public override void UpdateOutputPorts()
        {
            base.UpdateOutputPorts();
            revisedPrompt = artifact.m_ReceivedData.revisedPrompt;
        }

        public override IEnumerator ProcessAsync()
        {
            if (string.IsNullOrEmpty(prompt))
                throw new NullReferenceException("prompt should not be null or empty at the same time");
            var request = new HyTexttoImageV3Request()
            {
                prompt = prompt,
                revise = revise,
                seed = seed,
                size = size,
                enableThinking = enableThinking
            };

            var restCall = new HyTexttoImageV3RestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>
            {
                $"Prompt: {prompt}", $"Seed: {seed}",
                $"Size: {size}", $"Enable Thinking: {enableThinking}", $"Revised Prompt: {revisedPrompt}"
            };
            return true;
        }
    }
}

