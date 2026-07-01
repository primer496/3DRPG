using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Warning, the current serialization code does not handle unity objects
// in play mode outside of the editor (because of JsonUtility)

namespace GraphProcessor
{
    [Serializable]
    public struct JsonElement
    {
        public string type;
        public string jsonDatas;
        public List<UnityObjectReference> unityObjectReferences;

        public override string ToString()
        {
            var refs = string.Empty;
            if (unityObjectReferences != null)
                refs = string.Join(",", unityObjectReferences.Select(o => o.ToString()));
            return $"type={type}, jsonDatas={jsonDatas}, objRefs=[{refs}]";
        }
    }

    [Serializable]
    public struct UnityObjectReference
    {
        public string fieldPath;
        public int instanceID;
        public string objectType;

        public UnityObjectReference(string path, int id, Type type)
        {
            fieldPath = path;
            instanceID = id;
            objectType = type.AssemblyQualifiedName;
        }

        public bool TryRestoreObject(out UnityEngine.Object unityObject)
        {
            unityObject = null;

#if UNITY_EDITOR
            unityObject = EditorUtility.InstanceIDToObject(instanceID);
#else
            unityObject = FindObjectByInstanceID(instanceID);
#endif
            return unityObject != null;
        }

        private UnityEngine.Object FindObjectByInstanceID(int id)
        {
            // 运行时查找对象的替代方法, 注意：这种方法效率较低，只在必要时使用
            var allObjects = Resources.FindObjectsOfTypeAll<UnityEngine.Object>();
            return allObjects.FirstOrDefault(obj => obj.GetInstanceID() == id);
        }

        public override string ToString()
        {
            return $"UnityObjectReference(fieldPath={fieldPath}, instanceID={instanceID}, " +
                   $"objectType={objectType})";
        }
    }

    public static class JsonSerializer
    {
        private static readonly BindingFlags FieldBindingFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public static JsonElement Serialize(object obj)
        {
            var elem = new JsonElement
            {
                type = obj.GetType().AssemblyQualifiedName,
                unityObjectReferences = new List<UnityObjectReference>()
            };
            CollectUnityObjectReferences(obj, "", elem.unityObjectReferences);
#if UNITY_EDITOR
            elem.jsonDatas = EditorJsonUtility.ToJson(obj);
#else
			elem.jsonDatas = JsonUtility.ToJson(obj);
#endif
            return elem;
        }

        public static T Deserialize<T>(JsonElement e)
        {
            if (typeof(T) != Type.GetType(e.type))
                throw new ArgumentException("Deserializing type is not the same than Json element type");

            var obj = Activator.CreateInstance<T>();
#if UNITY_EDITOR
            EditorJsonUtility.FromJsonOverwrite(e.jsonDatas, obj);
#else
			JsonUtility.FromJsonOverwrite(e.jsonDatas, obj);
#endif
            if (e.unityObjectReferences is { Count: > 0 })
                RestoreUnityObjectReferences(obj, e.unityObjectReferences);
            return obj;
        }

        public static JsonElement SerializeNode(BaseNode node)
        {
            return Serialize(node);
        }

        public static BaseNode DeserializeNode(JsonElement e)
        {
            try
            {
                var baseNodeType = Type.GetType(e.type);

                if (e.jsonDatas == null || baseNodeType == null)
                    return null;

                var node = Activator.CreateInstance(baseNodeType) as BaseNode;
#if UNITY_EDITOR
                EditorJsonUtility.FromJsonOverwrite(e.jsonDatas, node);
#else
				JsonUtility.FromJsonOverwrite(e.jsonDatas, node);
#endif
                if (e.unityObjectReferences is { Count: > 0 })
                    RestoreUnityObjectReferences(node, e.unityObjectReferences);
                return node;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to deserialize node: {ex.Message}");
                return null;
            }
        }

        private static void CollectUnityObjectReferences(object obj, string currentPath,
            List<UnityObjectReference> references)
        {
            if (obj == null) return;

            // process UnityEngine.Object
            var type = obj.GetType();
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                if (obj is UnityEngine.Object unityObject)
                    references.Add(new UnityObjectReference(currentPath, unityObject.GetInstanceID(), type));
                return;
            }

            switch (obj)
            {
                // process array
                case Array array:
                {
                    for (var i = 0; i < array.Length; i++)
                        CollectUnityObjectReferences(array.GetValue(i), $"{currentPath}[{i}]", references);
                    return;
                }
                // process list
                case IList list:
                {
                    for (var i = 0; i < list.Count; i++)
                        CollectUnityObjectReferences(list[i], $"{currentPath}[{i}]", references);
                    return;
                }
            }

            // process all field recursively
            if (IsBasicType(type)) return;
            var fields = type.GetFields(FieldBindingFlags);
            foreach (var field in fields)
            {
                // skip static and const field
                if (field.IsStatic || field.IsLiteral || !IsFieldSerialized(field)) continue;

                var fieldValue = field.GetValue(obj);
                if (fieldValue == null) continue;
                var newPath = string.IsNullOrEmpty(currentPath) ? field.Name : $"{currentPath}.{field.Name}";
                CollectUnityObjectReferences(fieldValue, newPath, references);
            }
        }

        private static void RestoreUnityObjectReferences(object obj, List<UnityObjectReference> references)
        {
            if (obj == null) return;

            foreach (var reference in references)
            {
                if (!TryResolveObjectPath(obj, reference.fieldPath, out var tgtObj,
                        out var tgtField, out var index)) continue;
                if (reference.TryRestoreObject(out var unityObject))
                    SetObjectValue(tgtObj, tgtField, index, unityObject);
            }
        }

        private static void SetObjectValue(object targetObject, FieldInfo field, int index, UnityEngine.Object value)
        {
            if (targetObject == null) return;

            if (field != null)
            {
                field.SetValue(targetObject, value);
            }
            else if (index >= 0)
            {
                switch (targetObject)
                {
                    case Array array:
                        array.SetValue(value, index);
                        break;
                    case IList list:
                    {
                        if (index < list.Count)
                            list[index] = value;
                        break;
                    }
                }
            }
        }

        private static bool TryResolveObjectPath(object root, string path, out object targetObject,
            out FieldInfo targetField, out int arrayIndex)
        {
            targetObject = root;
            targetField = null;
            arrayIndex = -1;

            if (string.IsNullOrEmpty(path)) return false;
            var parts = path.Split('.');
            var curObj = root;

            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                // process array
                if (part.Contains("["))
                {
                    if (!part.EndsWith("]")) continue;
                    var leftInd = part.IndexOf('[');
                    var rightInd = part.IndexOf(']');
                    var fieldName = part[..leftInd];
                    var indStr = part[(leftInd + 1)..rightInd];

                    if (!string.IsNullOrEmpty(fieldName))
                    {
                        var field = curObj.GetType().GetField(fieldName, FieldBindingFlags);
                        if (field == null) return false;

                        curObj = field.GetValue(curObj);
                        if (curObj == null) return false;
                    }

                    if (!int.TryParse(indStr, out arrayIndex)) return false;
                    if (curObj is Array array)
                    {
                        if (i == parts.Length - 1)
                        {
                            targetObject = array;
                            return true;
                        }

                        curObj = array.GetValue(arrayIndex);
                    }
                    else if (curObj is IList list)
                    {
                        if (i == parts.Length - 1)
                        {
                            targetObject = list;
                            return true;
                        }

                        curObj = list[arrayIndex];
                    }
                }
                else
                {
                    var field = curObj.GetType().GetField(part, FieldBindingFlags);
                    if (field == null) return false;

                    if (i == parts.Length - 1)
                    {
                        targetObject = curObj;
                        targetField = field;
                        return true;
                    }

                    curObj = field.GetValue(curObj);
                    if (curObj == null) return false;
                }
            }

            return false;
        }

        private static bool IsBasicType(Type type)
        {
            if (type == null) return false;
            return type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime);
        }

        private static bool IsFieldSerialized(FieldInfo field)
        {
            if (Attribute.IsDefined(field, typeof(NonSerializedAttribute)))
                return false;
            return Attribute.IsDefined(field, typeof(SerializeField)) || field.IsPublic;
        }
    }
}