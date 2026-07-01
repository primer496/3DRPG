using System;
using UnityEngine.Networking;

/// <summary>
/// 后端请求回调接口，sprite/texture对应接口
/// </summary>
namespace UnityEngine.AIGraph.Backend
{
    internal class BeautifyRestCall : TJAIRestCall<BeautifyRequest, BeautifyResponse>
    {
        public BeautifyRestCall(ServerConfig asset, int mode) : base(asset, mode)
        {
        }

        public override string endPoint => $"/{serverTagList[serverTagIndex]}/v3/beautify";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }

    internal class CaptionRestCall : TJAIRestCall<CaptionRequest, CaptionResponse>
    {
        public CaptionRestCall(ServerConfig asset, int mode) : base(asset, mode)
        {
        }

        public override string endPoint => $"/{serverTagList[serverTagIndex]}/v3/caption";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }

    internal class SpriteGenerateRestCall : TJAIRestCall<GeneratorRequest, GenerateResponse>
    {
        public static class Status
        {
            public const string failed = "failed";
            public const string completed = "done";
            public const string waiting = "waiting";
            public const string working = "working";
        }

        public SpriteGenerateRestCall(ServerConfig asset, int mode) : base(asset, mode)
        {
        }

        public override string endPoint => $"/{serverTagList[serverTagIndex]}/v3/generate";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }

    internal class GetJobRestCall : TJAIRestCall<JobInfoRequest, JobInfoResponse>
    {
        public GetJobRestCall(ServerConfig asset, int mode) : base(asset, mode)
        {
        }

        public override string endPoint => $"/{serverTagList[serverTagIndex]}/v3/job";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }

    internal class GetArtifactUrlRestCall : TJAIRestCall<ArtifactRequest, GetArtifactUrlResponse>
    {
        public GetArtifactUrlRestCall(ServerConfig asset, int mode) : base(asset, mode)
        {
        }

        public override string endPoint => $"/{serverTagList[serverTagIndex]}/v3/artifact";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }

    internal class GetArtifactRestCall : TJAIRestCall<ArtifactRequest, byte[]>
    {
        public GetArtifactRestCall(ServerConfig asset, int mode) : base(asset, mode)
        {
        }

        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.GET;

        protected override byte[] ParseResponse(UnityWebRequest response)
            => response.downloadHandler.data;
    }

    internal class GetGridUrlRestCall : TJAIRestCall<GridRequest, GridUrlResponse>
    {
        public GetGridUrlRestCall(ServerConfig asset, int mode) : base(asset, mode)
        {
        }

        public override string endPoint => $"/{serverTagList[serverTagIndex]}/v3/img/grid";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }

    internal class UpscaleRestCall : TJAIRestCall<ServerRequest<EmptyPayload>, UpscaleResponse>
    {
        public UpscaleRestCall(ServerConfig asset, int mode) : base(asset, mode)
        {
        }

        public override string endPoint => $"/{serverTagList[serverTagIndex]}/v3/upscale";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }

    internal class BatchPbrRestCall : TJAIRestCall<BatchPbrRequest, BatchPbrResponse>
    {
        public BatchPbrRestCall(ServerConfig asset, int mode) : base(asset, mode)
        {
        }

        public override string endPoint => $"/{serverTagList[serverTagIndex]}/v3/pbr/batch";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }

    internal class RodinGenerateRestCall : TJAIRestCall<RodinGenerateRequest, RodinGenerateResponse>
    {
        public static class Status
        {
            public const string failed = "Failed";
            public const string completed = "Done";
            public const string waiting = "Generating";
        }

        public RodinGenerateRestCall(ServerConfig asset, int mode) : base(asset, mode)
        {
        }

        public override string endPoint => $"/{serverTagList[serverTagIndex]}/v1/generate";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }

    internal class RodinArtifactRestCall : TJAIRestCall<RodinArtifactRequest, RodinArtifactResponse>
    {
        public RodinArtifactRestCall(ServerConfig asset, int mode) : base(asset, mode)
        {
        }

        public override string endPoint => $"/{serverTagList[serverTagIndex]}/v1/download";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }

    internal class GetRodinJobRestCall : TJAIRestCall<JobInfoRequest, JobInfoResponse>
    {
        public GetRodinJobRestCall(ServerConfig asset, int mode) : base(asset, mode)
        {
        }

        public override string endPoint => $"/{serverTagList[serverTagIndex]}/v1/job";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }

    internal class GetRodinArtifactRestCall : TJAIRestCall<string, byte[]>
    {
        public GetRodinArtifactRestCall(ServerConfig asset, int mode) : base(asset, mode)
        {
        }

        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.GET;

        protected override byte[] ParseResponse(UnityWebRequest response)
            => response.downloadHandler.data;
    }

    internal class GetTokenRestCall : TJAIRestCall<string, TokenInfoResponse>
    {
        public GetTokenRestCall(ServerConfig asset, int mode) : base(asset, mode)
        {
        }

        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.GET;
        public override string endPoint => $"/api/editor/user/me";

        protected override TokenInfoResponse ParseResponse(UnityWebRequest response)
        {
            string stringResponse = System.Text.Encoding.UTF8.GetString(response.downloadHandler.data);
            return JsonUtility.FromJson<TokenInfoResponse>(stringResponse);
        }
    }

    internal class GetLatestVersionRestCall : TJAIRestCall<string, string>
    {
        public GetLatestVersionRestCall(ServerConfig asset, int mode) : base(asset, mode)
        {
        }

        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.GET;
        public override string endPoint => $"/api/editor/package-latest-version";
        protected override void PrepareRequest(string request, ref UnityWebRequest uRequest)
        {
            // step 1: set Authorization
            uRequest.SetRequestHeader("accept", "application/json");
            uRequest.SetRequestHeader("X-VS", Application.tuanjieVersion);

            var accessToken = UnityConnectProxy.instance.GetAccessToken();
            uRequest.SetRequestHeader("Authorization", "Bearer " + accessToken);
        }

        protected override string ParseResponse(UnityWebRequest response)
        {
            var stringResponse = System.Text.Encoding.UTF8.GetString(response.downloadHandler.data);
            return stringResponse;
        }
    }


    //internal abstract class GeneratorRestCall<T1, T2, T3> : QuarkRestCall<T1, T2, T3> where T3 : QuarkRestCall
    //{
    //    ServerConfig m_ServerConfig;
    //    public int index;
    //    public string[] server_name = new string[] { "sprite", "texture" };
    //    public GeneratorRestCall(ServerConfig serverConfig, T1 request, int serverIndex = 0)
    //    {
    //        m_ServerConfig = serverConfig;
    //        index = serverIndex;
    //        this.request = request;
    //        maxRetries = serverConfig.maxRetries;
    //        retryDelay = serverConfig.webRequestPollRate;
    //    }

    //    public override string server => m_ServerConfig.serverList[index];
    //}
    //internal class SpriteGenerateRestCall : GeneratorRestCall<GeneratorRequest, GenerateResponse, SpriteGenerateRestCall>
    //{
    //    public static class Status
    //    {
    //        public const string failed = "failed";
    //        public const string completed = "done";
    //        public const string waiting = "waiting";
    //        public const string working = "working";
    //    }

    //    public SpriteGenerateRestCall(ServerConfig asset, GeneratorRequest request, int mode)
    //        : base(asset, request, mode)
    //    {
    //        this.request = request;
    //    }

    //    public override string endPoint => $"/{server_name[index]}/v3/generate";
    //    public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;

    //    protected override string RequestLog()
    //    {
    //        return $"Request:{MakeEndPoint(this)} Payload:{request.GetRequestLog()}";
    //    }
    //}

    //internal class SpriteVariantRestCall : SpriteGenerateRestCall
    //{
    //    public SpriteVariantRestCall(ServerConfig asset, GeneratorRequest request, string generatorProfile)
    //        : base(asset, request, generatorProfile)
    //    {
    //    }

    //    public override string endPoint => $"/api/v2/sprite/variation";
    //    public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;

    //    protected override string RequestLog()
    //    {
    //        return $"Request:{MakeEndPoint(this)} Payload:{request.GetRequestLog()}";
    //    }
    //}

    //internal class SpriteScribbleRestCall : SpriteGenerateRestCall
    //{
    //    public SpriteScribbleRestCall(ServerConfig asset, GeneratorRequest request, string generatorProfile)
    //        : base(asset, request, generatorProfile)
    //    {
    //    }

    //    public override string endPoint => $"/api/v2/sprite/scribble";
    //    public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;

    //    protected override string RequestLog()
    //    {
    //        return $"Request:{MakeEndPoint(this)} Payload:{request.GetRequestLog()}";
    //    }
    //}

    //internal class GetSpriteGeneratorJobListRestCall : GeneratorRestCall<ServerRequest<EmptyPayload>, JobListResponse, GetSpriteGeneratorJobListRestCall>
    //{
    //    string m_GeneratorProfile;

    //    public GetSpriteGeneratorJobListRestCall(ServerConfig asset, ServerRequest<EmptyPayload> request, string generatorProfile)
    //        : base(asset, request)
    //    {
    //        request.guid = generatorProfile;
    //        this.request = request;
    //    }

    //    public override string endPoint => $"/api/v3/sprite/jobs";
    //    public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    //}

    //internal class GetJobRestCall : GeneratorRestCall<JobInfoRequest, JobInfoResponse, GetJobRestCall>
    //{
    //    public GetJobRestCall(ServerConfig asset, JobInfoRequest request, int mode)
    //        : base(asset, request, mode)
    //    {
    //        this.request = request;
    //    }

    //    public string jobID => request.job_id;
    //    public override string endPoint => $"/{server_name[index]}/v3/job";
    //    public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;

    //    protected override string ResponseLog()
    //    {
    //        var log = base.RequestLog();
    //        log += $"\n JobID:{jobID} {webRequest.downloadHandler.text}";
    //        return log;
    //    }
    //}

    //internal class GetArtifactUrlRestCall : GeneratorRestCall<ArtifactRequest, GetArtifactUrlResponse, GetArtifactUrlRestCall>
    //{
    //    public GetArtifactUrlRestCall(ServerConfig asset, ArtifactRequest request, int mode)
    //        : base(asset, request, mode)
    //    {
    //        this.request = request;
    //    }

    //    public override string endPoint => $"/{server_name[index]}/v3/artifact";
    //    public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    //}

    //class GetArtifactRestCall : GeneratorRestCall<ArtifactRequest, byte[], GetArtifactRestCall>
    //{
    //    string m_ImageDownloadURL = string.Empty;
    //    GetArtifactUrlRestCall m_GetImageURL;

    //    public GetArtifactRestCall(ServerConfig serverConfig, ArtifactRequest request, int mode)
    //        : base(serverConfig, request, mode)
    //    {
    //        this.request = request;

    //        m_GetImageURL = new GetArtifactUrlRestCall(serverConfig, request, mode);
    //        DependOn(m_GetImageURL);
    //        m_GetImageURL.RegisterOnSuccess(OnGetImageURLSuccess);
    //        m_GetImageURL.RegisterOnFailure(OnGetImageURLFailed);
    //    }

    //    void OnGetImageURLFailed(GetArtifactUrlRestCall obj)
    //    {
    //        if (obj.retriesFailed)
    //        {
    //            maxRetries = 0;
    //            Debug.LogError($"Failed to get URL for image {request.job_id}");
    //            SignalRequestCompleted(EState.Error);
    //            OnError();
    //        }
    //    }

    //    void OnGetImageURLSuccess(GetArtifactUrlRestCall arg1, GetArtifactUrlResponse response)
    //    {
    //        m_ImageDownloadURL = response.url;
    //    }

    //    public override string server => m_ImageDownloadURL;
    //    public override string endPoint => "";

    //    protected override byte[] ParseResponse(UnityWebRequest response)
    //    {
    //        return response.downloadHandler.data;
    //    }

    //    public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.GET;
    //}


    //internal class UpscaleRestCall : GeneratorRestCall<ServerRequest<EmptyPayload>, UpscaleResponse, UpscaleRestCall>
    //{
    //    public UpscaleRestCall(ServerConfig asset, string guid, int mode)
    //        : base(asset, new ServerRequest<EmptyPayload>(), mode)
    //    {
    //        var r = request;
    //        r.guid = guid;
    //        request = r;
    //    }

    //    public override string endPoint => $"/{server_name[index]}/v3/upscale";
    //    public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    //}

    //internal class BeautifyRestCall : GeneratorRestCall<BeautifyRequest, BeautifyResponse, BeautifyRestCall>
    //{
    //    public BeautifyRestCall(ServerConfig asset, BeautifyRequest request, int mode)
    //        : base(asset, request, mode)
    //    {
    //        this.request = request;
    //    }
    //    public override string endPoint => $"/{server_name[index]}/v3/beautify";
    //    public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    //}

    //internal class BatchPbrRestCall : GeneratorRestCall<BatchPbrRequest, BatchPbrResponse, BatchPbrRestCall>
    //{
    //    public BatchPbrRestCall(ServerConfig asset, BatchPbrRequest request, int mode)
    //        : base(asset, request, mode)
    //    {
    //        this.request = request;
    //    }
    //    public override string endPoint => $"/{server_name[index]}/v3/pbr/batch";
    //    public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    //}

    // REVIEW: 请求体和响应体要和后端对应，可以考虑用protobuf
    [Serializable]
    struct Settings
    {
        public string prompt;
        public string negative_prompt;
        public string[] beauty_en;
        public string[] beauty_zh;
        public bool remove_bg;
        public int width;
        public int height;
        public int seed;
        public string model_id;
        public string style_id;
        public float style_weight;
    }

    [Serializable]
    struct GeneratorRequest
    {
        public string init_img;
        public string mask_img;
        public float img_weight;
        public int mode;
        public CtrlUnit[] ctrl_units;
        public Settings settings;

        public string GetRequestLog()
        {
            var logRequest = this;
            logRequest.init_img = $"Image data removed for logging size:{logRequest.init_img?.Length}";
            logRequest.mask_img = $"Image data removed for logging size:{logRequest.mask_img?.Length}";
            return JsonUtility.ToJson(logRequest);
        }
    }

    [Serializable]
    struct GenerateResponse
    {
        public bool success;
        public string error;
        public string job_id;
    }

    //[Serializable]
    //struct JobListResponse
    //{
    //    public string[] jobIDs;
    //}

    [Serializable]
    struct JobInfoRequest
    {
        public string job_id;
    }

    [Serializable]
    struct ArtifactRequest
    {
        public string job_id;
    }

    [Serializable]
    struct JobInfoResponse
    {
        public bool success;
        public string error;
        public string status;
    }

    [Serializable]
    struct EmptyPayload
    {
    }

    [Serializable]
    class ServerRequest<T>
    {
        public string guid;
        public T data;
    }

    [Serializable]
    struct GetArtifactUrlResponse
    {
        public string url;
        public bool success;
    }

    [Serializable]
    struct GridRequest
    {
        public string[] guids;
        public int row;
        public int col;
    }

    [Serializable]
    struct GridUrlResponse
    {
        public string url;
        public bool success;
        public string error;
    }

    [Serializable]
    struct UpscaleResponse
    {
        public bool success;
        public string error;
        public string guid;
        public uint seed;
        public string prompt;
    }

    struct BeautifyRequest
    {
        public string prompt;
    }

    [Serializable]
    struct BeautifyResponse
    {
        //public string prompt;
        public string[] beauty_zh;
        public string[] beauty_en;
    }

    struct CaptionRequest
    {
        public string image;
    }

    [Serializable]
    struct CaptionResponse
    {
        public SDPrompt prompt;
    }

    [Serializable]
    struct DatasetModel
    {
        public int id;
        public string name;
    }

    struct BatchPbrRequest
    {
        public string guid;
        public string[] map_types;
    }

    [Serializable]
    struct BatchPbrResponse
    {
        public bool success;
        public string error;
        public Pbrs pbrs;
    }

    [Serializable]
    struct Pbrs
    {
        public string emission;
        public string height;
        public string normal;

        public string metallic;

        //public string roughness;
        public string ao;
    }

    struct RodinGenerateRequest
    {
        public string prompt;
        public string image;
        public int seed;
        public string quality;
        public string tier;
        public bool use_hyper;
    }

    [Serializable]
    struct RodinGenerateResponse
    {
        public bool success;
        public string error;
        public string message;
        public string download_id;
        public string job_id;
    }

    struct RodinArtifactRequest
    {
        public string download_id;
    }

    [Serializable]
    struct RodinArtifactResponse
    {
        public string error;
        public RodinAsset[] assets;
    }

    [Serializable]
    struct RodinAsset
    {
        public string url;
        public string name;
    }


    [Serializable]
    struct DatasetModels
    {
        public DatasetModel[] models;
    }

    struct TokenInfoResponse
    {
        public string avatar;
        public CreditInfo credits;
        public string email;
        public string genesisUserId;
        public string id;
        public bool isAdmin;
        public bool isCP;
        public string loginType;
        public Org org;
        public string phone;
        public string role;
        public string type;
        public string username;
    }

    [Serializable]
    struct Org
    {
        public string orgDisplayName;
        public string orgId;
        public string orgName;
    }

    [Serializable]
    struct CreditInfo
    {
        public string userId;
        public string username;
        public string email;
        public int currentCredits;
        public int todayEarned;
        public int todaySpent;
        public int totalEarned;
        public int totalSpent;
        public string lastCreditDate;
    }
}