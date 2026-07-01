using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Tripo/Retarget Model(Tripo)")]
    [UseProcessAsync]
    public class VastRetargetNode : TJAIBaseAssetNode
    {
        [Input(name = "Model ID")]
        [Tooltip("The output model ID of rig task")]
        public VastTaskID inputModelID;
        [Input(name = "Bake Animation"), ShowAsDrawer]
        [Tooltip("Determines whether to bake the animation into the model upon output.")]
        public bool bakeAnimation = true;
        [HideInInspector]
        // [Input(name = "Export With Geometry"), ShowAsDrawer]
        [Tooltip("Determines whether to include geometry in the output")]
        public bool exportWithGeometry = true;

        // control container settings
        [HideInInspector] public List<string> animationChoices = new()
        {
            "preset:idle", "preset:walk", "preset:run", "preset:dive", "preset:climb", "preset:jump", "preset:slash",
            "preset:shoot", "preset:hurt", "preset:fall", "preset:turn", "preset:quadruped:walk", "preset:hexapod:walk",
            "preset:octopod:walk", "preset:serpentine:march", "preset:aquatic:march"
        };
        [HideInInspector] public List<string> animations = new();
        [HideInInspector] public string outFormat = "fbx";

        [Save(ReceivedDataType = typeof(VastMeshData))]
        [SerializeField, HideInInspector] 
        public GameObject obj;

        [Preview, SerializeField, HideInInspector]
        [Output("AnimationClip")]
        private AnimationClip m_Clip;

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

        [Output("Model ID")] public VastTaskID outputModelID;
        protected BaseArtifact<GameObject, VastMeshData> artifact => (BaseArtifact<GameObject, VastMeshData>)currentArtifact;

        [HideInInspector, SerializeField]
        public List<AnimationClip> results;

        public Action OnResultsChange;

        public override string name => LocalizationManager.Instance.GetLocalizedText("RetargetModel(Tripo)");
        
        public override string description => DescriptionConstants.VastRetargetNode;
        public override bool needTrigger => true;
        public override bool isRenamable => true;

        private const int serverIndex = 3;

        protected override void Enable()
        {
            base.Enable();
            allowHistory = false;
            onCancelled += () => { taskID = null; };
            onError += s => { taskID = null; };
            taskCostTime = 5;
        }

        public override void UpdateOutputPorts()
        {
            outputModelID.id = artifact.m_ReceivedData.vastTaskID;
            obj = currentArtifact.GetCacheUnityObject() as GameObject;
            results = MeshUtils.ImportAnimationClip(artifact.m_ReceivedData.assetPath);
            clip = results is { Count: > 0 } ? results[0] : null;
            OnResultsChange?.Invoke();
        }

        public override IEnumerator ProcessAsync()
        {
            if (string.IsNullOrEmpty(taskID))
            {
                // retarget
                var genReq = new VastRetargetRequest()
                {
                    animation = string.Empty,
                    animations = animations,
                    bakeAnimation = bakeAnimation,
                    exportWithGeometry = exportWithGeometry,
                    originalModelTaskId = inputModelID.id,
                    outFormat = outFormat
                };
                var genRestCall = new VastRetargetRestCall(serverConfig, serverIndex);
                yield return genRestCall.MakeServerRequest(genReq);

                if (!genRestCall.Success)
                    throw new Exception($"Failed to retarget, error message: {genRestCall.Result.message}");

                var genRsp = genRestCall.Result;
                taskID = genRsp.taskId;
            }

            var data = new VastMeshData
            {
                ID = taskID, progressCallback = UpdateStatus,
                assetPath = $"{GetResourceFolder()}/{taskID}/vast_{GetType().Name}_{taskID}"
            };

            var rigProcessor = new CoroutineProcessor<GameObject>();
            yield return rigProcessor.ProcessAsync(currentArtifact.ReadFromCache(data, serverIndex));

            if (status == NodeStatus.Init)
                yield break;
            UpdateOutputPorts();

            UpdateHistory();
            graph.tokenDataModel.UpdateToken(data.tokenRemaining);
            taskID = null;
        }

        public IEnumerator Generate() => ProcessAsync();
        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>
            {
                $"Bake Animation: {bakeAnimation}", $"Animations: [{string.Join(",", animations)}]"
            };
            return true;
        }
    }
}