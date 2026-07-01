using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using GraphProcessor;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.AIGraph.Backend;
using UnityEngine.Networking;

namespace UnityEditor.AIGraph
{
    public class NodeTemplateGenerator
    {
        public struct NodeDefinition
        {
            public string name;
            public string nodeMenu;
            public List<Parameter> inputParams;
            public List<Parameter> outputParams;
            public string description;
            public string endPoint;
            public string assetPrefix;
            public string artifactType;
            public string previewType;
            public string tag; // specify version
        }

        public struct Parameter
        {
            public string name;
            public string type;
            [CanBeNull] public string tooltip;
            public bool required;
            [CanBeNull] public string defaultValue;
            [CanBeNull] public string displayName;
            [CanBeNull] public List<string> choices;
        }

        public struct NodeConfig
        {
            public List<NodeDefinition> nodes;
            public string folder;
        }

        public static void GenerateCSharpTemplate(NodeConfig nodeConfig, string outputDirectory)
        {
            var orgNodeConfigs = LocalNodeConfigs.nodeConfigs;
            foreach (var node in nodeConfig.nodes)
            {
                var className = node.name.Replace("Node", "");
                var existNodePath = NodeProvider.GetNodePath(className + "Node");
                if (!string.IsNullOrEmpty(existNodePath))
                {
                    // compare tag
                    orgNodeConfigs.nodeTags.TryGetValue(className, out var curTag);
                    if (curTag == node.tag)
                        continue;
                    if (!string.IsNullOrEmpty(curTag))
                        File.Delete(existNodePath);
                }
                GenerateNodeClass(node, out var nodeContent, out var viewContent);

                // 创建输出目录（如果不存在）
                var nodeFolder = Path.Combine(outputDirectory, nodeConfig.folder);
                Directory.CreateDirectory(nodeFolder);

                // 写入C#文件
                var fileName = $"{className}Node.cs";
                var nodePath = Path.Combine(nodeFolder, fileName);
                File.WriteAllText(nodePath, nodeContent);
                orgNodeConfigs.nodeTags[className] = node.tag;

                if (!string.IsNullOrEmpty(viewContent))
                {
                    var viewFolder = Path.Combine(outputDirectory,
                        nodeConfig.folder.Replace("Runtime", "Editor").Replace("Node", "View"));
                    Directory.CreateDirectory(viewFolder);
                    var viewPath = Path.Combine(viewFolder, $"{className}NodeView.cs");
                    File.WriteAllText(viewPath, viewContent);
                    Debug.Log($"Generated view in {viewPath}");
                }

                Debug.Log($"Generated node in {nodePath}");
            }
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        private static void GenerateNodeClass(NodeDefinition node, out string nodeContent, out string viewContent)
        {
            var sb = new StringBuilder();

            // Using directives
            sb.AppendLine(@"
using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.AIGraph.Backend;
using UnityEngine;
using System.ComponentModel;
using JetBrains.Annotations;
using UnityEngine.AIGraph.Cache;
");

            // Namespace
            sb.AppendLine(@"
namespace UnityEngine.AIGraph
{");

            // Part 1-1: Request结构体
            var shortName = node.name.Replace("Node", "");
            sb.AppendLine($"    internal struct {shortName}Request");
            sb.AppendLine("    {");
            foreach (var param in node.inputParams)
            {
                sb.AppendLine($"        public {ConvertReqType(param.type)} {param.name};");
            }

            sb.AppendLine();

            // ToString方法
            sb.AppendLine("        public override string ToString()");
            sb.AppendLine("        {");
            sb.Append("            return $\"" + $"{shortName}Request(");
            var first = true;
            var newLine = 1;
            foreach (var param in node.inputParams)
            {
                if (!first) sb.Append(", ");
                newLine++;
                if (newLine % 3 == 0)
                {
                    newLine = 0;
                    sb.AppendLine("\" +");
                    sb.Append("$\"");
                }

                if (param.type == "string" || param.type.StartsWith("List") || param.type == "Texture2D")
                {
                    sb.Append(
                        $"{param.name}={{DebugUtils.ToString({param.name})}}");
                }
                else
                    sb.Append($"{param.name}={{{param.name}}}");

                first = false;
            }

            sb.AppendLine(")\";");

            sb.AppendLine("        }");
            sb.AppendLine("    }");

            // Part 1-2: REST调用类
            sb.AppendLine($@"
    internal class {shortName}RestCall : TJAIRestCall<{shortName}Request, TaskSubmitResponse>
    {{
        public {shortName}RestCall(ServerConfig asset, int mode) : base(asset, mode) {{ }}

        public override string endPoint => ""{node.endPoint}"";
        public override IQuarkEndpoint.EMethod method => IQuarkEndpoint.EMethod.POST;
    }}");

            // Part 1-3: Response输出结构
            sb.AppendLine($"    public struct {shortName}Output");
            sb.AppendLine("    {");
            foreach (var param in node.outputParams)
            {
                sb.AppendLine($"        [CanBeNull] public {param.type} {param.name};");
            }

            sb.AppendLine();
            sb.AppendLine("        public override string ToString()");
            sb.AppendLine("        {");
            sb.Append("            return $\"" + $"{shortName}Output(");
            first = true;
            newLine = 1;
            foreach (var param in node.outputParams)
            {
                if (!first) sb.Append(", ");
                if (newLine % 3 == 0)
                {
                    newLine = 0;
                    sb.AppendLine("\" +");
                    sb.Append("$\"");
                }

                newLine++;

                sb.Append($"{param.name}={{{param.name}}}");
                first = false;
            }

            sb.AppendLine(")\";");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();

            // Part 2: IReceivedData实现
            sb.AppendLine($@"
    [Serializable]
    public class {shortName}Data : IReceivedData
    {{
        public {shortName}Output output;

        public {shortName}Data() {{ }}

        public override Object transferToUnityObject()
        {{
            return ImportUtils.Import<{node.artifactType}>(assetPath);
        }}

        public Object GetStaticPreview()
        {{
            return ImportUtils.Import<{node.previewType}>(assetPath);
        }}

        public override IEnumerator RetrieveFromBackend(int serverIndex)
        {{
            var jobStatusRestCall = new GetJobStatusRestCall<{shortName}Request, {shortName}Output>(
                serverConfig, serverIndex, ID);
            var jobInfoRequest = new TaskStatusRequest();

            var processor = new CoroutineProcessor<{shortName}Output>();
            yield return processor.ProcessAsync(BackendUtils.RetrieveFromBackendCommon(
                jobStatusRestCall, jobInfoRequest, progressCallback));
            output = processor.Result;

            tokenRemaining = jobStatusRestCall.Result.creditBalance;
            var url = string.Empty;");
            first = true;
            foreach (var param in node.outputParams.Where(param => param.type == "string"))
            {
                sb.Append(!first
                    ? $@"
            else if (!string.IsNullOrEmpty(output.{param.name}) && output.{param.name}.StartsWith(""http""))
                url = output.{param.name};"
                    : $@"
            if (!string.IsNullOrEmpty(output.{param.name}) && output.{param.name}.StartsWith(""http""))
                url = output.{param.name};");
                first = false;
            }

            sb.AppendLine(@"
            if (string.IsNullOrEmpty(url))
                throw new NullReferenceException($""No valid download url in response, task: {ID}"");
            var ext = PathUtils.GetUrlExtension(url);
            if (!assetPath.EndsWith(ext))
                assetPath += ext;
            var downloadCoroutine = BackendUtils.DownloadFromUrl(url, serverIndex);
            yield return downloadCoroutine;
            var bytes = downloadCoroutine.Current as byte[];
            if (!string.IsNullOrEmpty(assetPath))
                BackendUtils.SaveBytesToFile(bytes, assetPath);
            yield return null;
        }
    }");

            // Part 3: 节点类
            sb.AppendLine($@"
    [System.Serializable, NodeMenuItem(""{node.nodeMenu}"")]
    [UseProcessAsync]
    public class {shortName}Node : TJAIBaseAssetNode
    {{");

            // 输入参数
            var hasView = false;
            foreach (var param in node.inputParams)
            {
                var attributeName = GetAttrName(param);
                var defaultValue = string.IsNullOrEmpty(param.defaultValue)
                    ? string.Empty
                    : $"= {ParseDefaultValueFromType(param.type, param.defaultValue)}";

                if (param.choices is { Count: > 0 })
                {
                    hasView = true;
                    sb.AppendLine($"        [HideInInspector] public {param.type} {param.name} {defaultValue};");
                    sb.AppendLine($"        [HideInInspector] public List<string> {param.name}Choices =  new(){{");
                    for (var i = 0; i < param.choices.Count; i++)
                    {
                        if (i % 5 == 4)
                            sb.Append(@"
    ");
                        sb.Append($"\"{param.choices[i]}\"");
                        sb.Append(", ");
                    }

                    sb.AppendLine("        };");
                }
                else
                {
                    var showAsDrawer = IsPrimitiveType(param.type);
                    sb.AppendLine(showAsDrawer
                        ? $"        [Input(name = \"{attributeName}\"), ShowAsDrawer]"
                        : $"        [Input(name = \"{attributeName}\")]");
                    if (!string.IsNullOrEmpty(param.tooltip))
                        sb.AppendLine($"        [Tooltip(\"{param.tooltip}\")]");
                    sb.AppendLine($"        public {param.type} {param.name} {defaultValue};");
                }

                sb.AppendLine();
            }

            // 输出参数
            foreach (var param in node.outputParams)
            {
                var attributeName = GetAttrName(param);
                sb.AppendLine(
                    $"        [Output(name = \"{attributeName}\")] public {param.type} {param.name};");
            }

            sb.AppendLine();

            // 其他属性和方法
            sb.AppendLine($@"
        public override string name => LocalizationManager.Instance.GetLocalizedText(""{shortName}"");
        public override string description => ""{node.description}"";");

            // ProcessAsync方法
            sb.AppendLine(@"
        public override IEnumerator ProcessAsync()
        {");
            foreach (var param in node.inputParams.Where(p => p.required))
            {
                sb.AppendLine($@"
            if (ParaUtils.IsNull({param.name}))
                throw new ArgumentNullException(nameof({param.name}));");
            }

            sb.AppendLine($@"
            var request = new {shortName}Request
            {{");
            foreach (var param in node.inputParams.Where(p => p.type != "Texture2D"))
            {
                sb.AppendLine($"                {param.name} = {param.name},");
            }

            sb.AppendLine("            };");
            foreach (var param in node.inputParams.Where(p => p.type == "Texture2D"))
            {
                sb.AppendLine($@"
            if ({param.name} != null)
                request.{param.name} = {param.name}.ToBase64();");
            }

            sb.AppendLine($@"
            var restCall = new {shortName}RestCall(serverConfig, serverIndex);
            yield return GenerateRestCall(request, restCall);
        }}

        public IEnumerator Generate() => ProcessAsync();

        [Save(ReceivedDataType = typeof({shortName}Data))]
        [Preview, SerializeField, HideInInspector]");
            var previewName = GetPreviewVariableName(node.previewType);
            sb.AppendLine($@"        [Output(name = ""{ObjectNames.NicifyVariableName(previewName)}"")]
        protected {node.previewType} m_{previewName};

        public {node.previewType} {previewName}
        {{
            get => m_{previewName};
            set
            {{
                if (m_{previewName} != value)
                {{
                    m_{previewName} = value;
                    this?.NotifyFieldChanged(""m_{previewName}"");
                }}
            }}
        }}

        public override bool needTrigger => true;
        public override bool isRenamable => true;
        protected const int serverIndex = 3;
        protected BaseArtifact<{node.artifactType}, {shortName}Data> artifact => (BaseArtifact<{node.artifactType}, {shortName}Data>)currentArtifact;

        protected override void Enable()
        {{
            base.Enable();
            onCancelled += () => {{ taskID = null; }};
        }}

        public override void UpdateOutputPorts()
        {{
            var cachedObj = currentArtifact.GetCacheUnityObject() as {node.previewType};
            if (cachedObj)
                {previewName} = cachedObj;");
            foreach (var param in node.outputParams)
            {
                sb.AppendLine($"            {param.name} = artifact.m_ReceivedData.output.{param.name};");
            }

            sb.AppendLine(@"        }

        public override IEnumerator RestoreHistory(string Guid)
        {");
            sb.AppendLine(
                $"            artifact.m_ReceivedData.assetPath = $\"{{GetResourceFolder()}}/{node.assetPrefix}_{{Guid}}\";");
            sb.AppendLine(@"
            return base.RestoreHistory(Guid);
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
                        $""Failed to generate artifact, task id: {response.taskId}, error message: {response.message}"");
                taskID = response.taskId;
            }");
            sb.Append($"            var data = new {shortName}Data");
            sb.Append($@"
            {{
                assetPath = $""{{GetResourceFolder()}}/{node.assetPrefix}_{{taskID}}"",
                ID = taskID, progressCallback = UpdateStatus
            }};
            var processor = new CoroutineProcessor<{node.artifactType}>();");
            sb.AppendLine(@"
            yield return processor.ProcessAsync(currentArtifact.ReadFromCache(data, serverIndex));

            if (status == NodeStatus.Init)
                yield break;
            UpdateOutputPorts();

            UpdateHistory();
            graph.tokenDataModel.UpdateToken(data.tokenRemaining);
            taskID = null;
        }

        internal override bool GetParam(out List<string> paramList)
        {
            paramList = new List<string>
            {");
            var pi = 0;
            foreach (var param in node.inputParams.Where(param =>
                         param.type != "Texture2D" && !param.name.Contains("url", StringComparison.OrdinalIgnoreCase)))
            {
                sb.Append($"$\"{param.name}: {{{param.name}}}\", ");
                if (pi % 3 == 2) sb.AppendLine();
                ++pi;
            }

            sb.AppendLine(@"
            };
            return true;
        }
    }
}");
            nodeContent = sb.ToString();
            viewContent = hasView ? GenerateNodeViewClass(node) : null;
        }

        private static string GenerateNodeViewClass(NodeDefinition node)
        {
            var shortName = node.name.Replace("Node", "");
            var nodeName = $"{shortName}Node";
            var sb = new StringBuilder();

            // Using directives for NodeView
            sb.AppendLine(@"
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEditor;
using UnityEditor.AIGraph;
using UnityEngine.UIElements;
using GraphProcessor;
");

            // NodeView类
            sb.AppendLine($@"
[NodeCustomEditor(typeof({nodeName}))]
public class {nodeName}View : TJAIBaseAssetNodeView
{{
    private {nodeName} node;");

            // 为有choices的参数生成对应的Dropdown字段
            foreach (var param in node.inputParams.Where(p => p.choices is { Count: > 0 }))
            {
                var fieldName = $"{param.name}Dropdown";
                sb.AppendLine($"    private DropdownField {fieldName};");
            }

            sb.AppendLine($@"
    public override void Enable()
    {{
        node = nodeTarget as {nodeName};
        if (node == null) return;");

            // 为有choices的参数生成对应的Dropdown控件
            foreach (var param in node.inputParams.Where(p => p.choices is { Count: > 0 }))
            {
                var fieldName = $"{param.name}Dropdown";
                var displayName = string.IsNullOrEmpty(param.displayName)
                    ? ObjectNames.NicifyVariableName(param.name.Replace("_", " "))
                    : param.displayName;

                sb.AppendLine($@"
        // {param.name} dropdown
        {fieldName} = new DropdownField(node.{param.name}Choices, 0)
        {{
            label = ""{displayName}"",
            name = ""{fieldName}"",
            tooltip = ""{param.tooltip}""
        }};
        controlsContainer.Add({fieldName});

        BindProperty<DropdownField, string, {nodeName}>(""{fieldName}"", nameof(node.{param.name}));");
            }

            // 基础设置
            if (node.name.Contains("Rigging"))
            {
                sb.AppendLine("        previewSettings.Add(\"Rigging\");");
            }
            else if (node.name.Contains("Image") || node.name.Contains("Texture"))
            {
                sb.AppendLine("        previewSettings.Add(\"Image\");");
            }

            sb.AppendLine(@"
        base.Enable();
        RefreshExpandedState();
    }
}");

            return sb.ToString();
        }

        private static Dictionary<string, string> reqTypeDict = new()
        {
            { "Texture2D", "string" }
        };

        private static string ConvertReqType(string type)
        {
            return reqTypeDict.GetValueOrDefault(type, type);
        }

        private static string GetPreviewVariableName(string type)
        {
            if (string.IsNullOrEmpty(type)) return type;
            return char.ToLower(type[0]) + type[1..];
        }

        private static bool IsPrimitiveType(string type)
        {
            return type.ToLower() switch
            {
                "int" or "integer" or "float" or "single" or "bool" or "boolean" => true,
                _ => false
            };
        }

        private static string ParseDefaultValueFromType(string type, string defaultValue)
        {
            return type == "string" ? $"\"{defaultValue}\"" : defaultValue;
        }

        private static string GetAttrName(Parameter param)
        {
            if (!string.IsNullOrEmpty(param.displayName))
                return param.displayName;
            var attrName = param.name;
            if (attrName.Contains("_"))
                attrName = StringUtils.UnderScoreToPascalCase(attrName);
            return ObjectNames.NicifyVariableName(attrName);
        }
    }
}