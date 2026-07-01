using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;
using UnityEngine.Serialization;

namespace UnityEngine.AIGraph
{
    [Serializable, UseProcessAsync]
    public abstract class Rodin3DGenerationNode : BaseRodinModelNode
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
        
        [Input(name = "Use Original Alpha"), ShowAsDrawer] 
        [Tooltip("Use the original transparency channel of the images when processing.")]
        public bool useOriginalAlpha = false;
        
        [HideInInspector]
        public string conditionMode;
        
        [Input(name = "Seed"), ShowAsDrawer] 
        [Tooltip("Seed value for randomization in mesh generation (0-65535).")]
        public ushort seed = 9009;
        
        [HideInInspector]
        public string geometryFileFormat = "fbx";
        
        [HideInInspector]
        public string material;
        
        [HideInInspector]
        public string quality;

        [HideInInspector] public int defaultPolygonCount = 18000;
        [FormerlySerializedAs("minPolygonCount")] [HideInInspector]
        public int minTriPolygonCount = 500;
        [FormerlySerializedAs("maxPolygonCount")] [HideInInspector]
        public int maxTriPolygonCount = 200000;
        [HideInInspector] public int minQuadPolygonCount = 1000;
        [HideInInspector] public int maxQuadPolygonCount = 100000;

        [HideInInspector]
        public int qualityOverride;
        
        [HideInInspector]
        public string tier { get; protected set; }
        
        [Input(name = "TA Pose"), ShowAsDrawer] 
        [Tooltip("Generate human-like model in T/A Pose.")]
        public bool TAPose;
        
        [SerializeField] 
        [Tooltip("Control the maximum size of generated model [Width(Y), Height(Z), Length(X)].")]
        public Vector3 bboxCondition;
        
        [HideInInspector]
        public string meshMode = "Quad";
        
        [Input(name = "Mesh Simplify"), ShowAsDrawer] 
        [Tooltip("Simplify the generated models.")]
        public bool meshSimplify = true;
        
        [Input(name = "Mesh Smooth"), ShowAsDrawer] 
        [Tooltip("Smooth the generated models.")]
        public bool meshSmooth = true;
        
        [HideInInspector]
        public List<string> addons;

        public override string name => LocalizationManager.Instance.GetLocalizedText("3DGeneration(Rodin3D)");
        
        public override string description => "Generate 3D model with Rodin3D API";
        
        [CustomPortInput(nameof(images), new Type[] { typeof(List<Texture2D>), typeof(Texture2D) })]
        protected void PullInputImages(List<SerializableEdge> edges)
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
                    if (edge.passThroughBuffer is List<Texture2D> { Count: > 0 } textures)
                        images.AddRange(textures);
                }
            }
        }
        
        [CustomPortInput(nameof(imageUrls), new Type[] { typeof(List<string>), typeof(string) })]
        protected void PullInputImageUrls(List<SerializableEdge> edges)
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

        protected override void Enable()
        {
            if (qualityOverride == 0) qualityOverride = defaultPolygonCount;
            base.Enable();
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
                conditionMode = conditionMode,
                seed = seed,
                geometryFormat = geometryFileFormat,
                material = material,
                quality = quality,
                qualityOverride = qualityOverride,
                tier = tier,
                taPose = TAPose,
                meshMode = meshMode,
                meshSimplify = meshSimplify,
                meshSmooth = meshSmooth
            };
            if (imageUrls is { Count: > 0 })
                request.imageUrls = imageUrls;
            else if (images is { Count: > 0 })
                request.imageBase64List = images.Select(i => i.ToBase64()).ToList();
            if (bboxCondition is { x: > 0, y: > 0, z: > 0 })
                request.bboxCondition = new List<float>(3) { bboxCondition.x, bboxCondition.y, bboxCondition.z };
            
            var restCall = new Rodin3DGenerationRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
    }

    [Serializable, UseProcessAsync, NodeMenuItem("Hyper3D-Rodin/3D Generation-Regular(Rodin3D)")]
    public class Rodin3DGenerationRegularNode : Rodin3DGenerationNode
    {
        public override string name => LocalizationManager.Instance.GetLocalizedText("3DGeneration-Regular(Rodin3D)");
        protected override void Enable()
        {
            tier = "Regular";
            base.Enable();
        }
    }
    
    [Serializable, UseProcessAsync, NodeMenuItem("Hyper3D-Rodin/3D Generation-Detail(Rodin3D)")]
    public class Rodin3DGenerationDetailNode : Rodin3DGenerationNode
    {
        public override string name => LocalizationManager.Instance.GetLocalizedText("3DGeneration-Detail(Rodin3D)");
        protected override void Enable()
        {
            tier = "Detail";
            base.Enable();
        }
    }
    
    [Serializable, UseProcessAsync, NodeMenuItem("Hyper3D-Rodin/3D Generation-Smooth(Rodin3D)")]
    public class Rodin3DGenerationSmoothNode : Rodin3DGenerationNode
    {
        public override string name => LocalizationManager.Instance.GetLocalizedText("3DGeneration-Smooth(Rodin3D)");
        protected override void Enable()
        {
            tier = "Smooth";
            base.Enable();
        }
    }
    
    [Serializable, UseProcessAsync, NodeMenuItem("Hyper3D-Rodin/3D Generation-Advanced(Rodin3D)")]
    public class Rodin3DGenerationAdvancedNode : Rodin3DGenerationNode
    {
        public override string name => LocalizationManager.Instance.GetLocalizedText("3DGeneration-Advanced(Rodin3D)");
        protected override void Enable()
        {
            tier = "Gen-2";
            defaultPolygonCount = 18000;
            minTriPolygonCount = 500;
            maxTriPolygonCount = 1000000;
            minQuadPolygonCount = 1000;
            maxQuadPolygonCount = 200000;
            base.Enable();
        }
    }
}