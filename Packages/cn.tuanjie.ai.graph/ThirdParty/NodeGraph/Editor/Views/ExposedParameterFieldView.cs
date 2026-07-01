using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GraphProcessor
{
	public class ExposedParameterFieldView : BlackboardField
	{
		protected BaseGraphView	graphView;

		public ExposedParameter	parameter { get; private set; }

		public ExposedParameterFieldView(BaseGraphView graphView, ExposedParameter param) : base(null, param.name, param.shortType)
		{
			this.graphView = graphView;
			parameter = param;
			this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
			this.RegisterCallback<MouseDownEvent>(e => {
                var toShowNodeViews = graphView.nodeViews.Where(n =>
                {
					if (n is ParameterNodeView)
					{
						ParameterNode node = ((ParameterNodeView)n).nodeTarget as ParameterNode;
						return node.parameterGUID == parameter.guid;
                    }
                    return false;
                }).ToList();
                foreach (var nodeViews in toShowNodeViews)
                {
                    ((ParameterNodeView)nodeViews).HighLightView();
                }
            });
			this.Q("icon").AddToClassList("parameter-" + param.shortType);
			this.Q("icon").visible = true;

			(this.Q("textField") as TextField).RegisterValueChangedCallback((e) => {
				param.name = e.newValue;
				text = e.newValue;
				graphView.graph.UpdateExposedParameterName(param, e.newValue);
			});
        }

		void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Rename", (a) => OpenTextEditor(), DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendAction("Delete", (a) => {
				graphView.graphViewChanged(new GraphViewChange { 
					elementsToRemove = new List<GraphElement> { this } });
            }, DropdownMenuAction.AlwaysEnabled);

            evt.StopPropagation();
        }
	}
}