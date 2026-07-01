
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SearchPopupWindow : EditorWindow
{
    public List<string> Entries;
    public System.Action<string> onSelectEntry;

    private GUIStyle btnStyle;
    private string searchString = "";
    private Vector2 scrollPosition;

    private void OnEnable()
    {
        Entries = new();
    }

    void InitStyle()
    {
        if (btnStyle != null)
            return;

        // Hover label style with different background color  
        btnStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            wordWrap = true,
            normal = {
                textColor = GUI.skin.label.normal.textColor,

            },
            hover = {
                textColor = new Color(0.267f, 0.753f, 1.0f, 1.0f)
            }
        };
    }

    private void OnGUI()
    {
        InitStyle();
        searchString = GUILayout.TextField(searchString);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        foreach (var entry in Entries)
        {
            if (string.IsNullOrWhiteSpace(searchString) || entry.ToLower().Contains(searchString.ToLower()))
            {
                if (GUILayout.Button(entry, btnStyle))
                {
                    onSelectEntry?.Invoke(entry);
                }
            }
        }
        GUILayout.EndScrollView();
    }

    private void OnLostFocus()
    {
        Close();
    }
}