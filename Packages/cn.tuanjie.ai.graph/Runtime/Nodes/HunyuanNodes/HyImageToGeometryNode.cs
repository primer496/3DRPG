using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hunyuan/Image To Model(Hunyuan)")]
    [UseProcessAsync]
    public class HyImageToGeometryNode : BaseHyModelNode
    {
        [Input(name = "Image"), Tooltip("must be in JPEG/JPG/PNG format, resolution > 128x128, size < 10MB")]
        public Texture2D image;
        [HideInInspector]
        public int faceCount = 1000;
        [Input(name = "Enable PBR"), ShowAsDrawer] public bool enablePBR = false;
        [Input(name = "Strict Face Count"), ShowAsDrawer]
        [Tooltip("Enforces generation strictly according to the target polygon count. When disabled, the system automatically adjusts polygon count based on geometric error tolerance.")]
        public bool strictFaceCount = false;
        [Input(name = "Generate Quadrilateral Model"), ShowAsDrawer]
        [Tooltip("Generate quadrilateral model instead of triangle model.")]
        public bool quadModel = false;

        public override string name => LocalizationManager.Instance.GetLocalizedText("ImageToModel(Hunyuan)");
        
        public override string description => DescriptionConstants.HyImageToGeometryNode;

        public override IEnumerator ProcessAsync()
        {
            if (image == null)
                throw new ArgumentNullException("image");
            var request = new HyImageToGeometryRequest()
            {
                n = 1, faceCount = faceCount, strictMode = strictFaceCount, enablePbr = enablePBR, 
                polygonType = quadModel ? "quadrilateral" : "triangle"
            };
            if (image != null)
                request.image = image.ToBase64();

            var restCall = new HyImageToGeometryRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>
            {
                $"Face Count: {faceCount}",
                $"Strict Face Count: {strictFaceCount}", $"Enable PBR: {enablePBR}"
            };
            return true;
        }
    }
}

