using System;
using System.Collections;
using System.Linq;

namespace UnityEngine.AIGraph
{
    public static class ParaUtils
    {
        public static bool IsNull(object obj)
        {
            if (obj == null)
                return true;
            var type = obj.GetType();
            if (typeof(string).IsAssignableFrom(type))
                return string.IsNullOrEmpty(obj as string);
            else if (obj is IList list)
                return list.Count == 0;
            return false;
        }

        public static bool IsNewer(string baseTime, string compTime)
        {
            var baseDate = ParseDate(baseTime);
            var compDate = ParseDate(compTime);
            return compDate >= baseDate;
        }

        private static DateTime ParseDate(string compactDate)
        {
            if (string.IsNullOrEmpty(compactDate) || compactDate.Length != 8)
                return new DateTime(0, 0, 0);
            var yearStr = compactDate.Substring(0, 4);
            var monthStr = compactDate.Substring(4, 2);
            var dayStr = compactDate.Substring(6, 2);
        
            // 转换为整数
            if (!int.TryParse(yearStr, out var year) ||
                !int.TryParse(monthStr, out var month) || 
                !int.TryParse(dayStr, out var day))
            {
                throw new ArgumentException("日期包含非数字字符");
            }
            return new DateTime(year, month, day);
        }
    }
}