using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;
using UnityEngine.Serialization;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Tripo/Texture Model(Tripo)")]
    [UseProcessAsync]
    public class VastTextureModelNode : BaseVastModelNode
    {
        // --------------------- port param ---------------------
        [Input(name = "Model ID")]
        [Tooltip("The task_id of a original model. Only the task IDs of the tasks below are supported:\ntext_to_model\nimage_to_model\nmultiview_to_model")]
        public VastTaskID originalModelTaskId;
        [HideInInspector]
        // [Input(name = "Part Names")]
        [Tooltip("The list of part names referred from Mesh Segmentation, the default value will be all part names generated from segmentation.")]
        public List<string> partNames = new List<string>();
        [Input(name = "Texture Prompt/Image")] [Tooltip("Texture prompt settings.")]
        public TexturePrompt texturePrompt;      
        [Input(name = "Style Image")]
        [Tooltip("Allows you to provide a reference image to influence the artistic style of the generated model.")]
        public Texture2D styleImage;
        [Input(name = "Enable Pbr"), ShowAsDrawer]
        [Tooltip("A boolean option to enable pbr. The default value is true, set false to get a model without pbr.")]
        public bool pbr = true;  
        [Input(name = "Bake"), ShowAsDrawer]
        [Tooltip("When set to true, bakes the model’s textures, combining advanced material effects into the base textures. The default value is true.")]
        public bool bake = true;
        
        // --------------------- controls container param ---------------------
        [FormerlySerializedAs("texture")]
        [HideInInspector]
        [Tooltip("A boolean option to enable texturing. The default value is true, set false to only update the pbr texture with pbr=true.")]
        public bool enableTexturing = true;
        [HideInInspector]
        [Tooltip("This is the random seed for texture generation. Using the same seed will produce identical textures. This parameter is an integer and is randomly chosen if not set.")]
        public int textureSeed = 1027;
        [HideInInspector]
        [Tooltip("Determines the prioritization of texture alignment in the 3D model. The default value is original_image.\noriginal_image: Prioritizes visual fidelity to the source image. This option produces textures that closely resemble the original image but may result in minor 3D inconsistencies.\ngeometry: Prioritizes 3D structural accuracy. This option ensures better alignment with the model’s geometry but may cause slight deviations from the original image appearance.")]
        public string textureAlignment = "original_image";
        [HideInInspector]
        [Tooltip("This parameter controls the texture quality. detailed provides high-resolution textures, resulting in more refined and realistic representation of intricate parts. This option is ideal for models where fine details are crucial for visual fidelity. The default value is standard.")]
        public string textureQuality = "standard";

        [FormerlySerializedAs("compress")]
        [HideInInspector]
        [Tooltip("Specifies the compression type to apply to the texture. Available values are:\n\"\" None: No compression (default)\ngeometry: Applies geometry-based compression to optimize the output, By default we use meshopt compression")]
        public string compressionType = "";
        public readonly List<string> modelVersionChoices = new() { "v2.5-20250123", "v2.0-20240919" };
        [HideInInspector]
        [Tooltip("Specifies the model version to use for texture generation.")]
        public string modelVersion = "v2.5-20250123";


        public override string name => LocalizationManager.Instance.GetLocalizedText("TextureModel(Tripo)");

        
        public override string description => DescriptionConstants.VastTextureModelNode;
        
        [CustomPortInput(nameof(texturePrompt), new Type[] { typeof(List<SDPrompt>), typeof(SDPrompt), typeof(string), typeof(Texture2D) })]
        void PullInputPrompt(List<SerializableEdge> edges)
        {
            if (edges.Count == 0) return;
            var e = edges.First();

            if (e.passThroughBuffer == null)
                return;

            if (typeof(SDPrompt).IsAssignableFrom(e.passThroughBuffer.GetType()))
            {
                texturePrompt.text = (e.passThroughBuffer as SDPrompt)?.prompt;
                texturePrompt.image = string.Empty;
            }
            else if (typeof(List<SDPrompt>).IsAssignableFrom(e.passThroughBuffer.GetType()))
            {
                var prompts = e.passThroughBuffer as List<SDPrompt>;
                if (prompts.Count == 0) return;
                texturePrompt.text = prompts.First().prompt;
                texturePrompt.image = string.Empty;
            }
            else if (typeof(string).IsAssignableFrom(e.passThroughBuffer.GetType()))
            {
                texturePrompt.text = (string)e.passThroughBuffer;
                texturePrompt.image = string.Empty;
            } else if (typeof(Texture2D).IsAssignableFrom(e.passThroughBuffer.GetType()))
            {
                var tex = e.passThroughBuffer as Texture2D;
                texturePrompt.image = tex.ToBase64();
                texturePrompt.text = string.Empty;
            }
        }

        public override IEnumerator ProcessAsync()
        { 
            if (string.IsNullOrEmpty(texturePrompt.text) && string.IsNullOrEmpty(texturePrompt.image))
                throw new NullReferenceException("Please provide a valid texture prompt or image.");
            if (styleImage != null)
                texturePrompt.styleImage = styleImage.ToBase64();
            var request = new VastTextureModelRequest()
            {
                bake = bake, compress = compressionType, modelVersion = modelVersion, originalModelTaskId = originalModelTaskId.id,
                partNames = partNames, pbr = pbr, texture = enableTexturing, textureAlignment = textureAlignment,
                texturePrompt = texturePrompt, textureQuality = textureQuality, textureSeed = textureSeed
            };
            var restCall = new VastTextureModelRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>
            {
                $"Texture Prompt: {texturePrompt.text}", $"Enable PBR: {pbr}",
                $"Bake Mesh: {bake}", $"Model Version: {modelVersion}",
                $"Compression Type: {compressionType}", $"Enable Texturing: {enableTexturing}",
                $"Texture Quality: {textureQuality}", $"Texture Seed: {textureSeed}",
                $"Texture Alignment: {textureAlignment}"
            };
            return true;
        }
    }
}