using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AIGraph;
using UnityEngine.UIElements;
using CircularProgress = Unity.AppUI.UI.CircularProgress;
using NodeView = UnityEditor.Experimental.GraphView.Node;
using Status = UnityEngine.UIElements.DropdownMenuAction.Status;

namespace GraphProcessor
{
	[NodeCustomEditor(typeof(BaseNode))]
	public class BaseNodeView : NodeView
	{
		public BaseNode							nodeTarget;

		public List< PortView >					inputPortViews = new List< PortView >();
		public List< PortView >					outputPortViews = new List< PortView >();

		public BaseGraphView					owner { private set; get; }

		protected Dictionary< string, List< PortView > > portsPerFieldName = new Dictionary< string, List< PortView > >();

		protected VisualElement                 progressElement;
		public VisualElement 					controlsContainer;
		protected VisualElement					debugContainer;
		protected VisualElement					rightTitleContainer;
		protected VisualElement					topPortContainer;
		protected VisualElement					bottomPortContainer;
		protected VisualElement					runContainer;
        public VisualElement					inspectorContainer;
        private VisualElement 					inputContainerElement;

		VisualElement							settings;
        NodeSettingsView                        settingsContainer;
		Button									settingButton;
		TextField								titleTextField;
		Button									triggerButton;
		Button									pauseButton;
		VisualElement                           circularContainer;
		Label                                   progressLabel;

		Label									computeOrderLabel = new Label();

		public event Action< PortView >			onPortConnected;
		public event Action< PortView >			onPortDisconnected;
		public event Action                     onOpenSettings;

		protected virtual bool					hasSettings { get; set; }

        public bool								initializing = false; //Used for applying SetPosition on locked node at init.

        readonly string							baseNodeStyle = "GraphProcessorStyles/BaseNodeView";

		bool									settingsExpanded = false;

		[System.NonSerialized]
		List< IconBadge >						badges = new List< IconBadge >();

		private List<Node> selectedNodes = new List<Node>();
		private float      selectedNodesFarLeft;
		private float      selectedNodesNearLeft;
		private float      selectedNodesFarRight;
		private float      selectedNodesNearRight;
		private float      selectedNodesFarTop;
		private float      selectedNodesNearTop;
		private float      selectedNodesFarBottom;
		private float      selectedNodesNearBottom;
		private float      selectedNodesAvgHorizontal;
		private float      selectedNodesAvgVertical;
		
		#region  Initialization
		
		public void Initialize(BaseGraphView owner, BaseNode node)
		{
			nodeTarget = node;

			nodeTarget.onResetTriggerButton -= EnableTriggerableView;
			nodeTarget.onResetTriggerButton += EnableTriggerableView;
			this.owner = owner;

			if (!node.deletable)
				capabilities &= ~Capabilities.Deletable;
			// Note that the Renamable capability is useless right now as it haven't been implemented in Graphview
			if (node.isRenamable)
				capabilities |= Capabilities.Renamable;
			
			// 注册事件（先移除再添加，避免重复注册）
			owner.computeOrderUpdated -= ComputeOrderUpdatedCallback;
			owner.computeOrderUpdated += ComputeOrderUpdatedCallback;

			node.onMessageAdded -= AddMessageView;
			node.onMessageAdded += AddMessageView;

			node.onMessageRemoved -= RemoveMessageView;
			node.onMessageRemoved += RemoveMessageView;

			node.onStatusUpdated -= UpdateStatusView;
			node.onStatusUpdated += UpdateStatusView;

			node.onFocuseUpdated -= UpdateFocusView;
			node.onFocuseUpdated += UpdateFocusView;

			node.onPortsUpdated -= UpdatePortView;
			node.onPortsUpdated += UpdatePortView;
			
            styleSheets.Add(Resources.Load<StyleSheet>(baseNodeStyle));

            if (!string.IsNullOrEmpty(node.layoutStyle))
                styleSheets.Add(Resources.Load<StyleSheet>(node.layoutStyle));

			InitializeView();
			InitializePorts();
			InitializeDebug();

			// If the standard Enable method is still overwritten, we call it
			if (GetType().GetMethod(nameof(Enable), new Type[]{}).DeclaringType != typeof(BaseNodeView))
				ExceptionToLog.Call(() => Enable());
			else
				ExceptionToLog.Call(() => Enable(false));

			InitializeSettings();

			RefreshExpandedState();

			this.RefreshPorts();

			UpdateStatusView(100f);

			RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
			RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
			OnGeometryChanged(null);
		}

		void InitializePorts()
		{
			var listener = owner.connectorListener;

			foreach (var inputPort in nodeTarget.inputPorts)
			{
				AddPort(inputPort.fieldInfo, Direction.Input, listener, inputPort.portData);
			}

			foreach (var outputPort in nodeTarget.outputPorts)
			{
				AddPort(outputPort.fieldInfo, Direction.Output, listener, outputPort.portData);
			}
		}

		void InitializeView()
		{
			progressElement = contentContainer.Q("divider", "horizontal");
			
            controlsContainer = new VisualElement{ name = "controls" };
			controlsContainer.AddToClassList("NodeControls");
			mainContainer.Add(controlsContainer);

			rightTitleContainer = new VisualElement{ name = "RightTitleContainer" };
			titleContainer.Add(rightTitleContainer);

			topPortContainer = new VisualElement { name = "TopPortContainer" };
			this.Insert(0, topPortContainer);

			bottomPortContainer = new VisualElement { name = "BottomPortContainer" };
			this.Add(bottomPortContainer);

			runContainer = new VisualElement { name = "RunContainer" };

            if (nodeTarget.showControlsOnHover)
			{
				bool mouseOverControls = false;
				controlsContainer.style.display = DisplayStyle.None;
				RegisterCallback<MouseOverEvent>(e => {
					controlsContainer.style.display = DisplayStyle.Flex;
					mouseOverControls = true;
				});
				RegisterCallback<MouseOutEvent>(e => {
					var rect = GetPosition();
					var graphMousePosition = owner.contentViewContainer.WorldToLocal(e.mousePosition);
					if (rect.Contains(graphMousePosition) || !nodeTarget.showControlsOnHover)
						return;
					mouseOverControls = false;
					schedule.Execute(_ => {
						if (!mouseOverControls)
							controlsContainer.style.display = DisplayStyle.None;
					}).ExecuteLater(500);
				});
			}

			Undo.undoRedoPerformed += UpdateFieldValues;

			debugContainer = new VisualElement{ name = "debug" };
			if (nodeTarget.debug)
				mainContainer.Add(debugContainer);

			initializing = true;

			UpdateTitle();
            SetPosition(nodeTarget.position);
			SetNodeColor(nodeTarget.color);
            
			AddInputContainer();

			// Add renaming capability
			if ((capabilities & Capabilities.Renamable) != 0)
				SetupRenamableTitle();

			if (nodeTarget.needTrigger)
			{
				triggerButton = new Button(TriggerButtonClicked) { name = "trigger-button" };
				triggerButton.style.backgroundImage = Resources.Load<Texture2D>("Icons/Icon-CodeRun");
                runContainer.Insert(0, triggerButton);

				pauseButton = new Button(PauseButtonClicked) { name = "pause-button" };
				pauseButton.style.backgroundImage = EditorGUIUtility.FindTexture("PauseButton On@2x");
				pauseButton.style.display = DisplayStyle.None;
                runContainer.Insert(1, pauseButton);

                nodeTarget.onReady -= EnablePauseView;
                nodeTarget.onReady += EnablePauseView;

                nodeTarget.beforeProcessSetup -= EnablePauseView;
                nodeTarget.beforeProcessSetup += EnablePauseView;

                nodeTarget.onProcessed -= EnableTriggerableView;
				nodeTarget.onProcessed += EnableTriggerableView;
			}

			circularContainer = new VisualElement() { name = "circularContainer" };
            progressLabel = new Label { name = "progress-label" };
            circularContainer.Add(new CircularProgress());
            circularContainer.Add(progressLabel);
            circularContainer.style.display = DisplayStyle.None;
            rightTitleContainer.Insert(0, circularContainer);
			rightTitleContainer.Add(runContainer);
        }

		/// <summary>
		/// 把节点更新到暂定状态视图
		/// </summary>
		public void EnablePauseView()
		{
			if (!nodeTarget.needTrigger) return;
			nodeTarget.isTriggered = true;
			triggerButton.style.display = DisplayStyle.None;
			pauseButton.style.display = DisplayStyle.Flex;
		}

		/// <summary>
		/// 把节点更新到初始状态（可被触发的状态）视图
		/// </summary>
		public void EnableTriggerableView()
		{
			if (!nodeTarget.needTrigger) return;
			nodeTarget.isTriggered = false;
			triggerButton.style.display = DisplayStyle.Flex;
			pauseButton.style.display = DisplayStyle.None;
		}

		public void TriggerButtonClicked()
		{
			if (!nodeTarget.needTrigger) return;
			EnablePauseView();
			owner.RegisterCompleteObjectUndo("Trigger node");
			owner.InvokeNodeTriggeredCallback(nodeTarget);
		}

		public void PauseButtonClicked()
		{
			if (!nodeTarget.needTrigger) return;
			owner.InvokeNodeCancelledCallback(nodeTarget);
		}

		/// <summary>
        /// 重命名系统，节点标题可编辑
        /// </summary>
        void SetupRenamableTitle()
		{
			var titleLabel = this.Q("title-label") as Label;

			titleTextField = new TextField{ isDelayed = true };
			titleTextField.style.display = DisplayStyle.None;
			titleLabel.parent.Insert(0, titleTextField);

			titleLabel.RegisterCallback<MouseDownEvent>(e => {
				if (e.clickCount == 2 && e.button == (int)MouseButton.LeftMouse)
					OpenTitleEditor();
			});

			titleTextField.RegisterValueChangedCallback(e =>
			{
				CloseAndSaveTitleEditor(e.newValue);
				NotifyNodeRenamed();
			});

			titleTextField.RegisterCallback<MouseDownEvent>(e => {
				if (e.clickCount == 2 && e.button == (int)MouseButton.LeftMouse)
					CloseAndSaveTitleEditor(titleTextField.value);
			});

			titleTextField.RegisterCallback<FocusOutEvent>(e => CloseAndSaveTitleEditor(titleTextField.value));

			void OpenTitleEditor()
			{
				// show title textbox
				titleTextField.style.display = DisplayStyle.Flex;
				titleLabel.style.display = DisplayStyle.None;
				titleTextField.focusable = true;

				titleTextField.SetValueWithoutNotify(title);
				titleTextField.Focus();
				titleTextField.SelectAll();
			}

			void CloseAndSaveTitleEditor(string newTitle)
			{
				owner.RegisterCompleteObjectUndo("Renamed node " + newTitle);
				nodeTarget.SetUniqueCustomName(newTitle);

				// hide title TextBox
				titleTextField.style.display = DisplayStyle.None;
				titleLabel.style.display = DisplayStyle.Flex;
				titleTextField.focusable = false;

				UpdateTitle();
			}
		}

		public void UpdateTitle()
		{
			title = (nodeTarget.GetCustomName() == null) ? nodeTarget.GetType().Name : nodeTarget.GetCustomName();
		}

		void InitializeSettings()
		{
			// Initialize settings button:
			if (hasSettings)
			{
				CreateSettingButton();
				settingsContainer = new NodeSettingsView();
				settingsContainer.visible = false;
				settings = new VisualElement();
				// Add Node type specific settings
				settings.Add(CreateSettingsView());
				settingsContainer.Add(settings);
				Add(settingsContainer);
				
				var fields = nodeTarget.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

				foreach(var field in fields)
					if(field.GetCustomAttribute(typeof(SettingAttribute)) != null) 
						AddSettingField(field);
			}
		}

		void OnGeometryChanged(GeometryChangedEvent evt)
		{
			if (settingButton != null)
			{
				var settingsButtonLayout = settingButton.ChangeCoordinatesTo(settingsContainer.parent, settingButton.layout);
				settingsContainer.style.top = settingsButtonLayout.yMax - 18f;
				settingsContainer.style.left = settingsButtonLayout.xMin - layout.width + 20f;
			}
		}

		// Workaround for bug in GraphView that makes the node selection border way too big
		VisualElement selectionBorder, nodeBorder;
		internal void EnableSyncSelectionBorderHeight()
		{
			if (selectionBorder == null || nodeBorder == null)
			{
				selectionBorder = this.Q("selection-border");
				nodeBorder = this.Q("node-border");

				schedule.Execute(() => {
					selectionBorder.style.height = nodeBorder.localBound.height;
				}).Every(17);
			}
		}
		
		void CreateSettingButton()
		{
			settingButton = new Button(ToggleSettings){name = "settings-button"};
			//settingButton.Add(new Image { name = "icon", scaleMode = ScaleMode.ScaleToFit });

            rightTitleContainer.Add(settingButton);
		}

		void ToggleSettings()
		{
			settingsExpanded = !settingsExpanded;
			if (settingsExpanded)
				OpenSettings();
			else
				CloseSettings();
		}

		public void OpenSettings()
		{
			if (settingsContainer != null)
			{
				owner.ClearSelection();
				owner.AddToSelection(this);

				settingButton.AddToClassList("clicked");
				settingsContainer.visible = true;
				settingsExpanded = true;
				onOpenSettings?.Invoke();
			}
		}

		public void CloseSettings()
		{
			if (settingsContainer != null)
			{
				settingButton.RemoveFromClassList("clicked");
				settingsContainer.visible = false;
				settingsExpanded = false;
			}
		}

		void InitializeDebug()
		{
			ComputeOrderUpdatedCallback();
			debugContainer.Add(computeOrderLabel);
		}

		#endregion

		#region API

		public List< PortView > GetPortViewsFromFieldName(string fieldName)
		{
			List< PortView >	ret;

			portsPerFieldName.TryGetValue(fieldName, out ret);

			return ret;
		}

		public PortView GetFirstPortViewFromFieldName(string fieldName)
		{
			return GetPortViewsFromFieldName(fieldName)?.First();
		}

		public PortView GetPortViewFromFieldName(string fieldName, string identifier)
		{
			return GetPortViewsFromFieldName(fieldName)?.FirstOrDefault(pv => {
				return (pv.portData.identifier == identifier) || (String.IsNullOrEmpty(pv.portData.identifier) && String.IsNullOrEmpty(identifier));
			});
		}


		public PortView AddPort(FieldInfo fieldInfo, Direction direction, BaseEdgeConnectorListener listener, PortData portData)
		{
			PortView p = CreatePortView(direction, fieldInfo, portData, listener);

			if (p.direction == Direction.Input)
			{
				inputPortViews.Add(p);

				if (portData.vertical)
					topPortContainer.Add(p);
				else
					inputContainer.Add(p);
			}
			else
			{
				outputPortViews.Add(p);

				if (portData.vertical)
					bottomPortContainer.Add(p);
				else
					outputContainer.Add(p);
			}

			p.Initialize(this, portData?.displayName);

			List< PortView > ports;
			portsPerFieldName.TryGetValue(p.fieldName, out ports);
			if (ports == null)
			{
				ports = new List< PortView >();
				portsPerFieldName[p.fieldName] = ports;
			}
			ports.Add(p);

			return p;
		}

		public void InsertPortView(int index, VisualElement container, PortView p)
		{
			if (index >= container.childCount)
				container.Add(p);
			else
				container.Insert(index, p);
		}

		public PortView InsertPort(int index, int count, FieldInfo fieldInfo, Direction direction, BaseEdgeConnectorListener listener, PortData portData)
		{
			PortView p = CreatePortView(direction, fieldInfo, portData, listener);

            //if (p.direction == Direction.Input)
            //{
            //	inputPortViews.Insert(index, p);

            //	if (portData.vertical)
            //		topPortContainer.Insert(count - 1 - index, p);
            //	else
            //		inputContainer.Insert(count - 1 - index, p);
            //}
            //else
            //{
            //	outputPortViews.Insert(index, p);

            //	if (portData.vertical)
            //		bottomPortContainer.Insert(count - 1 - index, p);
            //	else
            //		outputContainer.Insert(count - 1 - index, p);
            //}
            if (p.direction == Direction.Input)
            {
                inputPortViews.Insert(index, p);

				if (portData.vertical)
					InsertPortView(count - 1 - index, topPortContainer, p);
				else
					InsertPortView(count - 1 - index, inputContainer, p);
            }
            else
            {
                outputPortViews.Insert(index, p);

				if (portData.vertical)
					InsertPortView(count - 1 - index, bottomPortContainer, p);
				else
					InsertPortView(count - 1 - index, outputContainer, p);
            }

            p.Initialize(this, portData?.displayName);

			List<PortView> ports;
			portsPerFieldName.TryGetValue(p.fieldName, out ports);
			if (ports == null)
			{
				ports = new List<PortView>();
				portsPerFieldName[p.fieldName] = ports;
			}
			ports.Add(p);

			return p;
		}

		protected virtual PortView CreatePortView(Direction direction, FieldInfo fieldInfo, PortData portData, BaseEdgeConnectorListener listener)
        	=> PortView.CreatePortView(direction, fieldInfo, portData, listener);

        public void InsertPort(PortView portView, int index)
		{
			if (portView.direction == Direction.Input)
			{
				if (portView.portData.vertical)
					topPortContainer.Insert(index, portView);
				else
					inputContainer.Insert(index, portView);
			}
			else
			{
				if (portView.portData.vertical)
					bottomPortContainer.Insert(index, portView);
				else
					outputContainer.Insert(index, portView);
			}
		}

		public void RemovePort(PortView p)
		{
			// Remove all connected edges:
			var edgesCopy = p.GetEdges().ToList();
			foreach (var e in edgesCopy)
				owner.Disconnect(e, refreshPorts: false);

			if (p.direction == Direction.Input)
			{
				if (inputPortViews.Remove(p))
					p.RemoveFromHierarchy();
			}
			else
			{
				if (outputPortViews.Remove(p))
					p.RemoveFromHierarchy();
			}

			List< PortView > ports;
			portsPerFieldName.TryGetValue(p.fieldName, out ports);
			ports.Remove(p);
		}
		
		private void SetValuesForSelectedNodes()
		{
			selectedNodes = new List<Node>();
			owner.nodes.ForEach(node =>
			{
				if(node.selected) selectedNodes.Add(node);
			});

			if(selectedNodes.Count < 2) return; //	No need for any of the calculations below

			selectedNodesFarLeft   = int.MinValue;
			selectedNodesFarRight  = int.MinValue;
			selectedNodesFarTop    = int.MinValue;
			selectedNodesFarBottom = int.MinValue;

			selectedNodesNearLeft   = int.MaxValue;
			selectedNodesNearRight  = int.MaxValue;
			selectedNodesNearTop    = int.MaxValue;
			selectedNodesNearBottom = int.MaxValue;

			foreach(var selectedNode in selectedNodes)
			{
				var nodeStyle  = selectedNode.style;
				var nodeWidth  = selectedNode.localBound.size.x;
				var nodeHeight = selectedNode.localBound.size.y;

				if(nodeStyle.left.value.value > selectedNodesFarLeft) selectedNodesFarLeft                 = nodeStyle.left.value.value;
				if(nodeStyle.left.value.value + nodeWidth > selectedNodesFarRight) selectedNodesFarRight   = nodeStyle.left.value.value + nodeWidth;
				if(nodeStyle.top.value.value > selectedNodesFarTop) selectedNodesFarTop                    = nodeStyle.top.value.value;
				if(nodeStyle.top.value.value + nodeHeight > selectedNodesFarBottom) selectedNodesFarBottom = nodeStyle.top.value.value + nodeHeight;

				if(nodeStyle.left.value.value < selectedNodesNearLeft) selectedNodesNearLeft                 = nodeStyle.left.value.value;
				if(nodeStyle.left.value.value + nodeWidth < selectedNodesNearRight) selectedNodesNearRight   = nodeStyle.left.value.value + nodeWidth;
				if(nodeStyle.top.value.value < selectedNodesNearTop) selectedNodesNearTop                    = nodeStyle.top.value.value;
				if(nodeStyle.top.value.value + nodeHeight < selectedNodesNearBottom) selectedNodesNearBottom = nodeStyle.top.value.value + nodeHeight;
			}

			selectedNodesAvgHorizontal = (selectedNodesNearLeft + selectedNodesFarRight) / 2f;
			selectedNodesAvgVertical   = (selectedNodesNearTop + selectedNodesFarBottom) / 2f;
		}

		public static Rect GetNodeRect(Node node, float left = int.MaxValue, float top = int.MaxValue)
		{
			return new Rect(
				new Vector2(left != int.MaxValue ? left : node.style.left.value.value, top != int.MaxValue ? top : node.style.top.value.value),
				new Vector2(node.style.width.value.value, node.style.height.value.value)
			);
		}

		public void AlignToLeft()
		{
			SetValuesForSelectedNodes();
			if(selectedNodes.Count < 2) return;

			foreach(var selectedNode in selectedNodes)
			{
				selectedNode.SetPosition(GetNodeRect(selectedNode, selectedNodesNearLeft));
			}
		}

		public void AlignToCenter()
		{
			SetValuesForSelectedNodes();
			if(selectedNodes.Count < 2) return;

			foreach(var selectedNode in selectedNodes)
			{
				selectedNode.SetPosition(GetNodeRect(selectedNode, selectedNodesAvgHorizontal - selectedNode.localBound.size.x / 2f));
			}
		}

		public void AlignToRight()
		{
			SetValuesForSelectedNodes();
			if(selectedNodes.Count < 2) return;

			foreach(var selectedNode in selectedNodes)
			{
				selectedNode.SetPosition(GetNodeRect(selectedNode, selectedNodesFarRight - selectedNode.localBound.size.x));
			}
		}

		public void AlignToTop()
		{
			SetValuesForSelectedNodes();
			if(selectedNodes.Count < 2) return;

			foreach(var selectedNode in selectedNodes)
			{
				selectedNode.SetPosition(GetNodeRect(selectedNode, top: selectedNodesNearTop));
			}
		}

		public void AlignToMiddle()
		{
			SetValuesForSelectedNodes();
			if(selectedNodes.Count < 2) return;

			foreach(var selectedNode in selectedNodes)
			{
				selectedNode.SetPosition(GetNodeRect(selectedNode, top: selectedNodesAvgVertical - selectedNode.localBound.size.y / 2f));
			}
		}

		public void AlignToBottom()
		{
			SetValuesForSelectedNodes();
			if(selectedNodes.Count < 2) return;

			foreach(var selectedNode in selectedNodes)
			{
				selectedNode.SetPosition(GetNodeRect(selectedNode, top: selectedNodesFarBottom - selectedNode.localBound.size.y));
			}
		}

		public void OpenNodeViewScript()
		{
			var script = NodeProvider.GetNodeViewScript(GetType());

			if (script != null)
				AssetDatabase.OpenAsset(script.GetInstanceID(), 0, 0);
		}

		public void OpenNodeScript()
		{
			var script = NodeProvider.GetNodeScript(nodeTarget.GetType());

			if (script != null)
				AssetDatabase.OpenAsset(script.GetInstanceID(), 0, 0);
		}

		public void ToggleDebug()
		{
			nodeTarget.debug = !nodeTarget.debug;
			UpdateDebugView();
		}

		public void UpdateDebugView()
		{
			if (nodeTarget.debug)
				mainContainer.Add(debugContainer);
			else
				mainContainer.Remove(debugContainer);
		}

		public void AddMessageView(string message, Texture icon, Color color)
			=> AddBadge(new NodeBadgeView(message, icon, color));

		public void AddMessageView(string message, NodeMessageType messageType)
		{
			IconBadge	badge = null;
			switch (messageType)
			{
				case NodeMessageType.Warning:
					badge = new NodeBadgeView(message, EditorGUIUtility.IconContent("Collab.Warning").image, Color.yellow);
					break ;
				case NodeMessageType.Error:	
					badge = IconBadge.CreateError(message);
					break ;
				case NodeMessageType.Info:
					badge = IconBadge.CreateComment(message);
					break ;
				default:
				case NodeMessageType.None:
					badge = new NodeBadgeView(message, null, Color.grey);
					break ;
			}
			
			AddBadge(badge);
		}

		void AddBadge(IconBadge badge)
		{
			Add(badge);
			badges.Add(badge);
			badge.AttachTo(topContainer, SpriteAlignment.TopRight);
		}

		void RemoveBadge(Func<IconBadge, bool> callback)
		{
			badges.RemoveAll(b => {
				if (callback(b))
				{
					b.Detach();
					b.RemoveFromHierarchy();
					return true;
				}
				return false;
			});
		}

		public void RemoveMessageViewContains(string message) => RemoveBadge(b => b.badgeText.Contains(message));
		
		public void RemoveMessageView(string message) => RemoveBadge(b => b.badgeText == message);

		public void UpdateStatusView(float progress)
        {
	        ShowProgressElements(false);
			if (nodeTarget.status == NodeStatus.Init)
				progressElement.RemoveFromClassList("progress");
			else
				progressElement.AddToClassList("progress");
			switch (nodeTarget.status)
            {
				case NodeStatus.Done:
					SetNodeColor(Color.green);
                    break;
                case NodeStatus.Error:
                    SetNodeColor(Color.red);
					break;
				case NodeStatus.Working:
					SetNodeColor(Color.cyan, progress);
					ShowProgressElements(true);
					progressLabel.text = $"{progress:N0}%";
					break;
				case NodeStatus.Queued:
					SetNodeColor(Color.yellow, DataUtil.IsFloatZero(progress) ? 100 : 100f / progress);
					ShowProgressElements(true);
					progressLabel.text = $"-/{progress:N0}";
					break;
				case NodeStatus.Init:
				default:
					SetNodeColor(Color.clear);
					break;
            }
        }

		void ShowProgressElements(bool show)
		{
			var display = show ? DisplayStyle.Flex : DisplayStyle.None;
			// step 1: show circularContainer
			circularContainer.style.display = display;
			// step 2: show time cost label
			var label = mainContainer.Q<Label>("costTimeLabel");
			if (label != null)
				label.style.display = display;
		}

		public void UpdatePortView(string fieldName)
		{
			schedule.Execute(_ => UpdatePortsForField(fieldName)).ExecuteLater(0);
		}

		public void UpdateFocusView(bool focus)
        {
			if (focus) Highlight();
			else UnHighlight();
        }

		public void Highlight()
		{
			AddToClassList("Highlight");
		}

		public void UnHighlight()
		{
			RemoveFromClassList("Highlight");
		}

		#endregion

		#region Callbacks & Overrides

		void ComputeOrderUpdatedCallback()
		{
			//Update debug compute order
			computeOrderLabel.text = "Compute order: " + nodeTarget.computeOrder;
		}

		public virtual void Enable(bool fromInspector = false)
		{
			if (fromInspector)
			{
				CreateInspectorGUI(inspectorContainer);
			}
			else
			{
				CreateGUI(controlsContainer);
			}
		}

		public virtual void Enable()
		{
			CreateGUI(controlsContainer);
		}

		/// <summary>
		/// 创建节点内的控件视图，可以重载
		/// </summary>
		/// <param name="contentContainer">控件视图层级的容器，即controlsContainer </param>
		protected virtual void CreateGUI(VisualElement contentContainer) => DrawDefaultInspector(false);

		/// <summary>
		/// 创建节点的Inspector面板视图，可以重载
		/// </summary>
		/// <param name="contentContainer">Inspector视图层级的容器，即inspectorContainer </param>
		protected virtual void CreateInspectorGUI(VisualElement contentContainer) => DrawDefaultInspector(true);

		public virtual void Disable() {}
		public virtual void OnDetachFromPanel(DetachFromPanelEvent evt) { }

		Dictionary<string, List<(object value, VisualElement target)>> visibleConditions = new Dictionary<string, List<(object value, VisualElement target)>>();
		Dictionary<string, VisualElement>  hideElementIfConnected = new Dictionary<string, VisualElement>();
		Dictionary<FieldInfo, List<VisualElement>> fieldControlsMap = new Dictionary<FieldInfo, List<VisualElement>>();

		protected void AddInputContainer()
		{
			inputContainerElement = new VisualElement {name = "input-container"};
			mainContainer.parent.Add(inputContainerElement);
			inputContainerElement.SendToBack();
			inputContainerElement.pickingMode = PickingMode.Ignore;
		}

        public virtual void DisablePinView() { }
		public virtual void EnablePinView() { }

        /// <summary>
        /// ！！！(by yin)改为不可重载，此方法只负责实现创建视图的默认逻辑
        /// 如果用户要自定义节点内部或Inspector面板的视图，请重载并实现CreateGUI()或CreateInspectorGUI()方法
        /// </summary>
        /// <param name="fromInspector"></param>
        protected void DrawDefaultInspector(bool fromInspector = false)
		{
			var fields = nodeTarget.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				// Filter fields from the BaseNode type since we are only interested in user-defined fields
				// (better than BindingFlags.DeclaredOnly because we keep any inherited user-defined fields) 
				.Where(f => f.DeclaringType != typeof(BaseNode));
			
            fields = nodeTarget.OverrideFieldOrder(fields).Reverse();

            foreach (var field in fields)
			{
				//skip if the field is a node setting
				if(field.GetCustomAttribute(typeof(SettingAttribute)) != null)
				{
					hasSettings = true;
					continue;
				}

				//skip if the field is not serializable
				if((!field.IsPublic && field.GetCustomAttribute(typeof(SerializeField)) == null) || field.IsNotSerialized)
				{
					AddEmptyField(field, fromInspector);
					continue;
				}

				//skip if the field is an input/output and not marked as SerializedField
				bool hasInputAttribute         = field.GetCustomAttribute(typeof(InputAttribute)) != null;
				bool hasInputOrOutputAttribute = hasInputAttribute || field.GetCustomAttribute(typeof(OutputAttribute)) != null;
				bool showAsDrawer			   = !fromInspector && field.GetCustomAttribute(typeof(ShowAsDrawer)) != null;
				if (field.GetCustomAttribute(typeof(SerializeField)) == null && hasInputOrOutputAttribute && !showAsDrawer)
				{
					AddEmptyField(field, fromInspector);
					continue;
				}

				//skip if marked with NonSerialized or HideInInspector
				if (field.GetCustomAttribute(typeof(System.NonSerializedAttribute)) != null || field.GetCustomAttribute(typeof(HideInInspector)) != null)
				{
					AddEmptyField(field, fromInspector);
					continue;
				}

				// Hide the field if we want to display in in the inspector
				var showInInspector = field.GetCustomAttribute<ShowInInspector>();
				if (showInInspector != null && !showInInspector.showInNode && !fromInspector)
				{
					AddEmptyField(field, fromInspector);
					continue;
				}

				var showInputDrawer = field.GetCustomAttribute(typeof(InputAttribute)) != null && field.GetCustomAttribute(typeof(SerializeField)) != null;
				showInputDrawer |= field.GetCustomAttribute(typeof(InputAttribute)) != null && field.GetCustomAttribute(typeof(ShowAsDrawer)) != null;
				showInputDrawer &= !fromInspector; // We can't show a drawer in the inspector
				showInputDrawer &= !typeof(IList).IsAssignableFrom(field.FieldType);

				string displayName = ObjectNames.NicifyVariableName(field.Name);

				var inspectorNameAttribute = field.GetCustomAttribute<InspectorNameAttribute>();
				if (inspectorNameAttribute != null)
					displayName = inspectorNameAttribute.displayName;

				var elem = AddControlField(field, displayName, fromInspector, showInputDrawer);
				if (hasInputAttribute)
				{
					hideElementIfConnected[field.Name] = elem;

					// Hide the field right away if there is already a connection:
					if (portsPerFieldName.TryGetValue(field.Name, out var pvs))
						if (pvs.Any(pv => pv.GetEdges().Count > 0))
							elem.style.display = DisplayStyle.None;
				}
			}
		}

		/// <summary>
		/// 设置节点进度条的颜色和进度
		/// </summary>
		/// <param name="color">进度条颜色</param>
		/// <param name="progress">进度条进度, [0,100]</param>
        protected virtual void SetNodeColor(Color color, float progress = 100.0f)
		{
			progressElement.style.backgroundColor = color;
			progress = Mathf.Clamp(progress, 0f, 100f);
			progressElement.style.width = new StyleLength(Length.Percent(progress));
			// titleContainer.style.borderBottomColor = new StyleColor(color);
			// titleContainer.style.borderBottomWidth = new StyleFloat(color.a > 0 ? 3f : 0f);
		}
		
		private void AddEmptyField(FieldInfo field, bool fromInspector)
		{
			if (field.GetCustomAttribute(typeof(InputAttribute)) == null || fromInspector)
				return;

			if (field.GetCustomAttribute<VerticalAttribute>() != null)
				return;
			
			var box = new VisualElement {name = field.Name};
			box.AddToClassList("port-input-element");
			box.AddToClassList("empty");
			inputContainerElement.Add(box);
		}

		public void UpdateFieldVisibility(string fieldName, object newValue)
		{
			if (newValue == null)
				return;
			if (visibleConditions.TryGetValue(fieldName, out var list))
			{
				foreach (var elem in list)
				{
					if (newValue.Equals(elem.value))
						elem.target.style.display = DisplayStyle.Flex;
					else
						elem.target.style.display = DisplayStyle.None;
				}
			}
		}

		void UpdateOtherFieldValueSpecific<T>(FieldInfo field, object newValue)
		{
			foreach (var inputField in fieldControlsMap[field])
			{
				var nodePropertyField = inputField as NodePropertyField;
				if (nodePropertyField != null)
					nodePropertyField.SetValueWithoutNotify(newValue);

                var notify = inputField as INotifyValueChanged<T>;
                if (notify != null)
                    notify.SetValueWithoutNotify((T)newValue);
            }
		}

		static MethodInfo specificUpdateOtherFieldValue = typeof(BaseNodeView).GetMethod(nameof(UpdateOtherFieldValueSpecific), BindingFlags.NonPublic | BindingFlags.Instance);
		public void UpdateOtherFieldValue(FieldInfo info, object newValue)
		{
			// Warning: Keep in sync with FieldFactory CreateField
			var fieldType = info.FieldType.IsSubclassOf(typeof(UnityEngine.Object)) ? typeof(UnityEngine.Object) : info.FieldType;
			var genericUpdate = specificUpdateOtherFieldValue.MakeGenericMethod(fieldType);

			genericUpdate.Invoke(this, new object[]{info, newValue});
		}

		object GetInputFieldValueSpecific<T>(FieldInfo field)
		{
			if (fieldControlsMap.TryGetValue(field, out var list))
			{
				foreach (var inputField in list)
				{
					if (inputField is INotifyValueChanged<T> notify)
						return notify.value;
				}
			}
			return null;
		}

		static MethodInfo specificGetValue = typeof(BaseNodeView).GetMethod(nameof(GetInputFieldValueSpecific), BindingFlags.NonPublic | BindingFlags.Instance);
		object GetInputFieldValue(FieldInfo info)
		{
			// Warning: Keep in sync with FieldFactory CreateField
			var fieldType = info.FieldType.IsSubclassOf(typeof(UnityEngine.Object)) ? typeof(UnityEngine.Object) : info.FieldType;
			var genericUpdate = specificGetValue.MakeGenericMethod(fieldType);

			return genericUpdate.Invoke(this, new object[]{info});
		}

		protected VisualElement RegisterNodeChangingCallback(string fieldName) => AddControlField(fieldName: fieldName, noDisplay: true);

		protected VisualElement AddControlField(string fieldName, string label = null, bool fromInspector = false, bool showInputDrawer = false, bool noDisplay = false, Action valueChangedCallback = null)
			=> AddControlField(nodeTarget.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), label, fromInspector, showInputDrawer, noDisplay, valueChangedCallback);

		Regex s_ReplaceNodeIndexPropertyPath = new Regex(@"(^nodes.Array.data\[)(\d+)(\])");
		internal void SyncSerializedPropertyPathes()
		{
			int nodeIndex = owner.graph.nodes.FindIndex(n => n == nodeTarget);

			// If the node is not found, then it means that it has been deleted from serialized data.
			if (nodeIndex == -1)
				return;

			var nodeIndexString = nodeIndex.ToString();
			foreach (var propertyField in this.Query<NodePropertyField>().ToList())
			{
				propertyField.Unbind();
				// The property path look like this: nodes.Array.data[x].fieldName
				// And we want to update the value of x with the new node index:
				if(propertyField.bindingPath == null) continue;
				propertyField.bindingPath = s_ReplaceNodeIndexPropertyPath.Replace(propertyField.bindingPath, m => m.Groups[1].Value + nodeIndexString + m.Groups[3].Value);
				propertyField.Bind(owner.serializedGraph);
			}
		}

		protected SerializedProperty FindSerializedProperty(string fieldName)
		{
			int i = owner.graph.nodes.FindIndex(n => n == nodeTarget);
			return owner.serializedGraph.FindProperty("nodes").GetArrayElementAtIndex(i).FindPropertyRelative(fieldName);
		}
		
		public virtual void OnFieldChanged(string fieldName, object value)
		{
			
		}

		protected VisualElement AddControlField(FieldInfo field, string label = null, bool fromInspector = false, 
			bool showInputDrawer = false, bool noDisplay = false, Action valueChangedCallback = null,
			bool addDefaultValueChangedCallback = true)
		{
			if (field == null)
				return null;

			var element = new NodePropertyField(
				FindSerializedProperty(field.Name), 
				showInputDrawer ? "" : label, 
				field, 
				showInputDrawer,
				fromInspector,
				this, 
				valueChangedCallback);
			element.Bind(owner.serializedGraph);

#if UNITY_2020_3 // In Unity 2020.3 the empty label on property field doesn't hide it, so we do it manually
			if ((showInputDrawer || String.IsNullOrEmpty(label)) && element != null)
				element.AddToClassList("DrawerField_2020_3");
#endif

			if (typeof(IList).IsAssignableFrom(field.FieldType))
				EnableSyncSelectionBorderHeight();

			if (addDefaultValueChangedCallback)
				element.RegisterCallback<SerializedPropertyChangeEvent>(element.ChangeValueCallback);

			// Disallow picking scene objects when the graph is not linked to a scene
			if (element != null && !owner.graph.IsLinkedToScene())
			{
				var objectField = element.Q<ObjectField>();
				if (objectField != null)
					objectField.allowSceneObjects = false;
			}

			if (!fieldControlsMap.TryGetValue(field, out var inputFieldList))
				inputFieldList = fieldControlsMap[field] = new List<VisualElement>();
			inputFieldList.Add(element);

			if(element != null)
			{
				if (showInputDrawer)
				{
					var box = new VisualElement {name = field.Name};
					box.AddToClassList("port-input-element");
					box.Add(element);
					inputContainerElement.Add(box);
				}
				else if (fromInspector)
				{
					inspectorContainer?.Add(element);
				}
				else
				{
					controlsContainer.Add(element);
				}
				element.name = field.Name;
			}
			else
			{
				// Make sure we create an empty placeholder if FieldFactory can not provide a drawer
				if (showInputDrawer) AddEmptyField(field, false);
			}

			var visibleCondition = field.GetCustomAttribute(typeof(VisibleIf)) as VisibleIf;
			if (visibleCondition != null)
			{
				// Check if target field exists:
				var conditionField = nodeTarget.GetType().GetField(visibleCondition.fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				if (conditionField == null)
					Debug.LogError($"[VisibleIf] Field {visibleCondition.fieldName} does not exists in node {nodeTarget.GetType()}");
				else
				{
					visibleConditions.TryGetValue(visibleCondition.fieldName, out var list);
					if (list == null)
						list = visibleConditions[visibleCondition.fieldName] = new List<(object value, VisualElement target)>();
					list.Add((visibleCondition.value, element));
					UpdateFieldVisibility(visibleCondition.fieldName, conditionField.GetValue(nodeTarget));
				}
			}

			element.style.display = noDisplay ? DisplayStyle.None : DisplayStyle.Flex;

			return element;
		}

		void UpdateFieldValues()
		{
			foreach (var kp in fieldControlsMap)
				UpdateOtherFieldValue(kp.Key, kp.Key.GetValue(nodeTarget));
		}
		
		protected void AddSettingField(FieldInfo field)
		{
			if (field == null)
				return;

			var label = field.GetCustomAttribute<SettingAttribute>().name;

			var element = new PropertyField(FindSerializedProperty(field.Name));
			element.Bind(owner.serializedGraph);

			if (element != null)
			{
				settingsContainer.Add(element);
				element.name = field.Name;
			}
		}

		internal void OnPortConnected(PortView port)
		{
			if(port.direction == Direction.Input && inputContainerElement?.Q(port.fieldName) != null)
				inputContainerElement.Q(port.fieldName).AddToClassList("empty");
			
			if (hideElementIfConnected.TryGetValue(port.fieldName, out var elem))
				elem.style.display = DisplayStyle.None;

			onPortConnected?.Invoke(port);
		}

		internal void OnPortDisconnected(PortView port)
		{
			if (port.direction == Direction.Input && inputContainerElement?.Q(port.fieldName) != null)
			{
				inputContainerElement.Q(port.fieldName).RemoveFromClassList("empty");

				if (nodeTarget.nodeFields.TryGetValue(port.fieldName, out var fieldInfo))
				{
					var valueBeforeConnection = GetInputFieldValue(fieldInfo.info);

					if (valueBeforeConnection != null)
					{
						fieldInfo.info.SetValue(nodeTarget, valueBeforeConnection);
					}
				}
			}
			
			if (hideElementIfConnected.TryGetValue(port.fieldName, out var elem))
				elem.style.display = DisplayStyle.Flex;

			onPortDisconnected?.Invoke(port);
		}

		// TODO: a function to force to reload the custom behavior ports (if we want to do a button to add ports for example)

		public virtual void OnRemoved() {}
		public virtual void OnCreated() {}

		public override void SetPosition(Rect newPos)
		{
            if (initializing || !nodeTarget.isLocked)
            {
                base.SetPosition(newPos);

				if (!initializing)
					owner.RegisterCompleteObjectUndo("Moved graph node");

                nodeTarget.position = newPos;
                initializing = false;
            }
		}

		public Action<bool> OnExpandAction;
		public override bool	expanded
		{
			get { return base.expanded; }
			set
			{
				base.expanded = value;
				nodeTarget.expanded = value;
				OnExpandAction?.Invoke(value);
			}
		}

        /// <summary>
        /// refresh collapse state in group, only display portview if has outer input/output
        /// </summary>
        /// <param name="group"></param>
        public virtual void RefreshCollapseStateInGroup(Group group, ref Vector2 posOffset)
        {
			nodeTarget.inCollapsedGroup = !group.expanded;
			string name = nodeTarget.GetCustomName() + ".";
            if (group.expanded)
            {
				// step 1: remove classList for uss
				RemoveFromClassList("collapsed-in-group");
				// step 2: add element except PortView
				visible = true;
				InsertElement(0, titleContainer);
                AddElement(controlsContainer);
				AddElement(debugContainer);
				// step 3: update port visibily
				inputContainer.visible = true;
				outputContainer.visible = true;
				topContainer.RemoveFromClassList("full-input");
				topContainer.RemoveFromClassList("full-output");
				foreach (PortView pv in inputPortViews)
				{
					pv.GetEdges().ForEach(e => e.visible = true);
					// step 3-2: recover port display name
					pv.RemoveFromClassList("collapsed");
					pv.portName = pv.portData.displayName;
				}
				foreach (PortView pv in outputPortViews)
				{
					pv.GetEdges().ForEach(e => e.visible = true);
                    pv.RemoveFromClassList("collapsed");
                    pv.portName = pv.portData.displayName;
				}
				// step 3-3: recover input container(drawer)
				foreach (VisualElement element in inputContainerElement.Children())
				{
					element.RemoveFromClassList("collapsed");
				}
				// step 4: udpate position
				if (group.innerNodeOrgPos.ContainsKey(nodeTarget.GUID))
				{
					// expanded after collapse
					Rect rect = group.innerNodeOrgPos[nodeTarget.GUID];
					rect.position += group.position.position;
					SetPosition(rect);
					group.innerNodeOrgPos.Remove(nodeTarget.GUID);
				}
				// step 5: recover drag and drop
				pickingMode = PickingMode.Position;
            }
            else
            {
				// step 1: add class for uss
				AddToClassList("collapsed-in-group");
				// step 2: save current related position, make sure this operation is done before node change
				if (!group.innerNodeOrgPos.ContainsKey(nodeTarget.GUID))
					group.innerNodeOrgPos.Add(nodeTarget.GUID, new Rect(
                        nodeTarget.position.position - group.position.position, nodeTarget.position.size));
				// step 3: remove element except PortView
                //RemoveElement(titleContainer);
                RemoveElement(controlsContainer);
				RemoveElement(debugContainer);
				// step 4: check PortView
				// step 4-1: check if port is connected with outer port
				// remain the port if the port has outer in/out or indegree/outdegree = 0
				bool showInput = false, isOuterPort = false;
				foreach (PortView pv in inputPortViews)
				{
					if (pv.connectionCount == 0)
					{
						// remain the port is indegree = 0
						showInput = true;
						continue;
					}
					isOuterPort = false;
					foreach (EdgeView ev in pv.GetEdges())
					{
						if (!group.innerNodeGUIDs.Contains(ev.serializedEdge.outputNode.GUID))
						{
							isOuterPort = true;
							showInput = true;
							ev.visible = true;
						} else
						{
							ev.visible = false;
						}
					}
					showInput |= isOuterPort;
					if (!isOuterPort)
					{
						pv.AddToClassList("collapsed");
						inputContainerElement.Q(pv.fieldName)?.AddToClassList("collapsed");
					}
					pv.portName = name + pv.portName;
				}
				bool showOutput = false;
				foreach (PortView pv in outputPortViews)
				{
					if (pv.connectionCount == 0)
					{
						showOutput = true;
						continue;
					}
					isOuterPort = false;
					foreach (EdgeView ev in pv.GetEdges())
					{
						if (!group.innerNodeGUIDs.Contains(ev.serializedEdge.inputNode.GUID))
						{
							isOuterPort = true;
							showOutput = true;
							ev.visible = true;
						} else
						{
							ev.visible = false;
						}
					}
					showOutput |= isOuterPort;
                    if (!isOuterPort)
                        pv.AddToClassList("collapsed");
                    pv.portName = name + pv.portName;
				}
				inputContainer.visible = showInput;
				outputContainer.visible = showOutput;
                if (!(showInput || showOutput))
				{
					visible = false;             
					// step 2-1: set new position, default is zero
                    SetPosition(group.position);
                } else
				{
					SetPosition(new Rect(group.position.position + posOffset, group.position.size));
					float height = mainContainer.Q("contents").layout.height + mainContainer.Q("title").layout.height;
					if (float.IsNaN(height))
					{
						// NOTE: 24px is normal height for PortView, this should be changed if PortView.style.height changes
						height = Math.Max(inputPortViews.Count, outputPortViews.Count) * 24 + 9;
					}
					posOffset.y += height;
					if (showInput && !showOutput)
						topContainer.AddToClassList("full-input");
					else if (showOutput && !showInput)
						topContainer.AddToClassList("full-output");
				}
				// step 5: forbid drag and drop (move position)
				pickingMode = PickingMode.Ignore;
			}
        }

        /// <summary>
		/// add element to mainContainer if exists
		/// </summary>
		/// <param name="element"></param>
		protected void AddElement(VisualElement element)
        {
            if (!mainContainer.Contains(element))
                mainContainer.Add(element);
        }

        /// <summary>
        /// remove element from mainContainer if exists
        /// </summary>
        /// <param name="element"></param>
        protected void RemoveElement(VisualElement element)
        {
            if (mainContainer.Contains(element))
                mainContainer.Remove(element);
        }

        /// <summary>
        /// insert element to mainContainer if exists
        /// </summary>
        /// <param name="index"></param>
        /// <param name="element"></param>
        protected void InsertElement(int index, VisualElement element)
		{
			if (!mainContainer.Contains(element))
				mainContainer.Insert(index, element);
		}

        public void ChangeLockStatus()
        {
            nodeTarget.nodeLock ^= true;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
	        evt.menu.AppendAction("Open Node Script", (e) => OpenNodeScript(), OpenNodeScriptStatus);
	        evt.menu.AppendAction("Open Node View Script", (e) => OpenNodeViewScript(), OpenNodeViewScriptStatus);
#if TJAI_DEBUG
	        evt.menu.AppendAction("Debug", (e) => ToggleDebug(), DebugStatus);
#endif
	        if (nodeTarget.unlockable)
		        evt.menu.AppendAction((nodeTarget.isLocked ? "Unlock" : "Lock"), (e) => ChangeLockStatus(), LockStatus);
	        evt.menu.AppendSeparator();
	        BuildAlignMenu(evt);
        }

		protected void BuildAlignMenu(ContextualMenuPopulateEvent evt)
		{
			evt.menu.AppendAction("Align/To Left %&LEFT", (e) => AlignToLeft());
			evt.menu.AppendAction("Align/To Center %&C", (e) => AlignToCenter());
			evt.menu.AppendAction("Align/To Right %&RIGHT", (e) => AlignToRight());
			evt.menu.AppendSeparator("Align/");
			evt.menu.AppendAction("Align/To Top %&UP", (e) => AlignToTop());
			evt.menu.AppendAction("Align/To Middle %&M", (e) => AlignToMiddle());
			evt.menu.AppendAction("Align/To Bottom %&DOWN", (e) => AlignToBottom());
			evt.menu.AppendSeparator();
		}

        Status LockStatus(DropdownMenuAction action)
        {
            return Status.Normal;
        }

        Status DebugStatus(DropdownMenuAction action)
		{
			if (nodeTarget.debug)
				return Status.Checked;
			return Status.Normal;
		}

		Status OpenNodeScriptStatus(DropdownMenuAction action)
		{
			if (NodeProvider.GetNodeScript(nodeTarget.GetType()) != null)
				return Status.Normal;
			return Status.Disabled;
		}

		Status OpenNodeViewScriptStatus(DropdownMenuAction action)
		{
			if (NodeProvider.GetNodeViewScript(GetType()) != null)
				return Status.Normal;
			return Status.Disabled;
		}

		IEnumerable< PortView > SyncPortCounts(IEnumerable< NodePort > ports, IEnumerable< PortView > portViews)
		{
			var listener = owner.connectorListener;
			var portViewList = portViews.ToList();

			// Maybe not good to remove ports as edges are still connected :/
			foreach (var pv in portViews.ToList())
			{
				// If the port have disappeared from the node data, we remove the view:
				// We can use the identifier here because this function will only be called when there is a custom port behavior
				if (!ports.Any(p => p.portData.identifier == pv.portData.identifier))
				{
					RemovePort(pv);
					portViewList.Remove(pv);
				}
			}

			foreach (var p in ports)
			{
				// Add missing port views
				if (!portViews.Any(pv => p.portData.identifier == pv.portData.identifier))
				{
					Direction portDirection = nodeTarget.IsFieldInput(p.fieldName) ? Direction.Input : Direction.Output;
					int index = ports.ToList().IndexOf(p);
					var pv = InsertPort(index, ports.Count(), p.fieldInfo, portDirection, listener, p.portData);
					portViewList.Insert(index, pv);
				}
			}

			return portViewList;
		}

		void SyncPortOrder(IEnumerable< NodePort > ports, IEnumerable< PortView > portViews)
		{
			var portViewList = portViews.ToList();
			var portsList = ports.ToList();

			// Re-order the port views to match the ports order in case a custom behavior re-ordered the ports
			for (int i = 0; i < portsList.Count; i++)
			{
				var id = portsList[i].portData.identifier;

				var pv = portViewList.FirstOrDefault(p => p.portData.identifier == id);
				if (pv != null)
					InsertPort(pv, i);
			}
		}

		public virtual new bool RefreshPorts()
		{
			// If a port behavior was attached to one port, then
			// the port count might have been updated by the node
			// so we have to refresh the list of port views.
			UpdatePortViewWithPorts(nodeTarget.inputPorts, inputPortViews);
			UpdatePortViewWithPorts(nodeTarget.outputPorts, outputPortViews);

			void UpdatePortViewWithPorts(NodePortContainer ports, List< PortView > portViews)
			{
				if (ports.Count == 0 && portViews.Count == 0) // Nothing to update
					return;

				// When there is no current portviews, we can't zip the list so we just add all
				if (portViews.Count == 0)
					SyncPortCounts(ports, new PortView[]{});
				else if (ports.Count == 0) // Same when there is no ports
					SyncPortCounts(new NodePort[]{}, portViews);
				else if (portViews.Count != ports.Count)
					SyncPortCounts(ports, portViews);
				else
				{
					var p = ports.GroupBy(n => n.fieldName);
					var pv = portViews.GroupBy(v => v.fieldName);
					p.Zip(pv, (portPerFieldName, portViewPerFieldName) => {
						IEnumerable< PortView > portViewsList = portViewPerFieldName;
						if (portPerFieldName.Count() != portViewPerFieldName.Count())
							portViewsList = SyncPortCounts(portPerFieldName, portViewPerFieldName);
						SyncPortOrder(portPerFieldName, portViewsList);
						// We don't care about the result, we just iterate over port and portView
						return "";
					}).ToList();
				}

				// Here we're sure that we have the same amount of port and portView
				// so we can update the view with the new port data (if the name of a port have been changed for example)

				for (int i = 0; i < portViews.Count; i++)
					portViews[i].UpdatePortView(ports[i].portData);
				if (nodeTarget.inCollapsedGroup)
				{
					string name = nodeTarget.GetCustomName() + ".";
                    //portViews.ForEach(pv => pv.portName = name + pv.portName);
                    portViews.ForEach(pv => pv.portName = pv.portName);
                }
			}

			return base.RefreshPorts();
		}

		public void ForceUpdatePorts()
		{
			nodeTarget.UpdateAllPorts();

			RefreshPorts();
		}

		void UpdatePortsForField(string fieldName)
		{
			// TODO: actual code
			RefreshPorts();
		}

		protected virtual VisualElement CreateSettingsView() => new Label("Settings") {name = "header"};

		/// <summary>
		/// Send an event to the graph telling that the input property of this node have modified
		/// </summary>
		public void NotifyNodeChanging()
		{
			nodeTarget.UpdateStatus(NodeStatus.Init);
			owner.graph.NotifyNodeChanging(nodeTarget);
		}

		/// <summary>
		/// Send an event to the graph telling that the output port data of this node have refreshed
		/// </summary>
		public void NotifyNodeChanged() => owner.graph.NotifyNodeChanged(nodeTarget);

		/// <summary>
		/// Send an event to the graph telling that the node is renamed
		/// </summary>
		public void NotifyNodeRenamed() => owner.graph.NotifyNodeRenamed(nodeTarget);
		#endregion
	}

	internal class NodePropertyField : PropertyField
	{
		FieldInfo field;
		BaseNodeView nodeView;
		Action callback;
		bool isInitial;
		bool isAddChangeEvent;
		bool isInDrawer;
		bool isInInspector;
		object value;

		public NodePropertyField(
			SerializedProperty property, 
			string label, 
			FieldInfo field, 
			bool isInDrawer,
			bool isInInspector,
			BaseNodeView nodeView, 
			Action valueChangedCallback)
			: base(property, label)
		{
			this.field = field;
			this.nodeView = nodeView;
			this.callback = valueChangedCallback;
			this.isInitial = true;
			var changeEvent = field.GetCustomAttribute(typeof(ChangeEvent)) as ChangeEvent;
			this.isAddChangeEvent = changeEvent?.isAddChangeEvent ?? false;
			this.isInDrawer = isInDrawer;
			this.isInInspector = isInInspector;
			this.value = field.GetValue(nodeView.nodeTarget);
		}

		//T GetInputValueGeneric<T>()
  //      {
		//	VisualElement baseField = this.Q(className: "unity-base-field");
		//	var notify = baseField as INotifyValueChanged<T>;
		//	return notify != null ? notify.value : default(T);
  //      }

		//static MethodInfo specificGetValue = typeof(NodePropertyField).GetMethod(nameof(GetInputValueGeneric), BindingFlags.NonPublic | BindingFlags.Instance);
		//object GetInputValue()
		//{
		//	var getFunc = specificGetValue.MakeGenericMethod(field.FieldType);

		//	return getFunc.Invoke(this, new object[] { });
		//}

		//void SetInputValueGeneric<T>(T value)
  //      {
		//	VisualElement baseField = this.Q(className: "unity-base-field");
		//	var notify = baseField as INotifyValueChanged<T>;
		//	if (notify != null)
		//		notify.SetValueWithoutNotify(value);
  //      }

		//static MethodInfo specificSetValue = typeof(NodePropertyField).GetMethod(nameof(SetInputValueGeneric), BindingFlags.NonPublic | BindingFlags.Instance);
		//void SetInputValue(object value)
		//{
		//	var setAction = specificSetValue.MakeGenericMethod(field.FieldType);

		//	setAction.Invoke(this, new object[] { value });
		//}

		public void ChangeValueCallback(SerializedPropertyChangeEvent evt)
		{
			object newValue = field.GetValue(nodeView.nodeTarget);

			// Skip initialization
			if (isInitial)
            {
				//Debug.Log("init: " + nodeView.nodeTarget.GetCustomName() + " -- " + field.Name);
                isInitial = false;
				SetValueWithoutNotify(newValue);
                return;
            }

            // Skip if this drawer view do not show up
            // Could happen when an input port with drawer is connected by edge, in which case all callback
            // functions associated with the drawer should not be invoked
            if (isInDrawer && style.display == DisplayStyle.None)
                return;

            // Skip if value is not changed
            // (May experience unbinding and rebinding, and binding path may be different though)
			bool valueNotChanged = newValue != null ? newValue.Equals(value) : value == null;
			valueNotChanged &= evt.target == this;
			if (valueNotChanged)
            {
				//Debug.Log("equal: " + nodeView.nodeTarget.GetCustomName() + " -- " + field.Name);
				return;
            }

			//Debug.Log("change event: " + nodeView.nodeTarget.GetCustomName() + " -- " + field.Name);

			// Avoid redundant invoking from multiple PropertyFields that binds to the same field
			nodeView.UpdateOtherFieldValue(field, newValue);

			// Invoke value change event
			nodeView.UpdateFieldVisibility(field.Name, newValue);
			callback?.Invoke();
			nodeView.NotifyNodeChanging();

			if (isAddChangeEvent) 
				nodeView.OnFieldChanged(field.Name, field.GetValue(nodeView.nodeTarget));
		}

		public void SetValueWithoutNotify(object newValue)
        {
			value = newValue;
        }
	}
}