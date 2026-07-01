#if UNITY_2020_1_OR_NEWER
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor
{
    public class StickyNoteView : UnityEditor.Experimental.GraphView.StickyNote
    {
        public BaseGraphView owner;
        public StickyNote note;

        Label titleLabel;
        ColorField colorField;

        public StickyNoteView()
        {
            fontSize = StickyNoteFontSize.Small;
            theme = StickyNoteTheme.Classic;
            this.AddManipulator(new ContextualMenuManipulator(BuildDeleteContextualMenu));
            var stylesheet = Resources.Load<StyleSheet>("Styles/StickyNodeView");
            if (stylesheet) styleSheets.Add(stylesheet);
        }

        public void Initialize(BaseGraphView graphView, StickyNote note)
        {
            this.note = note;
            owner = graphView;

            this.Q<TextField>("title-field").RegisterCallback<ChangeEvent<string>>(e =>
            {
                note.title = e.newValue;
            });
            this.Q<TextField>("contents-field").RegisterCallback<ChangeEvent<string>>(e =>
            {
                note.content = e.newValue;
            });

            title = note.title;
            contents = note.content;
            SetPosition(note.position);
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            if (note != null)
                note.position = newPos;
        }

        public override void OnResized()
        {
            note.position = layout;
        }

        /// <summary>
        /// 右键菜单，添加Delete选项
        /// </summary>
        void BuildDeleteContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Delete", (e) =>
            {
                owner.RemoveStickyNoteView(this);
            }, DropdownMenuAction.AlwaysEnabled);
        }
    }
}
#endif