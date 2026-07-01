using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace GraphProcessor
{
	[System.Serializable]
	public abstract class BaseGraphWindow : EditorWindow
	{
		protected VisualElement		rootView;
		protected BaseGraphView		graphView;

		[SerializeField]
		protected BaseGraph			graph;

		readonly string				graphWindowStyle = "GraphProcessorStyles/BaseGraphView";

		public bool					isGraphLoaded
		{
			get { return graphView != null && graphView.graph != null; }
		}

		bool						reloadWorkaround = false;

		public event Action< BaseGraph >	graphLoaded;
		public event Action< BaseGraph >	graphUnloaded;

		/// <summary>
		/// Called by Unity when the window is enabled / opened
		/// </summary>
		protected virtual void OnEnable()
		{
			InitializeRootView();

			if (graph != null)
				LoadGraph();
			else
				reloadWorkaround = true;
		}

		protected virtual void Update()
		{
			// Workaround for the Refresh option of the editor window:
			// When Refresh is clicked, OnEnable is called before the serialized data in the
			// editor window is deserialized, causing the graph view to not be loaded
			if (reloadWorkaround && graph != null)
			{
				LoadGraph();
				reloadWorkaround = false;
			}
			graph?.OnUpdate();
		}

		void LoadGraph()
		{
            // We wait for the graph to be initialized
            if (graph.isEnabled)
                InitializeGraph(graph);
            else
                graph.onEnabled += () => InitializeGraph(graph);
		}

		/// <summary>
		/// Called by Unity when the window is disabled (happens on domain reload)
		/// </summary>
		protected virtual void OnDisable()
		{
			if (graph == null || graphView == null) return;
			foreach (var nodeView in graphView.nodeViews)
				nodeView.Disable();
		}
		
		/// <summary>
		/// Called by Unity when the window is closed
		/// </summary>
		protected virtual void OnDestroy() { }

		void InitializeRootView()
		{
			rootView = base.rootVisualElement;

			rootView.name = "graphRootView";

			rootView.styleSheets.Add(Resources.Load<StyleSheet>(graphWindowStyle));
		}

		public void InitializeGraph(BaseGraph graph)
		{
			if (this.graph != null && graph != this.graph)
			{
				// Save the graph to the disk
				EditorUtility.SetDirty(this.graph);
				graph.Save();
				// Unload the graph
				graphUnloaded?.Invoke(this.graph);
			}

			graphLoaded?.Invoke(graph);
			this.graph = graph;

			if (graphView != null)
				rootView.Remove(graphView);

			//Initialize will provide the BaseGraphView
			if (!InitializeWindow(graph))
			{
				Close();
			}

			graphView = rootView.Children().FirstOrDefault(e => e is BaseGraphView) as BaseGraphView;

			if (graphView == null)
			{
				//Debug.LogError("GraphView has not been added to the BaseGraph root view !");
				return ;
			}

			graphView.Initialize(graph);

			InitializeGraphView(graphView);

			// TOOD: onSceneLinked...

			if (graph.IsLinkedToScene())
				LinkGraphWindowToScene(graph.GetLinkedScene());
			else
				graph.onSceneLinked += LinkGraphWindowToScene;
			
			// restart running job
			var processingNode = string.Empty;
			if (EditorPrefs.HasKey(GlobalConstants.AI_PROCESSING_NODE_KEY))
			{
				processingNode = EditorPrefs.GetString(GlobalConstants.AI_PROCESSING_NODE_KEY);
				EditorPrefs.DeleteKey(GlobalConstants.AI_PROCESSING_NODE_KEY);
				foreach (var node in graph.nodes.Where(
					         node => processingNode == (graph.name + node.name) && !node.isUpdate))
				{
					graph.onScriptTriggerNode?.Invoke(node);
				}
			}
		}

		void LinkGraphWindowToScene(Scene scene)
		{
			EditorSceneManager.sceneClosed += CloseWindowWhenSceneIsClosed;

			void CloseWindowWhenSceneIsClosed(Scene closedScene)
			{
				if (scene == closedScene)
				{
					Close();
					EditorSceneManager.sceneClosed -= CloseWindowWhenSceneIsClosed;
				}
			}
		}

		public virtual void OnGraphDeleted(BaseGraph tmp)
		{
			if (tmp != graph)
				return;
			if (graph != null && graphView != null)
				graphView.Dispose();

			graphView = null;
			Close();
		}

		protected abstract bool	InitializeWindow(BaseGraph graph);
		protected virtual void InitializeGraphView(BaseGraphView view) {}
	}
}