// using System;
// using UnityEditor.AIGraph;
// using UnityEngine.UIElements;
// using GraphProcessor;
// using UnityEditor;
// using UnityEditor.UIElements;
// using UnityEngine;
// using UnityEngine.AIGraph;
//
// [NodeCustomEditor(typeof(HyUploadModelByUrlNode))]
// public class HyUploadModelByUrlNodeView : SDNodeView
// {
//     private HyUploadModelByUrlNode node;
//     private TextField glbUrlField;
//     private TextField fbxUrlField;
//     private VisualElement objContainer;
//
//     public override void Enable()
//     {
//         if (nodeTarget == null) return;
//         node = nodeTarget as HyUploadModelByUrlNode;
//         
//         var ussPath = "uss/VastNodeStyle";
//         var styleSheet = Resources.Load<StyleSheet>(ussPath);
//         styleSheets.Add(styleSheet);
//
//         var uxml = Resources.Load<VisualTreeAsset>(
//             "uxml/HyUploadModelNodeView");
//         uxml.CloneTree(controlsContainer);
//         
//         glbUrlField = controlsContainer.Q<TextField>("glbUrlField");
//         fbxUrlField = controlsContainer.Q<TextField>("fbxUrlField");
//         objContainer = controlsContainer.Q<VisualElement>("objContainer");
//         
//         BindProperty<TextField, string, HyUploadModelByUrlNode>("glbUrlField", nameof(node.glbUrl));
//         BindProperty<TextField, string, HyUploadModelByUrlNode>("fbxUrlField", nameof(node.fbxUrl));
//         BindProperty<TextField, string, HyUploadModelByUrlNode>("objUrlField", nameof(node.objUrl));
//         BindProperty<TextField, string, HyUploadModelByUrlNode>("mtlUrlField", nameof(node.mtlUrl));
//         BindProperty<TextField, string, HyUploadModelByUrlNode>("textureImageUrlField", nameof(node.textureUrl));
//         BindProperty<TextField, string, HyUploadModelByUrlNode>("pbrMetallicUrlField", nameof(node.pbrMetallicUrl));
//         BindProperty<TextField, string, HyUploadModelByUrlNode>("pbrRoughnessUrlField", nameof(node.pbrRoughnessUrl));
//         BindProperty<TextField, string, HyUploadModelByUrlNode>("pbrNormalUrlField", nameof(node.pbrNormalsUrl));
//         BindProperty<TextField, string, HyUploadModelByUrlNode>("pbrBaseMapUrlField", nameof(node.pbrBaseMapUrl));
//         
//         BindProperty<EnumField, Enum, HyUploadModelByUrlNode>("modelTypeEnum",
//             nameof(node.uploadModelType), OnModelTypeChanged);
//
//         base.Enable();
//         RefreshExpandedState();
//         RefreshField(node.uploadModelType);
//     }
//
//     void OnModelTypeChanged(ChangeEvent<Enum> evt)
//     {
//         var modelType = (UploadModelType)evt.newValue;
//         RefreshField(modelType);
//         node.uploadModelType = modelType;
//         NotifyNodeChanging();
//     }
//
//     void RefreshField(UploadModelType modelType)
//     {
//         if (modelType == UploadModelType.FBX)
//         {
//             fbxUrlField.style.display = DisplayStyle.Flex;
//             glbUrlField.style.display = DisplayStyle.None;
//             objContainer.style.display = DisplayStyle.None;
//         } else if (modelType == UploadModelType.GLB)
//         {
//             glbUrlField.style.display = DisplayStyle.Flex;
//             fbxUrlField.style.display = DisplayStyle.None;
//             objContainer.style.display = DisplayStyle.None;
//         }
//         else
//         {
//             objContainer.style.display = DisplayStyle.Flex;
//             glbUrlField.style.display = DisplayStyle.None;
//             fbxUrlField.style.display = DisplayStyle.None;
//         }
//     }
// }