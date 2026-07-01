using System;
using System.Collections;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

#if UNITY_EDITOR
#endif

namespace UnityEngine.AIGraph
{
    [UseProcessAsync]
    public abstract class BaseRodinModelNode : TJAIBaseAssetNode
    {
        [Save(ReceivedDataType = typeof(RodinMeshData))] [Preview, SerializeField, HideInInspector] [Output("Model")]
        protected GameObject m_Obj;
        public GameObject obj
        {
            get => m_Obj;
            set
            {
                if (m_Obj != value)
                {
                    m_Obj = value;
                    this?.NotifyFieldChanged("m_Obj");
                }
            }
        }

        protected override void Enable()
        {
            base.Enable();
            onCancelled += () => { taskID = null; };
            onError += s => { taskID = null; };
            taskCostTime = 8;
        }

        public override IEnumerator RestoreHistory(string Guid)
        {
            artifact.m_ReceivedData.assetPath = $"{GetResourceFolder()}/{Guid}/rodin_{this.GetType().Name}_{Guid}";
            yield return base.RestoreHistory(Guid);
        }

        protected BaseArtifact<GameObject, RodinMeshData> artifact => (BaseArtifact<GameObject, RodinMeshData>)currentArtifact;
        [Output("Model Url")] public RodinModelOutput outputModelUrl;
        public override bool needTrigger => true;
        public override bool isRenamable => true;

        protected const int serverIndex = 3;

        public override void UpdateOutputPorts()
        {
            if (currentArtifact.GetCacheUnityObject() != null)
                obj = currentArtifact.GetCacheUnityObject() as GameObject;
            outputModelUrl = artifact.m_ReceivedData.modelUrl;
        }


        internal IEnumerator GenerateRestCall<TReq, TRestCall>(TReq req, TRestCall restCall)
            where TRestCall : TJAIRestCall<TReq, TaskSubmitResponse>
        {
            if (string.IsNullOrEmpty(taskID))
            {
                yield return restCall.MakeServerRequest(req);
                var response = restCall.Result;
                if (!restCall.Success)
                    throw new Exception(
                        $"Failed to generate artifact, task id: {response.taskId}, error message: {response.message}");
                taskID = response.taskId;
            }

            var data = new RodinMeshData
            {
                assetPath = $"{GetResourceFolder()}/{taskID}/rodin_{this.GetType().Name}_{taskID}",
                ID = taskID, progressCallback = UpdateStatus
            };

            var processor = new CoroutineProcessor<GameObject>();
            yield return processor.ProcessAsync(currentArtifact.ReadFromCache(data, serverIndex));
            processor.HandleException();

            if (status == NodeStatus.Init)
                yield break;
            obj = processor.Result;
            outputModelUrl = artifact.m_ReceivedData.modelUrl;
            DebugUtils.ConditionLog($"{GetCustomName()} Output Model: {outputModelUrl}");

            UpdateHistory();
            graph.tokenDataModel.UpdateToken(data.tokenRemaining);
            taskID = null;
        }
    }
}