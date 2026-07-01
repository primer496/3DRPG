using GraphProcessor;
using UnityEngine;
using UnityEngine.AIGraph;

[System.Serializable, NodeMenuItem("Hidden/Game Object")]
public class GameObjectNode : SDNode, ICreateNodeFrom<GameObject>
{
	[Output(name = "Out"), SerializeField, Preview, HideInInspector]
	private GameObject			m_output;

    public GameObject output
    {
        get => m_output;
        set
        {
            if (m_output != value)
            {
                m_output = value;
                this?.NotifyFieldChanged("m_output");
            }
        }
    }

    public override string		name => "Game Object";

    public bool InitializeNodeFromObject(GameObject value)
	{
        if (value == null)
        {
            return false;
        }

        output = value;
        return true;
    }
}