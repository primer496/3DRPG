using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;
using UnityEngine.AIGraph;

/// <summary>
/// The runtime TJAI graph.
/// </summary>
[ExecuteAlways]
public class RuntimeTJAIGraph : MonoBehaviour
{
    public bool repeat = true;

    /// <summary>
    /// Gets or Sets the graph.
    /// </summary>
    public TJAIGraph Graph
    {
        get { return graph; }
        set
        {
            if (graph == value)
                return;
            if (exposedParams == null)
                exposedParams = new();
            else
            {
                exposedParams.ForEach(p => p.DeleteAsset());
                exposedParams.Clear();
            }
            graph.onGraphChanges -= OnGraphChanged;
            graph = value;
            graph.onGraphChanges += OnGraphChanged;
            if (value == null)
                return;
            graph.exposedParameters.ForEach(p => exposedParams.Add(p.Clone()));
        }
    }
    [SerializeReference, HideInInspector]
    public List<ExposedParameter> exposedParams;

    bool graphChanged;
    Coroutine coroutine;
    [SerializeReference]
    TJAIGraph graph;
    TJAIGraph runtimeGraph;
    TJAIGraphProcessor processor;

    private void Start()
    {
    }

    void Update()
    {
        if (!Application.isPlaying)
            return;
        if (Graph != null && coroutine == null)
        {
            // TOCHECK: if this works for runtime?
            // OPTIMIZE: use runtime graph? may have performance issue
            if (runtimeGraph != null && graphChanged)
            {
                // update runtimeGraph if graphChanged
                DestroyImmediate(runtimeGraph);
                runtimeGraph = null;
                graphChanged = false;
            }
            if (runtimeGraph == null)
            {
                runtimeGraph = Instantiate(Graph);
                runtimeGraph.hideFlags = HideFlags.HideAndDontSave;
                processor = new TJAIGraphProcessor(runtimeGraph);
            } else if (processor == null)
            {
                processor = new TJAIGraphProcessor(runtimeGraph);
            }
            RefreshParams();
            runtimeGraph.exposedParameters = exposedParams;
            processor.ResetAll();
            coroutine = StartCoroutine(processor.RunAllAsync(CoroutineFinishedCallBack));
        } 
        // ------ for demo ------
        //else
        //{
        //    AudioClip audio = graph.GetParameterValue<AudioClip>("audio");
        //    if (audio != null)
        //    {
        //        AudioSource audioSource = GetComponent<AudioSource>();
        //        if (audioSource != null && audioSource.clip == null)
        //        {
        //            audioSource.clip = audio;
        //            audioSource.Play();
        //            audioSource.loop = true;
        //        }
        //    }
        //}
    }

    private void OnDisable()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
        if (runtimeGraph != null)
        {
            DestroyImmediate(runtimeGraph);
            runtimeGraph = null;
        }
    }

    private void OnDestroy()
    {
        //Debug.Log($"call RuntimeTJAIGraph Destroy, in Play = {Application.isPlaying}");
        if (graph != null)
            graph.onGraphChanges -= OnGraphChanged;
    }

    /// <summary>
    /// for inspector, refresh exposeParam change(list, display)
    /// </summary>
    public void RefreshParams()
    {
        HashSet<string> oldGuids = exposedParams.Select(e => e.guid).ToHashSet();
        HashSet<string> curGuids = Graph.exposedParameters.Select(e => e.guid).ToHashSet();
        foreach (string guid in oldGuids.Except(curGuids))
        {
            // remove not-existed node
            var toDelParams = exposedParams.Where(e => e.guid == guid);
            foreach (ExposedParameter toDelParam in toDelParams)
                toDelParam.DeleteAsset();
            exposedParams.RemoveAll(e => e.guid == guid);
        }
        foreach (string guid in curGuids.Except(oldGuids))
        {
            // add new node
            var curParam = Graph.exposedParameters.FirstOrDefault(e => e.guid == guid);
            exposedParams.Add(curParam.Clone());
        }
        // refresh name/accessor
        foreach (ExposedParameter param in Graph.exposedParameters)
        {
            if (!oldGuids.Contains(param.guid))
                continue;
            var oldParam = exposedParams.FirstOrDefault(e => e.guid == param.guid);
            oldParam.name = param.name;
            oldParam.settings = param.settings;
        }
    }

    /// <summary>
    /// sometimes the graph changed, but runtimeGraph is old
    /// </summary>
    /// <param name="gc"></param>
    public void OnGraphChanged(GraphChanges gc)
    {
        Debug.Log("runtime ref graph changed!");
        // NOTE: make sure runtimeGraph is update if graph changed
        // stickyNote doesn't affect data flow
        if (gc.addedStickyNotes == null && gc.removedStickyNotes == null)
            graphChanged = true;
    }

    /// <summary>
    /// set flag to check if current coroutine is finished
    /// </summary>
    public void CoroutineFinishedCallBack()
    {
        Debug.Log("runtime TJAI graph coroutine finished.");
        if (repeat)
            coroutine = null;
        // for audio clip
        foreach (ExposedParameter parameter in exposedParams)
        {
            if (parameter.GetValueType() == typeof(AudioClip))
            {
                AudioClip clip = parameter.value as AudioClip;
                AudioSource audioSource = GetComponents<AudioSource>().FirstOrDefault(
                    a => a.clip == null || a.clip?.name == clip.name);
                if (audioSource != null)
                {
                    audioSource.Stop();
                    audioSource.clip = clip;
                    audioSource.Play();
                }
            } else if (parameter.GetValueType() == typeof(Texture2D))
            {
                UpdateTexture(gameObject, parameter.value as Texture2D);
            } else if (parameter.GetValueType() == typeof(RenderTexture))
            {
                UpdateTexture(gameObject, parameter.value as RenderTexture);
            }
        }
    }

    /// <summary>
    /// update all texture ref by parameter in given gameobject
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="newTex"></param>
    void UpdateTexture<T>(GameObject obj, T newTex) where T : Texture
    {
        if (newTex == null)
            return;
        // 获取所有 Renderer 组件
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        // 遍历所有 Renderer 组件
        foreach (Renderer renderer in renderers)
        {
            // 获取所有共享材质
            Material[] materials = renderer.sharedMaterials;

            // 遍历所有材质
            foreach (Material material in materials)
            {
                // 获取所有纹理属性
                string[] propertyNames = material.GetTexturePropertyNames();

                // 遍历所有纹理属性
                foreach (string propertyName in propertyNames)
                {
                    // 获取纹理
                    Texture texture = material.GetTexture(propertyName) as Texture;

                    if (texture != null && texture.name == newTex.name)
                    {
                        material.SetTexture(propertyName, newTex);
                    }
                }
            }
        }
    }
}