using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.AIGraph.Tests;

namespace UnityEditor.AIGraph.Tests
{
    [TestFixture]
    public class TJAIGraphProcessorTest
    {
        private BaseGraph graph;
        private TJAIGraphProcessor processor;
        private TestNode node1, node2, node3;
        private readonly Vector2 position = new Vector2(0, 0);

        [SetUp]
        public void SetUp()
        {
            graph = TestHelper.CreateTestGraph();
            processor = new TJAIGraphProcessor(graph);

            // Create test nodes
            node1 = BaseNode.CreateFromType<TestNode>(position);
            node2 = BaseNode.CreateFromType<TestNode>(position);
            node3 = BaseNode.CreateFromType<TestNode>(position);
            
            graph.AddNode(node1);
            graph.AddNode(node2);
            graph.AddNode(node3);
            
            graph.Connect(node2.inputPorts[0], node1.outputPorts[0]);
            graph.Connect(node3.inputPorts[0], node2.outputPorts[0]);

            graph.UpdateComputeOrder();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.DeleteTestGraph(graph);
        }

        [Test]
        public void Constructor_InitializesCorrectly()
        {
            Assert.IsNotNull(processor);
            Assert.IsFalse(processor.isRunning);
            Assert.IsNull(processor.Current);
        }

        [Test]
        public void UpdateComputeOrder_OrdersNodesCorrectly()
        {
            // Act
            processor.UpdateComputeOrder();

            // Assert - should not throw exception since all nodes have computeOrder >= 0
            Assert.DoesNotThrow(() => processor.UpdateComputeOrder());
        }

        [Test]
        public void UpdateComputeOrder_ThrowsExceptionWhenNodesMissingComputeOrder()
        {
            // Arrange
            var nodeWithoutOrder = BaseNode.CreateFromType<TestNode>(position);
            nodeWithoutOrder.computeOrder = -1;
            graph.AddNode(nodeWithoutOrder);

            // Act & Assert
            Assert.Throws<Exception>(() => processor.UpdateComputeOrder());
        }

        [Test]
        public void FindDescendants_ReturnsCorrectDescendants()
        {
            // Arrange
            processor.UpdateComputeOrder();

            // Act
            var descendants = processor.FindDescendants(node1);

            // Assert
            Assert.AreEqual(3, descendants.Count);
            Assert.Contains(node1, descendants.ToList());
            Assert.Contains(node2, descendants.ToList());
            Assert.Contains(node3, descendants.ToList());
        }

        [Test]
        public void FindAncestors_ReturnsCorrectAncestors()
        {
            // Arrange
            processor.UpdateComputeOrder();

            // Act
            var ancestors = processor.FindAncestors(node3);

            // Assert - should include all nodes in reverse order
            Assert.AreEqual(3, ancestors.Count);
            Assert.AreEqual(node3, ancestors[2]);
            Assert.AreEqual(node2, ancestors[1]);
            Assert.AreEqual(node1, ancestors[0]);
        }

        [Test]
        public void FindAllDone_ReturnsDoneNodes()
        {
            // Arrange
            processor.UpdateComputeOrder();
            node1.UpdateStatus(NodeStatus.Done);
            node3.UpdateStatus(NodeStatus.Done);

            // Act
            var doneNodes = processor.FindAllDone();

            // Assert
            Assert.AreEqual(2, doneNodes.Count);
            Assert.Contains(node1, doneNodes.ToList());
            Assert.Contains(node3, doneNodes.ToList());
            Assert.IsFalse(doneNodes.Contains(node2));
        }

        [Test]
        public void ResetAll_CancelsAllNodes()
        {
            // Arrange
            processor.UpdateComputeOrder();
            bool node1Cancelled = false;
            bool node2Cancelled = false;

            node1.onCancelled += () => node1Cancelled = true;
            node2.onCancelled += () => node2Cancelled = true;

            processor.Current = node1;

            // Act
            processor.ResetAll();

            // Assert
            Assert.IsTrue(node1Cancelled);
            Assert.IsTrue(node2Cancelled);
            Assert.IsNull(processor.Current);
        }

        [Test]
        public void ResetInit_ResetsNodesToInitStatus()
        {
            // Arrange
            processor.UpdateComputeOrder();
            node1.UpdateStatus(NodeStatus.Done);
            node2.UpdateStatus(NodeStatus.Working);

            // Act
            processor.ResetInit();

            // Assert
            Assert.AreEqual(NodeStatus.Init, node1.status);
            Assert.AreEqual(NodeStatus.Init, node2.status);
            Assert.AreEqual(NodeStatus.Init, node3.status);
        }

        [Test]
        public void Current_Setter_InvokesFocusEvents()
        {
            // Arrange
            bool node1Focused = false;
            bool node1Unfocused = false;
            bool node2Focused = false;

            node1.onFocuseUpdated += b =>
            {
                if (b) node1Focused = true;
                else node1Unfocused = true;
            };
            node2.onFocuseUpdated += b =>
            {
                if (b) node2Focused = true;
            };

            // Act
            processor.Current = node1;

            // Assert
            Assert.IsTrue(node1Focused);
            Assert.IsFalse(node1Unfocused);

            // Act - change current
            processor.Current = node2;

            // Assert
            Assert.IsTrue(node1Unfocused);
            Assert.IsTrue(node2Focused);
        }

        [Test]
        public void AfterInterruption_ResetsStateCorrectly()
        {
            // Arrange
            processor.UpdateComputeOrder();
            processor.Current = node1;
            node1.UpdateStatus(NodeStatus.Working);

            var nodeCancelled = false;
            node1.onCancelled += () => nodeCancelled = true;

            // Act
            processor.AfterInterruption();

            // Assert
            Assert.IsFalse(processor.isRunning);
            Assert.AreEqual(NodeStatus.Init, node1.status);
            Assert.IsTrue(nodeCancelled);
        }

        [Test]
        public void AfterCancellation_ResetsStateCorrectly()
        {
            // Arrange
            processor.UpdateComputeOrder();
            processor.Current = node1;
            TestHelper.SetPrivateField(processor, "currJobNodes", new HashSet<BaseNode> { node1 });

            // Act
            processor.AfterCancellation();

            // Assert
            Assert.IsFalse(processor.isRunning);
            Assert.IsNull(processor.Current);
            Assert.IsNull(TestHelper.GetPrivateField<HashSet<BaseNode>>(processor, "currJobNodes"));
        }

        [Test]
        public void OnGraphChanges_RemovesDescendantsFromCurrJobNodes()
        {
            // Arrange
            processor.UpdateComputeOrder();
            var currJobNodes = new HashSet<BaseNode> { node1, node2, node3 };
            TestHelper.SetPrivateField(processor, "currJobNodes", currJobNodes);

            var graphChanges = new GraphChanges
            {
                removedEdge = node1.GetOutputEdges()[0]
            };

            // Mock that node1 has no other input edges
            // This would normally be done by setting up proper node connections

            // Act
            processor.OnGraphChanges(graphChanges);

            // Assert - currJobNodes should be modified
            currJobNodes = TestHelper.GetPrivateField<HashSet<BaseNode>>(processor, "currJobNodes");
            Assert.IsNotNull(currJobNodes);
            Assert.IsTrue(currJobNodes.Contains(node1));
            Assert.IsFalse(currJobNodes.Contains(node2));
            Assert.IsFalse(currJobNodes.Contains(node3));
        }

        // Test node implementation for testing
        private class TestNode : SDNode
        {
            [Input(name = "Input")] public string input;
            [Output(name = "Output")] public string output;
        }
    }
}