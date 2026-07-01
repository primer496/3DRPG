using System;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.AIGraph.Backend;
using UnityEngine.Networking;

namespace UnityEditor.AIGraph
{
    internal class GetLatestNodesRestCall : TJAIRestCall<string, NodeTemplateGenerator.NodeConfig>
    {
        public GetLatestNodesRestCall(ServerConfig asset, int mode) : base(asset, mode)
        {
        }

        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.GET;
        public override string endPoint => $"/api/editor/package-latest-nodes";

        protected override void PrepareRequest(string request, ref UnityWebRequest uRequest)
        {
            uRequest.SetRequestHeader("accept", "application/json");
            uRequest.SetRequestHeader("X-VS", Application.tuanjieVersion);

            var accessToken = UnityConnectProxy.instance.GetAccessToken();
            uRequest.SetRequestHeader("Authorization", "Bearer " + accessToken);
        }

        protected override NodeTemplateGenerator.NodeConfig ParseResponse(UnityWebRequest response)
        {
            return JsonConvert.DeserializeObject<NodeTemplateGenerator.NodeConfig>(response.downloadHandler.text);
        }
    }

    public static class BackendNodeFetcher
    {
        public static IEnumerator FetchNodeFromServer(Action onComplete = null)
        {
            // do nothing currently
            yield break;
            
            // var restCall = new GetLatestNodesRestCall(ServerConfig.serverConfig, 4);
            // yield return restCall.MakeServerRequest(null);
            // NodeTemplateGenerator.GenerateCSharpTemplate(restCall.Result, 
            //     $"Packages/{GlobalConstants.PACK_NAME}");
            // onComplete?.Invoke();
        }
    }
}