using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [Obsolete("No need for format conversion")]
    // [System.Serializable, NodeMenuItem("Hunyuan/Format Conversion(Hunyuan)")]
    [UseProcessAsync]
    public class HyFormatConversionNode : BaseHyModelNode
    {
        [Input(name = "Model Url")] public HyModelOutput inputModelUrl;

        [HideInInspector] public string rspFormat = "stl";

        [HideInInspector] public List<string> rspFormatChoices = new()
        {
            "stl", "usdz", "fbx"
        };

        public override string name => LocalizationManager.Instance.GetLocalizedText("FormatConversion(Hunyuan)");

        public override string description => DescriptionConstants.HyFormatConversionNode;

        public override IEnumerator ProcessAsync()
        {
            if (inputModelUrl.IsNullOrEmpty())
                throw new ArgumentException("Input model url is null", nameof(inputModelUrl));
            var request = new Hy3DFormatConversionRequest()
            {
                responseFormat = rspFormat
            };
            var modelType = inputModelUrl.GetType();
            if (!string.IsNullOrEmpty(inputModelUrl.fbx_url))
                request.fbxUrl = inputModelUrl.fbx_url;
            else if (!string.IsNullOrEmpty(inputModelUrl.obj_zip_url))
                request.objZipUrl = inputModelUrl.obj_zip_url;
            else
                throw new ArgumentException("Only .fbx and .obj model are supported");
            var restCall = new Hy3DFormatConversionRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
    }
}