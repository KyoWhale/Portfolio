using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogCanvas : MonoBehaviour
{
    private static DialogCanvas m_instance;
    public static DialogCanvas instance { get => m_instance; }

    [SerializeField] GameObject m_dialogPanel;
    private TMP_Text m_nameText;
    private TMP_Text m_dialogText;

    [SerializeField] GameObject m_choicePanel;
    private List<Button> m_choiceButtons = new List<Button>();
    private List<TMP_Text> m_choiceTexts = new List<TMP_Text>();

    public event Action dialogClicked;
    public event Action<int> choiceClicked;

    private void Start()
    {
        if (m_instance != null && m_instance != this)
        {
            Destroy(this);
        }
        m_instance = this;

        Initialize();
    }

    private void Initialize()
    {
        Button dialogClickButton = m_dialogPanel.GetComponentInChildren<Button>();
        dialogClickButton.onClick.AddListener(() => dialogClicked?.Invoke());
        TMP_Text[] texts = m_dialogPanel.GetComponentsInChildren<TMP_Text>();
        m_nameText = texts[0];
        m_dialogText = texts[1];

        Button[] buttons = m_choicePanel.GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            m_choiceButtons.Add(buttons[i]);
            int j = i; // 클로져 문제로 인해 지역변수 사용
            buttons[i].onClick.AddListener(()=>choiceClicked?.Invoke(j));
            TMP_Text text = buttons[i].GetComponentInChildren<TMP_Text>();
            m_choiceTexts.Add(text);
        }

        m_dialogPanel.SetActive(false);
        m_choicePanel.SetActive(false);
    }

    public void ShowDialogPanel(string speaker, string dialogText)
    {
        m_nameText.text = speaker;
        m_dialogText.text = dialogText;

        m_dialogPanel.SetActive(true);
    }

    public void ShowChoicePanel(List<string> chocieTexts)
    {
        if (chocieTexts.Count > m_choiceTexts.Count)
        {
            Debug.LogError("너무 많은 선택지");
            return;
        }

        for (int i = 0; i < chocieTexts.Count; i++)
        {
            m_choiceTexts[i].text = chocieTexts[i];
            m_choiceTexts[i].gameObject.SetActive(true);
        }

        for (int i = chocieTexts.Count; i < m_choiceButtons.Count; i++)
        {
            m_choiceButtons[i].gameObject.SetActive(false);
        }

        m_choicePanel.SetActive(true);
    }

    public void CloseDialogPanel()
    {
        m_dialogPanel.SetActive(false);
    }

    public void CloseChoicePanel()
    {
        m_choicePanel.SetActive(false);
    }

    public void BtnClick()
    {
        Debug.Log("BtnClk");
    }
}
