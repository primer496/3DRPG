using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Tripo/Stylize Model(Tripo)")]
    [UseProcessAsync]
    public class VastStylizeModelNode : BaseVastModelNode
    {
        [Input(name = "Model ID")] [Tooltip("The task_id of a previous task.")]
        public VastTaskID inputModelID;

        // control container settings
        [HideInInspector] public List<string> modelStyleChoices = new() { "lego", "voxel", "voronoi", "minecraft" };
        [HideInInspector] public string modelStyle = "lego";

        [HideInInspector]
        [Tooltip("Specify the grid size. Should be ranged from 32 to 128, and the default value is 80. Currently only for minecraft")]
        public int blockSize = 80;

        public override string name => LocalizationManager.Instance.GetLocalizedText("StylizeModel(Tripo)");
        
        public override string description => DescriptionConstants.VastStylizeModelNode;

        public override IEnumerator ProcessAsync()
        {
            var request = new VastStylizeModelRequest()
            {
                blockSize = Math.Clamp(blockSize, 32, 128), originalModelTaskId = inputModelID.id,
                style = modelStyle
            };
            var restCall = new VastStylizeModelRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }

        public IEnumerator Generate() => ProcessAsync();
        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>
            {
                $"Model Style: {modelStyle}", $"Block Size: {blockSize}"
            };
            return true;
        }
    }
}