using System;
using System.IO;
using System.Reflection;
using GraphProcessor;
using UnityEditor;

namespace UnityEngine.AIGraph.Tests
{
    public static class TestHelper
    {
        public static T InvokePrivateStaticMethod<T>(Type type, string methodName, params object[] parameters)
        {
            var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
                throw new MissingMethodException($"Could not find method {methodName} in type {type}");
            return (T)method.Invoke(null, parameters);
        }

        public static void InvokePrivateStaticMethod(Type type, string methodName, params object[] parameters)
        {
            var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
                throw new MissingMethodException($"Could not find method {methodName} in type {type}");
            method.Invoke(null, parameters);
        }

        public static T InvokePrivateMethod<T>(object obj, string methodName, params object[] parameters)
        {
            var method = obj.GetType().GetMethod(methodName,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (method == null)
                throw new MissingMethodException($"Could not find method {methodName} in type {obj.GetType()}");
            return (T)method.Invoke(obj, parameters);
        }

        public static void InvokePrivateMethod(object obj, string methodName, params object[] parameters)
        {
            var method = obj.GetType().GetMethod(methodName,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (method == null)
                throw new MissingMethodException($"Could not find method {methodName} in type {obj.GetType()}");
            method.Invoke(obj, parameters);
        }
        
        // 辅助方法用于获取私有字段值
        public static T GetPrivateField<T>(object obj, string fieldName)
        {
            var fieldInfo = obj.GetType().GetField(fieldName, 
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo == null)
                throw new MissingFieldException($"Could not find field {fieldName} in type {obj.GetType()}");
            return (T)fieldInfo.GetValue(obj);
        }
    
        // 辅助方法用于设置私有字段值
        public static void SetPrivateField(object obj, string fieldName, object value)
        {
            var fieldInfo = obj.GetType().GetField(fieldName, 
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo == null)
                throw new MissingFieldException($"Could not find field {fieldName} in type {obj.GetType()}");
            fieldInfo.SetValue(obj, value);
        }
        
        public static TJAIGraph CreateTestGraph()
        {
            var graph = ScriptableObject.CreateInstance<TJAIGraph>();
#if UNITY_EDITOR
            const string folder = "Assets/Tests";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            var assetPath = $"{folder}/{Guid.NewGuid()}.asset";
            AssetDatabase.CreateAsset(graph, assetPath);
#endif
            return graph;
        }

        public static void DeleteTestGraph(BaseGraph graph)
        {
            if (graph == null) return;
            var path = AssetDatabase.GetAssetPath(graph);
            Object.DestroyImmediate(graph, true);
            if (!string.IsNullOrEmpty(path))
                AssetDatabase.DeleteAsset(path);
        }
    }
}