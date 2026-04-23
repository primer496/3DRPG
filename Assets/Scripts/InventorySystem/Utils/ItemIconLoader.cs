using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Model;

namespace InventorySystem.Utils
{
    public static class ItemIconLoader
    {
        private static Dictionary<string, Sprite> iconCache = new Dictionary<string, Sprite>();

        /// <summary>
        /// 加载物品图标 (加入缓存池优化)
        /// </summary>
        /// <param name="iconPath">图标路径</param>
        /// <returns>加载的Sprite</returns>
        public static Sprite LoadItemIcon(string iconPath)
        {
            if (string.IsNullOrEmpty(iconPath))
            {
                return null;
            }

            // 命中缓存则直接返回
            if (iconCache.TryGetValue(iconPath, out var cachedSprite))
            {
                return cachedSprite;
            }

            // Resources.Load 不允许带有文件后缀，如果有则移除
            string loadPath = iconPath;
            int dotIndex = loadPath.LastIndexOf('.');
            if (dotIndex > 0)
            {
                loadPath = loadPath.Substring(0, dotIndex);
            }

            try
            {
                // 先尝试加载为Sprite
                Sprite loadedSprite = Resources.Load<Sprite>(loadPath);
                if (loadedSprite != null)
                {
                    iconCache[iconPath] = loadedSprite; // 存入缓存
                    return loadedSprite;
                }

                // 如果Sprite为空，尝试加载为Texture2D并转换为Sprite (以防导入资源时未设置为Sprite 2D模式)
                Texture2D loadedTex = Resources.Load<Texture2D>(loadPath);
                if (loadedTex != null)
                {
                    Debug.LogWarning($"图片被作为Texture2D加载，为了性能请将 {loadPath} 在导入设置中修改为 Sprite (2D and UI)。");
                    Sprite newSprite = Sprite.Create(loadedTex, new Rect(0, 0, loadedTex.width, loadedTex.height), new Vector2(0.5f, 0.5f));
                    iconCache[iconPath] = newSprite; // 存入缓存
                    return newSprite;
                }

                Debug.LogWarning($"无法找到物品图标: {loadPath}。请检查路径是否在Resources文件夹下且拼写正确(不带后缀)。");
                iconCache[iconPath] = null; // 存入空标记，防止下次依然尝试反复读取硬盘
                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"加载物品图标失败: {loadPath}, 错误: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 构建物品图标路径
        /// </summary>
        /// <param name="category">物品分类</param>
        /// <param name="itemID">物品ID</param>
        /// <returns>构建的图标路径</returns>
        public static string BuildIconPath(ItemCategory category, string itemID)
        {
            string categoryName = category.ToString();
            return $"PackageIcon/{categoryName}/{itemID}";
        }
    }
}
