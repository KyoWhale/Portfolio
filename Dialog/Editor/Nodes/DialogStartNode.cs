using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogStartNode : DialogNode
{
    public Port beforePort { get; private set; }
    public Port ongoingPort { get; private set; }
    public Port afterPort { get; private set; }

    public DialogStartNode(Vector2 position) : base(position, "시작 노드")
    {
        beforePort = AddOutputPort("퀘스트 시작 전");
        ongoingPort = AddOutputPort("퀘스트 진행 중");
        afterPort = AddOutputPort("퀘스트 완료 후");
    }

    public DialogStartNode(DialogNodeData data) : base(data, "시작 노드")
    {
        Load();
    }
}
