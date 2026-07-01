using System;
using GraphProcessor;
using NUnit.Framework;

namespace UnityEngine.AIGraph.Tests
{
    public class TJAIGraphTest
    {
        private TJAIGraph graph;
        private StringNode node1, node2, node3;
        private readonly Vector2 position = new Vector2(300, 300);

        [SetUp]
        public void Setup()
        {
            graph = TestHelper.CreateTestGraph();

            node1 = BaseNode.CreateFromType<StringNode>(position);
            node2 = BaseNode.CreateFromType<StringNode>(position);
            node3 = BaseNode.CreateFromType<StringNode>(position);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.DeleteTestGraph(graph);
        }

        [Test]
        public void Constructor_InitializesGraph()
        {
            Assert.IsNotNull(graph);
            Assert.IsNotNull(graph.history);
            Assert.IsNotNull(graph.tokenDataModel);
            Assert.AreEqual(0, graph.nodes.Count);
            Assert.AreEqual(0, graph.nodesPerGUID.Count);
            Assert.AreEqual(0, graph.edges.Count);
        }

        #region Graph element curd

        [Test]
        public void AddNode_AddNodeToGraph()
        {
            var addedNode = graph.AddNode(node1);

            Assert.AreEqual(1, graph.nodes.Count);
            Assert.AreEqual(node1, addedNode);
            Assert.IsTrue(graph.nodesPerGUID.ContainsKey(node1.GUID));
            Assert.AreEqual(node1, graph.nodesPerGUID[node1.GUID]);
        }


        [Test]
        public void RemoveNode_RemoveNodeFromGraph()
        {
            graph.AddNode(node1);
            graph.AddNode(node2);

            graph.RemoveNode(node1);

            Assert.AreEqual(1, graph.nodes.Count);
            Assert.AreEqual(node2, graph.nodes[0]);
            Assert.IsFalse(graph.nodesPerGUID.ContainsKey(node1.GUID));
        }

        [Test]
        public void RemoveAllNode_RemoveAllNodesFromGraph()
        {
            graph.AddNode(node1);
            graph.AddNode(node2);
            graph.AddNode(node3);

            graph.RemoveAllNode();

            Assert.AreEqual(0, graph.nodes.Count);
            Assert.AreEqual(0, graph.nodesPerGUID.Count);
        }

        [Test]
        public void AddNode_TriggersGraphChangesEvent()
        {
            GraphChanges receivedChanges = null;
            graph.onGraphChanges += changes => receivedChanges = changes;

            graph.AddNode(node1);

            Assert.IsNotNull(receivedChanges);
            Assert.AreEqual(node1, receivedChanges.addedNode);
        }

        [Test]
        public void RemoveNode_TriggersGraphChangesEvent()
        {
            graph.AddNode(node1);
            GraphChanges receivedChanges = null;
            graph.onGraphChanges += changes => receivedChanges = changes;

            graph.RemoveNode(node1);

            Assert.IsNotNull(receivedChanges);
            Assert.AreEqual(node1, receivedChanges.removedNode);
        }

        #endregion

        #region serialize and deserialize tests

        [Test]
        public void OnBeforeSerialize_CleansUpNullNodes()
        {
            graph.AddNode(node1);
            graph.AddNode(node2);

            // 模拟一个null节点
            graph.nodes.Add(null);

            graph.OnBeforeSerialize();

            Assert.AreEqual(2, graph.nodes.Count);
            Assert.IsTrue(graph.nodes.Contains(node1));
            Assert.IsTrue(graph.nodes.Contains(node2));
        }

        [Test]
        public void Deserialize_ReinitializesGraphElements()
        {
            graph.AddNode(node1);
            graph.AddNode(node2);

            // 模拟序列化后的状态
            graph.nodesPerGUID.Clear();
            graph.edgesPerGUID.Clear();

            graph.Deserialize();

            Assert.AreEqual(2, graph.nodesPerGUID.Count);
            Assert.IsTrue(graph.nodesPerGUID.ContainsKey(node1.GUID));
            Assert.IsTrue(graph.nodesPerGUID.ContainsKey(node2.GUID));
        }

        #endregion

        #region edge connection tests

        [Test]
        public void Connect_CreatesEdgeBetweenNodes()
        {
            graph.AddNode(node1);
            graph.AddNode(node2);

            var inputPort = node1.inputPorts[0];
            var outputPort = node2.outputPorts[0];

            var edge = graph.Connect(inputPort, outputPort);

            Assert.AreEqual(1, graph.edges.Count);
            Assert.AreEqual(edge, graph.edges[0]);
            Assert.AreEqual(inputPort, edge.inputPort);
            Assert.AreEqual(outputPort, edge.outputPort);
            Assert.AreEqual(1, node1.GetInputEdges().Count);
            Assert.AreEqual(1, node2.GetOutputEdges().Count);
        }
        
        [Test]
        public void Connect_GetPortByName()
        {
            graph.AddNode(node1);
            graph.AddNode(node2);

            var inputPort = node1.GetPort(nameof(node1.inputString), null);
            var outputPort = node2.GetPort(nameof(node2.output), null);

            var edge = graph.Connect(inputPort, outputPort);

            Assert.AreEqual(1, graph.edges.Count);
            Assert.AreEqual(edge, graph.edges[0]);
            Assert.AreEqual(inputPort, edge.inputPort);
            Assert.AreEqual(outputPort, edge.outputPort);
            Assert.AreEqual(1, node1.GetInputEdges().Count);
            Assert.AreEqual(1, node2.GetOutputEdges().Count);
        }

        [Test]
        public void Connect_TriggersGraphChangesEvent()
        {
            graph.AddNode(node1);
            graph.AddNode(node2);
            var inputPort = node1.inputPorts[0];
            var outputPort = node2.outputPorts[0];

            GraphChanges receivedChanges = null;
            graph.onGraphChanges += changes => receivedChanges = changes;

            graph.Connect(inputPort, outputPort);

            Assert.IsNotNull(receivedChanges);
            Assert.IsNotNull(receivedChanges.addedEdge);
        }

        [Test]
        public void Disconnect_RemovesEdge()
        {
            graph.AddNode(node1);
            graph.AddNode(node2);
            var inputPort = node1.inputPorts[0];
            var outputPort = node2.outputPorts[0];

            var edge = graph.Connect(inputPort, outputPort);
            graph.Disconnect(edge);

            Assert.AreEqual(0, graph.edges.Count);
        }

        [Test]
        public void Disconnect_TriggersGraphChangesEvent()
        {
            graph.AddNode(node1);
            graph.AddNode(node2);
            var inputPort = node1.inputPorts[0];
            var outputPort = node2.outputPorts[0];

            var edge = graph.Connect(inputPort, outputPort);
            GraphChanges receivedChanges = null;
            graph.onGraphChanges += changes => receivedChanges = changes;

            graph.Disconnect(edge);

            Assert.IsNotNull(receivedChanges);
            Assert.AreEqual(edge, receivedChanges.removedEdge);
        }

        [Test]
        public void Connect_WithAutoDisconnect_RemovesExistingConnections()
        {
            graph.AddNode(node1);
            graph.AddNode(node2);
            graph.AddNode(node3);

            var inputPort = node1.inputPorts[0];
            var outputPort1 = node2.outputPorts[0];
            var outputPort2 = node3.outputPorts[0];

            // 先连接第一个输出端口
            var edge1 = graph.Connect(inputPort, outputPort1);

            // 再连接第二个输出端口（应该自动断开第一个连接）
            var edge2 = graph.Connect(inputPort, outputPort2, true);

            Assert.AreEqual(1, graph.edges.Count);
            Assert.AreEqual(edge2, graph.edges[0]);
        }

        #endregion
        
        #region Group operation tests

        [Test]
        public void AddGroup_AddsGroupToGraph()
        {
            var group = new Group { title = "Test Group" };

            graph.AddGroup(group);

            Assert.AreEqual(1, graph.groups.Count);
            Assert.AreEqual(group, graph.groups[0]);
        }

        [Test]
        public void RemoveGroup_RemovesGroupFromGraph()
        {
            var group = new Group { title = "Test Group" };
            graph.AddGroup(group);

            graph.RemoveGroup(group);

            Assert.AreEqual(0, graph.groups.Count);
        }

        [Test]
        public void AddGroup_TriggersGraphChangesEvent()
        {
            var group = new Group { title = "Test Group" };
            GraphChanges receivedChanges = null;
            graph.onGraphChanges += changes => receivedChanges = changes;

            graph.AddGroup(group);

            Assert.IsNotNull(receivedChanges);
            Assert.AreEqual(group, receivedChanges.addedGroups);
        }

        #endregion

        #region StackNode operation tests

        [Test]
        public void AddStackNode_AddsStackNodeToGraph()
        {
            var stackNode = new TestStackNode(position);

            graph.AddStackNode(stackNode);

            Assert.AreEqual(1, graph.stackNodes.Count);
            Assert.AreEqual(stackNode, graph.stackNodes[0]);
        }

        [Test]
        public void RemoveStackNode_RemovesStackNodeFromGraph()
        {
            var stackNode = new TestStackNode(position);
            graph.AddStackNode(stackNode);

            graph.RemoveStackNode(stackNode);

            Assert.AreEqual(0, graph.stackNodes.Count);
        }

        #endregion

        #region StickyNote operation tests

        [Test]
        public void AddStickyNote_AddsStickyNoteToGraph()
        {
            var stickyNote = new StickyNote("Test Note", position);

            graph.AddStickyNote(stickyNote);

            Assert.AreEqual(1, graph.stickyNotes.Count);
            Assert.AreEqual(stickyNote, graph.stickyNotes[0]);
        }

        [Test]
        public void RemoveStickyNote_RemovesStickyNoteFromGraph()
        {
            var stickyNote = new StickyNote("Test Note", position);
            graph.AddStickyNote(stickyNote);

            graph.RemoveStickyNote(stickyNote);

            Assert.AreEqual(0, graph.stickyNotes.Count);
        }

        #endregion

        #region Compute order test

        [Test]
        public void UpdateComputeOrder_SetsCorrectComputeOrder()
        {
            graph.AddNode(node1);
            graph.AddNode(node2);
            graph.AddNode(node3);

            // 创建连接：node2 -> node1 -> node3
            graph.Connect(node1.inputPorts[0], node2.outputPorts[0]);
            graph.Connect(node3.inputPorts[0], node1.outputPorts[0]);

            graph.UpdateComputeOrder();

            // node2 应该在 node1 之前计算，node1 在 node3 之前
            Assert.Less(node2.computeOrder, node1.computeOrder);
            Assert.Less(node1.computeOrder, node3.computeOrder);
        }

        [Test]
        public void UpdateComputeOrder_WithCycle_MarksNodesWithLoopComputeOrder()
        {
            graph.AddNode(node1);
            graph.AddNode(node2);

            // 创建循环连接：node1 -> node2 -> node1
            graph.Connect(node2.inputPorts[0], node1.outputPorts[0]);
            graph.Connect(node1.inputPorts[0], node2.outputPorts[0]);

            graph.UpdateComputeOrder();

            // 循环中的节点应该被标记为 loopComputeOrder
            Assert.AreEqual(BaseGraph.loopComputeOrder, node1.computeOrder);
            Assert.AreEqual(BaseGraph.loopComputeOrder, node2.computeOrder);
        }

        #endregion

        #region ExposedParameter tests

        [Test]
        public void AddExposedParameter_AddsParameterToGraph()
        {
            var parameterGuid = graph.AddExposedParameter("TestParam", typeof(TestExposedParameter));

            Assert.AreEqual(1, graph.exposedParameters.Count);
            Assert.IsNotNull(graph.GetExposedParameterFromGUID(parameterGuid));
        }

        [Test]
        public void RemoveExposedParameter_RemovesParameterFromGraph()
        {
            var parameterGuid = graph.AddExposedParameter("TestParam", typeof(TestExposedParameter));

            graph.RemoveExposedParameter(parameterGuid);

            Assert.AreEqual(0, graph.exposedParameters.Count);
        }

        [Test]
        public void UpdateExposedParameter_UpdatesParameterValue()
        {
            var parameterGuid = graph.AddExposedParameter("TestParam", typeof(TestExposedParameter));
            var newValue = "New Value";

            graph.UpdateExposedParameter(parameterGuid, newValue);

            var parameter = graph.GetExposedParameterFromGUID(parameterGuid);
            Assert.AreEqual(newValue, parameter.value);
        }

        #endregion

        #region helper

        private class TestStackNode : BaseStackNode
        {
            public TestStackNode(Vector2 position, string title = "Stack", bool acceptDrop = true,
                bool acceptNewNode = true) : base(position, title, acceptDrop, acceptNewNode)
            {
            }
        }

        [Serializable]
        public class TestExposedParameter : ExposedParameter
        {
            public override Type GetValueType() => typeof(string);
        }

        #endregion
    }
}