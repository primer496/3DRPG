using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UnityEngine.AIGraph
{
    public static class StringUtils
    {
        public static void ValidFootnote(ref string footnote)
        {
            if (string.IsNullOrEmpty(footnote)) return;
            footnote = footnote.Replace("\n", "").Replace(" ", "");
            if (footnote.Length <= 16) return;
            Debug.LogWarning("Footnote cannot exceed 16 characters, overflow part will be ignored.");
            footnote = footnote[..Math.Min(footnote.Length, 16)];
        }

        public static string UnderScoreToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            input = input.Replace(" ", string.Empty);
            var parts = input.Split('_', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return string.Empty;

            var result = new StringBuilder();

            foreach (var part in parts)
            {
                if (part.Length <= 0) continue;
                result.Append(char.ToUpper(part[0]));
                if (part.Length > 1)
                {
                    result.Append(part[1..].ToLower());
                }
            }

            return result.ToString();
        }


        // Windows 文件名非法字符
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

        private static readonly Regex InvalidCharsRegex =
            new Regex(@"[<>:""/\\|?*\u0000-\u001F]", RegexOptions.Compiled);

        private static readonly Regex ChineseCharsRegex = new Regex(@"[\u4e00-\u9fff]", RegexOptions.Compiled);

        /// <summary>
        /// 清理文件名，确保合法且不包含中文
        /// </summary>
        /// <param name="input">原始文件名</param>
        /// <param name="defaultValue">默认文件名（无扩展名部分）</param>
        /// <returns>合法的文件名</returns>
        public static string CleanFileName(string input, string defaultValue = "model")
        {
            if (string.IsNullOrEmpty(input))
                return GetSafeFileName(defaultValue);

            string ext;
            try
            {
                ext = Path.GetExtension(input);
            }
            catch (ArgumentException)
            {
                ext = ExtractExtension(input);
            }
            var name = string.IsNullOrEmpty(ext) ? input : input[..^ext.Length];

            var cleanedName = CleanFileNamePart(name, defaultValue);
            if (string.IsNullOrWhiteSpace(cleanedName))
                cleanedName = GetSafeFileName(defaultValue);
            var cleanedExtension = CleanExtension(ext);
            return string.IsNullOrEmpty(cleanedExtension) ? cleanedName : $"{cleanedName}{cleanedExtension}";
        }

        /// <summary>
        /// 清理文件名部分（不含扩展名）
        /// </summary>
        private static string CleanFileNamePart(string fileName, string defaultValue)
        {
            if (string.IsNullOrEmpty(fileName))
                return GetSafeFileName(defaultValue);
            var noChinese = ChineseCharsRegex.Replace(fileName, "");
            var noInvalidChars = InvalidCharsRegex.Replace(noChinese, "");
            var cleaned = new string(noInvalidChars.Where(c => !char.IsControl(c)).ToArray());
            cleaned = cleaned.Trim().Trim('.');
            return string.IsNullOrWhiteSpace(cleaned) ? GetSafeFileName(defaultValue) : cleaned;
        }

        /// <summary>
        /// 清理扩展名
        /// </summary>
        private static string CleanExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return string.Empty;

            // 移除扩展名中的非法字符，但保留点号
            var cleaned = new string(extension
                .Where(c => c == '.' || !InvalidFileNameChars.Contains(c) && !char.IsControl(c)).ToArray());

            if (!cleaned.StartsWith(".") && !string.IsNullOrEmpty(cleaned))
                cleaned = "." + cleaned;

            // 移除多余的点号
            cleaned = Regex.Replace(cleaned, @"\.+", ".");

            // 限制扩展名长度（通常不超过10个字符）
            if (cleaned.Length > 11) // 1个点号 + 10个字符
                cleaned = cleaned[..11];

            return cleaned;
        }

        /// <summary>
        /// 自定义扩展名提取（用于处理包含非法字符的路径）
        /// </summary>
        private static string ExtractExtension(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var lastDotIndex = input.LastIndexOf('.');
            if (lastDotIndex < 0 || lastDotIndex == input.Length - 1)
                return string.Empty;

            var potentialExtension = input[lastDotIndex..];
            // 简单验证扩展名（通常只包含字母和数字）
            return Regex.IsMatch(potentialExtension, @"^\.[a-zA-Z0-9]{1,10}$") ? potentialExtension : string.Empty;
        }

        /// <summary>
        /// 自定义移除扩展名
        /// </summary>
        private static string RemoveExtension(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var lastDotIndex = input.LastIndexOf('.');
            return lastDotIndex < 0 ? input : input[..lastDotIndex];
        }

        /// <summary>
        /// 获取安全的文件名（确保默认值也是合法的）
        /// </summary>
        private static string GetSafeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "file";

            var safeName = InvalidCharsRegex.Replace(fileName, "");
            safeName = ChineseCharsRegex.Replace(safeName, "");
            safeName = new string(safeName.Where(c => !char.IsControl(c)).ToArray());
            safeName = safeName.Trim().Trim('.');

            return string.IsNullOrWhiteSpace(safeName) ? "file" : safeName;
        }
    }
}