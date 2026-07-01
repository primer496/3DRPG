using System.Reflection;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.AIGraph;

namespace UnityEditor.AIGraph
{
    public class TJAIGraphAssetCallback
    {
        public static readonly string Extension = "asset";

        [MenuItem("Window/Tuanjie AI/Tuanjie AI Graph %M")]
        [MenuItem("Assets/Create/Tuanjie AI Graph/Tuanjie AI Graph", false, 10)]
        public static void CreateGraphPorcessor()
        {
            var graph = ScriptableObject.CreateInstance<TJAIGraph>();
            System.Type projectWindowUtilType = typeof(Editor).Assembly.GetType("UnityEditor.ProjectWindowUtil");
            MethodInfo getActiveFolderPathMethod = projectWindowUtilType.GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);

            if (getActiveFolderPathMethod != null)
            {
                string activeFolderPath = (string)getActiveFolderPathMethod.Invoke(null, null);
                if (activeFolderPath.Contains("Packages"))
                {
                    EditorUtility.DisplayDialog("Folder is not correct",
                        "Please create Tuanjie AI Graph asset in the Assets folder",
                        "Get!");
                }
                else
                    ProjectWindowUtil.CreateAsset(graph, "NewTJAIGraph.asset");
            }
            else
                Debug.Log("UnityEditor.ProjectWindowUtil.GetActiveFolderPath has problem");

        }

        [OnOpenAsset(0)]
        public static bool OnBaseGraphOpened(int instanceID, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceID) as TJAIGraph;

            if (asset == null) return false;
            
            var path = AssetDatabase.GetAssetPath(asset);
            var graph = SDEditorUtils.GetGraphAtPath(path);
            
            if (graph == null)
                return false;

            TJAIGraphWindow.Open(graph);

            
            return true;
        }

        [MenuItem("Window/Tuanjie AI/Graph Template")]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<TemplateWindow>();
            window.titleContent = new GUIContent("Graph Template");
            window.minSize = new Vector2(800, 600);
        }
    }
}