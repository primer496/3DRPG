using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HotUpdate.Editor
{
    /// <summary>
    /// 修复 HotUpdateTestLoader 的 URL 序列化值（从 8080 改为 8000）
    /// </summary>
    public static class FixHotUpdateTestLoaderUrl
    {
        [MenuItem("Tools/HotUpdate/Fix URL (8080 -> 8000)")]
        public static void Fix()
        {
            var go = GameObject.Find("HotUpdateTestLoader");
            if (go == null) { Debug.LogError("HotUpdateTestLoader not found"); return; }
            var loader = go.GetComponent<HotUpdateTestLoader>();
            if (loader == null) { Debug.LogError("Component not found"); return; }

            var so = new SerializedObject(loader);
            var prop = so.FindProperty("hotUpdateDllUrl");
            if (prop != null)
            {
                prop.stringValue = "http://localhost:8000/StandaloneWindows64/HotUpdate.dll";
                so.ApplyModifiedProperties();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Debug.Log("[Fix] hotUpdateDllUrl updated to port 8000. Save the scene (Ctrl+S)!");
            }
        }
    }
}
