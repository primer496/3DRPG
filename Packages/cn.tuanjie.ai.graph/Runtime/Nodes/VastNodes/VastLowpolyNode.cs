using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Tripo/Mesh Simplification(Tripo)")]
    [UseProcessAsync]
    public class VastLowpolyNode : BaseVastModelNode
    {
        [Input(name = "Model ID")] public VastTaskID inputModelID;
        [Input(name = "Face Limit"), ShowAsDrawer]
        [Tooltip("Limits the number of faces on the output model. Range[1000, 16000]" +
            "If enable quad mesh output, the number of faces after model imported could be more than facelimit since triangulation.")]
        public int faceLimit = 3000;
        [Input(name = "Quad"), ShowAsDrawer]
        [Tooltip("Determined if the final model generated in quad or triangle face")]
        public bool quad = false;
        [Input(name = "Bake"), ShowAsDrawer]
        [Tooltip("When set to true, the model will be baked when generation")]
        public bool bake = true;
        
        [HideInInspector]
        public string modelVersion = "P-v1.0-20250506";
        [HideInInspector]
        public List<string> partNames = new List<string>();
        
        public override string name => LocalizationManager.Instance.GetLocalizedText("MeshSimplification(Tripo)");
        
        public override string description => DescriptionConstants.VastLowpolyNode;

        public override IEnumerator ProcessAsync()
        {
            if (string.IsNullOrEmpty(inputModelID.id))
                throw new System.ArgumentNullException(nameof(inputModelID));
            var request = new VastLowpolyRequest()
            {
                bake = bake,
                faceLimit = faceLimit, modelVersion = modelVersion,
                originalModelTaskId = inputModelID.id, quad = quad, partNames = partNames
            };
            var restCall = new VastLowpolyRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>
            {
                $"Face Limit: {faceLimit}", $"Output Quad Mesh: {quad}", $"Bake Mesh: {bake}",
                $"Model Version: {modelVersion}"
            };
            return true;
        }

        protected override void Enable()
        {
            base.Enable();
            taskCostTime = 8;
        }
    }
}

