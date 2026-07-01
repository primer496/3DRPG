using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GraphProcessor;

namespace UnityEngine.AIGraph
{
    /// <summary>
    /// General interface for invoking node process method, whether synchronously or asynchronously.
    /// </summary>
    public class TJAINodeProcessor : CoroutineProcessor
    {
        /// <summary>
        /// Entrance of unified node processing.
        /// If the given node is synchronous (i.e. isAsync == false), we call node.Process(), otherwise node.ProcessAsync().
        /// Since both Process() and ProcessAsync() are protected methods, we get them by reflection with cache.
        /// </summary>
        /// <param name="node">Node to be processed</param>
        /// <returns></returns>
        public IEnumerator ProcessNode(BaseNode node)
        {
            node.isUpdate = true;
            Reset();
            var isAsync = ProcessMethodHelper.IsAsyncNode(node.GetType());
            try
            {
                node.UpdateStatus(NodeStatus.Working, "", 0f);
                node.InvokeBeforeProcessSetup();
                if (!isAsync)
                    node.Process();
            }
            catch (Exception e)
            {
                RecordNodeException(e, node);
                yield break;
            }
            
            if (isAsync)
            {
                NodeProcessDelegate nodeProcessDelegate = node.ProcessAsync;
                yield return ProcessAsync(nodeProcessDelegate);
            }
            if (!Success)
            {
                node.isUpdate = false;
                node.InvokeOnError(Ex.Message);
                yield break;
            }

            try
            {
                node.outputPorts.PushDatas();
                node.InvokeOnProcessed();
            }
            catch (Exception e)
            {
                RecordNodeException(e, node);
                yield break;
            }
            node.isUpdate = false;
        }

        delegate IEnumerator NodeProcessDelegate();

        IEnumerator ProcessAsync(NodeProcessDelegate nodeProcessDelegate)
        {
            Reset();
            IEnumerator routine;
            try
            {
                routine = new CatchableEnumerator(nodeProcessDelegate.Invoke());
            }
            catch (Exception e)
            {
                Success = false;
                Ex = e;
                Debug.LogException(e);
                yield break;
            }
            yield return ProcessAsync(routine);
        }
        
        public override string HandleException()
        {
            // if (Ex != null)
            // {
            //     Debug.LogException(Ex);
            //     // NOTE: don't throw exception here!!!
            // }
            return Ex?.Message ?? "";
        }

        private void RecordNodeException(Exception e, BaseNode node)
        {
            Ex = e;
            Success = false;
            node.isUpdate = false;
            Debug.LogException(e);
            node.InvokeOnError(e.Message);
        }

        private static class ProcessMethodHelper
        {
            private static HashSet<Type> asyncNodes = new();

            static ProcessMethodHelper()
            {
                foreach (var type in AppDomain.CurrentDomain.GetAllTypes())
                {
                    if (type.IsAbstract || type.ContainsGenericParameters)
                        continue;
                    if (!type.IsSubclassOf(typeof(BaseNode)))
                        continue;

                    // type must be a concrete node class
                    var asyncAttr = type.GetCustomAttribute<UseProcessAsyncAttribute>();
                    if (asyncAttr != null)
                        asyncNodes.Add(type);
                }
            }
            
            public static bool IsAsyncNode(Type nodeType) => asyncNodes.Contains(nodeType);
        }
    }
}
