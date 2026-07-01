using GraphProcessor;

[System.Serializable, NodeMenuItem("Constant/Int")]
public class IntNode : BaseNode
{
    [Output("Out")]
    public int output;

    [Input("In")]
    public int input;

    public override string name => "Int";

    public override void Process() => output = input;
}