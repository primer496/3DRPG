using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

// using Unity.Entities;

namespace GraphProcessor
{
    /// <summary>
    /// Graph processor
    /// </summary>
    public class SDProcessGraphProcessor : BaseGraphProcessor
    {
        List<BaseNode> processList;

        /// <summary>
        /// Manage graph scheduling and processing
        /// </summary>
        /// <param name="graph">Graph to be processed</param>
        public SDProcessGraphProcessor(BaseGraph graph) : base(graph)
        {
        }

        public override void UpdateComputeOrder()
        {
            processList = graph.nodes.OrderBy(n => n.computeOrder).ToList();
        }

        /// <summary>
        /// Process all the nodes following the compute order.
        /// </summary>
        public override void Run()
        {
            int count = processList.Count;

            var cmd = CommandBufferPool.Get("SD Graph: " + graph.name);
            for (int i = 0; i < count; i++)
            {
                processList[i].OnProcess();
                processList[i].OnProcess(cmd);
            }
            Graphics.ExecuteCommandBuffer(cmd);
        }
    }
}