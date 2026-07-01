using System.Collections.Generic;
using System.IO;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.AIGraph
{
    public static class MeshUtils
    {
        public static GameObject Import(string localPath)
        {
#if UNITY_EDITOR
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(localPath, ImportAssetOptions.ForceUpdate);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(localPath);
            // prefab.hideFlags = HideFlags.HideAndDontSave;
            if (prefab != null) return prefab;
            Debug.LogError($"Failed to load GameObject from {localPath}");
            return null;
#else
            return null;
#endif
        }

        public static List<AnimationClip> ImportAnimationClip(string localPath)
        {
#if UNITY_EDITOR
            if (localPath == null || localPath == string.Empty)
                return null;

            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(localPath, ImportAssetOptions.ForceUpdate);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(localPath);
            if (assets == null || assets.Length == 0)
            {
                return null;
            }

            List<AnimationClip> loadedClips = new List<AnimationClip>();

            foreach (Object asset in assets)
            {
                if (asset is AnimationClip && !asset.name.StartsWith("__preview__"))
                {
                    loadedClips.Add(asset as AnimationClip);
                }
            }

            return loadedClips;
#else
            return null;
#endif
        }

        public static Mesh BytesToMesh(byte[] bytes, string localPath)
        {
            if (localPath.EndsWith(".fbx"))
                return ImportFBXFromBytes(bytes, localPath);
            else
            {
                Debug.Log($"Writing mesh data to {localPath}");
                File.WriteAllBytes(localPath, bytes);
                return new Mesh();
            }
        }

        public static Mesh ImportFBXFromBytes(byte[] fbxBytes, string localPath)
        {
            string directory = Path.GetDirectoryName(localPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(localPath, fbxBytes);
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
            return ImportFBXFromFile(localPath);
        }

        public static Mesh ImportFBXFromFile(string localPath)
        {
#if UNITY_EDITOR
            AssetDatabase.ImportAsset(localPath, ImportAssetOptions.ForceUpdate);
            Object[] objects = AssetDatabase.LoadAllAssetsAtPath(localPath);
            foreach (Object obj in objects)
            {
                if (obj is Mesh)
                    return obj as Mesh;
            }
#endif
            return null;
        }
        
        
        public static string GetUrlExtension(string url)
        {
            var lastDotIndex = url.LastIndexOf('.');
            return lastDotIndex < 0 ? "" : url[lastDotIndex..];
        }
    }
}