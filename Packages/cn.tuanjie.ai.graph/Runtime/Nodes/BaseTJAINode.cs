/******************************************************************************
* Company:         UnityChina
* Author:          may.luo
* CreateTime:      2024-09-18 14:21:30
* Version:         0.0.1   
* UnityVersion:    2022.3.17f1c1
* Description:
******************************************************************************/

using System.IO;
using GraphProcessor;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.AIGraph
{
    /// <summary>
    /// rewrite some style
    /// compared with SDNode, BaseTJAINode strip texture related things
    /// </summary>
    public class BaseTJAINode : BaseNode
    {
        // for TJAIGraph only thing: HistoryAsset system and so on
        public new TJAIGraph graph => base.graph as TJAIGraph;
        // pin the node with fixed position in graph
        [HideInInspector] public bool isPinned = false;

        protected float m_nodeWidth = SDUtil.defaultNodeWidth;
        /// <summary>
        /// return 0f if you want a flexible width, else fixed size
        /// </summary>
        public virtual float nodeWidth { get => m_nodeWidth; set => m_nodeWidth = value; }

        private string resourceFolder;
        public string GetResourceFolder()
        {
            if (!string.IsNullOrEmpty(resourceFolder)) return resourceFolder;
            resourceFolder = Path.Combine(graph.GetResourceFolder(), GUID);
            if (!Directory.Exists(resourceFolder)) Directory.CreateDirectory(resourceFolder);
            return resourceFolder;
        }
        protected override void Destroy()
        {
            base.Destroy();
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(resourceFolder)) return;
            if (Directory.Exists(resourceFolder))
                Directory.Delete(resourceFolder, true);
            var metaFile = $"{resourceFolder}.meta";
            if (File.Exists(metaFile))
                File.Delete(metaFile);
            AssetDatabase.Refresh();
#endif
        }
    }
}