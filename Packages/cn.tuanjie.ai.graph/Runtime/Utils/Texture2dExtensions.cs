using System;
using System.IO;
using UnityEditor;

namespace UnityEngine.AIGraph
{
    public static class Texture2DExtensions
    {
        /// <summary>
        /// 将Texture2D转换为Base64字符串
        /// </summary>
        /// <param name="texture">要转换的纹理</param>
        /// <param name="format">图像格式（默认JPG）</param>
        /// <returns>Base64编码的字符串</returns>
        public static string ToBase64(this Texture2D texture, ImageFormat format = ImageFormat.PNG)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture), "Texture cannot be null");

            try
            {
                byte[] imageData;
                // var assetPath = string.Empty;
                // #if UNITY_EDITOR
                // assetPath = AssetDatabase.GetAssetPath(texture);
                // #endif
                // if (!string.IsNullOrEmpty(assetPath))
                // {
                //     imageData = File.ReadAllBytes(assetPath);
                // }
                if (texture.isReadable && !IsCompressedFormat(texture.format))
                {
                    // 检查纹理是否可读
                    imageData = EncodeTexture(texture, format);
                }
                else
                {
                    // 创建可读副本后再编码
                    var readableTex = TextureUtils.CopyTexture(texture);
                    imageData = EncodeTexture(readableTex, format);
                }

                return Convert.ToBase64String(imageData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to convert texture to Base64: {ex.Message}");
                throw;
            }
        }

        private static byte[] EncodeTexture(Texture2D texture, ImageFormat format)
        {
            return format switch
            {
                ImageFormat.PNG => texture.EncodeToPNG(),
                ImageFormat.JPG => texture.EncodeToJPG(),
                ImageFormat.EXT => texture.EncodeToEXR(),
                ImageFormat.TGA => texture.EncodeToTGA(),
                _ => throw new ArgumentException($"Unsupported image format: {format}", nameof(format))
            };
        }
        private static bool IsCompressedFormat(TextureFormat format)
        {
            return format switch
            {
                TextureFormat.DXT1 or TextureFormat.DXT5 or TextureFormat.BC4 or TextureFormat.BC5 
                    or TextureFormat.BC6H
                    or TextureFormat.BC7 or TextureFormat.PVRTC_RGB2 or TextureFormat.PVRTC_RGBA2
                    or TextureFormat.PVRTC_RGB4 or TextureFormat.PVRTC_RGBA4 or TextureFormat.ETC_RGB4
                    or TextureFormat.ETC2_RGB or TextureFormat.ETC2_RGBA1 or TextureFormat.ETC2_RGBA8
                    or TextureFormat.ASTC_4x4 or TextureFormat.ASTC_5x5 or TextureFormat.ASTC_6x6
                    or TextureFormat.ASTC_8x8 or TextureFormat.ASTC_10x10 or TextureFormat.ASTC_12x12 => true,
                _ => false
            };
        }
    }

    /// <summary>
    /// 图像格式枚举
    /// </summary>
    public enum ImageFormat
    {
        JPG,
        PNG,
        EXT,
        TGA
    }
}