using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;

public class DialogMultiChoiceNode : DialogNode
{
    private ToolbarMenu toolbarMenu;

    private VisualElement startDialogActionContainer;
    private VisualElement endDialogActionContainer;
    
    private List<ObjectField> m_startDialogActionFields = new List<ObjectField>();
    public List<ObjectField> startDialogActionFields { get => m_startDialogActionFields; }
    private List<ObjectField> m_endDialogActionFields = new List<ObjectField>();
    public List<ObjectField> endDialogActionFields { get => m_endDialogActionFields; }
    
    private TextField dialogKeyField;

    protected readonly string playerKeyBaseText = "플레이어 대사 키 값";

    public DialogMultiChoiceNode(Vector2 position) : base(position, "대사 노드")
    {
        AddTitleDropdown();
        AddChoiceAddButton();

        AddInputPort();
        AddOutputPort(playerKeyBaseText);

        AddDialogActionFields();
        AddDialogKeyFields();

        RefreshExpandedState(); // this also calls RefreshPorts()
    }

    public DialogMultiChoiceNode(DialogNodeData data) : base(data, "대사 노드")
    {
        AddTitleDropdown();
        AddChoiceAddButton();

        AddInputPort();
        
        AddDialogActionFields();
        AddDialogKeyFields();
        
        Load();
        AddOutputPort(playerKeyBaseText);

        RefreshExpandedState(); // this also calls RefreshPorts()
    }
#region Title Dropdown
    protected virtual void AddTitleDropdown()
    {
        toolbarMenu = new ToolbarMenu();
        List<string> characters = DialogGraphViewWindow.instance.speakers;
        toolbarMenu.text = characters[0];
        (dialogData as DialogMultiChoiceNodeData).speaker = toolbarMenu.text;
        foreach (var character in characters)
        {
            AddSpeakerToDropdownItem(character);
        }
        AddSpeakerToDropdownItem("플레이어");
        titleButtonContainer.Add(toolbarMenu);

        DialogGraphViewWindow.instance.OnSpeakerAdded += OnSpeakerAdded;
        DialogGraphViewWindow.instance.OnSpeakerRemoved += OnSpeakerRemoved;
    }

    private void AddSpeakerToDropdownItem(string character)
    {
        DropdownMenuItem item = new DropdownMenuAction(character, OnDropdownAction, OnDropdownStatus);
        toolbarMenu.menu.MenuItems().Add(item);
    }

    private void OnDropdownAction(DropdownMenuAction action)
    {
        toolbarMenu.text = action.name;
        (dialogData as DialogMultiChoiceNodeData).speaker = toolbarMenu.text;
    }

    private DropdownMenuAction.Status OnDropdownStatus(DropdownMenuAction action)
    {
        return DropdownMenuAction.Status.Normal;
    }

    private void OnSpeakerAdded(string speaker)
    {
        DropdownMenuItem player = toolbarMenu.menu.MenuItems().Find(item => (item as DropdownMenuAction)?.name == "플레이어");
        toolbarMenu.menu.MenuItems().Remove(player);
        AddSpeakerToDropdownItem(speaker);
        AddSpeakerToDropdownItem("플레이어");
    }

    private void OnSpeakerRemoved(string speaker)
    {
        DropdownMenuItem removing = toolbarMenu.menu.MenuItems().Find(item => (item as DropdownMenuAction)?.name == speaker);
        if (removing != null)
        {
            toolbarMenu.menu.MenuItems().Remove(removing);
        }

        if (toolbarMenu.text == speaker)
        {
            toolbarMenu.text = DialogGraphViewWindow.instance.speakers[0];
            (dialogData as DialogMultiChoiceNodeData).speaker = toolbarMenu.text;
        }
    }
#endregion

    private void AddChoiceAddButton()
    {
        Button addChoiceButton = DialogElementUtility.CreateButton("선택지 추가", () =>
        {
            AddOutputPort(playerKeyBaseText);
        });
        addChoiceButton.AddToClassList("ds-node__button");
        mainContainer.Insert(1, addChoiceButton);
    }

    protected override Port AddOutputPort(string portName)
    {
        Port outputPort = base.AddOutputPort("");

        TextField playerKeyField = DialogElementUtility.CreatePortField(portName);
        playerKeyField.RegisterValueChangedCallback((_)=>SavePlayerKey());
        Button deleteChoiceButton = DialogElementUtility.CreateButton("삭제", () => {
            OnDeleteButtonClick(outputPort);
        });
        outputPort.Add(playerKeyField);
        outputPort.Add(deleteChoiceButton);

        return outputPort;
    }

    private void OnDeleteButtonClick(Port removingPort)
    {
        if (outputContainer.childCount == 1)
        {
            return;
        }

        DialogGraphViewWindow.instance.graphView.DeleteElements(removingPort.connections);
        outputContainer.Remove(removingPort);
        m_outputPorts.Remove(removingPort);

        RefreshPorts();
    }

    protected virtual void AddDialogActionFields()
    {
        startDialogActionContainer = AddDialogActionContainer("시작");
        AddStartDialogActionField();

        extensionContainer.Add(new Label("")); // Space
        
        endDialogActionContainer = AddDialogActionContainer("종료");
        AddEndDialogActionField();
    }

    private VisualElement AddDialogActionContainer(string containerLabelText)
    {
        VisualElement dialogAcitonConatiner = new VisualElement();
        extensionContainer.Add(new Label(containerLabelText + " 액션"));
        extensionContainer.Add(dialogAcitonConatiner);

        return dialogAcitonConatiner;
    }

    protected virtual void AddStartDialogActionField()
    {
        ObjectField dialogActionField = new ObjectField()
        {
            objectType = typeof(DialogAction),
            allowSceneObjects = false
        };
        dialogActionField.RegisterValueChangedCallback(OnStartDialogActionValueChanged);

        startDialogActionFields.Add(dialogActionField);
        startDialogActionContainer.Add(dialogActionField);
    }

    private void OnStartDialogActionValueChanged(ChangeEvent<Object> changeEvent)
    {
        SaveDialogAction();

        int index = startDialogActionContainer.IndexOf(changeEvent.currentTarget as VisualElement);

        if (changeEvent.newValue == null)
        {
            if (index == startDialogActionContainer.childCount-1) // is Last
            {
                return;
            }

            startDialogActionContainer.RemoveAt(index);
            EditorWindow.focusedWindow.Close();
        }
        else
        {
            if (index == startDialogActionContainer.childCount-1)
            {
                AddStartDialogActionField();
            }
        }
    }
    
    protected virtual void AddEndDialogActionField()
    {
        ObjectField dialogActionField = new ObjectField()
        {
            objectType = typeof(DialogAction),
            allowSceneObjects = false
        };
        dialogActionField.RegisterValueChangedCallback(OnEndDialogActionValueChanged);

        endDialogActionFields.Add(dialogActionField);
        endDialogActionContainer.Add(dialogActionField);
    }

    private void OnEndDialogActionValueChanged(ChangeEvent<Object> changeEvent)
    {
        SaveDialogAction();

        int index = endDialogActionContainer.IndexOf(changeEvent.currentTarget as VisualElement);

        if (changeEvent.newValue == null)
        {
            if (index == endDialogActionContainer.childCount-1) // is Last
            {
                return;
            }

            endDialogActionContainer.RemoveAt(index);
            EditorWindow.focusedWindow.Close();
        }
        else
        {
            if (index == endDialogActionContainer.childCount-1)
            {
                AddEndDialogActionField();
            }
        }
    }

    protected virtual void AddDialogKeyFields()
    {
        dialogKeyField = DialogElementUtility.CreateQuoteField("대사 키 값");
        dialogKeyField.RegisterValueChangedCallback((_)=>SaveDialogKey());

        extensionContainer.Add(new Label(""));
        extensionContainer.Add(dialogKeyField);
    }

    private void SaveDialogAction()
    {
        Debug.Log("SaveDialogAction");
        var multiChoiceData = dialogData as DialogMultiChoiceNodeData;
        multiChoiceData.startActions = new List<DialogAction>(m_startDialogActionFields.Count);
        multiChoiceData.endActions = new List<DialogAction>(m_endDialogActionFields.Count);

        foreach (var field in m_startDialogActionFields)
        {
            if (field.value == null)
            {
                continue;
            }
            multiChoiceData.startActions.Add(field.value as DialogAction);
        }

        foreach (var field in m_endDialogActionFields)
        {
            if (field.value == null)
            {
                continue;
            }
            multiChoiceData.endActions.Add(field.value as DialogAction);
        }
    }

    private void SavePlayerKey()
    {
        Debug.Log("SavePlayerKey");
        var multiChoiceData = dialogData as DialogMultiChoiceNodeData;
        multiChoiceData.playerDialogKeys = new List<string>();
        
        foreach (var outputPort in outputPorts)
        {
            if (outputPort.connected == false)
            {
                continue;
            }

            foreach (var child in outputPort.Children())
            {
                if (child is not TextField)
                {
                    continue;
                }
                
                var text = (child as TextField).value;
                multiChoiceData.playerDialogKeys.Add(text == playerKeyBaseText ? "" : text);
            }
        }
    }

    private void SaveDialogKey()
    {
        Debug.Log("SaveDialogKey");
        (dialogData as DialogMultiChoiceNodeData).dialogKey = dialogKeyField.value;
    }

    public override void SavePortConnections()
    {
        base.SavePortConnections();

        for (int i = 0; i < outputPorts.Count; i++)
        {
            if (outputPorts[i].connected == false)
            {
                continue;
            }

            string text = (outputPorts[i].ElementAt(2) as TextField).text;
            dialogData.playerDialogKeys[i] = text == playerKeyBaseText ? "" : text;
        }
    }

    protected override void Load()
    {
        base.Load();
        
        var multiChoiceData = dialogData as DialogMultiChoiceNodeData;
        for (int i = 0; i < multiChoiceData.startActions.Count; i++)
        {
            AddStartDialogActionField();
            m_startDialogActionFields[i].value = multiChoiceData.startActions[i];
        }
        for (int i = 0; i < multiChoiceData.endActions.Count; i++)
        {
            AddEndDialogActionField();
            m_endDialogActionFields[i].value = multiChoiceData.endActions[i];
        }

        dialogKeyField.value = multiChoiceData.dialogKey;
    }
}
