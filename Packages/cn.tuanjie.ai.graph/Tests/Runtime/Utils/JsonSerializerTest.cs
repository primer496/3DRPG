using System;
using System.Collections.Generic;
using GraphProcessor;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UnityEngine.AIGraph.Tests
{
    [TestFixture]
    public class JsonSerializerTest
    {
        private BaseGraph graph;

        private GameObject testGameObject;
        private Texture2D testTexture;
        private Material testMaterial;

        [SetUp]
        public void Setup()
        {
            graph = TestHelper.CreateTestGraph();

            testGameObject = new GameObject("TestObject");
            testTexture = new Texture2D(64, 64);
            testMaterial = new Material(Shader.Find("Standard"));
        }

        [TearDown]
        public void Teardown()
        {
            TestHelper.DeleteTestGraph(graph);

            if (testGameObject != null)
                Object.DestroyImmediate(testGameObject);
            if (testTexture != null)
                Object.DestroyImmediate(testTexture);
            if (testMaterial != null)
                Object.DestroyImmediate(testMaterial);
        }

        [Test]
        public void JsonSerialize_SerializableEdge()
        {
            var outputNode = BaseNode.CreateFromType<StringNode>(new Vector2(300, 300));
            var inputNode = BaseNode.CreateFromType<HyImageGeneratingNode>(new Vector2(300, 600));
            graph.AddNode(inputNode);
            graph.AddNode(outputNode);
            var outputPort = outputNode.GetPort(nameof(outputNode.output), null);
            var inputPort = inputNode.GetPort(nameof(inputNode.prompt), null);
            var edge = graph.Connect(inputPort, outputPort);

            var serializedJson = JsonSerializer.Serialize(edge);
            var deserializedEdge = JsonSerializer.Deserialize<SerializableEdge>(serializedJson);

            // Assert.AreEqual(edge, deserializedEdge);
            Assert.AreEqual(edge.GUID, deserializedEdge.GUID, "GUID 不匹配");

            // 比较 owner 的 GUID（因为 ScriptableObject 引用在序列化中可能以 GUID 形式存储）
            var serOwner = TestHelper.GetPrivateField<BaseGraph>(edge, "owner");
            var desOwner = TestHelper.GetPrivateField<BaseGraph>(deserializedEdge, "owner");
            if (serOwner != null && desOwner != null)
            {
                Assert.AreEqual(serOwner.GetInstanceID(), desOwner.GetInstanceID(), "Owner instance 不匹配");
            }
            else
            {
                Assert.IsNull(serOwner);
                Assert.IsNull(desOwner);
            }

            Assert.AreEqual(TestHelper.GetPrivateField<string>(edge, "inputNodeGUID"),
                TestHelper.GetPrivateField<string>(deserializedEdge, "inputNodeGUID"),
                "InputNodeGUID 不匹配");
            Assert.AreEqual(TestHelper.GetPrivateField<string>(edge, "outputNodeGUID"),
                TestHelper.GetPrivateField<string>(deserializedEdge, "outputNodeGUID"),
                "OutputNodeGUID 不匹配");

            Assert.AreEqual(edge.inputFieldName, deserializedEdge.inputFieldName, "InputFieldName 不匹配");
            Assert.AreEqual(edge.outputFieldName, deserializedEdge.outputFieldName, "OutputFieldName 不匹配");

            if (string.IsNullOrEmpty(edge.inputPortIdentifier))
            {
                Assert.IsTrue(string.IsNullOrEmpty(deserializedEdge.inputPortIdentifier), "InputPortIdentifier 不匹配");
            }
            else
            {
                Assert.AreEqual(edge.inputPortIdentifier, deserializedEdge.inputPortIdentifier,
                    "InputPortIdentifier 不匹配");
            }

            if (string.IsNullOrEmpty(edge.outputPortIdentifier))
            {
                Assert.IsTrue(string.IsNullOrEmpty(deserializedEdge.outputPortIdentifier), "OutputPortIdentifier 不匹配");
            }
            else
            {
                Assert.AreEqual(edge.outputPortIdentifier, deserializedEdge.outputPortIdentifier,
                    "OutputPortIdentifier 不匹配");
            }

            // 验证非序列化字段应该为 null 或默认值（因为不会被序列化）
            Assert.IsNull(deserializedEdge.inputNode, "InputNode 应该为 null（非序列化字段）");
            Assert.IsNull(deserializedEdge.outputNode, "OutputNode 应该为 null（非序列化字段）");
            Assert.IsNull(deserializedEdge.inputPort, "InputPort 应该为 null（非序列化字段）");
            Assert.IsNull(deserializedEdge.outputPort, "OutputPort 应该为 null（非序列化字段）");
            Assert.IsNull(deserializedEdge.passThroughBuffer, "PassThroughBuffer 应该为 null（非序列化字段）");
        }

        [Test]
        public void JsonSerialize_SerializableNode()
        {
            var node = BaseNode.CreateFromType<StringNode>(new Vector2(100, 100));
            graph.AddNode(node);

            // Serialize the node  
            var serializedJson = JsonSerializer.SerializeNode(node);

            // Deserialize the node  
            var deserializedNode = JsonSerializer.DeserializeNode(serializedJson);

            // Assert that the deserialized node matches the original  
            Assert.AreEqual(node.GUID, deserializedNode.GUID, "GUID 不匹配");
            Assert.AreEqual(node.position, deserializedNode.position, "Position 不匹配");
            Assert.AreEqual(node.name, deserializedNode.name, "Name 不匹配");

            // Add additional checks as necessary for other fields  
            Assert.AreEqual(node.GetCustomName(), deserializedNode.GetCustomName(), "NodeCustomName 不匹配");

            // Verify non-serialized fields are null or default  
            Assert.IsTrue(deserializedNode.inputPorts == null || deserializedNode.inputPorts.Count == 0,
                "InputPorts 应该为 null（非序列化字段）");
            Assert.IsTrue(deserializedNode.outputPorts == null || deserializedNode.outputPorts.Count == 0,
                "OutputPorts 应该为 null（非序列化字段）");
        }

        [Test]
        public void JsonSerialize_SerializableTextureNode()
        {
            var node = BaseNode.CreateFromType<TextureNode>(new Vector2(100, 100));
            graph.AddNode(node);

            // Serialize the node  
            var serializedJson = JsonSerializer.SerializeNode(node);

            // Deserialize the node  
            var deserializedNode = JsonSerializer.DeserializeNode(serializedJson) as TextureNode;

            // Assert that the deserialized node matches the original  
            Assert.NotNull(deserializedNode);
            Assert.AreEqual(node.GUID, deserializedNode.GUID, "GUID 不匹配");
            Assert.AreEqual(node.position, deserializedNode.position, "Position 不匹配");
            Assert.AreEqual(node.name, deserializedNode.name, "Name 不匹配");

            // Add additional checks as necessary for other fields  
            Assert.AreEqual(node.outputTexture, deserializedNode.outputTexture, "Texture 不匹配");
            Assert.AreEqual(node.GetCustomName(), deserializedNode.GetCustomName(), "NodeCustomName 不匹配");

            // Verify non-serialized fields are null or default  
            Assert.IsTrue(deserializedNode.inputPorts == null || deserializedNode.inputPorts.Count == 0,
                "InputPorts 应该为 null（非序列化字段）");
            Assert.IsTrue(deserializedNode.outputPorts == null || deserializedNode.outputPorts.Count == 0,
                "OutputPorts 应该为 null（非序列化字段）");
        }

        [System.Serializable]
        public class TestDictClass
        {
            public Dictionary<GameObject, Texture> objectToTextureMap;
            public Dictionary<string, Material> stringToMaterialMap;
            public Dictionary<Transform, List<GameObject>> complexDict;
        }

        [System.Serializable]
        public class TestSimpleClass
        {
            public int intValue;
            public string stringValue;
            public GameObject gameObjectRef;
            public Texture textureRef;
        }

        [System.Serializable]
        public class TestNestedClass
        {
            public string name;
            public TestSimpleClass nestedObject;
            public Material materialRef;
        }

        [System.Serializable]
        public class TestCollectionClass
        {
            public List<GameObject> gameObjectList;
            public Texture[] textureArray;
            public Dictionary<string, Material> materialDict;
        }

        [Test]
        public void SerializeDeserialize_SimpleClass()
        {
            // 准备测试数据
            var original = new TestSimpleClass
            {
                intValue = 42,
                stringValue = "Hello World",
                gameObjectRef = testGameObject,
                textureRef = testTexture
            };

            // 序列化
            var element = JsonSerializer.Serialize(original);

            // 验证序列化结果
            Assert.IsNotNull(element);
            Assert.IsNotEmpty(element.jsonDatas);
            Assert.AreEqual(2, element.unityObjectReferences.Count); // 两个Unity对象引用

            // 反序列化
            var deserialized = JsonSerializer.Deserialize<TestSimpleClass>(element);

            // 验证反序列化结果
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(original.intValue, deserialized.intValue);
            Assert.AreEqual(original.stringValue, deserialized.stringValue);
            Assert.AreEqual(original.gameObjectRef, deserialized.gameObjectRef);
            Assert.AreEqual(original.textureRef, deserialized.textureRef);
        }

        [Test]
        public void SerializeDeserialize_NestedClass()
        {
            // 准备嵌套测试数据
            var nested = new TestSimpleClass
            {
                intValue = 100,
                stringValue = "Nested",
                gameObjectRef = testGameObject
            };

            var original = new TestNestedClass
            {
                name = "Parent",
                nestedObject = nested,
                materialRef = testMaterial
            };

            // 序列化
            var element = JsonSerializer.Serialize(original);

            // 验证序列化结果
            Assert.IsNotNull(element);
            Assert.IsNotEmpty(element.jsonDatas);
            Assert.GreaterOrEqual(element.unityObjectReferences.Count, 2); // 至少两个Unity对象引用

            // 反序列化
            var deserialized = JsonSerializer.Deserialize<TestNestedClass>(element);

            // 验证反序列化结果
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(original.name, deserialized.name);
            Assert.AreEqual(original.materialRef, deserialized.materialRef);

            Assert.IsNotNull(deserialized.nestedObject);
            Assert.AreEqual(original.nestedObject.intValue, deserialized.nestedObject.intValue);
            Assert.AreEqual(original.nestedObject.stringValue, deserialized.nestedObject.stringValue);
            Assert.AreEqual(original.nestedObject.gameObjectRef, deserialized.nestedObject.gameObjectRef);
        }

        [Test]
        public void SerializeDeserialize_WithNullUnityObjects()
        {
            // 测试包含null Unity对象的情况
            var original = new TestSimpleClass
            {
                intValue = 123,
                stringValue = "Test",
                gameObjectRef = null, // null引用
                textureRef = testTexture
            };

            // 序列化
            var element = JsonSerializer.Serialize(original);

            // 反序列化
            var deserialized = JsonSerializer.Deserialize<TestSimpleClass>(element);

            // 验证
            Assert.IsNotNull(deserialized);
            Assert.IsNull(deserialized.gameObjectRef); // null引用应该保持为null
            Assert.AreEqual(original.textureRef, deserialized.textureRef);
        }

        [Test]
        public void SerializeDeserialize_Collections()
        {
            // 准备集合测试数据
            var gameObject2 = new GameObject("TestObject2");
            var texture2 = new Texture2D(32, 32);

            try
            {
                var original = new TestCollectionClass
                {
                    gameObjectList = new List<GameObject> { testGameObject, gameObject2 },
                    textureArray = new Texture[] { testTexture, texture2 }
                };

                // 序列化
                var element = JsonSerializer.Serialize(original);

                // 验证序列化结果
                Assert.IsNotNull(element);
                Assert.IsNotEmpty(element.jsonDatas);
                Assert.GreaterOrEqual(element.unityObjectReferences.Count, 4); // 多个Unity对象引用

                // 反序列化
                var deserialized = JsonSerializer.Deserialize<TestCollectionClass>(element);

                // 验证反序列化结果
                Assert.IsNotNull(deserialized);
                Assert.IsNotNull(deserialized.gameObjectList);
                Assert.AreEqual(2, deserialized.gameObjectList.Count);
                Assert.AreEqual(testGameObject, deserialized.gameObjectList[0]);
                Assert.AreEqual(gameObject2, deserialized.gameObjectList[1]);

                Assert.IsNotNull(deserialized.textureArray);
                Assert.AreEqual(2, deserialized.textureArray.Length);
                Assert.AreEqual(testTexture, deserialized.textureArray[0]);
                Assert.AreEqual(texture2, deserialized.textureArray[1]);
            }
            finally
            {
                // 清理临时对象
                if (gameObject2 != null)
                    Object.DestroyImmediate(gameObject2);
                if (texture2 != null)
                    Object.DestroyImmediate(texture2);
            }
        }

        [Test]
        public void UnityObjectReference_Serialization()
        {
            // 测试UnityObjectReference结构本身的序列化
            var reference = new UnityObjectReference("testField", testGameObject.GetInstanceID(), typeof(GameObject));

            // 转换为JSON再转换回来
            var json = JsonUtility.ToJson(reference);
            var deserializedRef = JsonUtility.FromJson<UnityObjectReference>(json);

            // 验证
            Assert.AreEqual(reference.fieldPath, deserializedRef.fieldPath);
            Assert.AreEqual(reference.instanceID, deserializedRef.instanceID);
            Assert.AreEqual(reference.objectType, deserializedRef.objectType);
        }

        [Test]
        public void JsonElement_ToString()
        {
            var original = new TestSimpleClass
            {
                intValue = 1,
                stringValue = "Test",
            };

            var element = JsonSerializer.Serialize(original);
            var toStringResult = element.ToString();

            Assert.IsNotEmpty(toStringResult);
            Assert.IsTrue(toStringResult.Contains("type"));
            Assert.IsTrue(toStringResult.Contains("jsonDatas"));
            Assert.IsTrue(toStringResult.Contains("objRefs=[]"));
        }

        [UnityTest]
        public System.Collections.IEnumerator SerializeDeserialize_InPlayMode()
        {
            // 测试在PlayMode下的行为
            var original = new TestSimpleClass
            {
                intValue = 999,
                stringValue = "PlayMode Test",
                gameObjectRef = testGameObject
            };

            // 序列化
            var element = JsonSerializer.Serialize(original);

            yield return null; // 等待一帧

            // 反序列化
            var deserialized = JsonSerializer.Deserialize<TestSimpleClass>(element);

            // 验证
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(original.intValue, deserialized.intValue);
            Assert.AreEqual(original.stringValue, deserialized.stringValue);

            // 在PlayMode中，Unity对象引用应该能够正确恢复
            Assert.IsNotNull(deserialized.gameObjectRef);
            Assert.AreEqual(original.gameObjectRef.name, deserialized.gameObjectRef.name);
        }

        [Test]
        public void ErrorHandling_InvalidType()
        {
            var original = new TestSimpleClass { intValue = 1 };

            var element = JsonSerializer.Serialize(original);

            // 故意修改类型名使其无效
            element.type = "Invalid.Type.Name";

            Assert.Throws<ArgumentException>(() => { JsonSerializer.Deserialize<TestSimpleClass>(element); });
        }

        [Test]
        public void ErrorHandling_NullJsonData()
        {
            var element = new JsonElement
            {
                type = typeof(TestSimpleClass).AssemblyQualifiedName,
                jsonDatas = null
            };

            var result = JsonSerializer.Deserialize<TestSimpleClass>(element);

            // 应该返回一个默认构造的对象
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.intValue);
            Assert.IsNull(result.stringValue);
        }

        // [Test]
        // public void SerializeDeserialize_Dictionary()
        // {
        //     var test = new TestDictClass
        //     {
        //         objectToTextureMap = new Dictionary<GameObject, Texture>
        //         {
        //             { testGameObject, testTexture }
        //         }
        //     };
        //
        //     var data = JsonSerializer.Serialize(test);
        //     var restored = JsonSerializer.Deserialize<TestDictClass>(data);
        //     Assert.IsNotNull(restored);
        //     Assert.IsNotNull(restored.objectToTextureMap);
        //     Assert.IsTrue(restored.objectToTextureMap.ContainsKey(testGameObject));
        //     Assert.IsTrue(restored.objectToTextureMap.ContainsValue(testTexture));
        // }
    }
}