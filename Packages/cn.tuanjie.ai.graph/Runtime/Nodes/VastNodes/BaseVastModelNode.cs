using System;
using System.Collections;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

#if UNITY_EDITOR
#endif

namespace UnityEngine.AIGraph
{
    [UseProcessAsync]
    public abstract class BaseVastModelNode : TJAIBaseAssetNode
    {
        [Save(ReceivedDataType = typeof(VastMeshData))]
        [Preview, SerializeField, HideInInspector] [Output("Model")]
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
            taskCostTime = 5;
        }
        
        public override IEnumerator RestoreHistory(string Guid)
        {
            artifact.m_ReceivedData.assetPath = $"{GetResourceFolder()}/{Guid}/vast_{GetType().Name}_{Guid}";
            yield return base.RestoreHistory(Guid);
        }

        [Output("Model Url")] public VastTextToModelOutput outputModelUrl;
        [Output("Vast Task ID")] public VastTaskID outputModelID;
        protected BaseArtifact<GameObject, VastMeshData> artifact => (BaseArtifact<GameObject, VastMeshData>)currentArtifact;
        public override bool needTrigger => true;
        public override bool isRenamable => true;

        protected const int serverIndex = 3;

        public override void UpdateOutputPorts()
        {
            if (currentArtifact.GetCacheUnityObject() != null)
                obj = currentArtifact.GetCacheUnityObject() as GameObject;
            outputModelID.id = artifact.m_ReceivedData.vastTaskID;
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
                    throw new Exception($"Failed to generate artifact, task id: {response.taskId}, error message: {response.message}");
                taskID = response.taskId;
            }
            
            var data = new VastMeshData
            {
                assetPath = $"{GetResourceFolder()}/{taskID}/vast_{GetType().Name}_{taskID}",
                ID = taskID, progressCallback = UpdateStatus
            };

            var processor = new CoroutineProcessor<GameObject>();
            yield return processor.ProcessAsync(currentArtifact.ReadFromCache(data, serverIndex));
            processor.HandleException();

            if (status == NodeStatus.Init)
                yield break;
            obj = processor.Result;
            outputModelID.id = artifact.m_ReceivedData.vastTaskID;
            outputModelUrl = artifact.m_ReceivedData.modelUrl;

            UpdateHistory();
            graph.tokenDataModel.UpdateToken(data.tokenRemaining);
            taskID = null;
            yield return null;
        }
    }
}