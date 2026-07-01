using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.AIGraph;

namespace UnityEditor.AIGraph
{
    /// <summary>
    /// 确保纹理可enocde
    /// </summary>
    public class ConvertableTextureImporter : AssetPostprocessor
    {
        private static readonly List<string> SupportedFolders = new()
        {
            GlobalConstants.AI_GRAPH_FOLDER, GlobalConstants.AI_GRAPH_EXAMPLE_FOLDER
        };
        private static readonly List<string> SupportedPrefixes = new()
        {
            "tjai"
        };
        private bool IsOurTexture()
        {
            if (string.IsNullOrEmpty(assetPath) || !SupportedFolders.Any(assetPath.StartsWith))
                return false;
            var assetName = Path.GetFileName(assetPath);
            return SupportedPrefixes.Any(prefix => assetName.StartsWith($"{prefix}_"));
        }
        
        // 配置：支持 EncodeTo 方法的纹理格式
        private static readonly TextureImporterFormat[] SupportedFormats = new TextureImporterFormat[]
        {
            TextureImporterFormat.RGBA32,      // ✅ 支持 EncodeTo
            TextureImporterFormat.RGB24,       // ✅ 支持 EncodeTo
            TextureImporterFormat.ARGB32,      // ✅ 支持 EncodeTo
            TextureImporterFormat.RGBAHalf,    // ✅ 支持 EncodeTo (HDR)
            TextureImporterFormat.RGBAFloat,   // ✅ 支持 EncodeTo (HDR)
            TextureImporterFormat.R8,          // ✅ 支持 EncodeTo
            TextureImporterFormat.R16,         // ✅ 支持 EncodeTo
            TextureImporterFormat.Alpha8       // ✅ 支持 EncodeTo
        };
        
        /// <summary>
        /// 检查格式是否支持 EncodeTo 方法
        /// </summary>
        private static bool IsFormatSupportedForEncoding(TextureImporter importer)
        {
            // 获取平台特定的纹理设置（默认平台）
            TextureImporterPlatformSettings platformSettings = importer.GetDefaultPlatformTextureSettings();
        
            // 检查格式是否在支持的列表中
            foreach (var supportedFormat in SupportedFormats)
            {
                if (platformSettings.format == supportedFormat)
                {
                    return true;
                }
            }
        
            return false;
        }

        /// <summary>
        /// 设置纹理为支持 EncodeTo 的格式
        /// </summary>
        private static void SetSupportedFormat(TextureImporter importer)
        {
            TextureImporterPlatformSettings platformSettings = importer.GetDefaultPlatformTextureSettings();
        
            // 设置为推荐的格式（根据纹理是否有透明通道）
            bool hasAlpha = importer.DoesSourceTextureHaveAlpha();
            platformSettings.format = hasAlpha ? TextureImporterFormat.RGBA32 : TextureImporterFormat.RGB24;
        
            // 禁用压缩
            platformSettings.textureCompression = TextureImporterCompression.Uncompressed;
        
            importer.SetPlatformTextureSettings(platformSettings);
        }

        /// <summary>
        /// 在纹理导入完成前调整设置，强制开启 Read/Write（仅针对特定路径）
        /// </summary>
        void OnPreprocessTexture()
        {
            TextureImporter importer = assetImporter as TextureImporter;

            if (importer != null && IsOurTexture())
            {
                // 1. 启用 Read/Write
                if (!importer.isReadable)
                {
                    importer.isReadable = true;
                    UnityEngine.AIGraph.DebugUtils.ConditionLog($"Enabled Read/Write for texture: {importer.assetPath}");
                }
            
                // 2. 检查并设置支持的格式
                if (!IsFormatSupportedForEncoding(importer))
                {
                    SetSupportedFormat(importer);
                    UnityEngine.AIGraph.DebugUtils.ConditionLog($"Set supported format for EncodeTo: {importer.assetPath}");
                }
            
                // 3. 确保禁用压缩
                var platformSettings = importer.GetDefaultPlatformTextureSettings();
                if (platformSettings.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    platformSettings.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SetPlatformTextureSettings(platformSettings);
                    UnityEngine.AIGraph.DebugUtils.ConditionLog($"Disabled compression for texture: {importer.assetPath}");
                }
            }
        }
    }
}