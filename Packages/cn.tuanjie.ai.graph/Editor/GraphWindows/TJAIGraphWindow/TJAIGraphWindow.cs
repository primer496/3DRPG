using System;
using System.Collections;
using System.IO;
using GraphProcessor;
using Unity.EditorCoroutines.Editor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.AIGraph.Backend;
using GlobalConstants = UnityEngine.AIGraph.GlobalConstants;

namespace UnityEditor.AIGraph
{
    public class TJAIGraphWindow : BaseGraphWindow
    {     
        public TJAIGraph GetCurrentGraph() => graph as TJAIGraph;

        protected override void OnDestroy()
        {
            --curWindowSize;
            var curGraph = GetCurrentGraph();
            if (curGraph == null)
                return;
            var path = curGraph.mainAssetPath;

            if (string.IsNullOrEmpty(path) || path.StartsWith(GlobalConstants.AI_GRAPH_TMP_FOLDER))
            {
                int result = EditorUtility.DisplayDialogComplex(
                    "Unsave AIGraph",
                    $"AIGraph '{graph.name}' has not been Saved, Click Yes to Save",
                    "Yes",
                    "No",
                    "Cancel"
                );

                switch (result)
                {
                    case 0:
                        break;
                    case 1:
                        graphView?.Dispose();
                        DestroyImmediate(graph, true);
                        return;
                    case 2:
                        EditorApplication.delayCall += () =>
                        {
                            TJAIGraphWindow.Open(curGraph);
                        };
                        return;
                    default:
                        break;
                }
            }

            graphView?.SaveGraphToDisk();
            graphView?.Dispose(); 
        }

        private static string latestVersion;
        private static bool showDialog = true;

        private static IEnumerator GetLatestVersion(Action onComplete)
        {
            var restCall = new GetLatestVersionRestCall(ServerConfig.serverConfig, 3);
            yield return restCall.MakeServerRequest(null);
            // initialize nodes from server
            yield return BackendNodeFetcher.FetchNodeFromServer();
            latestVersion = restCall.Result?.Trim('"');
            onComplete?.Invoke();
        }

        private static void CheckVersion(TJAIGraph graph)
        {
            if (string.IsNullOrEmpty(latestVersion))
                EditorCoroutineUtility.StartCoroutine(GetLatestVersion(CheckVersion), graph);
            else if (showDialog)
                CheckVersion();
        }

        private static void CheckVersion()
        {
            var currentVersion = PackageVersionChecker.GetPackageVersion(GlobalConstants.PACK_NAME);
            if (PackageVersionComparer.IsNewerOrSame(currentVersion, latestVersion)) return;
            var installLatest = EditorUtility.DisplayDialogComplex(
                "Update Tuanjie AI Graph Package",
                $"Current package: {currentVersion} is older than last version: {latestVersion}.\nWould you like to install latest version now?",
                "Yes", "Don't show this message again", "Ignore");
            if (installLatest == 0)
            {
                Client.Add(GlobalConstants.PACK_NAME);
                PackageVersionChecker.GetPackageVersion(GlobalConstants.PACK_NAME, true);
            } else if (installLatest == 1)
                showDialog = false;
        }

        private static readonly int maxWindowSize = 5;
        private static int curWindowSize = 0;
        public static TJAIGraphWindow Open(TJAIGraph graph)
        {
            if (curWindowSize >= maxWindowSize)
            {
                EditorUtility.DisplayDialog("Open too much window",
                    "Open too much tuanjie ai graph window at the same time may be slow, please close some unused window and retry",
                    "OK");
                return null;
            }
            CheckVersion(graph);
            // Focus the window if the graph is already opened
            var TJAIGraphWindow = Resources.FindObjectsOfTypeAll<TJAIGraphWindow>();
            foreach (var TJAIWindow in TJAIGraphWindow)
            {
                if (TJAIWindow.graph == graph)
                {
                    TJAIWindow.Show();
                    TJAIWindow.Focus();
                    return TJAIWindow;
                }
            }
            ++curWindowSize;
            // create asset if not exist yet
            var assetPath = AssetDatabase.GetAssetPath(graph);
            if (string.IsNullOrEmpty(assetPath))
            {
                assetPath = ExportUtils.GetUniquePath(GlobalConstants.AI_GRAPH_TMP_FOLDER, graph.name.Replace("/", " "), "asset");
                assetPath = ExportUtils.GetAssetPath(assetPath);
                var dir = Path.GetDirectoryName(assetPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(GlobalConstants.AI_GRAPH_TMP_FOLDER);
                    AssetDatabase.Refresh();
                }

                AssetDatabase.CreateAsset(graph, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                AssetDatabase.ImportAsset(assetPath);
            }

            var graphWindow = EditorWindow.CreateWindow<TJAIGraphWindow>();
            graphWindow.graph = graph;
            graphWindow.Show();
            graphWindow.Focus();

            return graphWindow;
        }

        public TJAIToolbarView toolBarView;

        public void UpdateTitle()
        {
            if (EditorGUIUtility.isProSkin)
            {
                titleContent = new GUIContent(graph.name,
                    Resources.Load<Texture>("Icons/windowIcon@32x32_dark"));
            }
            else
            {
                titleContent = new GUIContent(graph.name,
                    Resources.Load<Texture>("Icons/windowIcon@32x32_light"));
            }
        }

        protected override bool InitializeWindow(BaseGraph graph)
        {
            if (!UnityConnectProxy.instance.IsLoggedIn())
            {
                if (EditorUtility.DisplayDialog("Login Required", "You are not logged in. Please log in to continue.", "Close"))
                {
                    // 用户点击 "Close" 关闭弹窗，同时关闭窗口  
                    return false;
                }
            }
            UpdateTitle();

            if (graphView == null)
            {
                graphView = new TJAIGraphView(this);
                // 左上角执行/自动更新 etc 的工具栏
                toolBarView = new TJAIToolbarView(graphView);
                // 小地图
                graphView.Add(new MiniMapView(graphView));
                graphView.Add(toolBarView);
                graphView.Add(new TokenWindowView(graphView, ((TJAIGraph)graph).tokenDataModel));
                graphView.Add(new FeedbackView());
            }

            rootView.Add(graphView);

            foreach (var node in graph.nodes)
            {
                if (node is TJAIBaseAssetNode ainode)
                    ainode.Refresh();
            }
            return true;
        }
    }

    class TJAIGraphWindowTitleSync : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            // Inspect assets-moved event, and update title tab for TJAIGraph window
            bool assetsMoved = movedAssets.Length > 0;
            if (assetsMoved)
            {
                var TJAIGraphWindow = Resources.FindObjectsOfTypeAll<TJAIGraphWindow>();
                foreach(var window in TJAIGraphWindow)
                {
                    window.UpdateTitle();
                }
            }
        }
    }
}