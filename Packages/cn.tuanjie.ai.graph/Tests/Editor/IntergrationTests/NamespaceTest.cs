using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine.AIGraph;

namespace UnityEditor.AIGraph.Tests
{
    [TestFixture]
    internal class NamespaceTest
    {
        [Test]
        public void NoDanglingNamespaces()
        {
            var myAssembly = Assembly.GetAssembly(typeof(BaseTJAINode));
            HashSet<string> namespaces = new HashSet<string>();
            foreach (var theType in myAssembly.GetTypes().Where(t => !string.IsNullOrEmpty(t.Namespace)))
            {
                namespaces.Add(theType.Namespace);
            }

            var invalidNames = new List<string>();
            foreach (var name in namespaces)
            {
                if (name.Contains("AIGraph"))
                    continue;
                if (name.Contains("UnityEditor"))
                    continue;
                if (name.Contains("UnityEngine"))
                    continue;
                if (name.Contains("GraphProcessor"))
                    continue;

                invalidNames.Add(name);
            }

            Assert.IsEmpty(invalidNames,
                "The following namespaces are invalid for the AI Graph package:\n" + string.Join("\n", invalidNames));
        }
    }
}