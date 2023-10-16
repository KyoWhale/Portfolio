using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using System;
using UnityEditor;
using System.Linq;

public abstract class DialogNode : Node
{
    private DialogNodeData m_dialogData;
    public DialogNodeData dialogData
    {
        get
        {
            if (m_dialogData == null)
            {
                m_dialogData = CreateNodeData();
                AssetDatabase.CreateAsset(m_dialogData, DialogDataEditor.baseFileSavePath + "/" + DialogGraphViewWindow.instance.dialogData.name + "/" + (++assetNumbering) + ".asset");
            }
            return m_dialogData;
        }
    }

    public DialogPort inputPort { get; protected set; }

    protected List<Port> m_outputPorts = new List<Port>();
    public List<Port> outputPorts { get => m_outputPorts; }

    private List<string> m_playerKeys = new List<string>();
    public List<string> playerKeys { get => m_playerKeys; }

    private static int assetNumbering = 0;

    public DialogNode()
    {
        AddStyles();
        RemoveTitleCollapseButton();
    }

    public DialogNode(Vector2 position, string nodeName = "노드", DialogNodeData data = null) : this()
    {
        title = nodeName;
        SetPosition(new Rect(position, Vector2.zero));
    }

    public DialogNode(DialogNodeData data, string nodeName = "노드") : this()
    {
        title = nodeName;
        m_dialogData = data;
    }

    protected virtual DialogNodeData CreateNodeData()
    {
        var data = ScriptableObject.CreateInstance<DialogMultiChoiceNodeData>();
        data.childrenNodeData = new List<DialogNodeData>();
        data.playerDialogKeys = new List<string>();
        data.startActions = new List<DialogAction>();
        data.endActions = new List<DialogAction>();
        return data;
    }

    private void RemoveTitleCollapseButton()
    {
        titleButtonContainer.RemoveAt(0);
    }

    protected virtual Port AddInputPort(string portName = "이전 노드")
    {
        inputPort = this.CreatePort(portName, Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
        inputContainer.Add(inputPort);
        return inputPort;
    }

    protected virtual Port AddOutputPort(string portName)
    {
        var outputPort = this.CreatePort(portName, Orientation.Horizontal, Direction.Output, Port.Capacity.Single);
        m_outputPorts.Add(outputPort);
        outputContainer.Add(outputPort);
        return outputPort;
    }

    private void AddStyles()
    {
        mainContainer.AddToClassList("ds-node__main-container");
        extensionContainer.AddToClassList("ds-node__extension-container");
    }

    public virtual void SavePortConnections()
    {
        Debug.Log("SavePortConnections");
        dialogData.childrenNodeData = new List<DialogNodeData>(outputPorts.Count);
        dialogData.playerDialogKeys = new List<string>(outputPorts.Count);

        for (int i = 0; i < outputPorts.Count; i++)
        {
            if (outputPorts[i].connected == false)
            {
                continue;
            }

            dialogData.childrenNodeData.Add((outputPorts[i].connections.ElementAt(0).input.node as DialogNode).dialogData);
            dialogData.playerDialogKeys.Add(outputPorts[i].portName);
        }
    }

    protected virtual void Load()
    {
        for (int i = 0; i < m_dialogData.childrenNodeData.Count; i++)
        {
            AddOutputPort(m_dialogData.playerDialogKeys[i]);
        }
    }

    public void Delete()
    {
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(dialogData));
    }
}
