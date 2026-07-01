using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


/// <summary>
/// Makes a field to select and display file paths. Supports both direct input and file browser selection.
/// </summary>
public class FilePathField : BaseField<string>
{
    /// <summary>
    /// Instantiates a <see cref="FilePathField"/> using the data read from a UXML file.
    /// </summary>
    public new class UxmlFactory : UxmlFactory<FilePathField, UxmlTraits>
    {
    }

    /// <summary>
    /// Defines <see cref="UxmlTraits"/> for the <see cref="FilePathField"/>.
    /// </summary>
    public new class UxmlTraits : BaseField<string>.UxmlTraits
    {
        UxmlStringAttributeDescription m_DefaultPath = new UxmlStringAttributeDescription
            { name = "default-path", defaultValue = "" };

        UxmlStringAttributeDescription m_FileExtension = new UxmlStringAttributeDescription
            { name = "file-extension", defaultValue = "" };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var field = (FilePathField)ve;
            field.defaultPath = m_DefaultPath.GetValueFromBag(bag, cc);
            field.fileExtension = m_FileExtension.GetValueFromBag(bag, cc);
        }
    }

    private string m_DefaultPath;
    private string m_FileExtension;
    private VisualElement m_inputContainer;
    private TextField m_TextField;
    private Button m_BrowseButton;

    /// <summary>
    /// The default path to open the file browser at.
    /// </summary>
    public string defaultPath
    {
        get => m_DefaultPath;
        set => m_DefaultPath = value;
    }

    /// <summary>
    /// The file extension filter (e.g. "json" or "png").
    /// </summary>
    public string fileExtension
    {
        get => m_FileExtension;
        set => m_FileExtension = value;
    }

    /// <summary>
    /// USS class name of elements of this type.
    /// </summary>
    public new static readonly string ussClassName = "unity-file-path-field";

    /// <summary>
    /// USS class name of labels in elements of this type.
    /// </summary>
    public new static readonly string labelUssClassName = ussClassName + "__label";

    /// <summary>
    /// USS class name of input elements in elements of this type.
    /// </summary>
    public new static readonly string inputUssClassName = ussClassName + "__input";

    /// <summary>
    /// USS class name of text elements in elements of this type.
    /// </summary>
    public static readonly string textUssClassName = ussClassName + "-display__label";

    /// <summary>
    /// USS class name of button elements in elements of this type.
    /// </summary>
    public static readonly string buttonUssClassName = "unity-object-field__selector";

    /// <summary>
    /// Constructor.
    /// </summary>
    public FilePathField() : this(null)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public FilePathField(string label) : base(label, null)
    {
        AddToClassList(ussClassName);
        labelElement.AddToClassList(labelUssClassName);

        // Create text field
        m_TextField = new TextField
        {
            isDelayed = true
        };
        m_TextField.AddToClassList(textUssClassName);

        // Create browse button
        m_BrowseButton = new Button(BrowseFile);
        m_BrowseButton.AddToClassList(buttonUssClassName);

        // Create container for input elements
        m_inputContainer = new VisualElement
        {
            style = { flexDirection = FlexDirection.Row }
        };
        m_inputContainer.AddToClassList("unity-base-field__input");
        m_inputContainer.AddToClassList(inputUssClassName);
        m_inputContainer.Add(m_TextField);
        m_inputContainer.Add(m_BrowseButton);
        RemoveAt(1);
        Add(m_inputContainer);

        // Bind text field to value
        RegisterCallback<AttachToPanelEvent>(
            evt => m_TextField.RegisterValueChangedCallback(OnTextChanged));
        RegisterCallback<DetachFromPanelEvent>(
            evt => m_TextField.UnregisterValueChangedCallback(OnTextChanged));

        // Enable drag and drop
        RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
        RegisterCallback<DragPerformEvent>(OnDragPerform);
        RegisterCallback<DragLeaveEvent>(OnDragLeave);
    }

    private void OnTextChanged(ChangeEvent<string> evt)
    {
        value = evt.newValue;
    }

    public override void SetValueWithoutNotify(string newValue)
    {
        base.SetValueWithoutNotify(newValue);
        m_TextField.SetValueWithoutNotify(newValue);
    }

    private void BrowseFile()
    {
        string directory = string.IsNullOrEmpty(value)
            ? (string.IsNullOrEmpty(defaultPath) ? Application.dataPath : defaultPath)
            : Path.GetDirectoryName(value);

        string path = EditorUtility.OpenFilePanel("Select File", directory, fileExtension);
        if (!string.IsNullOrEmpty(path))
        {
            // Convert to relative path if within project
            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }

            value = path;
        }
    }

    private void OnDragUpdated(DragUpdatedEvent evt)
    {
        if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            evt.StopPropagation();
        }
    }

    private void OnDragPerform(DragPerformEvent evt)
    {
        if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
        {
            string path = DragAndDrop.paths[0];
            if (File.Exists(path))
            {
                // Convert to relative path if within project
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                }

                value = path;
            }

            evt.StopPropagation();
        }
    }

    private void OnDragLeave(DragLeaveEvent evt)
    {
        // Clear any drag visuals
    }
}