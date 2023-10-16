using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogMultiChoiceNodeData : DialogNodeData
{
    public string speaker;
    public string dialogKey;
    public List<DialogAction> startActions;
    public List<DialogAction> endActions;
}
