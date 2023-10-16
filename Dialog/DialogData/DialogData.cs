using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DialogNodeData : ScriptableObject
{
    public List<DialogNodeData> childrenNodeData;
    public List<string> playerDialogKeys;

    private void OnDestroy()
    {
        Debug.Log("Node Destory");
    }

    private void OnDisable()
    {
        Debug.Log("Node Disable");
    }

    private void OnEnable()
    {
        Debug.Log("Node Enable");
    }

    private void Awake()
    {
        Debug.Log("Node Awake");
    }

    private void OnValidate()
    {
        Debug.Log("Node Validate");
    }
}

public class DialogData : ScriptableObject
{
    public List<string> speakers;
    public DialogNodeData startNode;
    
    private void OnDestroy()
    {
        string hasData = startNode ? "HasNode" : "NoNode";
        Debug.Log("Data Destory " + hasData);
    }

    private void OnDisable()
    {
        string hasData = startNode ? "HasNode" : "NoNode";
        Debug.Log("Data Disable " + hasData);
    }

    private void OnEnable()
    {
        string hasData = startNode ? "HasNode" : "NoNode";
        Debug.Log("Data Enable " + hasData);
    }

    private void Awake()
    {
        string hasData = startNode ? "HasNode" : "NoNode";
        Debug.Log("Data Awake " + hasData);
    }

    private void OnValidate()
    {
        string hasData = startNode ? "HasNode" : "NoNode";
        Debug.Log("Data Validate " + hasData);
    }
}
