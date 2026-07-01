using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hyper3D-Rodin/Texture Model(Rodin3D)")]
    [UseProcessAsync]
    public class RodinTextureNode : BaseRodinModelNode
    {
        [Input(name = "Image")] 
        [Tooltip("One binary image file to serve as texture references.")]
        public Texture2D image;
        
        [Input(name = "Prompt")] 
        [Tooltip("A texture description to guide texture generation.")]
        public string prompt;
        
        [Input(name = "Model Url")] 
        [Tooltip("One binary 3D model file to process. Maximum file size: 10MB")]
        public string modelUrl;
        
        [Input(name = "Seed")] 
        [Tooltip("Seed value for randomization in texture generation (0-65535).")]
        public ushort seed = 7766;
        
        [Input(name = "Reference Scale")] 
        [Tooltip("Reference scale of texture generation process.")]
        public float referenceScale;

        [Input(name = "EScore"), ShowAsDrawer] public float eScore = 3.5f;
        
        [HideInInspector]
        public string geometryFileFormat;
        
        [HideInInspector]
        public string material;
        
        [HideInInspector]
        public string resolution;

        public override string name => LocalizationManager.Instance.GetLocalizedText("Texture Model(Rodin3D)");
        
        public override string description => "Generate texture for 3D model using Rodin API";
        
        [CustomPortInput(nameof(modelUrl), new Type[] { typeof(RodinModelOutput), typeof(string) })]
        private void PullInputModelUrl(List<SerializableEdge> edges)
        {
            if (edges == null || edges.Count == 0) return;
            var edge = edges[0];
            if (edge.passThroughBuffer == null)
                return;
            var edgeType = edge.passThroughBuffer.GetType();
            if (typeof(string).IsAssignableFrom(edgeType))
                modelUrl = edge.passThroughBuffer as string;
            else if (typeof(RodinModelOutput).IsAssignableFrom(edgeType))
                modelUrl = (edge.passThroughBuffer as RodinModelOutput? ?? default).base_basic_pbr;
        }

        public override IEnumerator ProcessAsync()
        {
            if (image == null)
                throw new ArgumentException("Image is required", nameof(image));

            if (string.IsNullOrEmpty(modelUrl))
                throw new ArgumentException("Model is required", nameof(modelUrl));

            var request = new RodinTextureRequest()
            {
                imageBase64 = image.ToBase64(),
                prompt = prompt,
                modelUrl = modelUrl,
                seed = seed,
                referenceScale = referenceScale,
                geometryFileFormat = geometryFileFormat,
                material = material,
                resolution = resolution, escore = eScore
            };
            
            var restCall = new RodinTextureRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
    }
}