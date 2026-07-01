using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hunyuan/Multiview To Texture(Hunyuan)")]
    [UseProcessAsync]
    public class HyViewsToTextureNode : BaseHyModelNode
    {
        [HideInInspector]
        public string model = "hunyuan-3d-views2texture";
        
        [Input(name = "Model Url")] public HyModelOutput inputModelUrl;
        
        [Input(name = "Front Image")] 
        [Tooltip("Front view image. Must be JPEG/JPG/PNG format, resolution > 128x128, size < 10MB.")]
        public Texture2D frontImage;
        
        [Input(name = "Back Image")] 
        [Tooltip("Back view image. Must be JPEG/JPG/PNG format, resolution > 128x128, size < 10MB.")]
        public Texture2D backImage;
        
        [Input(name = "Left Image")] 
        [Tooltip("Left view image. Must be JPEG/JPG/PNG format, resolution > 128x128, size < 10MB.")]
        public Texture2D leftImage;
        
        [Input(name = "Right Image")] 
        [Tooltip("Right view image. Must be JPEG/JPG/PNG format, resolution > 128x128, size < 10MB.")]
        public Texture2D rightImage;
        
        [Input(name = "Keep UV"), ShowAsDrawer] 
        [Tooltip("Whether to keep UV. Default is false.")]
        public bool keepUv = false;
        
        [Input(name = "Seed"), ShowAsDrawer] 
        [Tooltip("Generation seed, only effective when generating 1 image. Range [1, 4294967295].")]
        public int seed = 0;

        [Input(name = "Enable PBR"), ShowAsDrawer] public bool enablePBR = false;

        public override string name => LocalizationManager.Instance.GetLocalizedText("MultiviewToTexture(Hunyuan)");
        
        public override string description => "Generate texture from multiple views using Hunyuan API";

        public override IEnumerator ProcessAsync()
        {
            if (frontImage == null)
                throw new ArgumentNullException(nameof(frontImage));
            if (backImage == null && leftImage == null && rightImage == null)
                throw new ArgumentException("Back/Left/Right image should have at least one image", nameof(leftImage));
            if (inputModelUrl.IsNullOrEmpty())
                throw new ArgumentException("Input model url is null", nameof(inputModelUrl));
            
            var request = new HyViewsToTextureRequest()
            {
                frontImage = frontImage.ToBase64(),
                keepUv = keepUv, seed = seed,
                n = 1, enablePbr = enablePBR
            };
            if (backImage != null)
                request.backImage = backImage.ToBase64();
            if (leftImage != null)
                request.leftImage = leftImage.ToBase64();
            if (rightImage != null)
                request.rightImage = rightImage.ToBase64();
            
            if (!string.IsNullOrEmpty(inputModelUrl.glb_url))
                request.glbUrl = inputModelUrl.glb_url;
            else if (!string.IsNullOrEmpty(inputModelUrl.obj_url))
                request.objUrl = inputModelUrl.obj_url;
            else
                throw new ArgumentException("Only .glb and .obj model are supported");

            if (!enablePBR)
            {
                var restCall = new HyViewsToTextureRestCall(serverConfig, serverIndex);
                yield return GenerateRestCall(request, restCall);
            }
            else
            {
                var restCall = new HyViewsToTexturePBRRestCall(serverConfig, serverIndex);
                yield return GenerateRestCall(request, restCall);
            }
        }

        public IEnumerator Generate() => ProcessAsync();
        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>
            {
                $"Keep UV: {keepUv}", $"Seed: {seed}", $"Enable PBR: {enablePBR}"
            };
            return true;
        }
    }
}