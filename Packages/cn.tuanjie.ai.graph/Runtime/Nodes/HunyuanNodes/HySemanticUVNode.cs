using System;
using System.Collections;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hunyuan/Semantic UV(Hunyuan)")]
    [UseProcessAsync]
    public class HySemanticUVNode : BaseHyModelNode
    {
  
        [Input(name = "Model Url")] public HyModelOutput inputModelUrl;

        [Input(name = "Enable Auto Smoothing"), ShowAsDrawer] public bool enableAutoSmoothing = true;

        public override string name => LocalizationManager.Instance.GetLocalizedText("Semantic UV(Hunyuan)");
        
        public override string description => DescriptionConstants.HySemanticUVNode;

        public override IEnumerator ProcessAsync()
        {
            if (inputModelUrl.IsNullOrEmpty())
                throw new ArgumentException("Input model url is null", nameof(inputModelUrl));
            var request = new HySemanticUVRequest()
            {
                n = 1, enableAutoSmoothing = enableAutoSmoothing
            };
            if (!string.IsNullOrEmpty(inputModelUrl.fbx_url))
                request.fbxUrl = inputModelUrl.fbx_url;
            else if (!string.IsNullOrEmpty(inputModelUrl.obj_url))
                request.objUrl = inputModelUrl.obj_url;
            else if (!string.IsNullOrEmpty(inputModelUrl.glb_url))
                request.glbUrl = inputModelUrl.glb_url;
            else
                throw new ArgumentException("Only fbx, glb and obj model are supported", nameof(inputModelUrl));
            var restCall = new HySemanticUVRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
    }
}

