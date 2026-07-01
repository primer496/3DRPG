using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GraphProcessor;
using UnityEngine.Assertions;
using UnityEngine.Video;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.AIGraph
{
    public class CustomTextureSettings
    {
#if UNITY_EDITOR
        public TextureImporterType type = TextureImporterType.Default;
#endif
        public bool isSRGB = false;
    }

    public static class ExportUtils
    {
        static string exportDirectory = Application.dataPath;

        const string title = "Export History Asset";

        const string assetsRoot = "Assets";

        const int maxSystemPathLength = 259;

        const int charactersReservedForUniquePath = 8;

        public enum OverwriteAction
        {
            Overwrite,
            Rename,
            Cancel,
            AskUser
        }

        /// <summary>
        /// 获取资产类型的默认扩展名
        /// </summary>
        private static string GetAssetExtension(Type assetType)
        {
            if (assetType == typeof(Texture2D)) return ".png";
            if (assetType == typeof(Mesh)) return ".mesh";
            if (assetType == typeof(Material)) return ".mat";
            if (assetType == typeof(AnimationClip)) return ".anim";
            if (assetType == typeof(AudioClip)) return ".wav";
            if (assetType == typeof(GameObject)) return ".prefab";
            return ".asset";
        }

        public static string GetAssetPath(string orgPath)
        {
            return GetAssetPath(orgPath, out _);
        }

        public static string GetAssetPath(string orgPath, out bool isOutsideAssets)
        {
            isOutsideAssets = false;
            orgPath = orgPath?.Trim(' ');
            if (string.IsNullOrEmpty(orgPath))
                return orgPath;

            var normalizedPath = orgPath.Replace('\\', '/');
            var dataPath = Application.dataPath.Replace('\\', '/');
            // 检查是否在 Assets 目录下
            if (normalizedPath.StartsWith(dataPath, StringComparison.OrdinalIgnoreCase))
                return "Assets" + normalizedPath[(dataPath.Length)..];
            if (!normalizedPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                if (IsRootedPath(normalizedPath))
                {
                    isOutsideAssets = true;
                    return normalizedPath;
                    // throw new ArgumentException($"Not allowed to export outside of project. Path: {orgPath}");
                }
                else
                    return $"Assets/{normalizedPath}";
            }
            return normalizedPath;
        }
        public static bool IsRootedPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            // 标准化路径（统一正斜杠）
            var normalizedPath = path.Replace('\\', '/').Trim();

            var isWindowsRoot = normalizedPath.Length >= 2 && char.IsLetter(normalizedPath[0]) &&
                                normalizedPath[1] == ':';
    
            // 检查常见的根路径模式
            return normalizedPath.StartsWith("//") || normalizedPath.StartsWith("\\\\") || 
                   normalizedPath.StartsWith("/") || isWindowsRoot;
        }

        public static string SaveAsset<T>(T asset, string folderPath, string assetName, 
            OverwriteAction overwriteAction = OverwriteAction.AskUser) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if (asset == null)
            {
                throw new ArgumentNullException("Saving asset cannot be null");
            }

            if (string.IsNullOrEmpty(assetName))
            {
                assetName = "New_" + typeof(T).Name;
            }

            // 获取资产路径
            var assetPath = Path.Combine(folderPath, assetName);
            assetPath = GetAssetPath(assetPath, out var isOutsideAssets);
            folderPath = Path.GetDirectoryName(assetPath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                AssetDatabase.Refresh();
            }

            // 检查资产是否已经存在
            var existingPath = AssetDatabase.GetAssetPath(asset);
            var extension = string.IsNullOrEmpty(existingPath)
                ? GetAssetExtension(typeof(T))
                : Path.GetExtension(existingPath);
            assetPath += extension;
            var outsideAssetPath = string.Empty;
            if (isOutsideAssets)
            {
                outsideAssetPath = assetPath;
                assetPath = $"Assets/Tmp/{Path.GetFileName(assetPath)}";
            }
            if (!string.IsNullOrEmpty(existingPath))
            {
                var fullExistingPath = Path.GetFullPath(existingPath);
                var fullAssetPath = Path.GetFullPath(assetPath);
                if (!fullExistingPath.Equals(fullAssetPath, StringComparison.OrdinalIgnoreCase))
                {
                    if (!isOutsideAssets)
                    {
                        File.Copy(fullExistingPath, fullAssetPath, true);
                        AssetDatabase.Refresh();
                        return assetPath;
                    }
                    else
                    {
                        File.Copy(fullExistingPath, outsideAssetPath, true);
                        return outsideAssetPath;
                    }
                }
            }
            
            // 处理已存在的资产
            void OverwriteCallback(OverwriteAction action)
            {
                switch (action)
                {
                    case OverwriteAction.Overwrite:
                        // 覆盖现有资产
                        AssetDatabase.DeleteAsset(assetPath);
                        SaveAssetInternal(asset, assetPath);
                        break;

                    case OverwriteAction.Rename:
                        // 生成唯一名称
                        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                        SaveAssetInternal(asset, assetPath);
                        break;

                    case OverwriteAction.Cancel:
                        break;
                    default:
                        break;
                }
            }

            T existingAsset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (existingAsset != null)
            {
                if (overwriteAction == OverwriteAction.AskUser)
                {
                    // 显示覆盖确认对话框
                    ShowOverwriteDialog(assetName, OverwriteCallback);
                } else 
                    OverwriteCallback(overwriteAction);
            }
            else
            {
                SaveAssetInternal(asset, assetPath);
            }

            if (isOutsideAssets)
            {
                // move from Assets/ to outside folder
                folderPath = Path.GetDirectoryName(outsideAssetPath);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    AssetDatabase.Refresh();
                }
                File.Move(assetPath, outsideAssetPath);
            }
            return assetPath;
#else
            throw new ArgumentNullException("Cannot save asset outside Editor");
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// 内部保存资产实现
        /// </summary>
        internal static void SaveAssetInternal<T>(T asset, string assetPath, object settings = null) where T : UnityEngine.Object
        {
            try
            {
                bool success = false;

                if (typeof(T) == typeof(Texture2D))
                {
                    success = SaveTextureAsset(asset as Texture2D, assetPath, settings);
                }
                else if (typeof(T) == typeof(Mesh))
                {
                    success = SaveMeshAsset(asset as Mesh, assetPath, settings);
                }
                else if (typeof(T) == typeof(Material))
                {
                    success = SaveMaterialAsset(asset as Material, assetPath, settings);
                }
                else if (typeof(T) == typeof(GameObject))
                {
                    success = SaveGO(asset as GameObject, assetPath, settings);
                }
                else if (typeof(T) == typeof(VideoClip) || typeof(T) == typeof(AudioClip))
                {
                    success = SaveFileBackedImportedAsset(asset, assetPath);
                }

                if (!success) 
                {
                    throw new UnityException("Exception while saving");
                }
            }
            catch (Exception ex)
            {
                throw new UnityException($"Exception while saving: {ex.Message}");
            }
        }
        private static bool SaveFileBackedImportedAsset(UnityEngine.Object asset, string assetPath)
        {
            if (asset == null) return false;

            var srcPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(srcPath)) return false;

            var fullSrc = Path.GetFullPath(srcPath);
            var fullDst = Path.GetFullPath(assetPath);

            var dstDir = Path.GetDirectoryName(fullDst);
            if (!Directory.Exists(dstDir)) Directory.CreateDirectory(dstDir);

            File.Copy(fullSrc, fullDst, true);
            AssetDatabase.Refresh();
            return true;
        }

        /// <summary>
        /// 显示覆盖确认对话框
        /// </summary>
        private static void ShowOverwriteDialog(string assetName, Action<OverwriteAction> callback)
        {
            // 在编辑器中弹出对话框
            int option = EditorUtility.DisplayDialogComplex(
                "Save path already has an existing asset",
                $"Asset '{assetName}' already exist. Would you like to overwrite it?",
                "Overwrite",
                "Cancel",
                "Rename"
            );

            switch (option)
            {
                case 0: // 覆盖
                    callback?.Invoke(OverwriteAction.Overwrite);
                    break;

                case 1: // 取消
                    callback?.Invoke(OverwriteAction.Cancel);
                    break;

                case 2: // 重命名
                    callback?.Invoke(OverwriteAction.Rename);
                    break;
            }
        }

        /// <summary>
        /// 特殊处理：保存Texture2D资产
        /// </summary>
        public static bool SaveTextureAsset(Texture2D texture, string assetPath, object settings = null)
        {
            if (!texture.isReadable)
            {
                RenderTexture tmp = RenderTexture.GetTemporary(
                    texture.width,
                    texture.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

                Graphics.Blit(texture, tmp);
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = tmp;

                Texture2D copy = new Texture2D(texture.width, texture.height);
                copy.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
                copy.Apply();

                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(tmp);

                texture = copy;
            }

            byte[] pngData = texture.EncodeToPNG();
            File.WriteAllBytes(assetPath, pngData);

            AssetDatabase.Refresh();

            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (textureImporter != null)
            {
                if (settings == null)
                {
                    // sprite
                    textureImporter.textureType = TextureImporterType.Sprite;
                    textureImporter.spriteImportMode = SpriteImportMode.Single;
                }
                else if (settings is CustomTextureSettings cts && cts != null)
                {
                    // texture
                    var textureImporterSettings = new TextureImporterSettings();
                    textureImporter.ReadTextureSettings(textureImporterSettings);

                    textureImporterSettings.textureType = cts.type;
                    if (textureImporterSettings.textureType == TextureImporterType.NormalMap)
                        textureImporterSettings.convertToNormalMap = false;

                    textureImporterSettings.sRGBTexture = cts.isSRGB;
                    textureImporter.SetTextureSettings(textureImporterSettings);
                }
                textureImporter.isReadable = true;

                textureImporter.SaveAndReimport();
            }
            else
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 特殊处理：保存Mesh资产
        /// </summary>
        public static bool SaveMeshAsset(Mesh mesh, string assetPath, object settings = null)
        {
            AssetDatabase.CreateAsset(mesh, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;
        }

        /// <summary>
        /// 特殊处理：保存Material资产
        /// </summary>
        public static bool SaveMaterialAsset(Material material, string assetPath, object settings = null)
        {
            AssetDatabase.CreateAsset(material, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;
        }

        public static bool SaveGO(GameObject go, string assetPath, object settings = null)
        {
            go.hideFlags = HideFlags.HideInHierarchy;
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, assetPath);
            go.hideFlags = HideFlags.HideAndDontSave;
            return true;
        }
#endif

        public static IEnumerator ExportArtifact(TJAIBaseAssetNode node, previewData data)
        {
            if (node == null || data.Guid == null || data.Guid == string.Empty)
            {
                throw new ArgumentNullException("Data cannot be found in Database");
            }

            var artifact = node.currentArtifact;

#if UNITY_EDITOR
            string defaultName = artifact.GetGuID();
            string path = EditorUtility.SaveFilePanel(title, exportDirectory, defaultName, artifact.extension);
            if (string.IsNullOrEmpty(path))
                yield break;

            path = GetUniquePath(path);
            Assert.IsFalse(string.IsNullOrEmpty(path));

            // if (!IsInAssets(path, out _))
            // {
            //     throw new ArgumentNullException("Not allowed to export outside of project.");
            // }

            exportDirectory = Path.GetDirectoryName(path);

            yield return artifact.Export(path, 3, data.Guid);
#else
            yield break;
#endif
        }

        public static IEnumerator ExportArtifacts(Dictionary<BaseNode, IReadOnlyList<previewData>> artifacts)
        {

#if UNITY_EDITOR
            string directory = EditorUtility.SaveFolderPanel(title, exportDirectory, "");
            if (string.IsNullOrEmpty(directory))
                yield break;

            // if (!IsInAssets(directory, out _))
            // {
            //     throw new ArgumentNullException("Not allowed to export outside of project.");
            // }

            foreach (var p in artifacts)
            {
                TJAIBaseAssetNode currNode = p.Key as TJAIBaseAssetNode;
                string subdir = Path.Combine(directory, currNode.GetCustomName());
                if (!Directory.Exists(subdir))
                    Directory.CreateDirectory(subdir);

                var artifact = currNode.currentArtifact;
                foreach (var preview in p.Value)
                {
                    string defaultName = preview.Guid;
                    string path = GetUniquePath(subdir, defaultName, artifact.extension);
                    Assert.IsFalse(string.IsNullOrEmpty(path));

                    yield return artifact.Export(path, 3, preview.Guid);
                }
            }
#else
            yield break;
#endif
        }


        public static string GetUniquePath(string path)
            => GetUniquePath(directory: Path.GetDirectoryName(path),
                             fileName: Path.GetFileNameWithoutExtension(path),
                             extension: Path.GetExtension(path).Substring(1));

        /// <summary>
        /// Gets a unique path of with a specified directory, file name, and extension.
        /// </summary>
        /// <param name="directory">Directory.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="extension">File extension.</param>
        /// <returns>Unique full save path for a file.</returns>
        public static string GetUniquePath(string directory, string fileName, string extension)
        {
            var path = GetPath(directory, fileName, extension, maxSystemPathLength - charactersReservedForUniquePath);
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            if (!File.Exists(path))
                return path;

            directory = Path.GetDirectoryName(path);
            fileName = Path.GetFileNameWithoutExtension(path);

            Debug.Assert(directory != null);
            Debug.Assert(fileName != null);

            var uniqueFileName = FindUniqueFileName(directory, fileName, extension);
            var uniquePath = Path.Combine(directory, $"{uniqueFileName}.{extension}");

            return uniquePath;
        }

        static string FindUniqueFileName(string directory, string fileName, string extension)
        {
            var runningNumber = 1;

            while (File.Exists(GetPath(directory, $"{fileName} {runningNumber}", extension)))
                runningNumber++;

            return $"{fileName} {runningNumber}";
        }

        static string GetPath(string directory, string fileName, string extension, int maxPathLength = maxSystemPathLength)
        {
            if (string.IsNullOrEmpty(directory))
            {
                Debug.Log($"Incorrect directory: {directory}.");
                return string.Empty;
            }

            directory = GetAbsolutePath(directory);

            if (string.IsNullOrEmpty(fileName))
            {
                Debug.Log($"Incorrect file name: {fileName}.");
                return string.Empty;
            }

            if (string.IsNullOrEmpty(extension))
            {
                Debug.Log($"Incorrect extension: {extension}.");
                return string.Empty;
            }

            var path = Path.Combine(directory, $"{fileName}.{extension}");
            var characters = path.Length;
            if (characters <= maxPathLength)
                return path;

            var exceedingCharacters = characters - maxPathLength;
            if (exceedingCharacters < fileName.Length)
            {
                fileName = fileName.Substring(0, fileName.Length - exceedingCharacters);
                return Path.Combine(directory, $"{fileName}.{extension}");
            }

            Debug.Log($"The specified path is too long: {path}.");
            return string.Empty;
        }

        static string GetAbsolutePath(string path)
        {
            if (path.StartsWith(assetsRoot))
                path = path.Replace("Assets", Application.dataPath);
            path = path.Replace("\\", "/");
            return path;
        }

        /// <summary>
        /// Checks whether the specified path is within the Assets folder.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="relativePath">Path relative to the Assets folder.</param>
        /// <returns>True if the path is within the Assets folder.</returns>
        public static bool IsInAssets(string path, out string relativePath)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                relativePath = null;
                return false;
            }

            relativePath = GetPathRelativeToRoot(path);
            return !string.IsNullOrWhiteSpace(relativePath) && relativePath.StartsWith(assetsRoot);
        }

        static string GetPathRelativeToRoot(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;
#if UNITY_EDITOR
            return path.StartsWith(assetsRoot) ? path : FileUtil.GetProjectRelativePath(path);
#else
            // 获取项目根目录的绝对路径
            string projectPath = Application.dataPath.Replace("/Assets", "");

            // 确保路径是绝对路径
            if (!Path.IsPathRooted(path))
            {
                Debug.LogError("The provided path is not an absolute path.");
                return null;
            }

            // 将绝对路径转换为相对于项目根目录的路径
            if (path.StartsWith(projectPath))
            {
                string relativePath = path.Substring(projectPath.Length + 1);
                return relativePath;
            }
            else
            {
                Debug.LogError("The provided path is not within the project directory.");
                return null;
            }
#endif
        }
    }
}
