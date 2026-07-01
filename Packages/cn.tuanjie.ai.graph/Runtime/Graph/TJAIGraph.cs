using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GraphProcessor;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.AIGraph
{
    public enum NodeInheritanceMode
    {
        InheritFromGraph = -1,
        InheritFromParent = -2,
        InheritFromChild = -3,
    }

    [Serializable]
    public class TJAIGraph : BaseGraph
    {
#if UNITY_EDITOR
        [InitializeOnLoad]
        public static class PlayModeNotifier
        {
            static PlayModeNotifier()
            {
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            }

            private static string processingNode;

            private static void OnPlayModeStateChanged(PlayModeStateChange state)
            {
                if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode)
                {
                    int maxOrder = -1;
                    processingNode = string.Empty;
                    var allSO = Resources.FindObjectsOfTypeAll<TJAIGraph>();
                    foreach (var so in allSO)
                    {
                        if (so.isEnabled)
                        {
                            so.Save();
                            foreach (var node in so.nodes)
                            {
                                if (node.GetType().IsSubclassOf(typeof(BaseVastModelNode)) &&
                                    node.isTriggered && node.computeOrder > maxOrder)
                                {
                                    maxOrder = node.computeOrder;
                                    processingNode = so.name + node.name;
                                }
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(processingNode))
                        EditorPrefs.SetString(GraphProcessor.GlobalConstants.AI_PROCESSING_NODE_KEY, processingNode);
                    // DebugUtils.ConditionLog($"Set processing node: {processingNode}");
                }
            }
        }
#endif

        public TJAINodeSetting settings = new TJAINodeSetting()
        {
            // Default graph values:
            width = 512,
            height = 512,
            depth = 1,
            widthScale = 1,
            heightScale = 1,
            depthScale = 1,
            dimension = OutputDimension.Texture2D,
            outputChannels = OutputChannel.RGBA,
            outputPrecision = OutputPrecision.Half,
        };

        private string _mainAssetPath;
        public string mainAssetPath
        {
            get
            {
#if UNITY_EDITOR
                if (string.IsNullOrEmpty(_mainAssetPath))
                    _mainAssetPath = AssetDatabase.GetAssetPath(this);
#endif
                return _mainAssetPath;
            }
            private set { _mainAssetPath = value; }
        }

        // Important: note that order is not guaranteed 
        [SerializeField] List<Texture> _outputTextures = null;

        [SerializeField, HideInInspector] public HistoryAssets history = null;
        [SerializeField, HideInInspector] public TokenDataModel tokenDataModel = null;

        public List<Texture> outputTextures
        {
            get
            {
#if UNITY_EDITOR
                if (_outputTextures == null || _outputTextures.Count == 0)
                    _outputTextures = AssetDatabase.LoadAllAssetsAtPath(mainAssetPath).OfType<Texture>().ToList();
#endif
                _outputTextures.RemoveAll(t => t == null);

                return _outputTextures;
            }
        }

        Texture _mainOutputTexture;

        public Texture mainOutputTexture
        {
            get
            {
#if UNITY_EDITOR
                if (_mainOutputTexture == null)
                    _mainOutputTexture = AssetDatabase.LoadAssetAtPath<Texture>(mainAssetPath);
#endif
                return _mainOutputTexture;
            }
            set
            {
                outputTextures.Remove(_mainOutputTexture);
                outputTextures.Add(value);
                _mainOutputTexture = value;
            }
        }

        public NodeInheritanceMode defaultNodeInheritanceMode = NodeInheritanceMode.InheritFromParent;

        protected override void OnEnable()
        {
            MigrateGraph();
            SanitizeSettings();
            InitializeHistoryAssets();
            InitializeTokenDataModel();
            base.OnEnable();
        }

        public override void OnAssetDeleted()
        {
            DisposeHistoryAssets();
            // check if resource folder is current graph
            if (!string.IsNullOrEmpty(resourceFolder) &&
                (resourceFolder.Contains(mainAssetPath) || resourceFolder.Contains(GetInstanceID().ToString())))
            {
                if (Directory.Exists(resourceFolder))
                    Directory.Delete(resourceFolder, true);
                var metaFile = $"{resourceFolder}.meta";
                if (File.Exists(metaFile))
                    File.Delete(metaFile);
            }
            base.OnAssetDeleted();
        }
        
        [SerializeField, HideInInspector]
        private string resourceFolder;
        public string GetResourceFolder()
        {
            if (!string.IsNullOrEmpty(resourceFolder)) return resourceFolder;
#if UNITY_EDITOR
            // step 1: get graph guid
            var assetPath = AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(assetPath))
            {
                resourceFolder = Path.Combine(GlobalConstants.AI_GRAPH_FOLDER, GetInstanceID().ToString());
                return resourceFolder;
            }
            // step 2: combine path: {AI_GRAPH_FOLDER}/{graphID}
            var graphID = AssetDatabase.AssetPathToGUID(assetPath);
            // NOTE: graph instanceID may change when project is close and reopen
            resourceFolder = Path.Combine(GlobalConstants.AI_GRAPH_FOLDER,
                !string.IsNullOrEmpty(graphID) ? graphID : GetInstanceID().ToString());
            if (!Directory.Exists(resourceFolder)) Directory.CreateDirectory(resourceFolder);
            return resourceFolder;
#else
            return resourceFolder = Application.dataPath;
#endif
        }

        void MigrateGraph()
        {
            foreach (var node in nodes)
            {
                // Migrate node settings
                if (node is not SDNode n) continue;
                if (n.settings.outputChannels == 0)
                    n.settings.outputChannels = OutputChannel.InheritFromGraph;
                if (n.settings.outputPrecision == 0)
                    n.settings.outputPrecision = OutputPrecision.InheritFromGraph;
                if (n.settings.dimension == 0)
                    n.settings.dimension = OutputDimension.InheritFromParent;
                if (n.settings.sizeMode == 0)
                    n.settings.sizeMode = OutputSizeMode.InheritFromParent;
                if (n.settings.widthScale == 0)
                    n.settings.widthScale = 1;
                if (n.settings.heightScale == 0)
                    n.settings.heightScale = 1;
                if (n.settings.depthScale == 0)
                    n.settings.depthScale = 1;
            }
            settings.refreshMode = RefreshMode.EveryXMillis;
        }

        void SanitizeSettings()
        {
            // Avoid undefined values in settings
            if (settings.outputChannels.Inherits())
                settings.outputChannels = OutputChannel.RGBA;
            if (settings.outputPrecision.Inherits())
                settings.outputPrecision = OutputPrecision.Half;
            if (settings.dimension.Inherits())
                settings.dimension = OutputDimension.Texture2D;
            if (settings.wrapMode.Inherits())
                settings.wrapMode = OutputWrapMode.Mirror;
            if (settings.filterMode.Inherits())
                settings.filterMode = OutputFilterMode.Trilinear;
            if (settings.sizeMode.Inherits())
                settings.sizeMode = OutputSizeMode.Absolute;
            if (settings.potSize == 0)
                settings.SetPOTSize(512);
            if (settings.widthScale == 0)
                settings.widthScale = 1;
            if (settings.heightScale == 0)
                settings.heightScale = 1;
            if (settings.depthScale == 0)
                settings.depthScale = 1;

            settings.editFlags = EditFlags.TargetFormat;
        }

        void InitializeHistoryAssets()
        {
            if (history == null)
                history = new HistoryAssets(this);
            history.OnEnable();
        }

        void DisposeHistoryAssets()
        {
            if (history == null)
                return;
            history.Dispose();
            history = null;
        }

        void InitializeTokenDataModel()
        {
            if (tokenDataModel == null)
                tokenDataModel = new TokenDataModel();
        }

        public override void Save()
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(mainAssetPath) || mainAssetPath.StartsWith(GlobalConstants.AI_GRAPH_TMP_FOLDER))
            {
                var filePath = EditorUtility.SaveFilePanelInProject(
                    "Save AIGraph", name, "asset", "Choose FilePath"
                );
                if (string.IsNullOrEmpty(filePath))
                    return;

                try
                {
                    var directory = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    if (string.IsNullOrEmpty(mainAssetPath))
                    {
                        AssetDatabase.CreateAsset(this, filePath);
                    }
                    else
                    {
                        AssetDatabase.MoveAsset(mainAssetPath, filePath);
                    }

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    mainAssetPath = filePath;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Exception while Save AIGraph: {e.Message}");
                    EditorUtility.DisplayDialog("Exception", $"Fail to Save: {e.Message}", "Cancel");
                }
            }
#endif
            base.Save();
        }
    }
}