using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GraphProcessor;
using UnityEditor;

namespace UnityEngine.AIGraph
{
    /// <summary>
    /// 历史资产管理系统，基于节点进行管理，调用方法：
    /// step 1：NodeView继承自TJAIBaseAssetNodeView，会出现<历史存储>的toggle按钮
    /// step 2：在Node的Process/ProcessAsync函数调用UpdateHistory() 
    /// </summary>
    [Serializable]
    public class HistoryAssets
    {
        public enum HistoryChangeType
        {
            Register,
            Unregister,
            Modify,
            Rename
        }

        public event Action<HistoryChangeType, BaseNode[]> onHistoryChanged;

        public Action onRefreshHistoryView;

        [SerializeField, SerializeReference]
        protected TJAIGraph graph;

        [SerializeField, SerializeReference]
        internal NodeAssetsCache assetsCache;

        [SerializeField, SerializeReference]
        public List<BaseNode> orderedNodes;

        public int Count => orderedNodes.Count;

        public HistoryAssets(TJAIGraph graph)
        {
            this.graph = graph;
        }

        public void OnEnable()
        {
            assetsCache ??= new();
            orderedNodes ??= new();

            if (Count != assetsCache.Count)
            {
                SDUtil.LogError("Inconsistency occured in HistoryAssets. Clear it.");
                // Q：看起来assetsCache是基于内存的cache，是不是项目关了就没了？
                // A:加了SerializeField属性，其内容会序列化到TJAIgraph.asset，会保留的
                assetsCache.Clear();
                orderedNodes.RemoveAll(o => o == null);
                foreach (var node in orderedNodes)
                {
                    assetsCache.Add(node, new NodeHistory());
                }
            }

            //onHistoryChanged -= ReportBehavior;
            //onHistoryChanged += ReportBehavior;

            if (graph)
            {
                graph.onGraphChanges -= OnGraphChanged;
                graph.onGraphChanges += OnGraphChanged;
            }

        }
    
        public void Dispose()
        {
            foreach (var item in assetsCache)
            {
                foreach(var preview in item.Value.IDToPreview)
                    preview.ClearMetadata();
            }
        }

        public bool RegisterNode(BaseNode node)
        {
            bool succ = assetsCache.TryAdd(node, new NodeHistory());
            if (succ)
            {
                orderedNodes.Add(node);
                onHistoryChanged?.Invoke(HistoryChangeType.Register, new BaseNode[] { node });
            }

            return succ;
        }

        public bool UnregisterNode(BaseNode node)
        {
            bool succ = node != null ? assetsCache.Remove(node) : false;
            if (succ)
                succ = orderedNodes.Remove(node);
            if (succ)
                onHistoryChanged?.Invoke(HistoryChangeType.Unregister, new BaseNode[] { node });

            return succ;
        }

        public bool IsRegistered(BaseNode node) => assetsCache.ContainsKey(node);

        public bool AddAsset(TJAIBaseAssetNode node, BaseArtifact artifact)
        {
            bool succ = IsRegistered(node);
            if (succ)
            {
                artifact.GetStaticPreview(staticPreview =>
                {
                    if (staticPreview != null)
                    {
                        bool hasParamList = node.GetParam(out var paramList);

                        var newPreviewData = new previewData(artifact.GetGuID(), hasParamList, paramList, staticPreview);
                        newPreviewData.UpdateMetadata(node.GetResourceFolder());
                        assetsCache[node].IDToPreview.Insert(0, newPreviewData);

                        onRefreshHistoryView?.Invoke();
                        onHistoryChanged?.Invoke(HistoryChangeType.Modify, new BaseNode[] { node });
                    }
                    else
                    {
                        artifact.GetDefaultStaticPreview(staticPreview =>
                        {
                            if (staticPreview != null)
                            {
                                bool hasParamList = node.GetParam(out var paramList);

                                var newPreviewData = new previewData(artifact.GetGuID(), hasParamList, paramList, staticPreview);
                                newPreviewData.UpdateMetadata(node.GetResourceFolder());
                                assetsCache[node].IDToPreview.Insert(0, newPreviewData);
                            }
                            onRefreshHistoryView?.Invoke();
                            onHistoryChanged?.Invoke(HistoryChangeType.Modify, new BaseNode[] { node });
                        }, this);
                    }

                }, this);
            }

            return succ;
        }

        public bool RemoveAsset(BaseNode node, BaseArtifact artifact)
        {
            var removeItems = assetsCache[node].IDToPreview.FindAll(pair => pair.Guid == artifact.GetGuID());
            foreach (var item in removeItems)
            {
                item.ClearMetadata();
            }
            
            // REVIEW：需要先check一下node是否注册，否则会报错
            bool succ = assetsCache[node].IDToPreview.RemoveAll(pair => pair.Guid == artifact.GetGuID()) != 0;
            if (succ)
                onHistoryChanged?.Invoke(HistoryChangeType.Modify, new BaseNode[] { node });

            return succ;
        }

        public bool ClearAssets(BaseNode node)
        {
            bool succ = assetsCache.TryGetValue(node, out NodeHistory assetsList);
            if (succ)
            {
                foreach(var item in assetsList.IDToPreview)
                {
                    item.ClearMetadata();
                }

                assetsList.IDToPreview.Clear();
                onHistoryChanged?.Invoke(HistoryChangeType.Modify, new BaseNode[] { node });
            }

            return succ;
        }

        public bool RemoveSelectedAssets(Dictionary<BaseNode, IReadOnlyList<previewData>> selections)
        {
            bool succ = true;

            foreach (var p in selections)
            {
                var node = p.Key;
                if (IsRegistered(node))
                {
                    foreach (var artifact in p.Value)
                    {
                        artifact.ClearMetadata();
                        if (!assetsCache[node].IDToPreview.Remove(artifact))
                            succ = false;
                    }
                }
                else
                {
                    succ = false;
                }
            }

            onHistoryChanged?.Invoke(HistoryChangeType.Modify, selections.Keys.ToArray());
            return succ;
        }

        void OnGraphChanged(GraphChanges changes)
        {
            if (changes.removedNode != null && IsRegistered(changes.removedNode))
            {
                UnregisterNode(changes.removedNode);
            }
            else if (changes.addedNode != null)
            {
                var node = changes.addedNode as TJAIBaseAssetNode;
                if (node != null && node.saveHistory)
                {
                    RegisterNode(node);
                }
            }
            else if (changes.nodeRenamed != null && IsRegistered(changes.nodeRenamed))
            {
                onHistoryChanged?.Invoke(HistoryChangeType.Rename, new BaseNode[] { changes.nodeRenamed });
            }
        }

        void ReportBehavior(HistoryChangeType type, BaseNode[] nodes)
        {
            string report = type.ToString() + ": ";
            foreach (var node in nodes)
                report += node.GetCustomName() + " ";
            Debug.Log(report);
        }
    }

    [Serializable]
    public class previewData
    {
        public string Guid;
        public bool HasInfo;
        public List<string> Settings;
        public Texture2D StaticPreview;

        public previewData()
        {
            Guid = string.Empty;
            StaticPreview = null;
        }

        public previewData(string _Guid, bool _HasInfo, List<string> _Settings, Texture2D _StaticPreview)
        {
            Guid = _Guid;
            HasInfo = _HasInfo;
            StaticPreview = _StaticPreview;
            Settings = _Settings;
        }

        public void UpdateMetadata(string dir)
        {
#if UNITY_EDITOR
            if (StaticPreview == null) return;
            var assetPath = Path.Combine(dir, $"preview_{Guid}.png".Replace("\\", "/"));
            if (!ExportUtils.SaveTextureAsset(StaticPreview, assetPath)) return;
            AssetDatabase.Refresh();
            StaticPreview = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            // string directory = GlobalConstants.AI_PREVIEW_FOLDER;
            // if (!Directory.Exists(directory))
            // {
            //     Directory.CreateDirectory(directory);
            // }
            //
            // if (StaticPreview != null)
            // {
            //     string path = Path.Combine(GlobalConstants.AI_PREVIEW_FOLDER, $"{Guid}.png").Replace("\\", "/");
            //     if (ExportUtils.SaveTextureAsset(StaticPreview as Texture2D, path))
            //     {
            //         AssetDatabase.Refresh();
            //         StaticPreview = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            //     }
            // }
#endif
        }

        public void ClearMetadata()
        {
#if UNITY_EDITOR
            if (StaticPreview == null) return;
            var assetPath = AssetDatabase.GetAssetPath(StaticPreview);
            if (string.IsNullOrEmpty(assetPath)) return;
            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.Refresh();
            // string directory = GlobalConstants.AI_PREVIEW_FOLDER;
            // if (!Directory.Exists(directory))
            // {
            //     return;
            // }
            //
            // string path = Path.Combine(GlobalConstants.AI_PREVIEW_FOLDER, $"{Guid}.png").Replace("\\", "/");
            // if (File.Exists(path))
            // {
            //     AssetDatabase.DeleteAsset(path);
            //     AssetDatabase.Refresh();
            // }
#endif
        }
    }

    /// <summary>
    /// Wrapper of List, which is used to tackle the problem of serializing nested List in Dictionary
    /// </summary>
    [Serializable]
    class NodeHistory
    {
        [SerializeField]
        public bool expanded;

        /// <summary>
        /// [SerializeField, SerializeReference]：这个属性用于在Unity编辑器中显示并允许引用类型的序列化。
        /// m_List被标记为SerializeReference意味着在序列化时，m_List中的每个previewData对象都会被单独序列化，而不是作为NodeHistory类的一部分进行序列化。
        /// </summary>
        [SerializeField, SerializeReference]
        private List<previewData> m_IDWithPreview;

        // list => m_List为表达式主体定义，表示属性的get访问器将返回右侧表达式的值，可用于简化只读属性的声明
        public List<previewData> IDToPreview => m_IDWithPreview;

        public int Count => m_IDWithPreview.Count;

        public NodeHistory(bool expanded = true)
        {
            this.expanded = expanded;
            m_IDWithPreview = new List<previewData>(10);
        }
    }

    [Serializable]
    class NodeAssetsCache : SerializedReferenceDictionary<BaseNode, NodeHistory>
    { }
}
