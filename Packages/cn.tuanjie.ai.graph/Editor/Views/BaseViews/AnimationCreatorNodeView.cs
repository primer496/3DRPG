using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEditor.Animations;
using UnityEditor.UIElements;
using UnityEngine.AIGraph;

[NodeCustomEditor(typeof(AnimatorCreatorNode))]
public class AnimatorCreatorNodeView : SDNodeView
{
    private AnimatorCreatorNode node;
    private ObjectField goField;
    public override void Enable()
    {
        node = nodeTarget as AnimatorCreatorNode;
        if (node == null) return;

        goField = new ObjectField("Controller")
        {
            name = "controllerField",
            objectType = typeof(AnimatorController),
            allowSceneObjects = false,
            value = node.m_Controller
        };
        goField.SetEnabled(false);
        controlsContainer.Add(goField);

        base.Enable();
        RefreshExpandedState();
    }
}