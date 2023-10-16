using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogConditionNode : DialogNode
{
    public Port truePort { get; private set; }
    public Port falsePort { get; private set; }

    public ObjectField dialogConditionField { get; private set; }

    public DialogConditionNode(Vector2 position) : base(position, "조건 노드")
    {
        AddInputPort();
        truePort = AddOutputPort("참");
        falsePort = AddOutputPort("거짓");

        AddDialogConditionField();

        RefreshExpandedState(); // this also calls RefreshPorts()
    }

    public DialogConditionNode(DialogNodeData data) : base(data, "조건 노드")
    {
        
        AddInputPort();
        truePort = AddOutputPort("참");
        falsePort = AddOutputPort("거짓");

        AddDialogConditionField();
        
        Load();

        RefreshExpandedState(); // this also calls RefreshPorts()
    }

    protected override DialogNodeData CreateNodeData()
    {
        var data = ScriptableObject.CreateInstance<DialogConditionNodeData>();
        data.childrenNodeData = new List<DialogNodeData>() { null, null };
    
        return data;
    }

    private void AddDialogConditionField()
    {
        dialogConditionField = new ObjectField()
        {
            objectType = typeof(DialogCondition),
            allowSceneObjects = false
        };
        dialogConditionField.RegisterValueChangedCallback((_)=>SaveDialogConditionField());
        extensionContainer.Add(new Label("조건"));
        extensionContainer.Add(dialogConditionField);
    }

    private void SaveDialogConditionField()
    {
        var conditionData = dialogData as DialogConditionNodeData;

        conditionData.childrenNodeData[0] = truePort.connected ? (truePort.connections.ElementAt(0).input.node as DialogNode).dialogData : null;
        conditionData.childrenNodeData[1] = falsePort.connected ? (falsePort.connections.ElementAt(0).input.node as DialogNode).dialogData : null;

        conditionData.dialogCondition = dialogConditionField.value as DialogCondition;
    }

    protected override void Load()
    {
        var conditionData = dialogData as DialogConditionNodeData;
        dialogConditionField.value = conditionData.dialogCondition;
    }
}