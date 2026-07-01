using GraphProcessor;

[System.Serializable, NodeMenuItem("Constant/Float")]
public class FloatNode : BaseNode
{
    [Output("Out")]
	public float		output;
	
    [Input("In")]
	public float		input;

	public override string name => "Float";

	public override void Process() => output = input;
}