// using UnityEditor.AIGraph;
// using UnityEngine.UIElements;
// using GraphProcessor;
// using UnityEditor;
// using UnityEngine;
// using UnityEngine.AIGraph;
//
// [NodeCustomEditor(typeof(HyFormatConversionNode))]
// public class HyFormatConversionNodeView : TJAIBaseAssetNodeView
// {
//     private HyFormatConversionNode node;
//     private DropdownField rspFormatDropdown;
//
//     public override void Enable()
//     {
//         if (nodeTarget == null) return;
//         node = nodeTarget as HyFormatConversionNode;
//
//         var ussPath = "uss/VastNodeStyle";
//         var styleSheet = Resources.Load<StyleSheet>(ussPath);
//         styleSheets.Add(styleSheet);
//
//         rspFormatDropdown = new DropdownField(node.rspFormatChoices, 0)
//         {
//             label = "Target Format", name = "rspFormatDropdown",
//             tooltip = "To convert model format"
//         };
//         rspFormatDropdown.AddToClassList("vast-dropdown");
//         controlsContainer.Add(rspFormatDropdown);
//
//         BindProperty<DropdownField, string, HyFormatConversionNode>("rspFormatDropdown", nameof(node.rspFormat));
//
//         base.Enable();
//         RefreshExpandedState();
//     }
// }