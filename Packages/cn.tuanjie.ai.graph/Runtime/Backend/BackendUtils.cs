using System;
using System.Collections;
using System.IO;
using GraphProcessor;
using UnityEditor;
using UnityEngine.AIGraph.Backend;

#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif

namespace UnityEngine.AIGraph
{
    public static class BackendUtils
    {
        public static ServerConfig serverConfig => ServerConfig.serverConfig;

        public static readonly int maxPolling = 60;

#if UNITY_EDITOR
        static EditorWaitForSeconds waitFixed = new EditorWaitForSeconds(5.0f);
#else
        static WaitForSecondsRealtime waitFixed = new WaitForSecondsRealtime(serverConfig.webRequestPollRate);
#endif

        internal static IEnumerator RetrieveFromBackendCommon<TInput, TOutput>(
            GetJobStatusRestCall<TInput, TOutput> jobStatusRestCall, TaskStatusRequest jobInfoRequest,
            Action<NodeStatus, string, float> progressCallback = null)
        {
            var jobCompleted = false;
            var progress = 10f;
            while (true)
            {
                yield return jobStatusRestCall.MakeServerRequest(jobInfoRequest);

                if (!jobStatusRestCall.Success)
                    throw new Exception($"Failed to generate. Please try again");

                var jobStatus = jobStatusRestCall.Result.status;
                if (string.Equals(jobStatus, "completed", StringComparison.OrdinalIgnoreCase))
                {
                    jobCompleted = true;
                    break;
                }
                else if (string.Equals(jobStatus, "failed", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception(
                        $"Failed to generate, error: {jobStatusRestCall.Result.error}, task id: {jobStatusRestCall.Result.id}. Please try again");
                }
                else
                {
                    if (string.Equals(jobStatus, "queued", StringComparison.OrdinalIgnoreCase))
                        progressCallback?.Invoke(NodeStatus.Queued, string.Empty, jobStatusRestCall.Result.queueNum);
                    else
                    {
                        progress = Mathf.Max(progress, jobStatusRestCall.Result.progress);
                        progressCallback?.Invoke(NodeStatus.Working, string.Empty, progress);
                    }

                    yield return waitFixed;
                }
            }

            if (!jobCompleted)
                throw new TimeoutException("Time out: Job not completed with maximum patience");

            yield return jobStatusRestCall.Result.output.data.result;
        }

        public static IEnumerator DownloadFromUrl(string url, int serverIndex)
        {
            var getArtifactRestCall = new VastDownloadRestCall(serverConfig, serverIndex);
            yield return getArtifactRestCall.MakeServerRequest(null, url: url);

            if (!getArtifactRestCall.Success)
                throw new Exception($"Failed to get artifact from url {url}");

            byte[] bytes = getArtifactRestCall.Result;
            yield return bytes;
        }

        public static void SaveBytesToFile(byte[] bytes, string localPath)
        {
#if UNITY_EDITOR
            // var assetPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
            var directory = Path.GetDirectoryName(localPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(localPath, bytes);
            AssetDatabase.ImportAsset(localPath, ImportAssetOptions.Default);
#endif
        }
    }
}