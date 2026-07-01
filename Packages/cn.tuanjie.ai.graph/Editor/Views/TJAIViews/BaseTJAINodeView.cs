using GraphProcessor;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

namespace UnityEditor.AIGraph
{
    [NodeCustomEditor(typeof(BaseTJAINode))]
    public class BaseTJAINodeView : BaseNodeView
    {
        protected new TJAIGraphView owner => base.owner as TJAIGraphView;

        protected new BaseTJAINode nodeTarget => base.nodeTarget as BaseTJAINode;
        const string k_ussSkinPath = "uss/AIGraph";
        const string k_ussCommonPath = k_ussSkinPath + "Common";

        protected virtual string header => string.Empty;

        protected Image pinIcon;
        protected Button pinButton;

        public override void Enable()
        {
            var stylesheet = Resources.Load<StyleSheet>(k_ussCommonPath);
            if (!styleSheets.Contains(stylesheet))
                styleSheets.Add(stylesheet);
            ChangeSkinStyle();

            // Fix the size of the node, implemented in uss file
            //if (!DataUtil.IsFloatZero(nodeTarget.nodeWidth))
            //    style.width = nodeTarget.nodeWidth;

            controlsContainer.AddToClassList("ControlsContainer");

            if (!string.IsNullOrEmpty(header))
            {
                var title = new Label(header);
                title.AddToClassList("PropertyEditorTitle");
                controlsContainer.Add(title);
            }

            pinIcon = new Image { image = SDEditorUtils.unpinIcon, scaleMode = ScaleMode.ScaleToFit };
            pinButton = new UnityEngine.UIElements.Button(() => {
                if (nodeTarget.isPinned)
                    UnpinView();
                else
                    PinView();
            });
            pinButton.Add(pinIcon);
            if (nodeTarget.isPinned)
                PinView();

            pinButton.AddToClassList("PinButton");
            titleContainer.Insert(0, pinButton);

            base.Enable();
        }

        private int lastSkin = 0;
        internal void ChangeSkinStyle()
        {
            var curSkin = EditorGUIUtility.isProSkin ? 1 : -1;
            if (curSkin == lastSkin) return;
            lastSkin = curSkin;
            var darkStyleSheet = Resources.Load<StyleSheet>(k_ussSkinPath + "_dark");
            var lightStyleSheet = Resources.Load<StyleSheet>(k_ussSkinPath + "_light");
            if (EditorGUIUtility.isProSkin)
            {
                if (darkStyleSheet) styleSheets.Add(darkStyleSheet);
                if (lightStyleSheet && styleSheets.Contains(lightStyleSheet))
                    styleSheets.Remove(lightStyleSheet);
            }
            else
            {
                if (lightStyleSheet) styleSheets.Add(lightStyleSheet);
                if (darkStyleSheet && styleSheets.Contains(darkStyleSheet))
                    styleSheets.Remove(darkStyleSheet);
            }
        }

        internal void UnpinView()
        {
            nodeTarget.isPinned = false;
            nodeTarget.nodeLock = false;
            pinIcon.tintColor = Color.white;
            pinIcon.image = SDEditorUtils.unpinIcon;
            pinIcon.transform.rotation = Quaternion.identity;
        }

        internal void PinView()
        {
            nodeTarget.isPinned = true;
            nodeTarget.nodeLock = true;
            pinIcon.tintColor = new Color32(245, 127, 23, 255);
            pinIcon.image = SDEditorUtils.pinIcon;
        }

        /// <summary>
        /// pin is forbidden when node is in group
        /// </summary>
        public override void DisablePinView()
        {
            if (nodeTarget.isPinned)
                UnpinView();
            pinIcon.tintColor = new Color32(128, 128, 128, 128);
            pinButton.SetEnabled(false);
        }

        public override void EnablePinView()
        {
            pinIcon.tintColor = Color.white;
            pinButton.SetEnabled(true);
        }
    }
}
