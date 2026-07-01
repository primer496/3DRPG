using GraphProcessor;
using UnityEngine;
using UnityEngine.AIGraph;

[System.Serializable, NodeMenuItem("Constant/String")]
public class StringNode : BaseTJAINode
{
    [Input(name = "In")] public string inputString;
    [Output(name = "Out")] public string output;

    [HideInInspector]
    public string textFiledValue = "";

    public override string name => "String";
    public override bool isRenamable => true;


    [HideInInspector]
    public bool isShowString = true;

    public override void Process()
    {
        base.Process();
        if (!string.IsNullOrEmpty(inputString))
        {
            output = inputString;
            textFiledValue = inputString;
        }
        else
            output = textFiledValue;
        //output = inputString + textFiledValue;
        // Debug.Log($"{GetCustomName()}: {output}");
    }
}