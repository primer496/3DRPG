using System;
using GraphProcessor;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;

namespace UnityEditor.AIGraph
{
	
	public class TJAIGraphView : BaseGraphView
	{
		public new TJAIGraph	graph => base.graph as TJAIGraph;

		public TJAIGraphView(EditorWindow window) : base(window)
		{
            /// 注册一个回调函数，当鼠标指针在视图上抬起时调用
            /// TrickleDown.TrickleDown表示该回调函数会在AtTarget和TrickleDown阶段执行
            /// 补充： 事件执行的三个阶段：
            /// AtTarget（目标阶段）：这个阶段指的是事件已经到达它的目标对象，即事件绑定的元素或组件
            /// BubbleUp（冒泡阶段）：这个阶段指的是事件从目标对象开始，向上冒泡到它的父对象，直到到达根对象
            /// TrickleDown（捕获阶段）：这个阶段指的是事件从根对象开始，向下捕获到目标对象，直到到达目标对象
			RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
			RegisterCallback<GeometryChangedEvent>(evt =>
			{
				var miniMap = this.Q<MiniMapView>();
				miniMap?.ResizeMaxRect(evt.newRect);
			});
		}

		/// <summary>
        /// 在graph窗口内右键展示的菜单
        /// </summary>
        /// <param name="evt"></param>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
            // Create Node的子菜单
			evt.menu.AppendSeparator();

			foreach (var nodeMenuItem in NodeProvider.GetNodeMenuEntries())
			{
				var mousePos = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
				Vector2 nodePosition = mousePos;
				evt.menu.InsertAction(0, "Create/" + nodeMenuItem.path,
					(e) => CreateNodeOfType(nodeMenuItem.type, nodePosition),
					DropdownMenuAction.AlwaysEnabled
				);
			}

			base.BuildContextualMenu(evt);
		}

		void CreateNodeOfType(Type type, Vector2 position)
		{
			RegisterCompleteObjectUndo("Added " + type + " node");
			AddNode(BaseNode.CreateFromType(type, position));
		}

		public override BaseNodeView AddRelayNode(PortView inputPort, PortView outputPort, Vector2 position)
		{
			if (inputPort.portData.displayType == typeof(ConditionalLink)
			    || inputPort.portData.displayType == typeof(ConditionalLink))
			{
                Debug.Log("TJAI relay node created.");
				var relayNode = BaseNode.CreateFromType<SDRelayNode>(position);
				var view = AddNode(relayNode) as SDRelayNodeView;

				if (outputPort != null)
					Connect(view.inputPortViews[0], outputPort);
				if (inputPort != null)
					Connect(inputPort, view.outputPortViews[0]);

				return view;
			}
			else
			{
                Debug.Log("Origin Relay Node Created.");
				var relayNode = BaseNode.CreateFromType<RelayNode>(position);
				var view = AddNode(relayNode) as RelayNodeView;

				if (outputPort != null)
					Connect(view.inputPortViews[0], outputPort);
				if (inputPort != null)
					Connect(inputPort, view.outputPortViews[0]);

				return view;
			}
		}

		// Workaround for missing PointerUpEvent when selecting gird items in HistoryAssetsView
		void OnPointerUp(PointerUpEvent evt)
        {
			if (evt.target != this) {
				// We only care about evt that is targeted at GraphView itself.
				// Otherwise let it go as normal.
				return;
			}

            // BugFix: 历史资产系统之前接收不到鼠标点击事件
			var view = this.Q<HistoryAssetsView>();
			if(view != null && view.worldBound.Contains(evt.position) && evt.actionKey)
            {
				evt.StopImmediatePropagation();
                // 从对象池中获取PointerUpEvent对象并使用evt初始化
				var newEvt = PointerUpEvent.GetPooled(evt);
				newEvt.target = view;
				SendEvent(newEvt);
            }
        }
	}
}
