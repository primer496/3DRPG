using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using UnityEngine.Networking;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif

namespace UnityEngine.AIGraph.Backend
{
    internal abstract class BaseRestCall<TRequest, TResponse> : IQuarkEndpoint
    {
        static int s_RequestCount = 0;

        const int k_MaxRequest = 5;

        static readonly WaitUntil waitUntil = new WaitUntil(() => s_RequestCount < k_MaxRequest);

        public virtual string server { get; }

        public virtual string endPoint { get; }
        public virtual string baseUrl { get; }
        public virtual string accessToken { get; }

        public abstract IQuarkEndpoint.EMethod method { get; }

        public int maxRetries { get; protected set; } = 3;

        public float retryDelay { get; protected set; } = 1f;

        protected event Action<UnityWebRequest> onSuccess;

        protected event Action<UnityWebRequest> onFailure;

        public TResponse Result { get; protected set; }

        public bool Success { get; protected set; }

        public void RegisterOnSuccess(Action<UnityWebRequest> callback)
        {
            onSuccess -= callback;
            onSuccess += callback;
        }

        public void RegisterOnFailure(Action<UnityWebRequest> callback)
        {
            onFailure -= callback;
            onFailure += callback;
        }

        public IEnumerator MakeServerRequest(TRequest request, string url = null)
        {
            Success = false;
            Result = default;

#if UNITY_EDITOR
            var waitFixed = new EditorWaitForSeconds(retryDelay);
#else
            var waitFixed = new WaitForSecondsRealtime(retryDelay);
#endif
            url = string.IsNullOrEmpty(url) ? baseUrl : url;
            url = string.IsNullOrEmpty(url) ? server + endPoint : url;
            DebugUtils.ConditionLog($"Send request to {url}, request: {request}");
            for (int retry = 0; retry < maxRetries; ++retry)
            {
                // Prepare UnityWebRequest data
                var uRequest = new UnityWebRequest(url, method.ToString());
                PrepareRequest(request, ref uRequest);
                uRequest.downloadHandler = new DownloadHandlerBuffer();

                // Do send web request
                yield return SendRequest(uRequest);

                // Resolve
                if (uRequest.result == UnityWebRequest.Result.Success)
                {
                    Result = ParseResponse(uRequest);
                    Success = true;
                    DebugUtils.ConditionLog($"Received response: {Result}");
                    onSuccess?.Invoke(uRequest);
                }
                else
                {
                    Debug.LogError($"Failed to send request, response code: {uRequest.responseCode}, result: {uRequest.result}, error: {uRequest.error}, response message: {uRequest.downloadHandler?.text}");
                    onFailure?.Invoke(uRequest);

                    if (retry < maxRetries - 1)
                        // Wait for a fixed time and retry sending request
                        yield return waitFixed;
                }

                uRequest.uploadHandler?.Dispose();
                uRequest.downloadHandler?.Dispose();
                uRequest.Dispose();

                if (Success)
                    break;
            }
        }

        private static IEnumerator SendRequest(UnityWebRequest uRequest)
        {
            // Wait until current request count does't exceed limit
            yield return waitUntil;

            ++s_RequestCount;
            yield return uRequest.SendWebRequest();
            --s_RequestCount;
        }

        protected virtual void PrepareRequest(TRequest request, ref UnityWebRequest uRequest)
        {
            uRequest.SetRequestHeader("accept", "application/json");
            uRequest.SetRequestHeader("X-VS", Application.tuanjieVersion);
#if UNITY_EDITOR
            var packVersion = PackageVersionChecker.GetPackageVersion(GlobalConstants.PACK_NAME);
            packVersion ??= "0.0.1";
            uRequest.SetRequestHeader("X-Package-Version", packVersion);
#else
                uRequest.SetRequestHeader("X-Package-Version", "0.0.1");
#endif

            string token = string.IsNullOrEmpty(accessToken) ? UnityConnectProxy.instance.GetAccessToken() : accessToken;
            uRequest.SetRequestHeader("Authorization", "Bearer " + token);

            if (method == IQuarkEndpoint.EMethod.POST)
            {
                string uRequestJSON = JsonConvert.SerializeObject(request);

                uRequest.SetRequestHeader("content-type", "application/json; charset=UTF-8");
                uRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(uRequestJSON));
            }
        }

        protected virtual TResponse ParseResponse(UnityWebRequest response)
            => JsonUtility.FromJson<TResponse>(response.downloadHandler?.text);

        ~BaseRestCall()
        {
            onSuccess = null;
            onFailure = null;
        }
    }
}
