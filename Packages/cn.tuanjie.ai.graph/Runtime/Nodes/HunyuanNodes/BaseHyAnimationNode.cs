using System;
using System.Collections;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [UseProcessAsync]
    public class BaseHyAnimationNode : TJAIBaseAssetNode
    {
        [Save(ReceivedDataType = typeof(HyMeshData))] [HideInInspector]
        protected GameObject m_Obj;
        
        [Preview, SerializeField, HideInInspector]
        [Output("Animation Clip")]
        protected AnimationClip m_Clip;

        public AnimationClip clip
        {
            get => m_Clip;
            set
            {
                if (m_Clip != value)
                {
                    m_Clip = value;
                    this?.NotifyFieldChanged("m_Clip");
                }
            }
        }

        protected override void Enable()
        {
            base.Enable();
            onCancelled += () => { taskID = null; };
            onError += s => { taskID = null; };
            taskCostTime = 3;
        }

        public override IEnumerator RestoreHistory(string Guid)
        {
            artifact.m_ReceivedData.assetPath = $"{GetResourceFolder()}/{Guid}/hunyuan_{Guid}";
            yield return base.RestoreHistory(Guid);
        }
        
        protected BaseArtifact<GameObject, HyMeshData> artifact => (BaseArtifact<GameObject, HyMeshData>)currentArtifact;

        [Output("Model Url")] public HyModelOutput outputModelUrl;

        public override bool needTrigger => true;
        public override bool isRenamable => true;
        protected const int serverIndex = 3;

        public override void UpdateOutputPorts()
        {
            outputModelUrl = artifact.m_ReceivedData.modelUrl;
            clip = ImportUtils.Import<AnimationClip>(artifact.m_ReceivedData.assetPath);
            m_Obj = ImportUtils.Import<GameObject>(artifact.m_ReceivedData.assetPath);
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

            var data = new HyMeshData
            {
                assetPath = $"{GetResourceFolder()}/{taskID}/hunyuan_{taskID}", 
                ID = taskID, progressCallback = UpdateStatus
            };

            var processor = new CoroutineProcessor<AnimationClip>();
            yield return processor.ProcessAsync(currentArtifact.ReadFromCache(data, serverIndex));
            processor.HandleException();

            if (status == NodeStatus.Init)
                yield break;
            UpdateOutputPorts();

            UpdateHistory();
            graph.tokenDataModel.UpdateToken(data.tokenRemaining);
            taskID = null;
        }
    }
}