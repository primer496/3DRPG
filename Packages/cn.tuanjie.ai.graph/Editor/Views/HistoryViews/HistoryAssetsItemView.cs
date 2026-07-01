using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

namespace UnityEditor.AIGraph
{
    class HistoryTextureItemView : VisualElement
    {
        protected Texture2D previewTexture;

        protected LoadableImage preview;

        protected Chip chip;

        public HistoryTextureItemView(Texture2D staticPreview, VisualElement owner, bool hasInfo, List<string> infoList)
        {
            preview = new LoadableImage(autoLoading: true);
            chip = preview.Q<Chip>();

            RegisterCallback<MouseOverEvent>(evt =>
            {
                chip.style.display = DisplayStyle.None;
            });
            RegisterCallback<MouseLeaveEvent>(evt =>
            {
                if (!string.IsNullOrEmpty(chip.label))
                    chip.style.display = DisplayStyle.Flex;
            });

            if (staticPreview != null)
                preview.OnLoaded(staticPreview);
            else
                preview.OnError("Fail to load preview!");

            Add(preview);
            if (hasInfo)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var info in infoList)
                {
                    sb.Append(info);
                    if (info != infoList.Last())
                        sb.Append("\n");
                }

                this.tooltip = sb.ToString();
            }
        }
    }
}
