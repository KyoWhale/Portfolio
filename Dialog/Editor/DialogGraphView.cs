using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public class DialogGraphView : GraphView
{
    public DialogData dialogData { get; private set; }

    public DialogStartNode startNode { get; private set; }

    public DialogGraphView()
    {
        AddStyles();
        AddGridBackground();
        AddManipulators();

        dialogData = DialogGraphViewWindow.instance.dialogData;

        bool hasData = dialogData.startNode != null;
        if (hasData)
        {
            Load();
        }
        else
        {
            AddInitialNodes();
        }

        graphViewChanged = OnGraphChange;
    }

    private void AddManipulators()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        this.AddManipulator(CreateNodeContextualMenu("[대사 노드]", typeof(DialogMultiChoiceNode)));
        this.AddManipulator(CreateNodeContextualMenu("[조건 노드]", typeof(DialogConditionNode)));
    }

    private IManipulator CreateNodeContextualMenu(string additionalMenuInfo, System.Type node)
    {
        ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
            menuEvent => menuEvent.menu.AppendAction("Add " + additionalMenuInfo, actionEvent => AddElement(CreateNode(node, actionEvent.eventInfo.localMousePosition)))
        );

        return contextualMenuManipulator;
    }

    private DialogNode CreateNode(System.Type node, Vector2 position)
    {
        if (node.Equals(typeof(DialogConditionNode)))
        {
            return new DialogConditionNode(position);
        }
        else
        {
            return new DialogMultiChoiceNode(position);
        }
    }

    private void AddInitialNodes()
    {
        startNode = new DialogStartNode(Vector2.one * 100);
        startNode.SavePortConnections();
        var before = new DialogMultiChoiceNode(Vector2.one * 100 + Vector2.right * 300);
        var ongoing = new DialogMultiChoiceNode(Vector2.one * 100 + Vector2.right * 300 + Vector2.up * 350);
        var after = new DialogMultiChoiceNode(Vector2.one * 100 + Vector2.right * 300 + Vector2.up * 700);
        AddElement(startNode);
        AddElement(before);
        AddElement(ongoing);
        AddElement(after);
        AddElement(startNode.beforePort.ConnectTo(before.inputPort));
        AddElement(startNode.ongoingPort.ConnectTo(ongoing.inputPort));
        AddElement(startNode.afterPort.ConnectTo(after.inputPort));

        dialogData.startNode = startNode.dialogData;
    }

    private void AddGridBackground()
    {
        GridBackground gridBackground = new GridBackground();
        gridBackground.StretchToParentSize();
        Insert(0, gridBackground);
    }

    private void AddStyles()
    {
        this.AddStyleSheets("Assets/02Script/Dialog/Editor/StyleSheets/DialogGraphStyle.uss");
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> compatiblePorts = new List<Port>();

        ports.ForEach(port =>
        {
            if (startPort == port)
            {
                return;
            }

            if (startPort.node == port.node)
            {
                return;
            }

            if (startPort.direction == port.direction)
            {
                return;
            }

            compatiblePorts.Add(port);
        });

        return compatiblePorts;
    }

    private GraphViewChange OnGraphChange(GraphViewChange change)
    {
        if (change.elementsToRemove != null)
        {
            foreach (GraphElement e in change.elementsToRemove)
            {
                if (e is DialogNode)
                {
                    (e as DialogNode).Delete();
                }
            }
        }

        return change;
    }

    public void Load()
    {
        string[] guids = AssetDatabase.FindAssets("", new string[] { DialogDataEditor.baseFileSavePath + "/" + DialogGraphViewWindow.instance.dialogData.name + "/" });
        Dictionary<DialogNodeData, DialogNode> dataNodeMap = new Dictionary<DialogNodeData, DialogNode>();

        CreateNodes(guids, dataNodeMap);
        ConnectPorts(dataNodeMap);
        SetNodesPositions(dataNodeMap);
    }

    private void CreateNodes(string[] guids, Dictionary<DialogNodeData, DialogNode> dataNodeMap)
    {
        startNode = new DialogStartNode(dialogData.startNode);
        dataNodeMap[dialogData.startNode] = startNode;
        AddElement(startNode);
        startNode.SetPosition(new Rect(Vector2.one * 100, Vector2.zero));
        for (int i = 1; i < guids.Length; i++) // 0 is startNode
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            DialogNodeData nodeData = (DialogNodeData)AssetDatabase.LoadAssetAtPath(assetPath, typeof(DialogNodeData));
            if (nodeData is DialogMultiChoiceNodeData)
            {
                dataNodeMap[nodeData] = new DialogMultiChoiceNode(nodeData);
            }
            else if (nodeData is DialogConditionNodeData)
            {
                dataNodeMap[nodeData] = new DialogConditionNode(nodeData);
            }
            AddElement(dataNodeMap[nodeData]);
        }
    }

    private void ConnectPorts(Dictionary<DialogNodeData, DialogNode> dataNodeMap)
    {
        foreach (var (data, node) in dataNodeMap)
        {
            var nextNodes = data.childrenNodeData;
            for (int i = 0; i < node.outputPorts.Count; i++)
            {
                if (nextNodes.Count <= i)
                {
                    break;
                }

                if (nextNodes[i] == null) // ConditionData's children could be null
                {
                    continue;
                }

                var nextNodeInputPort = dataNodeMap[nextNodes[i]].inputPort;
                AddElement(node.outputPorts[i].ConnectTo(nextNodeInputPort));
            }
        }
    }

    private void SetNodesPositions(Dictionary<DialogNodeData, DialogNode> dataNodeMap)
    {
        // Queue<(previous, current, depth)>
        Queue<(DialogNode, DialogNode, int)> nodes = new Queue<(DialogNode, DialogNode, int)>();
        Queue<(DialogNode, DialogNode, int)> nextNodes = new Queue<(DialogNode, DialogNode, int)>();
        
        for (int i = 0; i < startNode.dialogData.childrenNodeData.Count; i++)
        {
            var nextNode = dataNodeMap[startNode.dialogData.childrenNodeData[i]];
            nextNodes.Enqueue((startNode, nextNode, 1));
        }

        while (nextNodes.Count > 0)
        {
            while (nextNodes.Count > 0)
            {
                nodes.Enqueue(nextNodes.Dequeue());
            }

            int index = 0;
            while (nodes.Count > 0)
            {
                var (previous, current, depth) = nodes.Dequeue();
                var position = new Rect(new Vector2(depth * 400, index++ * 400), Vector2.zero);
                current.SetPosition(position);

                foreach (var nextNodeData in current.dialogData.childrenNodeData)
                {
                    if (nextNodeData == null) // ConditionNodeData could be null
                    {
                        continue;
                    }
                    var nextNode = dataNodeMap[nextNodeData];
                    nextNodes.Enqueue((current, nextNode, depth+1));
                }
            }
        }
    }
}
