using System;
using System.Collections;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hunyuan/Auto Rigging(Hunyuan)")]
    [UseProcessAsync]
    public class HyAutoRiggingNode : BaseHyModelNode
    {
        [Input(name = "Model Url")] public HyModelOutput inputModelUrl;
        
        public override string name => LocalizationManager.Instance.GetLocalizedText("AutoRigging(Hunyuan)");
        
        public override string description => DescriptionConstants.HyAutoRiggingNode;

        public override IEnumerator ProcessAsync()
        {
            if (inputModelUrl.IsNullOrEmpty())
                throw new ArgumentException("Input model url is null", nameof(inputModelUrl));
            var request = new HyAutoRiggingRequest()
            {
                n = 1
            };
            if (!string.IsNullOrEmpty(inputModelUrl.fbx_url))
                request.fbxUrl = inputModelUrl.fbx_url;
            else if (!string.IsNullOrEmpty(inputModelUrl.glb_url))
                request.glbUrl = inputModelUrl.glb_url;
            else if (!string.IsNullOrEmpty(inputModelUrl.obj_url))
            {
                request.objUrl = inputModelUrl.obj_url;
                request.mtlUrl = inputModelUrl.mtl_url;
                request.textureImageUrl = inputModelUrl.texture_image_url;
                request.pbrImageUrl = inputModelUrl.pbr_image_url;
                request.pbrMetallicImageUrl = inputModelUrl.pbr_metallic_image_url;
                request.pbrNormalImageUrl = inputModelUrl.pbr_normal_image_url;
                request.pbrRoughnessImageUrl = inputModelUrl.pbr_roughness_image_url;
            }
            var restCall = new HyAutoRiggingRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
    }
}

