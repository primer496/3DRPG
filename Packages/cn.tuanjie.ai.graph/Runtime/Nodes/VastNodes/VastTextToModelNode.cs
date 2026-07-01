using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Tripo/Text to Model(Tripo)")]
    [UseProcessAsync]
    public class VastTextToModelNode : BaseVastModelNode
    {
        [Input(name = "Prompt")]
        [Tooltip("Text input that directs the model generation, supports multiple languages. However, emojis and certain special Unicode characters are not supported")]
        public string prompt;
        [Input(name = "Negative Prompt")]
        [Tooltip("Unlike prompt, it provides a reverse direction to assist in generating content contrasting with the original prompt. The maximum length is 255 characters.")]
        public string negativePrompt = string.Empty;
        [HideInInspector]
        public string modelVersion = "v2.5-20250123";
        [Input(name = "Image Seed"), ShowAsDrawer]
        public int imageSeed = 400820;
        [Input(name = "Model Seed"), ShowAsDrawer]
        [Tooltip("This is the random seed for model generation. For model_version>=v2.0-20240919, the seed controls the geometry generation process, ensuring identical models when the same seed is used. This parameter is an integer and is randomly chosen if not set.")]
        public int modelSeed = 3378;
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
        public int textureSeed = 5851;
        [HideInInspector]
        public string textureQuality = "standard";
        
        [Input(name = "Enable Pbr"), ShowAsDrawer]
        public bool enablePbr = true;
        [Input(name = "Auto Size"), ShowAsDrawer]
        [Tooltip("Automatically scale the model to real-world dimensions, with the unit in meters.")]
        public bool autoSize = false;
        [HideInInspector]
        [Tooltip("Defines the artistic style or transformation to be applied to the 3D model, altering its appearance according to preset options. Omit this option to keep the original style and apperance.")]
        public string modelStyle;
        // force to set quad=true to generate fbx file
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
        
        public override string name => LocalizationManager.Instance.GetLocalizedText("TextToModel(Tripo)");
        
        public override string description => DescriptionConstants.VastTextToModelNode;

        [CustomPortInput(nameof(prompt), new Type[] { typeof(List<SDPrompt>), typeof(SDPrompt), typeof(string) })]
        void PullInputPrompt(List<SerializableEdge> edges)
        {
            if (edges.Count == 0) return;
            SerializableEdge e = edges.First();
            if (e.passThroughBuffer == null)
                return;
            if (typeof(SDPrompt).IsAssignableFrom(e.passThroughBuffer.GetType()))
            {
                prompt = (e.passThroughBuffer as SDPrompt)?.prompt;
            }
            else if (typeof(List<SDPrompt>).IsAssignableFrom(e.passThroughBuffer.GetType()))
            {
                var prompts = e.passThroughBuffer as List<SDPrompt>;
                if (prompts.Count == 0) return;
                prompt = prompts.First()?.prompt;
            }
            else if (typeof(string).IsAssignableFrom(e.passThroughBuffer.GetType()))
            {
                prompt = e.passThroughBuffer as string;
            }
        }

        public override IEnumerator ProcessAsync()
        {
            var request = new VastTextToModelRequest()
            {
                prompt = prompt, negativePrompt = negativePrompt, autoSize = autoSize,
                compress = compressionType, faceLimit = faceLimit, generateParts = generateParts,
                imageSeed = imageSeed, modelSeed = modelSeed, modelVersion = modelVersion,
                pbr = enablePbr, quad = enableQuadMesh, smartLowPoly = smartLowPloy, style = modelStyle == "None" ? string.Empty : modelStyle,
                texture = enableTexturing, textureQuality = textureQuality, textureSeed = textureSeed
            };
            var restCall = new VastTextToModelRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>
            {
                $"Prompt: {prompt}", $"Negative Prompt: {negativePrompt}",
                $"Image Seed: {imageSeed}", $"Model Seed: {modelSeed}",
                $"Face Limit: {faceLimit}", $"Auto Size: {autoSize}",
                $"Smart Low Ploy: {smartLowPloy}", $"Output Quad Mesh: {enableQuadMesh}",
                $"Model Version: {modelVersion}", $"Compression Type: {compressionType}",
                $"Model Style: {modelStyle}", $"Enable Texturing: {enableTexturing}",
                $"Texture Quality: {textureQuality}", $"Texture Seed: {textureSeed}"
            };
            return true;
        }
    }
}

