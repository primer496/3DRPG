using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

/// <summary>
/// 后端请求回调接口
/// </summary>
namespace UnityEngine.AIGraph.Backend
{
    internal abstract class TJAIRestCall<TRequest, TResponse> : BaseRestCall<TRequest, TResponse>
    {
        protected ServerConfig serverConfig;

        public int serverTagIndex;

        public string[] serverTagList = new string[] { "sprite", "texture", "3d" };

        public TJAIRestCall(ServerConfig config, int index = 0)
        {
            serverConfig = config;
            serverTagIndex = index;
            maxRetries = serverConfig.maxRetries;
            retryDelay = serverConfig.webRequestPollRate;
        }

        // public override string server => serverConfig.serverList[serverTagIndex];
        public override string server => serverConfig.server;
    }

    internal struct TaskSubmitResponse
    {
        public string message;
        public string status;
        public string taskId;

        public override string ToString()
        {
            return $"TaskSubmitResponse(message={message}, status={status}, taskId={taskId})";
        }
    }

    internal struct TaskStatusRequest
    {
    }
    
    internal struct TaskStatusResponse<TInput, TOutput>
    {
        public struct TaskInputData
        {
            public string tripoTaskId;
        }
        public struct TaskInput
        {
            public TaskInputData data;
            public TInput param;
        }

        public struct TaskOutputData
        {
            public TOutput result;
        }

        public struct TaskOutput
        {
            public TaskOutputData data;
        }

        public string createdTime;
        public string updatedTime;
        public string deletedTime;
        public string createdBy;
        public string updatedBy;
        public string deletedBy;
        public string id;
        public string taskId;
        public string userId;
        public string name;
        public string type;
        public string status;
        public int creditBalance;
        public TaskInput input;
        public TaskOutput output;
        public int retryCount;
        public string error;
        public int progress;
        public int queueNum;

        public override string ToString()
        {
            return $"TaskStatusResponse<{typeof(TInput).Name}, {typeof(TOutput).Name}>(" +
                   $"type={type}, output=({output.data.result}, progress={progress}, queueNum={queueNum}, " +
                   $"id={id}, taskId={taskId}, userId={userId}, name={name}, " +
                   $"status={status}, creditBalance={creditBalance}, retryCount={retryCount}, error={error},\n" +
                   $"createdTime={createdTime}, updatedTime={updatedTime}, deletedTime={deletedTime}," +
                   $"createdBy={createdBy}, updatedBy={updatedBy}, deletedBy={deletedBy}," +
                   $"input=(\n\tdata=(tripoTaskId={input.data.tripoTaskId})\n\tparam={input.param}))\n";
        }
    }
    
     internal class GetJobStatusRestCall<TInput, TOutput> : TJAIRestCall<TaskStatusRequest, TaskStatusResponse<TInput, TOutput>>
    {
        private readonly string taskId;

        public GetJobStatusRestCall(ServerConfig asset, int mode, string taskId) : base(asset, mode)
        {
            this.taskId = taskId;
        }

        public override string endPoint => $"/api/editor/task/{taskId}/id-status";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.GET;

        protected override TaskStatusResponse<TInput, TOutput> ParseResponse(UnityWebRequest response)
        {
            TaskStatusResponse<TInput, TOutput> parsedRsp = new();
            var jsonObject = JObject.Parse(response.downloadHandler.text);
            parsedRsp.createdTime = jsonObject["createdTime"]?.ToString();
            parsedRsp.updatedTime = jsonObject["updatedTime"]?.ToString();
            parsedRsp.deletedTime = jsonObject["deletedTime"]?.ToString();
            parsedRsp.createdBy = jsonObject["createdBy"]?.ToString();
            parsedRsp.updatedBy = jsonObject["updatedBy"]?.ToString();
            parsedRsp.deletedBy = jsonObject["deletedBy"]?.ToString();
            parsedRsp.id = jsonObject["id"]?.ToString();
            parsedRsp.taskId = jsonObject["taskId"]?.ToString();
            parsedRsp.userId = jsonObject["userId"]?.ToString();
            parsedRsp.name = jsonObject["name"]?.ToString();
            parsedRsp.type = jsonObject["type"]?.ToString();
            parsedRsp.status = jsonObject["status"]?.ToString();
            parsedRsp.retryCount = jsonObject["retryCount"]?.Value<int>() ?? 0;
            parsedRsp.error = jsonObject["error"]?.ToString();
            parsedRsp.progress = jsonObject["progress"]?.Value<int>() ?? 0;
            parsedRsp.queueNum = jsonObject["queueNum"]?.Value<int>() ?? 0;
            long cb = jsonObject["creditBalance"]?.Value<long>() ?? 0L;

            parsedRsp.creditBalance = cb > int.MaxValue
                ? int.MaxValue
                : (int)cb;

            if (jsonObject["output"] is JObject output)
            {
                parsedRsp.output = new TaskStatusResponse<TInput, TOutput>.TaskOutput();
                if (output["data"] is JObject dataOutput && dataOutput["result"] is JObject resultOutput)
                {
                    parsedRsp.output.data.result = resultOutput.ToObject<TOutput>();
                }
            }
            return parsedRsp;
        }
    }
}