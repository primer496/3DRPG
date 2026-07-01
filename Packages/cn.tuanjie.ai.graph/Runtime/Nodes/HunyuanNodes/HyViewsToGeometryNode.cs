using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hunyuan/Multiview To Model(Hunyuan)")]
    [UseProcessAsync]
    public class HyViewsToGeometryNode : BaseHyModelNode
    {
        [Input(name = "Front Image"), Tooltip("Front view, must be in JPEG/JPG/PNG format, resolution > 128x128, size < 10MB")]
        public Texture2D frontImage;
        [Input(name = "Back Image")]
        [Tooltip("Back view, must be in JPEG/JPG/PNG format, resolution > 128x128, size < 10MB (at least one of Back/Left/Right view is required)")]
        public Texture2D backImage;
        [Input(name = "Left Image")]
        [Tooltip("Left view, must be in JPEG/JPG/PNG format, resolution > 128x128, size < 10MB (at least one of Back/Left/Right view is required)")]
        public Texture2D leftImage;
        [Input(name = "Right Image")]
        [Tooltip("Right view, must be in JPEG/JPG/PNG format, resolution > 128x128, size < 10MB (at least one of Back/Left/Right view is required)")]
        public Texture2D rightImage;
        [Input(name = "Seed"), ShowAsDrawer] public int seed = 0;
        
        [Input(name = "Strict Mode"), ShowAsDrawer]
        [Tooltip("Generate face count in a strict mode if true, else may adjust face count according to geometry")]
        public bool strictMode = false;
        [Input(name = "Generate Quadrilateral Model"), ShowAsDrawer]
        [Tooltip("Generate quadrilateral model instead of triangle model.")]
        public bool quadModel = false;

        [HideInInspector] public int faceCount = 1000;
        
        public override string name => LocalizationManager.Instance.GetLocalizedText("MultiviewToModel(Hunyuan)");
        
        public override string description => DescriptionConstants.HyViewsToGeometryNode;

        public override IEnumerator ProcessAsync()
        {
            if (frontImage == null)
                throw new NullReferenceException("Front image is null");
            if (backImage == null && leftImage == null && rightImage == null)
                throw new NullReferenceException("Back/Left/Right image should have at least one image");
            var request = new HyViewsToGeometryRequest()
            {
                seed = seed, n = 1,
                frontImage = frontImage.ToBase64(), faceCount = faceCount, 
                strictMode = strictMode, polygonType = quadModel ? "quadrilateral" : "triangle"
            };
            if (backImage != null)
                request.backImage = backImage.ToBase64();
            if (leftImage != null)
                request.leftImage = leftImage.ToBase64();
            if (rightImage != null)
                request.rightImage = rightImage.ToBase64();
            var restCall = new HyViewsToGeometryRestCall(serverConfig, serverIndex);            
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>
            {
                $"Seed: {seed}", $"Face Count: {faceCount}", $"Strict Mode: {strictMode}"
            };
            return true;
        }
    }
}

