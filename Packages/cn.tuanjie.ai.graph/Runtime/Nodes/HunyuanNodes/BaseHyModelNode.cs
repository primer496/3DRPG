using System;
using System.Collections;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

#if UNITY_EDITOR
#endif

namespace UnityEngine.AIGraph
{
    [Serializable]
    public class HyModelUrl
    {
    }

    [Serializable]
    public class HyGlbModelUrl : HyModelUrl
    {
        public string glbUrl;
        public override string ToString()
        {
            return $"HyGlbModelUrl(glbUrl={glbUrl})";
        }
    }

    [Serializable]
    public class HyFBXModelUrl : HyModelUrl
    {
        public string fbxUrl;
        public override string ToString()
        {
            return $"HyFBXModelUrl(fbxUrl={fbxUrl})";
        }
    }

    [Serializable]
    public class HyObjModelUrl : HyModelUrl
    {
        public string objUrl;
        public string mtlUrl;
        public string textureImageUrl; // 纹理贴图
        public string pbrMetallicImageUrl; // pbr金属度贴图
        public string pbrRoughnessImageUrl; // pbr粗糙度贴图
        public string pbrNormalImageUrl; // pbr法线贴图
        public string pbrImageUrl; // pbr基础颜色贴图
        public string objZipUrl; // total zip url
        public override string ToString()
        {
            return $"HyObjModelUrl(objUrl={objUrl}, mtlUrl={mtlUrl},\n" +
                   $"  textureImageUrl={textureImageUrl}, pbrMetallicImageUrl={pbrMetallicImageUrl},\n" +
                   $"  pbrRoughnessImageUrl={pbrRoughnessImageUrl}, pbrNormalImageUrl={pbrNormalImageUrl},\n" +
                   $"  pbrImageUrl={pbrImageUrl}, objZipUrl={objZipUrl})";
        }
    }
    [Serializable]
    public class HySTLModelUrl : HyModelUrl
    {
        public string stlUrl;
    
        public override string ToString()
        {
            return $"HySTLModelUrl(stlUrl={stlUrl})";
        }
    }
    [Serializable]
    public class HyUSDZModelUrl : HyModelUrl
    {
        public string usdzUrl;
    
        public override string ToString()
        {
            return $"HyUSDZModelUrl(usdzUrl={usdzUrl})";
        }
    }
    [Serializable]
    public class HyMP4VideoUrl : HyModelUrl
    {
        public string mp4Url;
    
        public override string ToString()
        {
            return $"HyMP4VideoUrl(mp4Url={mp4Url})";
        }
    }

    [Serializable]
    public class HyGifVideoUrl : HyModelUrl
    {
        public string gifUrl;
        public override string ToString()
        {
            return $"HyGifVideoUrl(gifUrl={gifUrl})";
        }
    }

    [UseProcessAsync]
    public abstract class BaseHyModelNode : TJAIBaseAssetNode
    {
        [Save(ReceivedDataType = typeof(HyMeshData))] 
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
            artifact.m_ReceivedData.assetPath = $"{GetResourceFolder()}/{Guid}/hunyuan_{this.GetType().Name}_{Guid}";
            yield return base.RestoreHistory(Guid);
        }

        protected BaseArtifact<GameObject, HyMeshData> artifact => (BaseArtifact<GameObject, HyMeshData>)currentArtifact;
        [Output("Model Url")] public HyModelOutput outputModelUrl;
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

            var data = new HyMeshData
            {
                assetPath = $"{GetResourceFolder()}/{taskID}/hunyuan_{this.GetType().Name}_{taskID}",
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