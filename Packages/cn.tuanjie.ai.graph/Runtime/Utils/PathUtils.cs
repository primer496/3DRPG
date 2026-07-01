using System;
using System.IO;

namespace UnityEngine.AIGraph
{
    public static class PathUtils
    {
        public static bool IsSamePath(string lhs, string rhs)
        {
            var fullLhs = Path.GetFullPath(lhs).Replace("\\", "/");
            var fullRhs = Path.GetFullPath(rhs).Replace("\\", "/");
            return string.Equals(fullLhs, fullRhs, StringComparison.OrdinalIgnoreCase);
        }

        public static string GetUrlExtension(string url, bool includeDot = true)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            try
            {
                // 创建Uri对象处理URL编码和格式
                var uri = new Uri(url);

                // 获取路径部分
                var path = uri.AbsolutePath;

                // 使用Path类获取扩展名
                var name = Path.GetFileNameWithoutExtension(path);
                if (string.IsNullOrEmpty(name))
                    return null;
                var extension = Path.GetExtension(path);

                // 移除点号并转换为小写
                if (!string.IsNullOrEmpty(extension) && extension.Length > 1)
                {
                    return includeDot ? extension.ToLower() : extension[1..].ToLower();
                }

                return null;
            }
            catch (UriFormatException)
            {
                // 如果URL格式无效，使用备用方法
                return GetUrlExtensionFallback(url);
            }
        }

        private static string GetUrlExtensionFallback(string url)
        {
            // 移除查询参数和哈希片段
            var queryIndex = url.IndexOf('?');
            var hashIndex = url.IndexOf('#');

            var cleanUrl = url;
            if (queryIndex >= 0)
                cleanUrl = cleanUrl[..queryIndex];
            if (hashIndex >= 0 && (queryIndex < 0 || hashIndex < queryIndex))
                cleanUrl = cleanUrl[..hashIndex];

            // 获取最后一部分路径
            var lastSlashIndex = cleanUrl.LastIndexOf('/');
            var filename = lastSlashIndex >= 0 ? cleanUrl[(lastSlashIndex + 1)..] : cleanUrl;

            // 获取扩展名
            var dotIndex = filename.LastIndexOf('.');
            if (dotIndex > 0 && dotIndex < filename.Length - 1) // 确保点不在开头
            {
                return filename[dotIndex..].ToLower();
            }

            return null;
        }
    }
}