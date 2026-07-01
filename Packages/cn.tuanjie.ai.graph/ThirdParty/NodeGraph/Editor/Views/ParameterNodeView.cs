using System.Linq;
using GraphProcessor;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(ParameterNode))]
public class ParameterNodeView : BaseNodeView
{
    ParameterNode parameterNode;
    //private bool isHighlighted = false;  
    private IVisualElementScheduledItem highlightScheduleItem;  

    public override void Enable(bool fromInspector = false)
    {
        parameterNode = nodeTarget as ParameterNode;
        
        UpdatePort();
        
        //    Find and remove expand/collapse button
        titleContainer.Remove(titleContainer.Q("title-button-container"));
        //    Remove Port from the #content
        topContainer.parent.Remove(topContainer);
        //    Add Port to the #title
        titleContainer.Add(topContainer);

        parameterNode.parameterChanged += UpdateView;
        UpdateView();
        this.mainContainer.parent.RegisterCallback<PointerDownEvent>(OnMouseClick); 
    }

    void UpdateView()
    {
        title = parameterNode.parameter?.name;
        UpdatePort();
    }
    
    void UpdatePort()
    {
        if (parameterNode.parameter.settings.accessor == ParameterAccessor.Set)
        {
            titleContainer.AddToClassList("input");
        }
        else
        {
            titleContainer.RemoveFromClassList("input");
        }
        // disconnect all edges
        nodeTarget.UpdateAllPorts();
        RefreshPorts();
    }
  
    private void OnMouseClick(PointerDownEvent evt)  
    {  
        var rect = GetPosition();
		var graphMousePosition = owner.contentViewContainer.WorldToLocal(evt.position);
        if (evt.button == (int)MouseButton.LeftMouse && rect.Contains(graphMousePosition))  
        {
            var toShowNodeViews = base.owner.nodeViews.Where(n =>
            {
                if (n is ParameterNodeView)
                {
                    ParameterNode node = ((ParameterNodeView)n).nodeTarget as ParameterNode;
                    return node.parameterGUID == parameterNode.parameterGUID;
                }
                return false;
            }).ToList();
            foreach (var nodeViews in toShowNodeViews)
            {
                ((ParameterNodeView)nodeViews).HighLightView();
            }
        }
        if (owner.GetPinnedElementStatus<ExposedParameterView>() != DropdownMenuAction.Status.Hidden)
        {
            owner.Q<ExposedParameterView>().HighlightParameterEvent(parameterNode.parameterGUID);
        }

    }  
  
    public void HighLightView()  
    {  
        // 添加高亮样式类  
        this.mainContainer.AddToClassList("highlighted");  
        //isHighlighted = true;  
  
        // 取消之前的定时任务  
        highlightScheduleItem?.Pause();  
  
        // 设置新的定时任务，在5秒后取消高亮  
        highlightScheduleItem = this.schedule.Execute(ResetHighlight).StartingIn(1000);  
    }  
  
    public void ResetHighlight()  
    {  
        // 移除高亮样式类  
        this.mainContainer.RemoveFromClassList("highlighted");  
        //isHighlighted = false;  
    }
}
