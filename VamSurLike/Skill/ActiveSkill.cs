using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageType
{
    Normal,
    Slow,
    Stun
}

public enum DamageDotType
{
    None,
    Fire,
    Ice,
    Bolt,
    Poison
}

public abstract class ActiveSkill : Skill
{
    [SerializeField] private DamageType m_damageType;
    [SerializeField] private DamageDotType m_damageDotType;

    [SerializeField] private DamagerLauncher m_launcherPrefab;
    protected List<DamagerLauncher> m_launchers = new List<DamagerLauncher>();
    [SerializeField] private int m_startLauncherCount = 1;
    [SerializeField] private int m_increaseLauncherPerLevel = 1;

    [SerializeField] private bool m_looping = true; // looping은 필살기를 제외하고 항상 true => 우리가 직접 눌러서 스킬을 사용하는게 아닌 시간이 되면 다시 돌아야하기 때문
    [SerializeField] private float m_duration;
    [SerializeField] private float m_speed;
    [SerializeField] private float m_angle = 30f;
    [SerializeField] private Vector3 m_3DStartSizeMin = Vector3.one;
    [SerializeField] private Vector3 m_3DStartSizeMax = Vector3.one;

#region Skill Upgrade Properties
    public int launcherCount
    {
        get
        {
            return m_launchers.Count;
        }
        set
        {
            if (value - m_launchers.Count <= 0) return;

            int increaseCount = value - m_launchers.Count;
            for (int i = 0; i < increaseCount; i++)
            {
                var newLauncher = IncreaseLauncher();
                SetLauncherDetail(newLauncher);
            }
        }
    }

    protected abstract void SetLauncherTransform(DamagerLauncher launcher, int index, float angle);
    protected abstract void SetLauncherDetail(DamagerLauncher launcher);

    public float Speed
    {
        get
        {
            return m_speed;
        }
        set
        {
            if (value <= m_speed) return;

            SetSpeed(value);
            m_speed = value;
        }
    }

    protected virtual void SetSpeed(float value)
    {
        foreach (var launcher in m_launchers)
        {
            launcher.speed = value;
        }
    }

    public float Duration
    {
        get
        {
            return m_duration;
        }
        set
        {
            if (value <= m_duration) return;

            SetDuration(value);
            m_duration = value;
        }
    }

    protected virtual void SetDuration(float value)
    {
        foreach (var launcher in m_launchers)
        {
            launcher.duration = value;
        }
    }
#endregion
    protected virtual void Start()
    {
        launcherCount = m_startLauncherCount;
    }

    public override void LevelUp()
    {
        if (m_skillLevel < m_maxSkillLevel)
        {
            m_skillLevel++;
            launcherCount = m_increaseLauncherPerLevel * skillLevel;
        }
    }

    private DamagerLauncher IncreaseLauncher()
    {
        var newLauncher = Instantiate(m_launcherPrefab, Vector3.zero, Quaternion.identity, transform);
        m_launchers.Add(newLauncher);
        UpdateSettings();
        return newLauncher;
    }

    private void UpdateSettings()
    {
        float startAngle = -(m_launchers.Count - 1) * m_angle / 2;
        for (int i = 0; i < m_launchers.Count; ++i)
        {
            SetLauncherTransform(m_launchers[i], i, Mathf.Deg2Rad * (startAngle + m_angle * i));
            m_launchers[i].Stop();
            m_launchers[i].Clear();
            m_launchers[i].Play();
        }
    }
}