using UnityEditor;

namespace UnityEngine.AIGraph
{
    public static class ImportUtils
    {
        public static T Import<T>(string localPath) where T : Object
        {
#if UNITY_EDITOR
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(localPath, ImportAssetOptions.ForceUpdate);
            var asset = AssetDatabase.LoadAssetAtPath<T>(localPath);
            if (asset != null) return asset;
            Debug.LogError($"Failed to load {typeof(T).Name} from {localPath}");
#endif
            return null;
        }
    }
}