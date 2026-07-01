using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hunyuan/Text To Model(Hunyuan)")]
    [UseProcessAsync]
    public class HyTextToGeometryNode : BaseHyModelNode
    {
        [Input(name = "Prompt")] public string prompt;
        [Input(name = "Enable PBR"), ShowAsDrawer] public bool enablePBR = false;
        [Input(name = "Strict Face Count"), ShowAsDrawer]
        [Tooltip("Enforces generation strictly according to the target polygon count. When disabled, the system automatically adjusts polygon count based on geometric error tolerance.")]
        public bool strictFaceCount = false;
        [Input(name = "Generate Quadrilateral Model"), ShowAsDrawer]
        [Tooltip("Generate quadrilateral model instead of triangle model.")]
        public bool quadModel = false;

        [HideInInspector] public int faceCount = 1000;
        
        public override string name => LocalizationManager.Instance.GetLocalizedText("TextToModel(Hunyuan)");
        
        public override string description => DescriptionConstants.HyTextToGeometryNode;

        public override IEnumerator ProcessAsync()
        {
            if (string.IsNullOrEmpty(prompt))
                throw new ArgumentException("Empty prompt is invalid", nameof(prompt));
            var request = new HyImageToGeometryRequest()
            {
                prompt = prompt, n = 1,
                faceCount = faceCount, strictMode = strictFaceCount, enablePbr = enablePBR,
                polygonType = quadModel ? "quadrilateral" : "triangle"
            };

            var restCall = new HyTextToGeometryRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>
            {
                $"Prompt: {prompt}", $"Face Count: {faceCount}",
                $"Strict Face Count: {strictFaceCount}", $"Enable PBR: {enablePBR}"
            };
            return true;
        }
    }
}

