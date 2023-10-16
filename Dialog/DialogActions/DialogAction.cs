using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DialogAction : ScriptableObject
{
    public abstract void Execute(DialogSpeaker speaker);
}
