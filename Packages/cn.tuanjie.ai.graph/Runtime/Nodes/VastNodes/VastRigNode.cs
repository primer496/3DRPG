using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Tripo/Rig Model(Tripo)")]
    [UseProcessAsync]
    public class VastRigNode : BaseVastModelNode
    {
        [Input(name = "Model ID"), SerializeField]
        [Tooltip(
            "The task_id of a previous task. Only the task IDs of the tasks below are supported:\ntext_to_model\nimage_to_model\nmultiview_to_model\ntexture_model\nrefine_model")]
        public VastTaskID inputModelID;

        // control container settings
        [HideInInspector] public string modelVersion = "v2.0-20250506";
        [HideInInspector] public string rigMethod = "mixamo";
        [HideInInspector] public string outputFormat = "fbx";

        public override string name => LocalizationManager.Instance.GetLocalizedText("RigModel(Tripo)");

        
        public override string description => DescriptionConstants.VastRigNode;

        public override IEnumerator ProcessAsync()
        {
            // pre rig check
            var preCheckReq = new VastPreRigCheckRequest()
            {
                originalModelTaskId = inputModelID.id
            };
            var preRigCheckRestCall = new VastPreRigCheckRestCall(serverConfig, serverIndex);
            yield return preRigCheckRestCall.MakeServerRequest(preCheckReq);

            if (!preRigCheckRestCall.Success)
                throw new Exception(
                    $"Failed to check rigging condition, error message: {preRigCheckRestCall.Result.message}");

            var jobStatusRestCall = new GetJobStatusRestCall<VastTextToModelRequest, VastPreRigCheckOutput>(
    serverConfig, serverIndex, preRigCheckRestCall.Result.taskId);
            var jobInfoRequest = new TaskStatusRequest();
            var preRigCheckProcessor = new CoroutineProcessor<VastPreRigCheckOutput>();
            yield return preRigCheckProcessor.ProcessAsync(
                BackendUtils.RetrieveFromBackendCommon(jobStatusRestCall, jobInfoRequest));
            if (!preRigCheckProcessor.Result.riggable)
                throw new Exception($"Current model is not suitable for rigging after checking");

            // rig
            var rigReq = new VastRigRequest()
            {
                modelVersion = modelVersion, originalModelTaskId = inputModelID.id, outFormat = outputFormat,
                rigType = preRigCheckProcessor.Result.rig_type, spec = rigMethod
            };
            var rigRestCall = new VastRigRestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(rigReq, rigRestCall);
        }

        public IEnumerator Generate() => ProcessAsync();
        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>
            {
                $"Model Version: {modelVersion}", $"Rig Method: {rigMethod}"
            };
            return true;
        }
    }
}