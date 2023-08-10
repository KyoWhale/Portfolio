using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    [SerializeField] private Skill[] m_skillPrefabList;
    private Dictionary<string, Skill> m_instanceSkillDict = new Dictionary<string, Skill>();

    public Skill[] skillList
    {
        get => m_skillPrefabList;
    }

    private static SkillManager m_instance;
    public static SkillManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<SkillManager>();
            }
            
            return m_instance;
        }

        set
        {
            if (m_instance && m_instance != value)
            {
                Destroy(value);
            }
            else
            {
                m_instance = value;
            }
        }
    }

    public List<Skill> GetUpgradeableSkillList()
    {
        List<Skill> skillList = new List<Skill>();
        
        foreach (var skill in m_skillPrefabList)
        {
            if (m_instanceSkillDict.ContainsKey(skill.skillName))
            {
                if (!m_instanceSkillDict[skill.skillName].isMaxLevel)
                {
                    skillList.Add(skill);
                }
            }
            else
            {
                skillList.Add(skill);
            }
        }

        return skillList;
    }

    public void LevelUp(string skillName)
    {
        if (m_instanceSkillDict.ContainsKey(skillName))
        {
            m_instanceSkillDict[skillName].LevelUp();
            return;
        }

        foreach (var skill in m_skillPrefabList)
        {
            if (skill.name.Equals(skillName))
            {
                var newSkill = Instantiate(skill, Vector3.zero, Quaternion.identity, transform);
                newSkill.LevelUp();
                m_instanceSkillDict[skillName] = newSkill;
                return;
            }
        }

        return;
    }
}
