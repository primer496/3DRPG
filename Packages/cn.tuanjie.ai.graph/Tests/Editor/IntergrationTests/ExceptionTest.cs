using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.AIGraph.Tests;
using UnityEngine.TestTools;

namespace UnityEditor.AIGraph.Tests
{
    /// <summary>
    /// test if exception is correctly catched by node
    /// </summary>
    public class ExceptionTest
    {
        private TJAIGraph graph;
        private TJAIGraphProcessor processor;
        private readonly Vector2 position = new Vector2(0, 0);

        [SetUp]
        public void Setup()
        {
            graph = TestHelper.CreateTestGraph();
            processor = new TJAIGraphProcessor(graph);
            graph.onGraphChanges += OnGraphChanges;
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.DeleteTestGraph(graph);
        }

        void OnGraphChanges(GraphChanges changes)
        {
            if (changes.addedNode != null)
            {
                graph.UpdateComputeOrder();
                processor.UpdateComputeOrder();
            }
            else if (changes.removedNode != null)
            {
                graph.UpdateComputeOrder();
                processor.UpdateComputeOrder();
            }
        }

        [UnityTest]
        public IEnumerator CatchException_Layer0()
        {
            var node = BaseNode.CreateFromType<ExceptionLayer0TestNode>(position);
            graph.AddNode(node);

            var callbackInvoked = false;
            void Callback() => callbackInvoked = true;

            LogAssert.Expect(LogType.Exception, "Exception: test exception layer 0");
            yield return processor.RunAllAsync(Callback, true);

            Assert.IsTrue(callbackInvoked);
            Assert.AreEqual(NodeStatus.Error, node.status);
        }

        [UnityTest]
        public IEnumerator CatchException_BeforeProcessSetup()
        {
            var node = BaseNode.CreateFromType<ExceptionBeforeProcessSetupTestNode>(position);
            graph.AddNode(node);

            var callbackInvoked = false;
            void Callback() => callbackInvoked = true;

            LogAssert.Expect(LogType.Exception, "Exception: test exception in BeforeProcessSetup");
            yield return processor.RunAllAsync(Callback, true);

            Assert.IsTrue(callbackInvoked);
            Assert.AreEqual(NodeStatus.Error, node.status);
        }

        [UnityTest]
        public IEnumerator CatchException_Process()
        {
            var node = BaseNode.CreateFromType<ExceptionProcessTestNode>(position);
            graph.AddNode(node);

            var callbackInvoked = false;
            void Callback() => callbackInvoked = true;

            LogAssert.Expect(LogType.Exception, "Exception: test exception in Process");
            yield return processor.RunAllAsync(Callback, true);

            Assert.IsTrue(callbackInvoked);
            Assert.AreEqual(NodeStatus.Error, node.status);

            graph.RemoveNode(node);
        }

        [UnityTest]
        public IEnumerator CatchException_OnProcessed()
        {
            var node = BaseNode.CreateFromType<ExceptionOnProcessedTestNode>(position);
            graph.AddNode(node);

            var callbackInvoked = false;
            void Callback() => callbackInvoked = true;

            LogAssert.Expect(LogType.Exception, "Exception: test exception in OnProcessed");
            yield return processor.RunAllAsync(Callback, true);

            Assert.IsTrue(callbackInvoked);
            Assert.AreEqual(NodeStatus.Error, node.status);

            graph.RemoveNode(node);
        }

        [UnityTest]
        public IEnumerator CatchException_OutputPorts()
        {
            var node = BaseNode.CreateFromType<ExceptionOutputPortsTestNode>(position);
            graph.AddNode(node);

            var callbackInvoked = false;
            void Callback() => callbackInvoked = true;

            LogAssert.Expect(LogType.Exception, "Exception: test exception in outputPorts.PushDatas");
            yield return processor.RunAllAsync(Callback, true);

            Assert.IsTrue(callbackInvoked);
            Assert.AreEqual(NodeStatus.Error, node.status);

            graph.RemoveNode(node);
        }

        [UnityTest]
        public IEnumerator CatchException_AsyncProcess()
        {
            var node = BaseNode.CreateFromType<ExceptionAsyncProcessTestNode>(position);
            graph.AddNode(node);

            var callbackInvoked = false;
            void Callback() => callbackInvoked = true;

            LogAssert.Expect(LogType.Exception, "Exception: test exception in async process");
            yield return processor.RunAllAsync(Callback, true);

            Assert.IsTrue(callbackInvoked);
            Assert.AreEqual(NodeStatus.Error, node.status);

            graph.RemoveNode(node);
        }

        [UnityTest]
        public IEnumerator CatchException_Nested()
        {
            var parentNode = BaseNode.CreateFromType<SuccessProcessTestNode>(position);
            var childNode = BaseNode.CreateFromType<ExceptionLayer0TestNode>(position);

            graph.AddNode(parentNode);
            graph.AddNode(childNode);

            // 创建连接（需要根据实际端口情况调整）
            graph.Connect(childNode.inputPorts[0], parentNode.outputPorts[0]);

            var callbackInvoked = false;
            void Callback() => callbackInvoked = true;

            LogAssert.Expect(LogType.Exception, "Exception: test exception layer 0");
            yield return processor.RunAllAsync(Callback, true);

            Assert.IsTrue(callbackInvoked);
            Assert.AreEqual(NodeStatus.Done, parentNode.status);
            Assert.AreEqual(NodeStatus.Error, childNode.status);

            graph.RemoveNode(parentNode);
            graph.RemoveNode(childNode);
        }

        [UnityTest]
        public IEnumerator CatchException_MultipleNodes()
        {
            var node1 = BaseNode.CreateFromType<ExceptionLayer0TestNode>(position);
            var node2 = BaseNode.CreateFromType<ExceptionProcessTestNode>(position);

            graph.AddNode(node1);
            graph.AddNode(node2);

            var callbackInvoked = false;
            void Callback() => callbackInvoked = true;

            // 期望捕获多个异常（实际可能只捕获第一个）
            LogAssert.Expect(LogType.Exception, "Exception: test exception layer 0");
            LogAssert.Expect(LogType.Exception, "Exception: test exception in Process");
            yield return processor.RunAllAsync(Callback, true);

            Assert.IsTrue(callbackInvoked);
            Assert.AreEqual(NodeStatus.Error, node1.status);
            Assert.AreEqual(NodeStatus.Error, node2.status);

            graph.RemoveNode(node1);
            graph.RemoveNode(node2);
        }

        [UnityTest]
        public IEnumerator CatchException_BreakIfPreFailed()
        {
            var node1 = BaseNode.CreateFromType<ExceptionLayer0TestNode>(position);
            var node2 = BaseNode.CreateFromType<ExceptionProcessTestNode>(position);

            graph.AddNode(node1);
            graph.AddNode(node2);

            graph.Connect(node2.inputPorts[0], node1.outputPorts[0]);

            var callbackInvoked = false;
            void Callback() => callbackInvoked = true;

            // 期望捕获多个异常（实际可能只捕获第一个）
            LogAssert.Expect(LogType.Exception, "Exception: test exception layer 0");
            yield return processor.RunAllAsync(Callback, true);

            Assert.IsTrue(callbackInvoked);
            Assert.AreEqual(NodeStatus.Error, node1.status);
            // node2 should not process if node1 failed, cause node1 is pre node
            Assert.AreEqual(NodeStatus.Init, node2.status);

            graph.RemoveNode(node1);
            graph.RemoveNode(node2);
        }

        [UnityTest]
        public IEnumerator CatchException_NestedFunctionCall()
        {
            var node = BaseNode.CreateFromType<NestedExceptionTestNode>(position);
            graph.AddNode(node);

            var callbackInvoked = false;
            void Callback() => callbackInvoked = true;

            LogAssert.Expect(LogType.Exception, "Exception: test exception in nested function");
            yield return processor.RunAllAsync(Callback, true);

            Assert.IsTrue(callbackInvoked);
            Assert.AreEqual(NodeStatus.Error, node.status);

            graph.RemoveNode(node);
        }

        [UnityTest]
        public IEnumerator CatchException_NestedAsync()
        {
            var node = BaseNode.CreateFromType<NestedAsyncExceptionTestNode>(position);
            graph.AddNode(node);

            var callbackInvoked = false;
            void Callback() => callbackInvoked = true;

            LogAssert.Expect(LogType.Exception, "Exception: test exception in nested enumerator");
            yield return processor.RunAllAsync(Callback, true);

            Assert.IsTrue(callbackInvoked);
            Assert.AreEqual(NodeStatus.Error, node.status);

            graph.RemoveNode(node);
        }

        [UnityTest]
        public IEnumerator CatchException_DeeplyNested()
        {
            var node = BaseNode.CreateFromType<DeeplyNestedExceptionTestNode>(position);
            graph.AddNode(node);

            var callbackInvoked = false;
            void Callback() => callbackInvoked = true;

            LogAssert.Expect(LogType.Exception, "Exception: test exception in level 3");
            yield return processor.RunAllAsync(Callback, true);

            Assert.IsTrue(callbackInvoked);
            Assert.AreEqual(NodeStatus.Error, node.status);

            graph.RemoveNode(node);
        }

        [UnityTest]
        public IEnumerator CatchException_CoroutineNested()
        {
            var node = BaseNode.CreateFromType<CoroutineNestedExceptionTestNode>(position);
            graph.AddNode(node);

            var callbackInvoked = false;
            void Callback() => callbackInvoked = true;

            LogAssert.Expect(LogType.Exception, "Exception: test exception in nested coroutine with 3 layer");
            yield return processor.RunAllAsync(Callback, true);

            Assert.IsTrue(callbackInvoked);
            Assert.AreEqual(NodeStatus.Error, node.status);

            graph.RemoveNode(node);
        }

        [UnityTest]
        public IEnumerator CatchException_MultipleNodes2()
        {
            var mainNode = BaseNode.CreateFromType<SuccessProcessTestNode>(position);
            graph.AddNode(mainNode);

            // 为主节点添加一些会抛出异常的子节点
            var exceptionNode = BaseNode.CreateFromType<ExceptionLayer0TestNode>(position);
            graph.AddNode(exceptionNode);

            // 创建连接（需要根据实际端口情况调整）
            // graph.Connect(mainNode.outputPorts[0], exceptionNode.inputPorts[0]);

            var callbackInvoked = false;
            void Callback() => callbackInvoked = true;

            LogAssert.Expect(LogType.Exception, "Exception: test exception layer 0");
            yield return processor.RunAllAsync(Callback, true);

            Assert.IsTrue(callbackInvoked);
            Assert.AreEqual(NodeStatus.Done, mainNode.status);
            Assert.AreEqual(NodeStatus.Error, exceptionNode.status);

            graph.RemoveNode(mainNode);
            graph.RemoveNode(exceptionNode);
        }

        [UnityTest]
        public IEnumerator CatchException_SubProcessorWithMultipleNodes()
        {
            var mainNode = BaseNode.CreateFromType<SubProcessorExceptionTestNode>(position);
            graph.AddNode(mainNode);

            // 为主节点添加一些会抛出异常的子节点
            var exceptionNode = BaseNode.CreateFromType<ExceptionLayer0TestNode>(position);
            graph.AddNode(exceptionNode);

            var callbackInvoked = false;
            void Callback() => callbackInvoked = true;

            LogAssert.Expect(LogType.Exception, "Exception: nested enumerator node exception in sub processor");
            LogAssert.Expect(LogType.Exception, "Exception: test exception layer 0");
            yield return processor.RunAllAsync(Callback, true);

            Assert.IsTrue(callbackInvoked);
            Assert.AreEqual(NodeStatus.Error, mainNode.status);
            Assert.AreEqual(NodeStatus.Error, exceptionNode.status);

            graph.RemoveNode(mainNode);
            graph.RemoveNode(exceptionNode);
        }

        [UnityTest]
        public IEnumerator CatchException_RecursiveProcessor()
        {
            var node = BaseNode.CreateFromType<RecursiveProcessorExceptionTestNode>(position);
            graph.AddNode(node);

            var callbackInvoked = false;
            void Callback() => callbackInvoked = true;

            LogAssert.Expect(LogType.Exception, "Exception: test exception in recursive processor");
            yield return processor.RunAllAsync(Callback, true);

            Assert.IsTrue(callbackInvoked);
            Assert.AreEqual(NodeStatus.Error, node.status);

            graph.RemoveNode(node);
        }
    }

    [Serializable]
    public class TestNode : SDNode
    {
        [Input(name = "Input")] public string input;
        [Output(name = "Output")] public string output;
    }

    // test helpers
    [Serializable, UseProcessAsync]
    public class ExceptionLayer0TestNode : TestNode
    {
        public override IEnumerator ProcessAsync()
        {
            throw new Exception("test exception layer 0");
        }
    }

    [Serializable]
    public class ExceptionBeforeProcessSetupTestNode : TestNode
    {
        protected override void Enable()
        {
            base.Enable();
            beforeProcessSetup += BeforeProcessSetup;
        }

        protected void BeforeProcessSetup()
        {
            throw new Exception("test exception in BeforeProcessSetup");
        }
    }

    [Serializable]
    public class ExceptionProcessTestNode : TestNode
    {
        public override void Process()
        {
            throw new Exception("test exception in Process");
        }
    }

    [Serializable]
    public class ExceptionOnProcessedTestNode : TestNode
    {
        protected override void Enable()
        {
            base.Enable();
            onProcessed += OnProcessed;
        }

        protected void OnProcessed()
        {
            throw new Exception("test exception in OnProcessed");
        }
    }

    [Serializable]
    public class ExceptionOutputPortsTestNode : TestNode
    {
        [Output(name = "Test")] public string testOutput;

        [CustomPortOutput(nameof(testOutput), typeof(string))]
        protected void PushOutput(List<SerializableEdge> edges, NodePort port)
        {
            throw new Exception("test exception in outputPorts.PushDatas");
        }

        public override void Process()
        {
            // 正常处理，但在输出数据时抛出异常
        }
    }

    [Serializable, UseProcessAsync]
    public class ExceptionAsyncProcessTestNode : TestNode
    {
        public override IEnumerator ProcessAsync()
        {
            yield return null; // 先等待一帧
            throw new Exception("test exception in async process");
        }
    }

    [Serializable]
    public class SuccessProcessTestNode : TestNode
    {
        public override void Process()
        {
            // 正常处理，依赖的子节点会抛出异常
        }
    }

    [Serializable]
    public class NestedExceptionTestNode : TestNode
    {
        public override void Process()
        {
            // 调用嵌套函数，在嵌套函数中抛出异常
            ProcessNested();
        }

        private void ProcessNested()
        {
            ProcessDeeplyNested();
        }

        private void ProcessDeeplyNested()
        {
            throw new Exception("test exception in nested function");
        }
    }

    [Serializable, UseProcessAsync]
    public class NestedAsyncExceptionTestNode : TestNode
    {
        public override IEnumerator ProcessAsync()
        {
            // 创建子处理器并在其中抛出异常
            yield return NestedAsync();
        }

        private IEnumerator NestedAsync()
        {
            yield return null; // 模拟一些处理
            throw new Exception("test exception in nested enumerator");
        }
    }

    [Serializable]
    public class DeeplyNestedExceptionTestNode : TestNode
    {
        public override void Process()
        {
            Level1();
        }

        private void Level1()
        {
            Level2();
        }

        private void Level2()
        {
            Level3();
        }

        private void Level3()
        {
            throw new Exception("test exception in level 3");
        }
    }

    [Serializable, UseProcessAsync]
    public class CoroutineNestedExceptionTestNode : TestNode
    {
        public override IEnumerator ProcessAsync()
        {
            yield return NestedCoroutine1();
        }

        private IEnumerator NestedCoroutine1()
        {
            yield return null;
            yield return NestedCoroutine2();
        }

        private IEnumerator NestedCoroutine2()
        {
            yield return null;
            throw new Exception("test exception in nested coroutine with 3 layer");
        }
    }

    [Serializable, UseProcessAsync]
    public class SubProcessorExceptionTestNode : TestNode
    {
        public override IEnumerator ProcessAsync()
        {
            // 模拟创建子图处理器
            var processor = new CoroutineProcessor();
            yield return processor.ProcessAsync(ProcessSubGraph());
        }

        private IEnumerator ProcessSubGraph()
        {
            // 这里应该创建子处理器并运行
            // 为了测试，我们直接抛出异常或调用会抛出异常的方法
            yield return RunSubProcessorWithException();
        }

        private IEnumerator RunSubProcessorWithException()
        {
            yield return null;
            // 模拟子处理器中的节点抛出异常
            throw new Exception("nested enumerator node exception in sub processor");
        }
    }

    [Serializable, UseProcessAsync]
    public class RecursiveProcessorExceptionTestNode : TestNode
    {
        private int depth = 0;

        public override IEnumerator ProcessAsync()
        {
            depth++;
            if (depth > 3) // 防止无限递归
            {
                throw new Exception("test exception in recursive processor");
            }

            yield return ProcessRecursively();
        }

        private IEnumerator ProcessRecursively()
        {
            yield return null;
            // 递归调用自身
            yield return ProcessAsync();
        }
    }
}