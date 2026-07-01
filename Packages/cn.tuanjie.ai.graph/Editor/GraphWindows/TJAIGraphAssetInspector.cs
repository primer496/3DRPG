using GraphProcessor;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

namespace UnityEditor.AIGraph
{
    /// <summary>
    /// 在Asset添加TJAIGraph后会有一个TJAIGraph的资产
    /// TJAIGraphAssetInspector为单击该资产对应的inspector内容
    /// </summary>
	[CustomEditor(typeof(TJAIGraph), true)]
	public class TJAIGraphAssetInspector : GraphInspector
	{
		protected override void CreateInspector()
		{
			base.CreateInspector();

			root.Add(new Button(() => EditorWindow.GetWindow<TJAIGraphWindow>().InitializeGraph(target as BaseGraph))
			{
				text = "Open window"
			});
		}
	}

}
