using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Talkable))]
public class DialogSpeaker : MonoBehaviour
{
    [SerializeField] DialogData m_dialogData;
    private DialogNodeData m_currentNodeData;

    public void StartDialog()
    {
        var dialogCanvas = DialogCanvas.instance;
        dialogCanvas.dialogClicked += OnDialogClicked;
        dialogCanvas.choiceClicked += OnChoiceClicked;

        int i = 0; // TODO: Should Set isFirst, Doing, Finished
        m_currentNodeData = m_dialogData.startNode.childrenNodeData[i];
        m_currentNodeData = UntilMultiChoiceDataReveal();
        if (m_currentNodeData)
        {
            ShowDialog();
        }
        else
        {
            EndDialog();
            return;
        }
    }

    private void ShowDialog()
    {
        var multiChoiceData = m_currentNodeData as DialogMultiChoiceNodeData;
        var dialogCanvas = DialogCanvas.instance;

        dialogCanvas.ShowDialogPanel(multiChoiceData.speaker, multiChoiceData.dialogKey);
        if (m_currentNodeData.childrenNodeData != null && m_currentNodeData.childrenNodeData.Count > 1)
        {
            dialogCanvas.ShowChoicePanel(m_currentNodeData.playerDialogKeys);
        }

        foreach (var action in multiChoiceData.startActions)
        {
            action.Execute(this);
        }
    }

    private void ShowNextDialog(int index = 0)
    {
        var endActions = (m_currentNodeData as DialogMultiChoiceNodeData).endActions;
        foreach (var action in endActions)
        {
            action.Execute(this);
        }
        
        if (m_currentNodeData.childrenNodeData == null || m_currentNodeData.childrenNodeData.Count <= index)
        {
            EndDialog();
            return;
        }

        m_currentNodeData = m_currentNodeData.childrenNodeData[index];
        m_currentNodeData = UntilMultiChoiceDataReveal();
        if (m_currentNodeData)
        {
            ShowDialog();
        }
        else
        {
            EndDialog();
        }
    }

    private DialogNodeData UntilMultiChoiceDataReveal()
    {
        var nodeData = m_currentNodeData;
        if (nodeData == null)
        {
            return null;
        }

        while (nodeData is DialogConditionNodeData)
        {
            if (nodeData.childrenNodeData == null || nodeData.childrenNodeData.Count == 0)
            {
                return null;
            }

            var conditionData = nodeData as DialogConditionNodeData;
            if (conditionData.dialogCondition == null)
            {
                return null;
            }

            bool conditionResult = conditionData.dialogCondition.Execute();
            if (conditionResult == true)
            {
                nodeData = nodeData.childrenNodeData[0];
            }
            else
            {
                nodeData = nodeData.childrenNodeData[1];
            }

            if (nodeData == null)
            {
                return null;
            }
        }

        return nodeData;
    }

    private void OnDialogClicked()
    {
        ShowNextDialog();
    }

    private void OnChoiceClicked(int index)
    {
        var dialogCanvas = DialogCanvas.instance;
        dialogCanvas.CloseChoicePanel();

        ShowNextDialog(index);
    }

    private void EndDialog()
    {
        var dialogCanvas = DialogCanvas.instance;
        dialogCanvas.dialogClicked -= OnDialogClicked;
        dialogCanvas.choiceClicked -= OnChoiceClicked;

        dialogCanvas.CloseChoicePanel();
        dialogCanvas.CloseDialogPanel();
    }
}
