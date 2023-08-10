using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Skill : MonoBehaviour
{
    [SerializeField] private string m_skillName;
    [SerializeField] private Image m_skillIcon;
    [SerializeField] protected int m_skillLevel = 0;
    [SerializeField] protected int m_maxSkillLevel = 5;

    public string skillName { get => m_skillName; }
    public Image skillIcon { get => m_skillIcon; }
    public int skillLevel { get => m_skillLevel; }
    public bool isMaxLevel { get => m_skillLevel == m_maxSkillLevel; }

    public abstract void LevelUp();
}