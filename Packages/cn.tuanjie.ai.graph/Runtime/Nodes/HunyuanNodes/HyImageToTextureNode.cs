using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hunyuan/Image To Texture(Hunyuan)")]
    [UseProcessAsync]
    public class HyImageToTextureNode : BaseHyModelNode
    {
        [Input(name = "Image"), Tooltip("Resolution greater than 128*128, size less than 10MB")]
        public Texture2D image;
        [Input(name = "Model Url")] public HyModelOutput inputModelUrl;
        [Input(name = "Keep UV"), ShowAsDrawer] public bool keepUV = false;
        [Input(name = "Enable PBR"), ShowAsDrawer]
        [Tooltip("Output Lit Material if true, else output Unlit")]
        public bool enablePBR = false;
        
        public override string name => LocalizationManager.Instance.GetLocalizedText("ImageToTexture(Hunyuan)");
        
        public override string description => DescriptionConstants.HyImageToTextureNode;

        public override IEnumerator ProcessAsync()
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            if (inputModelUrl.IsNullOrEmpty())
                throw new ArgumentException("Input model url is null", nameof(inputModelUrl));
            var request = new HyImageToTextureRequest
            {
                keepUv = keepUV, n = 1, enablePbr = enablePBR
            };
            if (!string.IsNullOrEmpty(inputModelUrl.glb_url))
                request.glbUrl = inputModelUrl.glb_url;
            else if (!string.IsNullOrEmpty(inputModelUrl.obj_url))
                request.objUrl = inputModelUrl.obj_url;
            else
                throw new ArgumentException("Only .glb and .obj model are supported");
            request.image = image.ToBase64();
            var restCall = new HyImageToTextureRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>
            {
                $"Keep UV: {keepUV}", $"Enable PBR: {enablePBR}"
            };
            return true;
        }
    }
}

