using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine.AIGraph;

namespace UnityEditor.AIGraph
{
    public class NormalMapImporter : AssetPostprocessor
    {
        private static readonly string[] imageExtensions =
        {
            ".png", ".jpg", ".jpeg", ".tga", ".bmp", ".tif", ".tiff", ".psd", ".gif"
        };
        private static readonly List<string> SupportedFolders = new()
        {
            GlobalConstants.AI_GRAPH_FOLDER, GlobalConstants.AI_GRAPH_EXAMPLE_FOLDER
        };

        private static readonly List<string> SupportedKeywords = new()
        {
            "Normal", "_2.jpg"
        };
        private static readonly List<string> SkyboxKeywords = new()
        {
            "skybox_", ".exr"
        };
        private bool IsOurTexture()
        {
            if (string.IsNullOrEmpty(assetPath) || !IsImageFile(assetPath) ||
                !SupportedFolders.Any(assetPath.StartsWith))
                return false;
            var assetName = Path.GetFileName(assetPath);
            return SupportedKeywords.Any(kw => assetName.Contains(kw, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsSkyboxTexture()
        {
            if (string.IsNullOrEmpty(assetPath) || !IsImageFile(assetPath) ||
                !SupportedFolders.Any(assetPath.StartsWith))
                return false;
            var assetName = Path.GetFileName(assetPath);
            return SkyboxKeywords.Any(kw => assetName.Contains(kw, StringComparison.OrdinalIgnoreCase));
        }

        void OnPreprocessTexture()
        {
            var importer = assetImporter as TextureImporter;
            // process exr
            if (IsSkyboxTexture() && importer != null)
            {
                importer.isReadable = true;
                importer.mipmapEnabled = false;
                return;
            }

            // process normal map
            if (IsOurTexture() && importer != null)
            {
                importer.textureType = TextureImporterType.NormalMap;
            }
        }

        public static bool IsImageFile(string assetPath)
        {
            // 如果路径为空，直接返回false
            if (string.IsNullOrEmpty(assetPath))
                return false;

            // 转换为小写
            string lowerPath = assetPath.ToLower();

            // 检查路径是否指向一个文件（而不是目录）
            if (!string.IsNullOrEmpty(System.IO.Path.GetFileName(lowerPath)))
            {
                // 获取扩展名
                string extension = System.IO.Path.GetExtension(lowerPath);

                // 检查扩展名是否在图像扩展名列表中
                foreach (string ext in imageExtensions)
                {
                    if (extension == ext)
                        return true;
                }
            }

            return false;
        }
    }
}