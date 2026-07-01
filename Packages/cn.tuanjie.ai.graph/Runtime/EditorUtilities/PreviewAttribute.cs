using System;

namespace UnityEngine.AIGraph
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class PreviewAttribute : Attribute
    {
        public Type CustomRenderer { get; set; } = null;
        public Vector2 rectSize { get; set; } = new Vector2(300f, 200f);
        public GUIStyle background { get; set; } = GUIStyle.none;
    }
}
