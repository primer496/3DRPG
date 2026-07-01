using System.Linq;
using GraphProcessor;
using UnityEditor.AIGraph;
using UnityEngine;
using UnityEngine.AIGraph;

[NodeCustomEditor(typeof(VastMeshSegmentationNode))]
public class VastMeshSegmentationNodeView : TJAIBaseAssetNodeView
{
    private VastMeshSegmentationNode node;

    public override void Enable()
    {
        if (nodeTarget == null) return;
        node = nodeTarget as VastMeshSegmentationNode;
        node.onResultUpdated += UpdateResults;

        base.Enable();
        RefreshExpandedState();
    }

    public override void Disable()
    {
        base.Disable();
        node.onResultUpdated -= UpdateResults;
    }

    void UpdateResults()
    {
        // step 1: add node
        var transforms = node.obj.transform.GetComponentsInChildren<Transform>();
        if (transforms.Length > 10) return;
        var group = new Group($"{node.GetCustomName()} Results", 
            node.position.position + new Vector2(node.nodeWidth + 20, 50 * node.createdGroups.Count));
        var zeroPoint = node.position.position + new Vector2(node.nodeWidth + 50, 50);
        var nodePos = zeroPoint;
        var nodeType = typeof(MeshFilterNode);
        var i = 0;
        Vector2 posOffsetX = new Vector2(300, 0), posOffsetY = new Vector2(0, 320);
        foreach (var child in transforms)
        {
            if (child == node.obj.transform) continue;
            owner.RegisterCompleteObjectUndo($"Added {nodeType} node");
            var resultNode = BaseNode.CreateFromType<MeshFilterNode>(nodePos);
            resultNode.owner = child.gameObject;
            var resultNodeView = owner.AddNode(resultNode);
            resultNode.SetUniqueCustomName(child.name);
            resultNodeView.UpdateTitle();
            nodePos += posOffsetX;
            i++;
            if (i % 5 == 0)
            {
                nodePos += posOffsetY;
                nodePos.x = zeroPoint.x;
            }
            group.innerNodeGUIDs.Add(resultNode.GUID);
        }
        // step 2: add group
        // step 2-1: remove existed group which has no output connection
        group.OnCreated();
        var createdGroups = node.createdGroups;
        if (createdGroups.Count > 0)
        {
            createdGroups.RemoveAll(g => g == null);
            foreach (var toRemoveGroup in createdGroups.Where(ShouldRemove))
            {
                var idList = toRemoveGroup.innerNodeGUIDs.ToList();
                foreach (var id in idList)
                {
                    if (!node.graph.nodesPerGUID.TryGetValue(id, out var toRemoveNode)) continue;
                    owner.RemoveNode(toRemoveNode);
                }
                owner.RemoveGroup(toRemoveGroup);
            }
            createdGroups.RemoveAll(ShouldRemove);
        }
        createdGroups.Add(group);
        owner.AddGroup(group);
    }

    private bool ShouldRemove(Group group)
    {
        if (!group.innerNodeGUIDs.Any(node.graph.nodesPerGUID.ContainsKey)) return true;
        foreach (var id in group.innerNodeGUIDs)
        {
            if (!node.graph.nodesPerGUID.TryGetValue(id, out var curNode)) continue;
            var edges = curNode?.GetOutputEdges();
            if (edges != null && edges.Any()) return false;
        }
        return true;
    }
}