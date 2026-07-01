using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.U2D;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.AIGraph
{
    public static class TextureUtils
    {
        const int CurveTextureResolution = 512;
        static Color[] pixels = new Color[CurveTextureResolution];
        static Dictionary<TextureDimension, Texture> blackTextures = new Dictionary<TextureDimension, Texture>();
        static Dictionary<TextureDimension, Texture> whiteTextures = new Dictionary<TextureDimension, Texture>();
        public static Texture2D defaultTexture => Texture2D.grayTexture;
        
        // Do not change change these names, it would break all graphs that are using default texture values
        // static readonly string blackDefaultTextureName = "SDMix Black";
        //static readonly string whiteDefaultTextureName = "SDMix white";

        public static Texture2D Create(object context = null)
        {
            var texture = new Texture2D(2, 2);
            ObjectUtils.Retain(texture, context);
            return texture;
        }

        public static void SafeDestroy(this RenderTexture texture)
        {
            if (texture == null)
                return;

            texture.Release();

            ((UnityEngine.Object)texture).SafeDestroy();
        }

        public static Texture2D ToTexture2D(this byte[] bytes)
        {
            Texture2D texture = null;

            if (bytes?.Length > 0)
            {
                texture = Create();
                texture.LoadImage(bytes);
                texture.Apply();
            }

            return texture;
        }

        public static Texture2D ToTexture2D(this string base64)
        {
            Texture2D texture = null;

            if (!string.IsNullOrEmpty(base64))
            {
                texture = Create();
                texture.LoadImage(Convert.FromBase64String(base64));
                texture.Apply();
            }

            return texture;
        }

        public static Texture2D ToTexture2D(this RenderTexture rTex)
        {
            // Save current RenderTexture
            RenderTexture currentActiveRT = RenderTexture.active;

            // Set the supplied RenderTexture as the active one
            RenderTexture.active = rTex;

            // Create a new Texture2D and read the RenderTexture image into it
            Texture2D tex = new Texture2D(rTex.width, rTex.height, rTex.graphicsFormat, TextureCreationFlags.None);

            // Copy pixels to texture
            tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            tex.Apply();

            // Restore previously active RenderTexture
            RenderTexture.active = currentActiveRT;

            return tex;
        }

        public static Texture2D SetAlphaToOne(this Texture2D tex)
        {
            var temporary = RenderTexture.GetTemporary(tex.width, tex.height);
            ExportMaterial.SetMainTexture(tex);
            Graphics.Blit(tex, temporary, ExportMaterial.material);
            tex = temporary.ToTexture2D();
            RenderTexture.ReleaseTemporary(temporary);

            return tex;
        }

        public static Texture2D SetAlphaToRGB(this Texture2D tex)
        {
            var temporary = RenderTexture.GetTemporary(tex.width, tex.height);
            ImportMaterial.SetMainTexture(tex);
            Graphics.Blit(tex, temporary, ImportMaterial.material);
            tex = temporary.ToTexture2D();
            RenderTexture.ReleaseTemporary(temporary);

            return tex;
        }

        public static Texture2D CreateTemporaryDuplicate(Texture2D original, int width, int height, TextureFormat format = TextureFormat.RGBA32)
        {
            //if (!ShaderUtil.hardwareSupportsRectRenderTexture || !(bool)(UnityEngine.Object)original)
            if (original == null)
                return (Texture2D)null;
            RenderTexture active = RenderTexture.active;
            RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, SystemInfo.GetGraphicsFormat(DefaultFormat.LDR));
            Graphics.Blit((UnityEngine.Texture)original, temporary);
            RenderTexture.active = temporary;
            bool flag = width >= SystemInfo.maxTextureSize || height >= SystemInfo.maxTextureSize;
            Texture2D temporaryDuplicate = new Texture2D(width, height, format, original.mipmapCount > 1 || flag);
            temporaryDuplicate.ReadPixels(new Rect(0.0f, 0.0f, (float)width, (float)height), 0, 0);
            temporaryDuplicate.Apply();
#if UNITY_EDITOR
            temporaryDuplicate.alphaIsTransparency = original.alphaIsTransparency;
#endif
            RenderTexture.active = active;
            RenderTexture.ReleaseTemporary(temporary);
            return temporaryDuplicate;
        }

        public static Texture2D SaveTexture2DToFile(string fileName, Texture2D texture)
        {
#if UNITY_EDITOR
            if (texture != null)
            {
                if (!texture.isReadable)
                    texture = CreateTemporaryDuplicate(texture, texture.width, texture.height);
                var savedLocation = SaveBytesToFile(fileName, texture.EncodeToPNG());
                return AssetDatabase.LoadAssetAtPath<Texture2D>(savedLocation);
            }
#endif
            return null;
        }

        public static string SaveBytesToFile(string fileName, byte[] bytes)
        {
#if UNITY_EDITOR
            var f = AssetDatabase.GenerateUniqueAssetPath(fileName);
            File.WriteAllBytes(f, bytes);
            AssetDatabase.ImportAsset(f, ImportAssetOptions.Default);
            return f;
#else
            return string.Empty;
#endif

        }

        public static void LogFile(string filename, string log)
        {
#if UNITY_EDITOR
            File.AppendAllText(filename, log);
#endif
        }

        public static Texture2D SpriteAsTexture(UnityEngine.Sprite sprite)
        {
            var texture = sprite.texture;
            Matrix4x4 transform = Matrix4x4.identity;
            var uvs = sprite.GetVertexAttribute<Vector2>(VertexAttribute.TexCoord0);
            Vector2[] vertices = sprite.vertices;
            var triangles = sprite.triangles;
            Vector2 pivot = sprite.pivot;
            var spriteWidth = sprite.rect.width;
            var spriteHeight = sprite.rect.height;

            var restoreRT = RenderTexture.active;
            var renderTexture = new RenderTexture((int)sprite.rect.width, (int)sprite.rect.height, 0, RenderTextureFormat.ARGB32);

            RenderTexture.active = renderTexture;
            var temporary = RenderTexture.GetTemporary(renderTexture.descriptor);
            var copyMaterial = new Material(Shader.Find("Hidden/BlitCopy"));
            copyMaterial.mainTexture = texture;
            copyMaterial.mainTextureScale = Vector2.one;
            copyMaterial.mainTextureOffset = Vector2.zero;
            copyMaterial.SetPass(0);
            GL.Clear(true, true, new Color(1f, 1f, 1f, 0f));
            GL.PushMatrix();
            GL.LoadOrtho();
            GL.Begin(GL.TRIANGLES);
            Color color = Color.white;
            float pixelsToUnits = sprite.rect.width / sprite.bounds.size.x;
            for (int i = 0; i < triangles.Length; ++i)
            {
                ushort index = triangles[i];
                Vector3 vertex = vertices[index];
                vertex = transform.MultiplyPoint(vertex);
                Vector2 uv = uvs[index];
                GL.Color(color);
                GL.TexCoord(new Vector3(uv.x, uv.y, 0));
                GL.Vertex3((vertex.x * pixelsToUnits + pivot.x) / spriteWidth, (vertex.y * pixelsToUnits + pivot.y) / spriteHeight, 0);
            }
            GL.End();
            GL.PopMatrix();

            Texture2D copy = new Texture2D((int)spriteWidth, (int)spriteHeight, TextureFormat.RGBA32, false);
            copy.hideFlags = HideFlags.HideAndDontSave;
            copy.filterMode = texture != null ? texture.filterMode : FilterMode.Point;
            copy.anisoLevel = texture != null ? texture.anisoLevel : 0;
            copy.wrapMode = texture != null ? texture.wrapMode : TextureWrapMode.Clamp;
            copy.ReadPixels(new Rect(0, 0, spriteWidth, spriteHeight), 0, 0);
            copy.Apply();
            RenderTexture.ReleaseTemporary(temporary);

            RenderTexture.active = restoreRT;
            copyMaterial.SafeDestroy();
            renderTexture.SafeDestroy();
            return copy;
        }

        public static bool Validate(this Texture2D img)
        {
            if (img && !img.isReadable)
            {
                //Debug.LogError("<b>[TJAI]</b> Input image must be readable, please enable read/write in the import settings");
                return false;
            }

            if (img && IsTextureCompressed(img))
            {
                //Debug.LogError($"<b>[TJAI]</b> Input image must be not be compressed. Please remove compression from the import settings.");
                return false;
            }

            return true;
        }

        public static bool IsTextureCompressed(this Texture2D texture)
        {
            var format = texture.format;

            switch (format)
            {
                case TextureFormat.DXT1:
                case TextureFormat.DXT5:
                case TextureFormat.PVRTC_RGB2:
                case TextureFormat.PVRTC_RGBA2:
                case TextureFormat.PVRTC_RGB4:
                case TextureFormat.PVRTC_RGBA4:
                case TextureFormat.ETC_RGB4:
                case TextureFormat.ETC2_RGBA8:
                case TextureFormat.ASTC_4x4:
                    return true;
                default:
                    return false;
            }
        }

        public static void UpdateTextureFromCurve(AnimationCurve curve, ref Texture2D curveTexture)
        {
            if (curveTexture == null)
            {
                curveTexture = new Texture2D(CurveTextureResolution, 1, TextureFormat.RFloat, false, true);
                curveTexture.wrapMode = TextureWrapMode.Clamp;
                curveTexture.filterMode = FilterMode.Bilinear;
                curveTexture.hideFlags = HideFlags.HideAndDontSave;
            }

            for (int i = 0; i < CurveTextureResolution; i++)
            {
                float t = (float)i / (CurveTextureResolution - 1);
                pixels[i] = new Color(curve.Evaluate(t), 0, 0, 1);
            }
            curveTexture.SetPixels(pixels);
            curveTexture.Apply(false);
        }
        
        public static Texture GetBlackTexture(TextureDimension dim, int sliceCount = 0)
        {
            return Texture2D.blackTexture;
            // Texture blackTexture;
            //
            // if (dim == TextureDimension.Any || dim == TextureDimension.Unknown || dim == TextureDimension.None)
            //     throw new Exception($"Unable to create white texture for type {dim}");
            //
            // if (blackTextures.TryGetValue(dim, out blackTexture))
            // {
            //     // We don't cache texture arrays
            //     if (dim != TextureDimension.Tex2DArray && dim != TextureDimension.Tex2DArray)
            //         return blackTexture;
            // }
            //
            // blackTexture = CreateColorRenderTexture(dim, Color.black);
            // blackTexture.name = blackDefaultTextureName;
            // blackTextures[dim] = blackTexture;
            //
            // return blackTexture;
        }
        
        // public static RenderTexture CreateColorRenderTexture(TextureDimension dim, Color color)
        // {
        //     RenderTexture rt = new RenderTexture(1, 1, 0, GraphicsFormat.R8G8B8A8_UNorm, 1)
        //     {
        //         volumeDepth = 1,
        //         dimension = dim,
        //         enableRandomWrite = true,
        //         hideFlags = HideFlags.HideAndDontSave
        //     };
        //     rt.Create();
        //
        //     var cmd = CommandBufferPool.Get();
        //     for (int i = 0; i < GetSliceCount(rt); i++)
        //     {
        //         cmd.SetRenderTarget(rt, 0, (CubemapFace)i, i);
        //         cmd.ClearRenderTarget(false, true, color);
        //     }
        //
        //     Graphics.ExecuteCommandBuffer(cmd);
        //
        //     return rt;
        // }
        
        public static int GetSliceCount(Texture tex)
        {
            if (tex == null)
                return 0;

            switch (tex)
            {
                case Texture2D _:
                    return 1;
                case Texture2DArray t:
                    return t.depth;
                case Texture3D t:
                    return t.depth;
                case CubemapArray t:
                    return t.cubemapCount;
                case Cubemap _:
                    return 1;
                case RenderTexture rt:
                    if (rt.dimension == TextureDimension.Tex2D || rt.dimension == TextureDimension.Cube)
                        return 1;
                    else if (rt.dimension == TextureDimension.Tex3D || rt.dimension == TextureDimension.Tex2DArray || rt.dimension == TextureDimension.CubeArray)
                        return rt.volumeDepth;
                    else
                        return 0;
                default:
                    return 0;
            }
        }

        static class ExportMaterial
        {
            static readonly int k_MainTexture = Shader.PropertyToID("_MainTex");

            public static readonly Material material = new Material(Resources.Load<Shader>("Shaders/DoodleExport")) { hideFlags = HideFlags.HideAndDontSave };

            public static void SetMainTexture(Texture texture) => material.SetTexture(k_MainTexture, texture);
        }

        static class ImportMaterial
        {
            static readonly int k_MainTexture = Shader.PropertyToID("_MainTex");

            public static readonly Material material = new Material(Resources.Load<Shader>("Shaders/DoodleImport")) { hideFlags = HideFlags.HideAndDontSave };

            public static void SetMainTexture(Texture texture) => material.SetTexture(k_MainTexture, texture);
        }
        
        // public static Texture2D CombineMetallicAndRoughness(Texture2D metallic, Texture2D roughness)
        // {
        //     Texture2D normalTexture = new Texture2D(metallic.width, metallic.height, TextureFormat.RGBA32, true, true);
        //
        //     CombineMetallicAndRoughnessJob job = new CombineMetallicAndRoughnessJob()
        //     {
        //         metallicPixels = metallic.GetPixelData<Color32>(0),
        //         roughnessPixels = roughness.GetPixelData<Color32>(0),
        //         OutputPixels = normalTexture.GetPixelData<Color32>(0)
        //     };
        //
        //     var handle = job.Schedule(job.metallicPixels.Length, 32);
        //     handle.Complete();
        //
        //     normalTexture.SetPixelData(job.OutputPixels, 0);
        //     normalTexture.Apply();
        //
        //     return normalTexture;
        // }
        //
        //
        // [BurstCompile(FloatPrecision.Standard, FloatMode.Default, CompileSynchronously = true)]
        // struct CombineMetallicAndRoughnessJob : IJobParallelFor
        // {
        //     public NativeArray<Color32> metallicPixels;
        //     public NativeArray<Color32> roughnessPixels;
        //     public NativeArray<Color32> OutputPixels;
        //
        //     public void Execute(int index)
        //     {
        //         var gloss = (byte)(255 - roughnessPixels[index].r);
        //         OutputPixels[index] = new Color32(metallicPixels[index].r, 0, 0, gloss);
        //     }
        // }

        public static Texture2D CopyTexture(Texture value)
        {
            // FIX: Convert input value to RenderTexture and then convert back to Texture,
            // by this way can we import Textures that Read/Write property is False.
            RenderTexture rt = RenderTexture.GetTemporary(
                value.width, value.height, 
                0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
            Graphics.Blit(value, rt);
            
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            
            var tex = new Texture2D(value.width, value.height);
            tex.ReadPixels(new Rect(0, 0, value.width, value.height), 0, 0);
            tex.Apply();
            
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            return tex;
        }

        public static Texture2D ConvertToGrayscale(Texture2D source)
        {
            if (source == null)
            {
                Debug.LogError("Source texture is null.");
                return null;
            }

            var newTexture = new Texture2D(source.width, source.height, TextureFormat.Alpha8, false);

            var pixels = source.GetPixels();
            for (var i = 0; i < pixels.Length; i++)
            {
                var pixel = pixels[i];
                var gray = 0.299f * pixel.r + 0.587f * pixel.g + 0.114f * pixel.b;
                newTexture.SetPixel(i % source.width, i / source.width, new Color(gray, gray, gray, pixel.a));
            }
            newTexture.Apply();

            return newTexture;
        }

        public static Texture2D ConvertAlphaToGrayscale(Texture2D source, bool flip = false)
        {
            if (source == null)
            {
                Debug.LogError("Source texture is null.");
                return null;
            }

            var newTexture = new Texture2D(source.width, source.height, TextureFormat.R8, false);

            var colors = source.GetPixels();
            if (flip)
            {
                for (var i = 0; i < colors.Length; i++)
                {
                    var alpha = 1.0f - colors[i].a;
                    newTexture.SetPixel(i % source.width, i / source.width, new Color(alpha, alpha, alpha, 1.0f));
                }
            }
            else
            {
                for (var i = 0; i < colors.Length; i++)
                {
                    var alpha = colors[i].a;
                    newTexture.SetPixel(i % source.width, i / source.width, new Color(alpha, alpha, alpha, 1.0f));
                }
            }
            newTexture.Apply();

            return newTexture;
        }
        
        public struct TextureChannelMapping
        {
            public Texture2D srcTex;
            public int srcChannel, tgtChannel;
        }

        public static string TextureFormatName(TextureFormat format)
        {
            switch (format)
            {
                // 单通道格式
                case TextureFormat.Alpha8:
                    return "A";
                case TextureFormat.R8:
                case TextureFormat.R16:
                case TextureFormat.RFloat:
                case TextureFormat.RHalf:
                case TextureFormat.BC4:
                    return "R";
                
                // 双通道格式
                case TextureFormat.RG16:
                case TextureFormat.RG32:
                case TextureFormat.RGFloat:
                case TextureFormat.RGHalf:
                case TextureFormat.BC5:
                    return "RG";
                
                // 三通道格式
                case TextureFormat.RGB24:
                case TextureFormat.RGB565:
                case TextureFormat.DXT1:
                case TextureFormat.BC6H:
                case TextureFormat.ETC_RGB4:
                case TextureFormat.ETC2_RGB:
                case TextureFormat.PVRTC_RGB2:
                case TextureFormat.PVRTC_RGB4:
                    return "RGB";
                
                // 四通道格式
                case TextureFormat.RGBA32:
                case TextureFormat.DXT5:
                case TextureFormat.BC7:
                case TextureFormat.ASTC_4x4:
                case TextureFormat.ASTC_5x5:
                case TextureFormat.ASTC_6x6:
                case TextureFormat.ASTC_8x8:
                case TextureFormat.ASTC_10x10:
                case TextureFormat.ASTC_12x12:
                case TextureFormat.PVRTC_RGBA2:
                case TextureFormat.PVRTC_RGBA4:
                case TextureFormat.ETC2_RGBA1:
                case TextureFormat.ETC2_RGBA8:
                case TextureFormat.RGBAFloat:
                    return "RGBA";
                
                case TextureFormat.ARGB32:
                    return "ARGB";
                
                case TextureFormat.BGRA32:
                    return "BGRA";
                
                default:
                    Debug.LogWarning($"未知的纹理格式: {format}");
                    return "RGBA";
            }
        }

        public static Texture2D Combine(List<TextureChannelMapping> textures, TextureFormat format = TextureFormat.RGBA32)
        {
            // check texture size
            textures?.RemoveAll(t => t.srcTex == null);
            if (textures == null || textures.Count == 0)
                return null;
            var width = textures[0].srcTex.width;
            var height = textures[0].srcTex.height;
            for (var i = 1; i < textures.Count; ++i)
            {
                var curTex = textures[i].srcTex;
                if (curTex.width == width && curTex.height == height) continue;
                Debug.LogError($"Please make sure all input textures have the same size! Current input: ({width},{height}) != ({curTex.width},{curTex.height})");
                return null;
            }

            var mergedTex = new Texture2D(width, height, format, false);
            var tgtPixels = mergedTex.GetPixels();
            foreach (var texMapping in textures)
            {
                var srcTex = texMapping.srcTex;
                if (!srcTex.isReadable)
                    srcTex = TextureUtils.CopyTexture(srcTex);
                var srcInd = texMapping.srcChannel;
                var tgtInd = texMapping.tgtChannel;
                var srcPixels = srcTex.GetPixels();
                for (var i = 0; i < width; ++i)
                {
                    for (var j = 0; j < height; ++j)
                    {
                        var pixelInd = j * width + i;
                        tgtPixels[pixelInd][tgtInd] = srcPixels[pixelInd][srcInd];
                    }
                }
            }
            mergedTex.SetPixels(tgtPixels);
            mergedTex.Apply();
            return mergedTex;
        }

        // public enum PaddingMode
        // {
        //     Nearest, // 使用最近像素值
        //     Linear, // 线性插值
        //     Zero, // 填充零值
        //     Clamp, // 使用边界值
        //     CustomColor
        // }
        //
        // public struct PaddingSize
        // {
        //     public int PadLeft;
        //     public int PadRight;
        //     public int PadTop;
        //     public int PadBottom;
        //
        //     public PaddingSize(int padLeft, int padRight, int padTop, int padBottom)
        //     {
        //         PadLeft = padLeft;
        //         PadRight = padRight;
        //         PadTop = padTop;
        //         PadBottom = padBottom;
        //     }
        // }
        //
        // [BurstCompile]
        // private struct TexturePaddingJob : IJob
        // {
        //     [ReadOnly] public NativeArray<Color32> originalPixels;
        //     [WriteOnly] public NativeArray<Color> paddedPixels;
        //
        //     public int originalWidth;
        //     public int originalHeight;
        //     public int padLeft;
        //     public int padRight;
        //     public int padTop;
        //     public int padBottom;
        //     public int paddingMode;
        //     public Color32 customColor;
        //
        //     public void Execute()
        //     {
        //         var paddedWidth = originalWidth + padLeft + padRight;
        //         var paddedHeight = originalHeight + padTop + padBottom;
        //
        //         for (var paddedX = 0; paddedX < paddedWidth; paddedX++)
        //         {
        //             for (var paddedY = 0; paddedY < paddedHeight; paddedY++)
        //             {
        //                 var originalX = paddedX - padLeft;
        //                 var originalY = paddedY - padTop;
        //
        //                 Color32 pixelColor;
        //                 if (originalX >= 0 && originalX < originalWidth &&
        //                     originalY >= 0 && originalY < originalHeight)
        //                 {
        //                     var originalIndex = originalY * originalWidth + originalX;
        //                     pixelColor = originalPixels[originalIndex];
        //                 }
        //                 else
        //                 {
        //                     pixelColor = GetPixelColor(originalX, originalY);
        //                 }
        //
        //                 var paddedIndex = paddedY * paddedWidth + paddedX;
        //                 paddedPixels[paddedIndex] = new Color(pixelColor.r / 255f, pixelColor.g / 255f,
        //                     pixelColor.b / 255f, pixelColor.a / 255f);
        //             }
        //         }
        //     }
        //
        //     private Color32 GetPixelColor(int originalX, int originalY)
        //     {
        //         switch ((PaddingMode)paddingMode)
        //         {
        //             case PaddingMode.Nearest:
        //                 return GetNearestPixel(originalX, originalY);
        //
        //             case PaddingMode.Linear:
        //                 return GetLinearPixel(originalX, originalY);
        //
        //             case PaddingMode.Zero:
        //                 return new Color32(0, 0, 0, 0);
        //
        //             case PaddingMode.Clamp:
        //                 return GetClampedPixel(originalX, originalY);
        //
        //             default:
        //                 return customColor;
        //         }
        //     }
        //
        //     private Color32 GetNearestPixel(int originalX, int originalY)
        //     {
        //         var clampedX = math.clamp(originalX, 0, originalWidth - 1);
        //         var clampedY = math.clamp(originalY, 0, originalHeight - 1);
        //         var index = clampedY * originalWidth + clampedX;
        //         return originalPixels[index];
        //     }
        //
        //     private Color32 GetClampedPixel(int originalX, int originalY)
        //     {
        //         var clampedX = math.clamp(originalX, 0, originalWidth - 1);
        //         var clampedY = math.clamp(originalY, 0, originalHeight - 1);
        //         var index = clampedY * originalWidth + clampedX;
        //         return originalPixels[index];
        //     }
        //
        //     private Color32 GetLinearPixel(int originalX, int originalY)
        //     {
        //         float x = math.clamp(originalX, 0, originalWidth - 1);
        //         float y = math.clamp(originalY, 0, originalHeight - 1);
        //
        //         var x0 = (int)math.floor(x);
        //         var y0 = (int)math.floor(y);
        //         var x1 = math.min(x0 + 1, originalWidth - 1);
        //         var y1 = math.min(y0 + 1, originalHeight - 1);
        //
        //         var tx = x - x0;
        //         var ty = y - y0;
        //
        //         var c00 = originalPixels[y0 * originalWidth + x0];
        //         var c10 = originalPixels[y0 * originalWidth + x1];
        //         var c01 = originalPixels[y1 * originalWidth + x0];
        //         var c11 = originalPixels[y1 * originalWidth + x1];
        //
        //         return LerpColor(LerpColor(c00, c10, tx), LerpColor(c01, c11, tx), ty);
        //     }
        //
        //     private Color32 LerpColor(Color32 a, Color32 b, float t)
        //     {
        //         t = math.clamp(t, 0f, 1f);
        //         return new Color32(
        //             (byte)math.lerp(a.r, b.r, t),
        //             (byte)math.lerp(a.g, b.g, t),
        //             (byte)math.lerp(a.b, b.b, t),
        //             (byte)math.lerp(a.a, b.a, t)
        //         );
        //     }
        // }
        //
        // public static Texture2D PadTexture(Texture2D originalTexture,
        //     PaddingSize paddingSize,
        //     PaddingMode paddingMode = PaddingMode.Nearest,
        //     Color? customColor = null)
        // {
        //     if (originalTexture == null)
        //         throw new ArgumentNullException(nameof(originalTexture));
        //
        //     if (paddingSize.PadLeft < 0 || paddingSize.PadRight < 0 || paddingSize.PadTop < 0 ||
        //         paddingSize.PadBottom < 0)
        //         throw new ArgumentException("Padding values cannot be negative");
        //
        //     var originalWidth = originalTexture.width;
        //     var originalHeight = originalTexture.height;
        //     var paddedWidth = originalWidth + paddingSize.PadLeft + paddingSize.PadRight;
        //     var paddedHeight = originalHeight + paddingSize.PadTop + paddingSize.PadBottom;
        //
        //     if (paddedWidth <= 0 || paddedHeight <= 0)
        //         throw new ArgumentException("Padded texture size must be positive");
        //
        //     // 创建Native Arrays
        //     var originalPixels = originalTexture.GetPixels32();
        //     var originalNativeArray = new NativeArray<Color32>(originalPixels, Allocator.TempJob);
        //     var paddedNativeArray =
        //         new NativeArray<Color>(paddedWidth * paddedHeight, Allocator.TempJob);
        //
        //     // 创建并执行Job
        //     var job = new TexturePaddingJob
        //     {
        //         originalPixels = originalNativeArray,
        //         paddedPixels = paddedNativeArray,
        //         originalWidth = originalWidth,
        //         originalHeight = originalHeight,
        //         padLeft = paddingSize.PadLeft,
        //         padRight = paddingSize.PadRight,
        //         padTop = paddingSize.PadTop,
        //         padBottom = paddingSize.PadBottom,
        //         paddingMode = (int)paddingMode,
        //         customColor = customColor ?? new Color32(0, 0, 0, 0)
        //     };
        //
        //     // 调度并完成Job
        //     var handle = job.Schedule();
        //     handle.Complete();
        //
        //     // 创建新的纹理
        //     var rgbaTexture = new Texture2D(paddedWidth, paddedHeight, TextureFormat.RGBA32, false);
        //     rgbaTexture.SetPixels(paddedNativeArray.ToArray());
        //     rgbaTexture.Apply();
        //
        //     // 清理Native Arrays
        //     originalNativeArray.Dispose();
        //     paddedNativeArray.Dispose();
        //
        //     return rgbaTexture;
        // }
    }
}