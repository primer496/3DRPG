using System.IO;
using System.Linq;
using System.Reflection;
using GraphProcessor;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;
using Cursor = UnityEngine.UIElements.Cursor;

namespace UnityEditor.AIGraph
{
    public static class SDEditorUtils
    {
        public static TJAIGraph GetGraphAtPath(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path).FirstOrDefault(o => o is TJAIGraph) as
                TJAIGraph;
        }

        static Texture2D _pinIcon;

        public static Texture2D pinIcon
        {
            get => _pinIcon == null ? _pinIcon = LoadIcon("Icons/Pin") : _pinIcon;
        }

        static Texture2D _unpinIcon;

        public static Texture2D unpinIcon
        {
            get => _unpinIcon == null ? _unpinIcon = LoadIcon("Icons/Unpin") : _unpinIcon;
        }

        static Texture2D LoadIcon(string resourceName)
        {
            if (UnityEditorInternal.InternalEditorUtility.HasPro())
            {
                string darkIconPath = Path.GetDirectoryName(resourceName) + "/d_" + Path.GetFileName(resourceName);
                var darkIcon = Resources.Load<Texture2D>(darkIconPath);
                if (darkIcon != null)
                    return darkIcon;
            }

            return Resources.Load<Texture2D>(resourceName);
        }

        public static void ScheduleAutoHide(VisualElement target, BaseGraphView view)
        {
            target.schedule.Execute(() =>
            {
                target.visible = float.IsNaN(target.worldBound.x) ||
                                 target.worldBound.Overlaps(view.worldBound);
            })
                .Every(16); // refresh the visible for 60hz screens (should not cause problems for higher refresh rates)
        }

        private static ThemeStyleSheet appuiTss;
        private static void InitializeStyleSheets()
        {
            appuiTss = Resources.Load<ThemeStyleSheet>("tss/TJAITheme");
        }

        public static void SetEnableAppUI(VisualElement ve, bool enable)
        {
            if (appuiTss == null)
                InitializeStyleSheets();
            if (appuiTss == null)
                return;
            if (enable)
            {
                ve.styleSheets.Add(appuiTss);
                ve.AddToClassList("unity-editor");
                ve.AddToClassList("appui");
            }
            else
            {
                ve.styleSheets.Remove(appuiTss);
                ve.RemoveFromClassList("unity-editor");
                ve.RemoveFromClassList("appui");
            }
        }

        public static void SetCursor(VisualElement element, MouseCursor cursor)
        {
            object objCursor = new Cursor();
            PropertyInfo fields = typeof(Cursor).GetProperty("defaultCursorId", BindingFlags.NonPublic | BindingFlags.Instance);
            fields.SetValue(objCursor, (int)cursor);
            element.style.cursor = new StyleCursor((Cursor)objCursor);
        }
    }
}