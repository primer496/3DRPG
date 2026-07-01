using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor
{
    public class ProviderGroupView : GroupView
    {
        private List<GraphElement> baseNodeViews = new List<GraphElement>();
        private DropdownField dropDownField = null;

        public ProviderGroupView() : base() 
        {
            noPicking = true;
        }

        public override void Initialize(BaseGraphView graphView, Group block)
        {
            base.Initialize(graphView, block);

            dropDownField = new DropdownField((group as ProviderGroup).groupNames, 0);
            dropDownField.style.width = (group as ProviderGroup).dropdownFieldWidth;
            dropDownField.RegisterValueChangedCallback(OnModeChanged);
            dropDownField.SetValueWithoutNotify((group as ProviderGroup).groupNames[(group as ProviderGroup).currentNameIndex]);
            headerContainer.Add(dropDownField); 
        }

        void OnModeChanged(ChangeEvent<string> evt)
        {
            List<GraphElement> edgeViews = new List<GraphElement>();

            foreach (var edge in owner.edgeViews)
            {
                foreach (var currNodeView in baseNodeViews)
                {
                    if (currNodeView == edge.input.node || currNodeView == edge.output.node)
                    {
                        edgeViews.Add(edge);
                    }
                }
            }

            float minX = baseNodeViews.Count == 0 ? this.GetPosition().x : this.GetPosition().x + this.GetPosition().width;
            float minY = baseNodeViews.Count == 0 ? this.GetPosition().y : this.GetPosition().y + this.GetPosition().height;
            foreach (var currNodeView in baseNodeViews)
            {
                minX = Math.Min(minX, (currNodeView as BaseNodeView).nodeTarget.position.x);
                minY = Math.Min(minY, (currNodeView as BaseNodeView).nodeTarget.position.y);
            }

            owner.graphViewChanged(new GraphViewChange
            {
                elementsToRemove = baseNodeViews
            });

            owner.graphViewChanged(new GraphViewChange
            {
                elementsToRemove = edgeViews
            });

            baseNodeViews.Clear();

            List<BaseNodeView> nodeViews = new List<BaseNodeView>();
            float shift = 0;
            var pGroup = group as ProviderGroup;
            int groupIndex = 0;

            for (int i = 0; i < pGroup.groupNames.Count; ++i)
            {
                if (pGroup.groupNames[i] == dropDownField.value)
                {
                    groupIndex = i; 
                    pGroup.currentNameIndex = i;
                    break;
                }
            }

            foreach (var nodeToCreate in pGroup.constructInfo[groupIndex].nodes) 
            {
                BaseNode node = BaseNode.CreateFromType(Type.GetType(nodeToCreate), new Vector2(minX + shift, minY));
                nodeViews.Add(this.owner.AddNode(node));
                baseNodeViews.Add(nodeViews.Last());
                this.AddElement(nodeViews.Last());

                shift += 350f;
            }

            foreach (var edgeToCreate in pGroup.constructInfo[groupIndex].edges)
            {
                this.owner.Connect(nodeViews[edgeToCreate.inputNodeIndex].GetPortViewFromFieldName(edgeToCreate.inputNodePortField, edgeToCreate.inputNodePortIdentifier),
                    nodeViews[edgeToCreate.outputNodeIndex].GetPortViewFromFieldName(edgeToCreate.outputNodePortField, edgeToCreate.outputNodePortIdentifier));
            }

            foreach (var connectInfo in pGroup.constructInfo[groupIndex].connectPorts)
            {
                var target = pGroup.connnectPortList[connectInfo.connectPortInfoIndex];
                if (owner.graph.nodesPerGUID.ContainsKey(target.nodeGUID))
                {
                    if (target.input)
                        this.owner.Connect(this.owner.nodeViewsPerNode[owner.graph.nodesPerGUID[target.nodeGUID]].GetPortViewFromFieldName(target.portName, target.portIdentifier),
                             nodeViews[connectInfo.nodeIndex].GetPortViewFromFieldName(connectInfo.portField, connectInfo.portIdentifier));
                    else
                        this.owner.Connect(nodeViews[connectInfo.nodeIndex].GetPortViewFromFieldName(connectInfo.portField, connectInfo.portIdentifier),
                             this.owner.nodeViewsPerNode[owner.graph.nodesPerGUID[target.nodeGUID]].GetPortViewFromFieldName(target.portName, target.portIdentifier));
                }
            }
        }

        protected override void InitializeInnerNodes()
        {
            foreach (var nodeGUID in group.innerNodeGUIDs.ToList())
            {
                if (!owner.graph.nodesPerGUID.ContainsKey(nodeGUID))
                {
                    group.innerNodeGUIDs.Remove(nodeGUID);
                    continue;
                }
                var node = owner.graph.nodesPerGUID[nodeGUID];
                var nodeView = owner.nodeViewsPerNode[node];

                AddElement(nodeView);

                baseNodeViews.Add(nodeView);
            }
        }
    }
}