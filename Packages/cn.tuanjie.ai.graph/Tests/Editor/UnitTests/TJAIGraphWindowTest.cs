using GraphProcessor;
using NUnit.Framework;
using UnityEngine.AIGraph;
using UnityEngine.AIGraph.Tests;

namespace UnityEditor.AIGraph.Tests
{
    [TestFixture]
    public class TJAIGraphWindowTest
    {
        private TJAIGraph testGraph;

        [SetUp]
        public void Setup()
        {
            testGraph = TestHelper.CreateTestGraph();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.DeleteTestGraph(testGraph);
        }

        [Test]
        public void TestOpen_NewWindow()
        {
            var newWindow = TJAIGraphWindow.Open(testGraph);
            Assert.NotNull(newWindow);
            Assert.AreEqual(testGraph, newWindow.GetCurrentGraph());
            newWindow.Close();
        }

        [Test]
        public void TestOpen_ExistingWindow()
        {
            var firstWindow = TJAIGraphWindow.Open(testGraph);
            var secondWindow = TJAIGraphWindow.Open(testGraph);
            Assert.AreEqual(firstWindow, secondWindow);
            firstWindow.Close();
        }
#if !TJAI_DEBUG
        [Test]
        public void Graph_ShouldNotHaveDebugNode()
        {
            // should not have debug node within package
            var menuNodes = NodeProvider.GetNodeMenuEntries();
            foreach (var pair in menuNodes)
            {
                Assert.IsFalse(pair.path.StartsWith("Debug"));
            }
        }
#endif
    }
}