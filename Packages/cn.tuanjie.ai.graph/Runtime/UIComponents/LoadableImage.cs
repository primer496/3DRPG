using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace UnityEngine.AIGraph
{
    public class LoadableImage : Image
    {
        const string k_StylePath = "uss/LoadableImageStyle";
        const string k_ClassStatusElement = "li-element";

        internal readonly GenericLoader GenericLoader;
        readonly Chip m_ResolutionChip;

        internal GenericLoader.State LoadingState => GenericLoader.LoadingState;

        public LoadableImage(bool autoLoading = true)
        {
            var ss = Resources.Load<StyleSheet>(k_StylePath);
            styleSheets.Add(ss);
            AddToClassList(k_ClassStatusElement);

            GenericLoader = new GenericLoader(autoLoading ? GenericLoader.State.Loading : GenericLoader.State.None)
            {
                style =
                {
                    position = Position.Absolute,
                    width = Length.Percent(100),
                    height = Length.Percent(100)
                }
            };

            Add(GenericLoader);

            m_ResolutionChip = new Chip
            {
                variant = Chip.Variant.Filled,
                label = string.Empty,
                style =
                {
                   position = Position.Absolute,
                   alignSelf = Align.FlexEnd,
                   left = 3,
                   top = 3,
                   opacity = 0.6f,
                   display = DisplayStyle.None
                }
            };

            Add(m_ResolutionChip);
        }

        public void OnLoaded(UnityEngine.Texture texture, bool enableChip = true)
        {
            GenericLoader.SetState(GenericLoader.State.None);
            image = texture;

            if(enableChip)
                UpdateResolutionChip(texture);
        }

        public void OnError(string error)
        {
            GenericLoader.SetState(GenericLoader.State.Error, error);
        }

        public void OnLoading()
        {
            image = null;
            m_ResolutionChip.label = string.Empty;
            GenericLoader.SetState(GenericLoader.State.Loading);
        }

        void UpdateResolutionChip(UnityEngine.Texture texture)
        {
            if (texture)
            {
                m_ResolutionChip.label = texture.width switch
                {
                    1024 => "1K",
                    2048 => "2K",
                    4096 => "4K",
                    8192 => "8K",
                    _ => string.Empty
                };
                m_ResolutionChip.style.display = string.IsNullOrEmpty(m_ResolutionChip.label) ? DisplayStyle.None : DisplayStyle.Flex;
            }
            else
            {
                m_ResolutionChip.style.display = DisplayStyle.None;
            }
        }
    }
}
