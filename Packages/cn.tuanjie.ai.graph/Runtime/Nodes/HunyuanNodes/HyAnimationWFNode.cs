using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;
using ArgumentException = System.ArgumentException;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hunyuan/Animation(Hunyuan)")]
    [UseProcessAsync]
    public class HyAnimationWFNode : BaseHyAnimationNode
    {
        [Input(name = "Model Url")] public HyModelOutput inputModelUrl;
        
        [HideInInspector] public string motionType = "Stride";
        public Dictionary<string, int> motionTypeMap = new()
        {
            { "Stride", 9 }, { "Fall", 10 }, { "Jump", 11 }, { "Kick", 12 }, { "Swing", 13 },
            { "Walk", 14 }, { "Run", 15 }, { "Dance", 16 }
        };
        public override string name => LocalizationManager.Instance.GetLocalizedText("Animation(Hunyuan)");
        
        public override string description => DescriptionConstants.HyAnimationNode;

        public override IEnumerator ProcessAsync()
        {
            if (inputModelUrl.IsNullOrEmpty())
                throw new ArgumentException("Input model url is null", nameof(inputModelUrl));
            var motionTypeInt = motionTypeMap.GetValueOrDefault(motionType, 9);
            var request = new Hy3DAnimationWFRequest
            {
                motionType = motionTypeInt
            };
            if (!string.IsNullOrEmpty(inputModelUrl.fbx_url))
                request.fbxUrl = inputModelUrl.fbx_url;
            else if (!string.IsNullOrEmpty(inputModelUrl.obj_url))
            {
                request.objUrl = inputModelUrl.obj_url;
                if (string.IsNullOrEmpty(inputModelUrl.mtl_url))
                    throw new ArgumentException("Input mtl url is null in obj model", nameof(inputModelUrl.mtl_url));
                request.mtlUrl = inputModelUrl.mtl_url;
                if (string.IsNullOrEmpty(inputModelUrl.texture_image_url) && 
                    string.IsNullOrEmpty(inputModelUrl.pbr_image_url))
                    throw new ArgumentException("Input texture image url is null", nameof(inputModelUrl.texture_image_url));
                request.textureImageUrl = !string.IsNullOrEmpty(inputModelUrl.texture_image_url)
                    ? inputModelUrl.texture_image_url
                    : inputModelUrl.pbr_image_url;
            }
            else
                throw new ArgumentException("Only fbx and obj models are supported", nameof(inputModelUrl));
            var restCall = new Hy3DAnimationWFRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();

        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>(1) { "Motion: " + motionType };

            return true;
        }
    }
}

