using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Hunyuan/Lowpoly(Hunyuan)")]
    [UseProcessAsync]
    public class HyLowpolyNode : BaseHyModelNode
    {
        [Input(name = "Model Url")] public HyModelOutput inputModelUrl;

        [HideInInspector] public string polygonType = "triangle";

        [HideInInspector] public List<string> polygonTypeChoices = new()
        {
            "triangle", "quadrilateral"
        };

        public override string name => LocalizationManager.Instance.GetLocalizedText("Lowpoly(Hunyuan)");

        public override string description => DescriptionConstants.HyLowpolyNode;

        public override IEnumerator ProcessAsync()
        {
            if (inputModelUrl.IsNullOrEmpty())
                throw new ArgumentException("Input model url is null", nameof(inputModelUrl));
            var request = new HyLowPolyRequest()
            {
                polygonType = polygonType, n = 1
            };
            if (!string.IsNullOrEmpty(inputModelUrl.glb_url))
                request.glbUrl = inputModelUrl.glb_url;
            else if (!string.IsNullOrEmpty(inputModelUrl.obj_url))
                request.objUrl = inputModelUrl.obj_url;
            else
                throw new ArgumentException("Only .glb and .obj model are supported");
            var restCall = new HyLowPolyRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>
            {
                $"PolygonType: {polygonType}"
            };
            return true;
        }

        protected override void Enable()
        {
            base.Enable();
            taskCostTime = 5;
        }
    }
}