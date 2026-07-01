using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.AIGraph.Backend;
using UnityEngine.UIElements;

namespace UnityEditor.AIGraph
{
    public class FeedbackView : GraphElement
    {
        public FeedbackView()
        {
            var stylesheet = Resources.Load<StyleSheet>("uss/FeedbackView");
            if (stylesheet) styleSheets.Add(stylesheet);
            RemoveFromClassList("graphElement");
            AddFeedbackFoldout();
        }
        private void AddFeedbackFoldout()
        {
            var foldout = new Foldout
            {
                text = "反馈问卷",
                name = "TempFeedback-Foldout"
            };

            // 创建 Button
            var button = new Button
            {
                name = "TempFeedback-Button"
            };
            button.RegisterCallback<ClickEvent>(OnFeedbackButtonClicked);

            // 创建 Label
            var label = new Label
            {
                text = "二维码可以直接点击跳转",
                name = "TempFeedback-Label"
            };

            foldout.Add(button);
            foldout.Add(label);
            Add(foldout);
        }

        private void OnFeedbackButtonClicked(ClickEvent evt)
        {
            Application.OpenURL(GlobalConstants.feedbackUrl);
        }
    }
}