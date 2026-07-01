using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.AIGraph
{
    public static class DebugUtils
    {
        public static void ConditionLog(string msg)
        {
#if TJAI_DEBUG
            Debug.Log(msg);
#endif
        }

        public static string ToString<T>(IEnumerable<T> collection)
        {
            if (collection == null) return "null";
            var elements = collection.ToList();
            if (!elements.Any()) return "[]";
            if (typeof(string).IsAssignableFrom(typeof(T)))
            {
                var list = elements as List<string>;
                return $"[{string.Join(",", list.Select(ToString))}]";
            }
            return "[" + string.Join(", ", elements) + "]";
        }

        public static string ToString(string element)
        {
            if (string.IsNullOrEmpty(element)) return string.Empty;
            return element.Length > 50 ? element[..50] : element;
        }
    }
}