using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogGraphViewWindow : GraphViewEditorWindow
{
    public static DialogGraphViewWindow instance
    {
        get 
        {
            if (m_instance == null)
            {
                m_instance = GetWindow<DialogGraphViewWindow>();
            }

            return m_instance;
        }
    }
    private static DialogGraphViewWindow m_instance;

    public DialogGraphView graphView { get; private set; }
    public DialogData dialogData { get; private set; }
    public Toolbar toolbar { get; private set; }
    public TextField choiceTextField { get; private set; }

    private int beforeCharacterButtonCount;

    public List<string> speakers = new List<string>() {"대화 상대"};
    private List<Button> speakerButtons = new List<Button>();

    public event Action<string> OnSpeakerAdded;
    public event Action<string> OnSpeakerRemoved;
#region UI
    public static void ShowGraph(DialogData data)
    {
        DialogGraphViewWindow window = GetWindow<DialogGraphViewWindow>();
        m_instance = window;

        window.dialogData = data;
        window.titleContent.text = data.name;
        window.position = new Rect(Vector2.one * 100, new Vector2(1500, 1000));
        window.Show(true);

        window.AddStyles();
        window.AddGraphView();
        window.AddToolbar();
    }

    public void AddToolbar()
    {
        toolbar = new Toolbar();

        Button addButton = DialogElementUtility.CreateButton("캐릭터 추가", OnAddButtonClick);
        choiceTextField = new TextField()
        {
            value = "대화 상대 추가"
        };

        toolbar.Add(addButton);
        toolbar.Add(choiceTextField);
        toolbar.Add(new ToolbarSpacer());
        toolbar.Add(DialogElementUtility.CreateButton("플레이어"));
        beforeCharacterButtonCount = toolbar.childCount;
        
        foreach (var character in speakers)
        {
            AddCharacterButton(character);
        }

        toolbar.AddStyleSheets("Assets/02Script/Dialog/Editor/StyleSheets/ToolbarStyle.uss");
        rootVisualElement.Add(toolbar);
        toolbar.StretchToParentWidth();
    }

    private Button AddCharacterButton(string speaker)
    {
        Button deleteButton = DialogElementUtility.CreateButton(speaker);
        deleteButton.clicked += () => OnDeleteButtonClick(speaker);
        deleteButton.tooltip = "누르면 현재 대화 참여 인원에서 삭제됩니다";

        speakerButtons.Add(deleteButton);
        toolbar.Add(deleteButton);

        OnSpeakerAdded?.Invoke(speaker);

        return deleteButton;
    }

    private void OnAddButtonClick()
    {
        if (string.IsNullOrEmpty(choiceTextField.text))
        {
            return;
        }

        if (speakers.Contains(choiceTextField.text))
        {
            return;
        }

        speakers.Add(choiceTextField.text);
        AddCharacterButton(choiceTextField.text);
    }

    private void OnDeleteButtonClick(string name)
    {
        if (speakers.Count == 1)
        {
            return;
        }

        if (speakers.Contains(name) == false)
        {
            return;
        }

        int removeIndex = speakers.IndexOf(name);
        speakers.RemoveAt(removeIndex);
        toolbar.RemoveAt(removeIndex + beforeCharacterButtonCount);

        OnSpeakerRemoved?.Invoke(name);
    }

    public void AddGraphView()
    {
        graphView = new DialogGraphView();
        rootVisualElement.Add(graphView);
        graphView.StretchToParentSize();
    }

    public void AddStyles()
    {
        rootVisualElement.AddStyleSheets("Assets/02Script/Dialog/Editor/StyleSheets/DialogGraphStyle.uss");
        rootVisualElement.AddStyleSheets("Assets/02Script/Dialog/Editor/StyleSheets/ToolbarStyle.uss");
    }

    private void OnDestroy()
    {
        Debug.Log("Destroy Graph Window2");
    }

#endregion
}
