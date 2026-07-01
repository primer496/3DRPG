using System;
using System.Collections;
using GraphProcessor;
using Unity.EditorCoroutines.Editor;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

namespace UnityEditor.AIGraph
{
    public class TJAIGraphProcessorView : ProcessorView
    {
        TJAIGraphProcessor processor;

        BaseGraphView graphView;

        Unity.EditorCoroutines.Editor.EditorCoroutine coroutine;

        bool isRunning => coroutine != null;

        // bool initRun = true;

        public bool autoUpdate = false;

        BaseNode nodeToProcess = null;

        public TJAIGraphProcessorView()
        {

            title = "TJAI Process Panel";
            style.display = DisplayStyle.None;
        }

        protected override void Initialize(BaseGraphView graphView)
        {
            processor = new TJAIGraphProcessor(graphView.graph);
            this.graphView = graphView;
            coroutine = null;

            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        void OnAttach(AttachToPanelEvent evt)
        {
            this.graphView.computeOrderUpdated -= ComputeOrderUpdated;
            this.graphView.computeOrderUpdated += ComputeOrderUpdated;

            this.graphView.nodeTriggered -= OnTrigger;
            this.graphView.nodeTriggered += OnTrigger;

            this.graphView.nodeCancelled -= OnPause;
            this.graphView.nodeCancelled += OnPause;

            this.graphView.graph.onGraphChanges -= OnGraphChanges;
            this.graphView.graph.onGraphChanges += OnGraphChanges;

            Undo.undoRedoPerformed -= OnUndoRedo;
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        void OnDetach(DetachFromPanelEvent evt)
        {
            this.graphView.computeOrderUpdated -= ComputeOrderUpdated;
            this.graphView.nodeTriggered -= OnTrigger;
            this.graphView.nodeCancelled -= OnPause;
            this.graphView.graph.onGraphChanges -= OnGraphChanges;
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        Unity.EditorCoroutines.Editor.EditorCoroutine StartCoroutine(IEnumerator routine)
        {
            return Unity.EditorCoroutines.Editor.EditorCoroutineUtility.StartCoroutine(routine, graphView);
        }

        public void OnReset()
        {
            OnCancel();
            processor.ResetAll();
        }

        /// <summary>
        /// 执行整个图，跳过已经执行的节点
        /// </summary>
        public void OnRunAll()
        {
            OnCancel();
            // processor.ResetAll();
            coroutine = StartCoroutine(processor.RunAllAsync(forceRunInTrigger: true));
        }

        /// <summary>
        /// 单步执行，一个一个节点执行，trigger节点也强制执行
        /// </summary>
        public void OnRunStep()
        {
            OnPause();
            processor.ResetFrom(processor.Current);
            coroutine = StartCoroutine(processor.RunFromAsync(processor.Current, singleStep: true, forceTrigger: true));
        }

        public void OnContinue()
        {
            OnPause();
            processor.ResetFrom(processor.Current);
            coroutine = StartCoroutine(processor.RunFromAsync(processor.Current));
        }

        public void OnPause()
        {
            if (isRunning)
            {
                Unity.EditorCoroutines.Editor.EditorCoroutineUtility.StopCoroutine(coroutine);
                processor.AfterInterruption();
                coroutine = null;
            }
            graphView.OnPause();
        }

        /// <summary>
        /// graph层面暂停会触发的行为
        /// </summary>
        public void OnCancel()
        {
            if (!isRunning) return;
            Unity.EditorCoroutines.Editor.EditorCoroutineUtility.StopCoroutine(coroutine);
            processor.AfterCancellation();
            coroutine = null;
        }

        public void OnAutoUpdate(bool enable)
        {
            autoUpdate = enable;

            if (enable)
            {
                OnCancel();
                OnContinue();
            }
        }

        /// <summary>
        /// 面向触发型节点，点击节点右上方触发按钮时调用
        /// </summary>
        /// <param name="node"></param>
        void OnTrigger(BaseNode node)
        {
            OnCancel();

            var descendants = processor.FindDescendants(node);
            processor.ResetFrom(node, descendants);
            var ancestors = processor.FindAncestors(node);
            foreach (var preNode in ancestors)
            {
                if (preNode.status == NodeStatus.Done) continue;
                preNode.InvokeOnReady();
            }
            coroutine = StartCoroutine(processor.RunToAsync(ancestors));
        }

        /// <summary>
        /// 尝试触发节点并往下执行
        /// </summary>
        /// <param name="node"></param>
        /// <param name="skipFirst"></param>
        [Obsolete("AutoUpdate is not supported yet")]
        void OnSoftTrigger(BaseNode node, bool skipFirst = false)
        {
            //SDUtil.Log("SoftTrigger @ " + node.GetCustomName() + (skipFirst ? " (skip first node)" : ""));
            OnCancel();

            var descendants = processor.FindDescendants(node);
            if (skipFirst)
            {
                node.UpdateStatus(NodeStatus.Done);
                descendants.Remove(node);
            }
            processor.ResetFrom(node, descendants);
            processor.Current = null;

            if (autoUpdate)
            {
                coroutine = StartCoroutine(processor.RunFromAsync(node, jobNodes: descendants, skipFirst: skipFirst));
            }
        }

        void ComputeOrderUpdated()
        {
            try
            {
                processor.UpdateComputeOrder();
            }
            catch (Exception e)
            {
                SDUtil.LogError(e.Message);
                // Loop or other node disorder is detected, cancel soft trigger anyway.
                // Throw this exception up, and let edge listener to catch it to disconnect the edge.
                nodeToProcess = null;
                throw e;
            }
        }

        void OnGraphChanges(GraphChanges gc)
        {
            // Listen to graph changes.
            // Edge adding/removing & node adding/changing/changed are important events for processor refeshing.
            // For events that result in ComputeOrder recalculation, we cache the node for later process after the recalc.
            if(gc.addedEdge != null)
            {
                nodeToProcess = gc.addedEdge.inputNode;
            }
            else if(gc.addedNode != null)
            {
                nodeToProcess = gc.addedNode;
            }
            else if(gc.removedEdge != null)
            {
                nodeToProcess = gc.removedEdge.inputNode;
            }
            // else if(gc.nodeChanging != null)
            // {
            //     OnSoftTrigger(gc.nodeChanging);
            // }
            // else if(gc.nodeChanged != null)
            // {
            //     OnSoftTrigger(gc.nodeChanged, skipFirst: true);
            // }
            processor.OnGraphChanges(gc);
        }

        void OnUndoRedo()
        {
            var doneNodes = processor.FindAllDone();
            processor.ResetInit();
            OnCancel();
            coroutine = StartCoroutine(processor.RunFromAsync(null, jobNodes: doneNodes));
        }
    }
}
