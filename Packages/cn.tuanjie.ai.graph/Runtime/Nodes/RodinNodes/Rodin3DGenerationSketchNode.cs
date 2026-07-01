using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [Serializable, UseProcessAsync, NodeMenuItem("Hyper3D-Rodin/3D Generation-Sketch(Rodin3D)")]
    public class Rodin3DGenerationSketchNode : BaseRodinModelNode
    {
        [Input(name = "Input Images", allowMultiple = true)]
        [Tooltip("Images to be used in generation, up to 5 images. First image will be used for material generation.")]
        public List<Texture2D> images;
        
        
        [Input(name = "Image Urls", allowMultiple = true)] 
        [Tooltip("Images to be used in generation, up to 5 images. First image will be used for material generation.")]
        public List<string> imageUrls;
        
        [Input(name = "Prompt")] 
        [Tooltip("A textual prompt to guide the model generation.")]
        public string prompt;
        
        [HideInInspector]
        public string geometryFileFormat = "fbx";
        
        [HideInInspector]
        public string material;
        
        [HideInInspector]
        public string tier => "Sketch";

        public override string name => LocalizationManager.Instance.GetLocalizedText("3DGeneration-Sketch(Rodin3D)");
        
        public override string description => "Generate 3D model with Rodin API";
        
        [CustomPortInput(nameof(images), new Type[] { typeof(List<Texture2D>), typeof(Texture2D) })]
        void PullInputImages(List<SerializableEdge> edges)
        {
            if (edges == null || edges.Count == 0) return;
            images ??= new List<Texture2D>();
            images.Clear();
            foreach (var edge in edges)
            {
                if (edge.passThroughBuffer == null)
                    continue;
                var edgeType = edge.passThroughBuffer.GetType();
                if (typeof(Texture2D).IsAssignableFrom(edgeType))
                    images.Add(edge.passThroughBuffer as Texture2D);
                else if (typeof(List<Texture2D>).IsAssignableFrom(edgeType))
                {
                    if (edge.passThroughBuffer is List<Texture2D> textures && textures.Count > 0)
                        images.AddRange(textures);
                }
            }
        }
        
        [CustomPortInput(nameof(imageUrls), new Type[] { typeof(List<string>), typeof(string) })]
        void PullInputImageUrls(List<SerializableEdge> edges)
        {
            if (edges == null || edges.Count == 0) return;
            imageUrls ??= new List<string>();
            imageUrls.Clear();
            foreach (var edge in edges)
            {
                if (edge.passThroughBuffer == null)
                    continue;
                var edgeType = edge.passThroughBuffer.GetType();
                if (typeof(string).IsAssignableFrom(edgeType))
                    imageUrls.Add(edge.passThroughBuffer as string);
                else if (typeof(List<string>).IsAssignableFrom(edgeType))
                {
                    if (edge.passThroughBuffer is List<string> urls && urls.Count > 0)
                        imageUrls.AddRange(urls);
                }
            }
        }

        public override IEnumerator ProcessAsync()
        {
            if ((images == null || images.Count == 0) && (imageUrls == null || imageUrls.Count == 0) &&
                string.IsNullOrEmpty(prompt))
                throw new ArgumentException("Either images or prompt must be provided");

            if (images is { Count: > 5 })
                throw new ArgumentException("Maximum 5 images allowed", nameof(images));
            if (imageUrls is { Count: > 5 })
                throw new ArgumentException("Maximum 5 images allowed", nameof(imageUrls));
            if (images?.Count > 0 && imageUrls?.Count > 0)
                Debug.LogWarning("Both image and image url are given, use image url by default");
            
            var request = new Rodin3DGenerationRequest()
            {
                prompt = prompt,
                geometryFormat = geometryFileFormat,
                material = material,
                tier = tier, qualityOverride = 3000
            };
            if (imageUrls is { Count: > 0 })
                request.imageUrls = imageUrls;
            else if (images is { Count: > 0 })
                request.imageBase64List = images.Select(i => i.ToBase64()).ToList();
   
            var restCall = new Rodin3DGenerationRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
    }
}