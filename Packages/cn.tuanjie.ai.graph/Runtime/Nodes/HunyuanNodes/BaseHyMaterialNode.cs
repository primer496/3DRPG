// using System;
// using System.Linq;
// using System.Collections;
// using System.Collections.Generic;
// using GraphProcessor;
// using UnityEngine.AIGraph.Backend;
//
// #if UNITY_EDITOR
// using UnityEditor;
// #endif
//
// namespace UnityEngine.AIGraph
// {
//     [UseProcessAsync]
//     public abstract class BaseHyMaterialNode : TJAIBaseAssetNode
//     {
//         [Save(ReceivedDataType = typeof(HyMaterialData))] 
//         [Preview, SerializeField, HideInInspector] [Output("Material")]
//         protected Material m_Material;
//
//         public Material material
//         {
//             get => m_Material;
//             set
//             {
//                 if (m_Material != value)
//                 {
//                     this?.NotifyFieldChanged("m_Material");
//                 }
//             }
//         }
//
//         protected override void Destroy()
//         {
//             ReleaseObject(m_Material);
//             base.Destroy();
//         }
//
//         public override void CollectSubAssets()
//         {
//             base.CollectSubAssets();
//             if (m_Material == null) return;
// #if UNITY_EDITOR
//             SaveObject(m_Material);
//             AssetDatabase.SaveAssets();
// #endif
//         }
//
//         public override IEnumerator RestoreHistory(string Guid)
//         {
//             ((BaseArtifact<Material, HyMaterialData>)currentArtifact).m_ReceivedData.matPath =
//                 savePath = $"{GlobalConstants.AI_GRAPH_MAT_FOLDER}/{Guid}/{Guid}";
//
//             yield return base.RestoreHistory(Guid);
//         }
//
//         public override bool needTrigger => true;
//         public override bool isRenamable => true;
//
//         protected string savePath;
//
//         protected const int serverIndex = 3;
//
//         public override void UpdateOutputPorts()
//         {
//             if (currentArtifact.GetCacheUnityObject() != null)
//                 material = currentArtifact.GetCacheUnityObject() as Material;
//         }
//         
//         internal IEnumerator GenerateRestCall<TReq, TRestCall>(TReq req, TRestCall restCall, bool isPBR)
//             where TRestCall : TJAIRestCall<TReq, TaskSubmitResponse>
//         {
//             DebugUtils.ConditionLog($"Send Request: {req}");
//             yield return restCall.MakeServerRequest(req);
//
//             var response = restCall.Result;
//             DebugUtils.ConditionLog($"Receive Response: {response}");
//             if (!restCall.Success)
//                 throw new Exception(
//                     $"Failed to generate artifact, task id: {response.taskId}, error message: {response.message}");
//
//             savePath = $"{GlobalConstants.AI_GRAPH_MAT_FOLDER}/{response.taskId}/{response.taskId}";
//
//             var data = new HyMaterialData
//             {
//                 matPath = savePath, ID = response.taskId, progressCallback = UpdateStatus,
//                 isPBR = isPBR
//             };
//
//             var processor = new CoroutineProcessor<Material>();
//             yield return processor.ProcessAsync(currentArtifact.ReadFromCache(data, serverIndex));
//             processor.HandleException();
//
//             if (status == NodeStatus.Init)
//                 yield break;
//             material = processor.Result;
//
//             UpdateHistory();
//             graph.tokenDataModel.UseToken(data.tokenUsage);
//         }
//     }
// }