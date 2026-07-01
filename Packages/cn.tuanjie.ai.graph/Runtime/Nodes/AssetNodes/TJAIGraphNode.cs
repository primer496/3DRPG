/******************************************************************************
* Company:         UnityChina
* Author:          may.luo
* CreateTime:      2024-09-29 16:57:55
* Version:         0.0.1   
* UnityVersion:    2022.3.17f1c1
* Description:
******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;

#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif

namespace UnityEngine.AIGraph
{
    /// <summary>
    /// sub graph, use graph as node
    /// </summary>
    [Serializable, NodeMenuItem("Asset/TJAI Graph")]
    public class TJAIGraphNode : BaseTJAINode, ICreateNodeFrom<TJAIGraph>
    {
        [Input("InputParams")] public List<object> inputParams = new();

        [Output("OutputParams")] public List<object> outputParams = new();

        public override string name => LocalizationManager.Instance.GetLocalizedText("TJAI Graph");
        public override bool isRenamable => true;

        [HideInInspector] public TJAIGraph subGraph;
        TJAIGraphProcessor subProcessor;
#if UNITY_EDITOR
        EditorCoroutine subCoroutine;
#else
        // TODO: runtime needs monobehavior to start coroutine
        Coroutine subCoroutine;
#endif

        /// <summary>
        /// Called when the node is enabled
        /// </summary>
        protected override void Enable()
        {
            if (subGraph != null)
            {
                subProcessor = new TJAIGraphProcessor(subGraph);
                description = subGraph.description;
            }
            base.Enable();
        }

        /// <summary>
        /// Called when the node is disabled
        /// </summary>
        protected override void Disable()
        {
            OnCancel();
        }

        /// <summary>
        /// ref from TJAIGraphProcessorView
        /// </summary>
        void OnCancel()
        {
            if (subCoroutine != null)
            {
#if UNITY_EDITOR
                EditorCoroutineUtility.StopCoroutine(subCoroutine);
#else
                CoroutineManager.Instance.StopCoroutine(subCoroutine);
#endif
                subProcessor.AfterInterruption();
                subCoroutine = null;
            }
        }

        /// <summary>
        /// Override this method to implement custom processing
        /// </summary>
        public override void Process()
        {
            if (subGraph == null)
                return;
            if (subProcessor == null)
                subProcessor = new TJAIGraphProcessor(subGraph);
            OnCancel();
            subProcessor.ResetAll();
#if UNITY_EDITOR
            subCoroutine = EditorCoroutineUtility.StartCoroutine(subProcessor.RunAllAsync(), this);
#else
            subCoroutine = CoroutineManager.Instance.StartCoroutine(subProcessor.RunAllAsync());
#endif
        }

        public bool InitializeNodeFromObject(TJAIGraph refGraph)
        {
            if (refGraph == null)
                return false;
            Debug.Log($"create node from graph: {refGraph.name}");
            subGraph = refGraph;
//#if UNITY_EDITOR
//            if (EditorUtility.DisplayDialog(
//                "Create Graph Node", "How would you like to user this TJAIGraph asset?",
//                "Clone", "Reference"))
//            {
//                // method 1: use graph's copy
//                subGraph = ScriptableObject.Instantiate(refGraph);
//                // make sure copy will not auto save
//                subGraph.hideFlags = HideFlags.HideAndDontSave;
//            } else
//            {
//                // method 2: use graph reference
//                subGraph = refGraph;
//            }
//#else
//                // default is Clone
//                subGraph = ScriptableObject.Instantiate(refGraph);
//                subGraph.hideFlags = HideFlags.HideAndDontSave;
//#endif
            SetCustomName(subGraph.name);
            subProcessor = new TJAIGraphProcessor(subGraph);
            return true;
        }

        [CustomPortInput(nameof(inputParams), typeof(object))]
        public void PullSubGraphInput(List<SerializableEdge> edges, NodePort outputPort = null)
        {
            if (subGraph == null || edges.Count == 0)
                return;
            SerializableEdge edge = edges.First();
            subGraph.UpdateExposedParameter(edge.inputPortIdentifier, edge.passThroughBuffer);
        }

        [CustomPortBehavior(nameof(inputParams))]
        IEnumerable<PortData> CreateInputPortsBySubGraph(List<SerializableEdge> edges)
        {
            if (subGraph == null)
                yield break;
            foreach (ExposedParameter para in subGraph.exposedParameters)
            {
                if (para.settings.accessor == ParameterAccessor.Get)
                {
                    yield return new PortData
                    {
                        displayName = para.name,
                        displayType = para.GetValueType(),
                        identifier = para.guid
                    };
                }
            }
        }

        [CustomPortOutput(nameof(outputParams), typeof(object))]
        public void PushSubGraphOutput(List<SerializableEdge> edges)
        {
            if (subGraph == null || edges.Count == 0)
                return;
            foreach (var edge in edges)
            {
                if (edge.passThroughBuffer == null)
                    continue;
                edge.passThroughBuffer = subGraph.GetExposedParameterFromGUID(edge.outputPortIdentifier)?.value;
            }
        }

        [CustomPortBehavior(nameof(outputParams))]
        IEnumerable<PortData> CreateOutputPortsBySubGraph(List<SerializableEdge> edges)
        {
            if (subGraph == null)
                yield break;
            foreach (ExposedParameter para in subGraph.exposedParameters)
            {
                if (para.settings.accessor == ParameterAccessor.Set)
                {
                    yield return new PortData
                    {
                        displayName = para.name,
                        displayType = para.GetValueType(),
                        identifier = para.guid
                    };
                }
            }
        }

    }
}