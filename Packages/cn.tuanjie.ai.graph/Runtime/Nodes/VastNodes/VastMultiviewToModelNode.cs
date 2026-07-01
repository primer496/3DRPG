using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Tripo/Multiview to Model(Tripo)")]
    [UseProcessAsync]
    public class VastMultiviewToModelNode : BaseVastModelNode
    {
        // [Input("Images", allowMultiple = true)] public List<Texture2D> inputImages = new();
        [Input(name = "Front Image")] public Texture2D frontImage;
        [Input(name = "Left Image")] public Texture2D leftImage;
        [Input(name = "Back Image")] public Texture2D backImage;
        [Input(name = "Right Image")] public Texture2D rightImage;
        
        [HideInInspector]
        public string modelVersion = "v2.5-20250123";
        // These are only valid for model_version>=v2.0-20240919
        [Input(name = "Face Limit"), ShowAsDrawer]
        [Tooltip("Limits the number of faces on the output model. If this option is not set, the face limit will be adaptively determined." +
            "If enable quad mesh output, the number of faces after model imported could be more than facelimit since triangulation.")]
        public int faceLimit = 3000;
        [HideInInspector]
        public bool enableTexturing = true;
        // texture-related parameters
        [HideInInspector]
        [Tooltip("This is the random seed for texture generation for model_version>=v2.0-20240919. Using the same seed will produce identical textures. This parameter is an integer and is randomly chosen if not set. If you want a model with different textures, please use same model_seed and different texture_seed.")]
        public int textureSeed = 4994;
        [HideInInspector]
        public string textureQuality = "standard";
        [HideInInspector] public string textureAlignment = "original_image";
        [HideInInspector] public string orientation = "default";
        
        [Input(name = "Enable Pbr"), ShowAsDrawer]
        public bool enablePbr = true;
        [Input(name = "Auto Size"), ShowAsDrawer]
        [Tooltip("Automatically scale the model to real-world dimensions, with the unit in meters.")]
        public bool autoSize = false;
        [HideInInspector]
        // [Input(name = "Output Quad Mesh"), ShowAsDrawer]
        [Tooltip("Set true to enable quad mesh output. If quad=true and face_limit is not set, the default face_limit will be 10000.\nNote: Enabling this option will force the output to be an FBX model.")]
        public bool enableQuadMesh = true;
        [HideInInspector]
        [Tooltip("Specifies the compression type to apply to the texture. Available values are:\n\"\" (empty string): No compression (default)\ngeometry: Applies geometry-based compression to optimize the output, By default we use meshopt compression .")]
        public string compressionType;
        [Input(name = "Smart Low Ploy"), ShowAsDrawer]
        [Tooltip("Generate low-poly meshes with hand‑crafted topology. Inputs with less complexity work best. There is a possibility of failure for complex models.")]
        public bool smartLowPloy = false;
        [HideInInspector]
        // [Input(name = "Generate Parts"), ShowAsDrawer]
        [Tooltip("Generate segmented 3D models and make each part editable. The default value is false.\nNote: generate_parts is not compatible with texture=true, if you want to generate parts, please set texture=false; generate_parts is not compatible with quad=true, if you want to generate parts, please set quad=false.")]
        public bool generateParts = false;
        

        public override string name => LocalizationManager.Instance.GetLocalizedText("MultiviewToModel(Tripo)");
        
        public override string description => DescriptionConstants.VastMultiviewToModelNode;
        
        // [CustomPortInput(nameof(inputImages), new Type[] { typeof(List<Texture2D>), typeof(Texture2D) })]
        // void PullInputImages(List<SerializableEdge> edges)
        // {
        //     if (edges == null || edges.Count == 0) return;
        //     if (inputImages == null)
        //         inputImages = new List<Texture2D>();
        //     else
        //         inputImages.Clear();
        //     foreach (var e in edges)
        //     {
        //         if (typeof(Texture2D).IsAssignableFrom(e.passThroughBuffer.GetType()))
        //         {
        //             inputImages.Add(e.passThroughBuffer as Texture2D);
        //         }
        //         else if (typeof(List<Texture2D>).IsAssignableFrom(e.passThroughBuffer.GetType()))
        //         {
        //             var textures = e.passThroughBuffer as List<Texture2D>;
        //             inputImages.AddRange(textures);
        //         }
        //     }
        // }

        public override IEnumerator ProcessAsync()
        {
            if (frontImage == null || leftImage == null || backImage == null || rightImage == null)
                throw new NullReferenceException("Front/Left/Back/Right should not be null.");
            var inputImages = new List<Texture2D> { frontImage, leftImage, backImage, rightImage };
            var request = new VastMultiviewToModelRequest()
            {
                autoSize = autoSize, compress = compressionType,
                faceLimit = faceLimit, generateParts = generateParts,
                files =  inputImages.Select(
                    img => new VastMultiviewToModelRequest.VastMultiviewToModelFileData()
                    {
                        imageBase64 = img.ToBase64(),
                    }).ToList(),
                modelVersion = modelVersion, orientation = orientation, pbr = enablePbr,
                quad = enableQuadMesh, smartLowPoly = smartLowPloy, texture = enableTexturing,
                textureQuality = textureQuality, textureSeed = textureSeed, textureAlignment = textureAlignment
            };
            var restCall = new VastMultiviewToModelRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>
            {
                $"Face Limit: {faceLimit}", $"Enable Pbr: {enablePbr}", $"Auto Size: {autoSize}",
                $"Smart Low Ploy: {smartLowPloy}", $"Output Quad Mesh: {enableQuadMesh}",
                $"Model Version: {modelVersion}", $"Compression Type: {compressionType}",
                $"Orientation: {orientation}", $"Enable Texturing: {enableTexturing}",
                $"Texture Quality: {textureQuality}", $"Texture Seed: {textureSeed}",
                $"Texture Alignment: {textureAlignment}"
            };
            return true;
        }
    }
}