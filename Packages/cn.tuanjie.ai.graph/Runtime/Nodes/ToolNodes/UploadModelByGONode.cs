using System;
using System.Collections;
using System.IO;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Tools/Upload Model By GO")]
    [UseProcessAsync]
    public class UploadModelByGONode : SDNode
    {
        [Input(name = "Game Object")] public GameObject inputGO;
        
        [Output(name = "Model Url")] public string url;
        [HideInInspector] public bool uploaded = false;
        public override string description => DescriptionConstants.HyUploadModelNode;
        public override bool needTrigger => true;
        public override bool isRenamable => true;

        [Preview, SerializeField, HideInInspector, HideInPreviewSelector]
        protected GameObject m_Obj;

        public GameObject obj
        {
            get => m_Obj;
            set
            {
                if (m_Obj == value) return;
                m_Obj = value;
                this?.NotifyFieldChanged("m_Obj");
            }
        }

        public override string name => LocalizationManager.Instance.GetLocalizedText("UploadModelByGO");

        public override IEnumerator ProcessAsync()
        {
            if (inputGO != null)
                obj = inputGO;
            
            if (obj == null)
                throw new NullReferenceException("Empty model is invalid");
            // if (uploaded)
            //     yield break;
#if UNITY_EDITOR
            var assetPath = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(assetPath))
            {
                // try to get assetPath from mesh
                var meshFilter = obj.GetComponent<MeshFilter>();
                if (meshFilter == null)
                    throw new ArgumentNullException(nameof(meshFilter), "Selected game object doesn't have MeshFilter");
                assetPath = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);
            }
            if (!string.IsNullOrEmpty(assetPath))
            {
                if (!(assetPath.EndsWith(".fbx") || assetPath.EndsWith(".glb") || assetPath.EndsWith(".obj")))
                    throw new ArgumentException($"Only fbx, glb, obj files are supported", nameof(obj));
                var bytes = File.ReadAllBytes(assetPath);
                var request = new HyUploadRequest()
                {
                    fileName = Path.GetFileName(assetPath), fileData = bytes
                };
                var restCall = new HyUploadModelRestCall(ServerConfig.serverConfig, 0);
                yield return restCall.MakeServerRequest(request);
                if (!restCall.Success)
                    throw new Exception($"Failed to upload model {obj.name}, error: {restCall.Result}");
                url = restCall.Result.url;
            }
            else
            {
                throw new ArgumentException($"Game object {obj.name} not in Assets folder", nameof(obj));
                // TODO: read mesh? export to gltf
                // var exporter = new GameObjectExport();
                // exporter.AddScene(new GameObject[1] { obj });
                // using (MemoryStream stream = new MemoryStream())
                // {
                //     // 启动异步任务
                //     Task<bool> saveTask = exporter.SaveToStreamAndDispose(stream);
                //
                //     // 等待Task完成
                //     while (!saveTask.IsCompleted)
                //     {
                //         yield return null;
                //     }
                //
                //     // 检查结果
                //     if (saveTask.IsFaulted)
                //     {
                //         throw new Exception($"Failed to upload model: {saveTask.Exception}");
                //     }
                //
                //     bool result = saveTask.Result;
                //     if (!result)
                //     {
                //         throw new Exception($"Failed to upload model: {result}");
                //     }
                // }
            }

            uploaded = true;
#else
#endif
            yield return null;
        }

        public IEnumerator Generate() => ProcessAsync();
    }
}