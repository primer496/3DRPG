using System;
using GraphProcessor;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(IntNode))]
public class IntNodeView : BaseNodeView
{
    public override void Enable()
    {
        var intNode = nodeTarget as IntNode;

        IntegerField intField = new IntegerField
        {
            value = intNode.input
        };

        intNode.onProcessed += () => intField.value = intNode.input;

        intField.RegisterValueChangedCallback((v) => {
            owner.RegisterCompleteObjectUndo("Updated floatNode input");
            intNode.input = Convert.ToInt32(v.newValue);
            NotifyNodeChanging();
        });

        controlsContainer.Add(intField);
    }
}