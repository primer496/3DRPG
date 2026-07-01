using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    public class TJAIGraphProcessor : BaseGraphProcessor
    {
        List<BaseNode> processList;

        TJAINodeProcessor nodeProcessor;

        int Count => processList.Count;

        public bool isRunning { get; protected set; } = false;

        private HashSet<BaseNode> currJobNodes = null;

        private BaseNode current = null;

        public BaseNode Current
        {
            get => current;
            set
            {
                current?.InvokeOnUnfocused();
                current = value;
                current?.InvokeOnFocused();
            }
        }

        /// <summary>
        /// Manage graph scheduling and processing
        /// </summary>
        /// <param name="graph">Graph to be processed</param>
        public TJAIGraphProcessor(BaseGraph graph) : base(graph)
        {
            nodeProcessor = new TJAINodeProcessor();
        }

        public override void UpdateComputeOrder()
        {
            processList = graph.nodes.Where(n => n.computeOrder >= 0).OrderBy(n => n.computeOrder).ToList();
            if (processList.Count != graph.nodes.Count)
                throw new Exception("Update compute order failed");
        }

        public HashSet<BaseNode> FindDescendants(BaseNode root)
        {
            HashSet<BaseNode> descendants = new();

            int startIndex = processList.IndexOf(root);
            if (startIndex >= 0 && startIndex < Count)
            {
                descendants.Add(root);
                for(int i = startIndex; i < Count; ++i)
                {
                    BaseNode currNode = processList[i];
                    if (descendants.Contains(currNode))
                    {
                        foreach(var outNode in currNode.GetOutputNodes())
                        {
                            descendants.Add(outNode);
                        }
                    }
                }
            }

            return descendants;
        }

        public List<BaseNode> FindAncestors(BaseNode root)
        {
            List<BaseNode> ancestors = new();
            Queue<BaseNode> queue = new();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                ancestors.Add(current);
                foreach (var inputNode in current.GetInputNodes())
                {
                    queue.Enqueue(inputNode);
                }
            }
            ancestors.Reverse();
            return ancestors;
        }

        public HashSet<BaseNode> FindAllDone()
        {
            return processList.Where(n => n.status == NodeStatus.Done).ToHashSet();
        }

        public void ResetAll()
        {
            processList.ForEach(node => node.InvokeOnCancelled());
            current = null;
        }

        public void ResetInit()
        {
            processList.ForEach(node =>
            {
                if (node is TJAIBaseAssetNode { status: NodeStatus.Done } tjNode)
                    tjNode.Refresh();
                node.UpdateStatus(NodeStatus.Init);
            });
        }

        public void ResetFrom(BaseNode root, HashSet<BaseNode> descendants = null)
        {
            if(descendants == null)
                descendants = FindDescendants(root);

            descendants.ToList().ForEach(node => node.InvokeOnCancelled());
        }

        public override void Run()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// ！！！Important：图执行流程中数据如何流动
        /// </summary>
        /// <param name="breakOnFail"></param>
        /// <returns></returns>
        bool MoveNext(bool breakOnFail, bool forceRunInTrigger = false)
        {
            int index, startIndex = Current?.computeOrder + 1 ?? 0;
            for (index = startIndex; index < Count; ++index)
            {
                var node = processList[index];

                // If this node is done, then there is no need to process it again,
                // but further nodes may need its results as input.
                if (node.status == NodeStatus.Done)
                    continue;
                
                // 如果前面有节点执行失败了，后续的节点都跳过
                var inputReady = node.GetInputNodes().All(n => n.status == NodeStatus.Done);
                if (!inputReady)
                {
                    if (breakOnFail)
                        return false;
                    continue;
                }

                // Find the next node to process!
                break;
            }

            Current = index < Count ? processList[index] : null;
            return Current != null;
        }

        /// <summary>
        /// 异步根据compute order执行图流程
        /// </summary>
        /// <param name="breakOnFail">节点执行过程中报错则立即停止执行</param>
        /// <param name="singleStep">单步执行, 指只跑单个节点</param>
        /// <param name="forceRunInTrigger">强制执行所有节点, 包括trigger节点</param>
        /// <returns></returns>
        IEnumerator RunOnNodeListAsync(bool breakOnFail, bool singleStep = false, bool forceRunInTrigger = false)
        {
            // If Current is null, we try to start at the first ready node
            if (Current == null && !MoveNext(breakOnFail, forceRunInTrigger))
                yield break;

            do
            {
                if (Current.status == NodeStatus.Done)
                    continue;
                yield return nodeProcessor.ProcessNode(Current);
                if (!nodeProcessor.Success && breakOnFail)
                    break;

                if (currJobNodes != null && currJobNodes.Contains(current))
                {
                    MoveNext(breakOnFail, forceRunInTrigger);
                    break;
                }
            } while (MoveNext(breakOnFail, forceRunInTrigger) && !singleStep);
        }

        /// <summary>
        /// Process all the nodes following the compute order.
        /// </summary>
        public IEnumerator RunAllAsync(Action callback = null, bool forceRunInTrigger = false)
        {
            isRunning = true;

            Current = null;
            yield return RunOnNodeListAsync(breakOnFail: false, forceRunInTrigger: forceRunInTrigger);

            isRunning = false;
            callback?.Invoke();
        }

        /// <summary>
        /// 执行向下异步的流程
        /// </summary>
        /// <param name="root"></param>
        /// <param name="jobNodes"></param>
        /// <param name="forceTrigger">是否强制触发，只针对root节点判定</param>
        /// <param name="singleStep"></param>
        /// <param name="skipFirst">不执行root节点，为true则从root下一个节点开始执行</param>
        /// <returns></returns>
        public IEnumerator RunFromAsync(BaseNode root, HashSet<BaseNode> jobNodes = null, 
            bool forceTrigger = false, bool singleStep = false, bool skipFirst = false,
            bool forceRunInTrigger = false)
        {
            isRunning = true;

            currJobNodes = jobNodes;
            Current = root;
            yield return RunOnNodeListAsync(breakOnFail: false, singleStep: singleStep,
                forceRunInTrigger: forceRunInTrigger);

            currJobNodes = null;
            isRunning = false;
        }

        public IEnumerator RunToAsync(List<BaseNode> jobNodes)
        {
            isRunning = true;
            currJobNodes = jobNodes.ToHashSet();
            
            foreach (var node in jobNodes)
            {
                Current = node;
                if (currJobNodes != null && !currJobNodes.Contains(node))
                    break;
                if (Current.status == NodeStatus.Done)
                    continue;
                yield return nodeProcessor.ProcessNode(Current);
                if (!nodeProcessor.Success)
                    break;
            }
            
            currJobNodes = null;
            Current = null;
            isRunning = false;
        }

        public void AfterInterruption()
        {
            isRunning = false;
            Current?.UpdateStatus(NodeStatus.Init);
            Current?.InvokeOnCancelled();
        }

        public void AfterCancellation()
        {
            AfterInterruption();
            Current = null;
            currJobNodes = null;
        }

        public void OnGraphChanges(GraphChanges gc)
        {
            if (gc.removedEdge != null)
            {
                var inputNode = gc.removedEdge.inputNode;
                if (currJobNodes != null && inputNode != null && 
                    inputNode.GetInputEdges().Count(e => e != gc.removedEdge) == 0)
                {
                    var descendants = FindDescendants(inputNode);
                    foreach (var postNode in descendants)
                    {
                        currJobNodes.Remove(postNode);
                    }
                }
            }
        }
    }
}
