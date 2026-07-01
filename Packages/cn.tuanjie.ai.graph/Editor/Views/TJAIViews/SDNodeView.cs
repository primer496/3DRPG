using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphProcessor;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

namespace UnityEditor.AIGraph
{
    [NodeCustomEditor(typeof(SDNode))]
    public class SDNodeView : BaseTJAINodeView
    {
        protected VisualElement previewContainer;

        //protected new TJAIGraphView  owner => base.owner as TJAIGraphView;

        protected new SDNode nodeTarget => base.nodeTarget as SDNode;

        protected override bool hasSettings => nodeTarget.hasSettings;

        protected SDNodeSettingView settingsView;

        protected List<IMGUIContainer> iMGUIContainers;

        protected List<SmartPreviewComponent> smartPreviews = new List<SmartPreviewComponent>();

        protected List<string> previewSettings = new List<string>();

        protected override VisualElement CreateSettingsView()
        {
            settingsView = new SDNodeSettingView(nodeTarget.settings, owner);
            settingsView.AddToClassList("RTSettingsView");

            var currentDim = nodeTarget.settings.dimension;
            settingsView.RegisterChangedCallback(() => {
                nodeTarget.OnSettingsChanged();

                // When the dimension is updated, we need to update all the node ports in the graph
                var newDim = nodeTarget.settings.dimension;
                if (currentDim != newDim)
                {
                    // We delay the port refresh to let the settings finish it's update 
                    schedule.Execute(() => {
                        {
                            // Refresh ports on all the nodes in the graph
                            nodeTarget.UpdateAllPortsLocal();
                            RefreshPorts();
                        }
                    }).ExecuteLater(1);
                    currentDim = newDim;
                }
            });

            return settingsView;
        }

        public override void Enable()
        {
            base.Enable();

            RefreshExpandedState();

            if (nodeTarget.hasSave)
            {
                var saveButton = new UnityEngine.UIElements.Button(OnSave) { name = "save-button" };
                saveButton.style.backgroundImage = SDTextureHandle.SaveIcon;
                saveButton.style.alignSelf = Align.Auto;
                rightTitleContainer.Insert(1, saveButton);
                AddToClassList("preview");
            }

            previewContainer = new VisualElement();
            previewContainer.AddToClassList("Preview");


            if (nodeTarget != null)
            {
                if (nodeTarget.onFieldValueChangedHandlers != null && nodeTarget.onFieldValueChangedHandlers.Count != 0)
                    nodeTarget.onFieldValueChangedHandlers.Clear();
                else if (nodeTarget.onFieldValueChangedHandlers == null)
                    nodeTarget.onFieldValueChangedHandlers = new Dictionary<string, System.Action>();

                var fields = nodeTarget.GetType().GetFields(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );

                var hasPreview = false;
                foreach (var field in fields)
                {
                    var previewAttr = field.GetCustomAttribute<PreviewAttribute>();
                    if (previewAttr != null)
                    {
                        hasPreview = true;
                        // add object selector
                        var showInSelector = field.GetCustomAttribute<HideInPreviewSelector>() == null;
                        if (showInSelector)
                        {
                            var elem = AddControlField(field, ObjectNames.NicifyVariableName(field.Name),
                                addDefaultValueChangedCallback: false);
                            elem.SetEnabled(false);
                            previewContainer.Add(elem);
                        }

                        var preview = new SmartPreviewComponent(field, nodeTarget, previewAttr,
                            previewSettings.Contains("Rigging"), previewSettings.Contains("Footnote"));
                        smartPreviews.Add(preview);
                        previewContainer.Add(preview);

                        nodeTarget.onFieldValueChangedHandlers.Add(field.Name, () => preview.UpdatePreview());
                        nodeTarget.onFieldValueChangedHandlers[field.Name].Invoke();
                    }
                }

                if (hasPreview)
                {
                    Button togglePreviewButton = null;
                    togglePreviewButton = new Button(() =>
                    {
                        TogglePreviewCollapseState(togglePreviewButton);
                    });
                    togglePreviewButton.ClearClassList();
                    togglePreviewButton.AddToClassList("PreviewToggleButton");
                    previewContainer.Add(togglePreviewButton);
                }
            }

            if (smartPreviews.Count > 0)
            {
                this.style.width = SDUtil.previewNodeWidth;
                this.style.width = 300f;
                nodeTarget.nodeWidth = 300f;
                nodeTarget.hasPreview = true;
            }


            controlsContainer.Add(previewContainer);

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<AttachToPanelEvent>(evt => registerCallbackAction?.Invoke());
            RegisterCallback<DetachFromPanelEvent>(evt => unregisterCallbackAction?.Invoke());
        }
        protected Action registerCallbackAction;
        protected Action unregisterCallbackAction;

        protected void BindProperty<TVisualElement, TField, TNode>(string elementName, string fieldName,
            EventCallback<ChangeEvent<TField>> callback = null)
            where TVisualElement : BaseField<TField> where TNode : BaseTJAINode
        {
            RegisterNodeChangingCallback(fieldName);
            var element = controlsContainer.Q<TVisualElement>(elementName);
            if (element == null) return;
            element.style.display = DisplayStyle.Flex;
            var fieldInfo = typeof(TNode).GetField(fieldName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo == null || !typeof(TField).IsAssignableFrom(fieldInfo.FieldType))
            {
                Debug.LogError($"Failed to bind property, error: failed to find field {fieldName} in type {typeof(TNode).Name}");
                return;
            }
            var value = (TField)fieldInfo.GetValue(nodeTarget);
            if (value != null)
                element.SetValueWithoutNotify(value);
            else
               fieldInfo.SetValue(nodeTarget, element.value);

            callback ??= OnFieldChanged<TField, TNode>(fieldInfo);
            registerCallbackAction += () =>
            {
                element.RegisterValueChangedCallback(callback);
            };
            unregisterCallbackAction += () =>
            {
                element.UnregisterValueChangedCallback(callback);
            };
        }
        protected EventCallback<ChangeEvent<TField>> OnFieldChanged<TField, TNode>(FieldInfo fieldInfo)
            where TNode : BaseTJAINode
        {
            return (evt) =>
            {
                var oldValue = (TField)fieldInfo.GetValue(nodeTarget);
                if (oldValue.Equals(evt.newValue)) return;
                fieldInfo.SetValue(nodeTarget as TNode, evt.newValue);
                NotifyNodeChanging();
            };
        }

        void TogglePreviewCollapseState(Button togglePreviewButton)
        {
            if (smartPreviews == null || smartPreviews.Count == 0) return;
            var firstPreview = smartPreviews.First();
            var isCollapse = !firstPreview.collapse;
            var genMaskBtns = previewContainer.Query<Button>("genMaskBtn").ToList();
            if (!isCollapse)
            {
                foreach (var preview in smartPreviews)
                {
                    preview.collapse = false;
                    preview.style.display = DisplayStyle.Flex;
                }
                togglePreviewButton.RemoveFromClassList("Collapsed");
                foreach (var btn in genMaskBtns)
                    btn.style.display = DisplayStyle.Flex;
            }
            else
            {
                foreach (var preview in smartPreviews)
                {
                    preview.collapse = true;
                    preview.style.display = DisplayStyle.None;
                }
                togglePreviewButton.AddToClassList("Collapsed");
                foreach (var btn in genMaskBtns)
                    btn.style.display = DisplayStyle.None;
            }
        }

        public override void Disable()
        {
            // clear preview
            nodeTarget.onFieldValueChangedHandlers.Clear();
            foreach (var preview in smartPreviews)
            {
                preview.Cleanup();
            }
            smartPreviews.Clear();
            base.Disable();
        }

        public override void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            foreach (var preview in smartPreviews)
            {
                preview.Cleanup();
            }
            base.OnDetachFromPanel(evt);
        }

        public void OnAttachToPanel(AttachToPanelEvent evt)
        {
            foreach (var preview in smartPreviews)
            {
                preview.UpdatePreview();
            }
        }

        void UpdatePorts()
        {
            nodeTarget.UpdateAllPorts();
            RefreshPorts();
        }

        protected virtual void OnSave()
        {
        }

        ~SDNodeView()
        {

        }
    }
}
